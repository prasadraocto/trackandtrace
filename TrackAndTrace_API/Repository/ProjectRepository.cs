using TrackAndTrace_API.Interface;
using TrackAndTrace_API.Models;
using TrackAndTrace_API.Models.RequestModel;
using TrackAndTrace_API.Models.ResponseModel;
using Microsoft.EntityFrameworkCore;
using TrackAndTrace_API.Models.DBModel;
using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace TrackAndTrace_API.Repository
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IWarehouseRepository _interfaceWarehouse;
        public ProjectRepository(ApplicationDbContext context, IMapper mapper, IConfiguration configuration, IWarehouseRepository interfaceWarehouse)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
            _interfaceWarehouse = interfaceWarehouse;
        }

        public async Task<APIResponseDTO> Add(ProjectDto model, ExtractTokenDto token)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            try
            {
                var nameExists = await _context.Project.AnyAsync(x => x.name.ToLower() == model.name.ToLower() && x.id != model.id && x.company_id == token.CompanyId);
                if (nameExists)
                {
                    aPIResponseDTO.message = "Project Name already exists";
                    return aPIResponseDTO;
                }

                var codeExists = await _context.Project.AnyAsync(x => x.code.ToLower() == model.code.ToLower() && x.id != model.id && x.company_id == token.CompanyId);
                if (codeExists)
                {
                    aPIResponseDTO.message = "Project Code already exists";
                    return aPIResponseDTO;
                }

                if (model.project_user_mapping.Count() == 0)
                {
                    aPIResponseDTO.message = "Project Mapping required";
                    return aPIResponseDTO;
                }

                var dmo = _mapper.Map<Project>(model);

                if (dmo.id > 0)
                {
                    var existingProject = await _context.Project.AsNoTracking().FirstOrDefaultAsync(x => x.id == dmo.id);

                    if (existingProject == null)
                    {
                        aPIResponseDTO.message = "Project details not found";
                        return aPIResponseDTO;
                    }

                    WarehouseDto warehouseDto = new WarehouseDto
                    {
                        id = existingProject.warehouse_id,
                        code = model.code,
                        name = model.name,
                        address = model.address ?? "",
                        active_flag = true,
                    };
                    var result = await _interfaceWarehouse.Add(warehouseDto, token);

                    dmo.warehouse_id = existingProject.warehouse_id;
                    dmo.company_id = token.CompanyId;
                    dmo.created_by = existingProject.created_by;
                    dmo.created_date = existingProject.created_date;
                    dmo.updated_by = token.UserId;
                    dmo.updated_date = DateTime.Now;
                    _context.Project.Update(dmo);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    WarehouseDto warehouseDto = new WarehouseDto
                    {
                        code = model.code,
                        name = model.name,
                        address = model.address ?? "",
                        active_flag = true,
                    };
                    var result = await _interfaceWarehouse.Add(warehouseDto, token);

                    dmo.warehouse_id = Convert.ToInt32(result.data);
                    dmo.company_id = token.CompanyId;
                    dmo.created_by = token.UserId;
                    dmo.created_date = DateTime.Now;
                    dmo.updated_date = null;
                    await _context.Project.AddAsync(dmo);
                    await _context.SaveChangesAsync();
                }

                foreach (var mapping in model.project_user_mapping)
                {
                    mapping.project_id = dmo.id;
                }

                var response = await SaveProjectUserMapping(model.project_user_mapping);

                if (!response)
                {
                    aPIResponseDTO.success = false;
                    aPIResponseDTO.message = "Failed to save Project mapping";
                    return aPIResponseDTO;
                }

                aPIResponseDTO.success = true;
                aPIResponseDTO.message = "Project saved successfully";
                return aPIResponseDTO;
            }
            catch (Exception ex)
            {
                aPIResponseDTO.message = $"Failed saving details: {ex.Message}";
                return aPIResponseDTO;
            }
        }
        public async Task<bool> SaveProjectUserMapping(List<Project_User_Mapping> requestList)
        {
            try
            {
                // Extract project_id and user_id pairs from the requestList
                var requestPairs = requestList.Select(request => new { request.project_id, request.user_id }).ToHashSet();

                // Fetch only the relevant existing mappings from the database
                var projectIds = requestList.Select(r => r.project_id).ToHashSet();
                var existingMappings = await _context.Project_User_Mapping.Where(mapping => projectIds.Contains(mapping.project_id)).ToListAsync();

                // Prepare lists for new mappings and updates
                var newMappings = new List<Project_User_Mapping>();
                var updatedMappings = new List<Project_User_Mapping>();

                foreach (var existing in existingMappings)
                {
                    // Check if the existing mapping is in the requestList
                    var isInRequest = requestPairs.Contains(new { existing.project_id, existing.user_id });

                    if (isInRequest)
                    {
                        // If the mapping exists in the request but has delete_flag = true, update it
                        if (existing.delete_flag)
                        {
                            existing.delete_flag = false;
                            updatedMappings.Add(existing);
                        }
                    }
                    else
                    {
                        // If the mapping is not in the requestList and is not already deleted, mark it as deleted
                        if (!existing.delete_flag)
                        {
                            existing.delete_flag = true;
                            updatedMappings.Add(existing);
                        }
                    }
                }

                // Add only new mappings
                var existingPairs = existingMappings.Select(existing => new { existing.project_id, existing.user_id }).ToHashSet();

                foreach (var request in requestList)
                {
                    if (!existingPairs.Contains(new { request.project_id, request.user_id }))
                    {
                        newMappings.Add(request);
                    }
                }

                // Apply updates to existing records
                if (updatedMappings.Any())
                {
                    _context.Project_User_Mapping.UpdateRange(updatedMappings);
                }

                // Add new mappings to the database
                if (newMappings.Any())
                {
                    _context.Project_User_Mapping.AddRange(newMappings);
                }

                // Save changes to the database
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }
        public async Task<APIResponseDTO> GetList(CommonRequestDto request, ExtractTokenDto token)
        {
            APIResponseDTO response = new APIResponseDTO();
            try
            {
                int totalCount = 0;
                var list = new List<dynamic>();

                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (var command = new SqlCommand("get_project_list", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 120;
                        command.Parameters.AddWithValue("@company_id", token.CompanyId);
                        command.Parameters.AddWithValue("@page", request.page);
                        command.Parameters.AddWithValue("@page_size", request.page_size);
                        command.Parameters.AddWithValue("@search_query", request.search_query);
                        command.Parameters.AddWithValue("@sort_column", request.sort_column);
                        command.Parameters.AddWithValue("@sort_direction", request.sort_direction);

                        command.Parameters.Add("@total_count", SqlDbType.Int).Direction = ParameterDirection.Output;

                        await connection.OpenAsync();

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var data = new
                                {
                                    id = reader.GetInt32(reader.GetOrdinal("id")),
                                    code = reader.GetString(reader.GetOrdinal("code")),
                                    name = reader.GetString(reader.GetOrdinal("name")),
                                    description = reader.GetString(reader.GetOrdinal("description")),
                                    address = reader.GetString(reader.GetOrdinal("address")),
                                    client = reader.GetString(reader.GetOrdinal("client")),
                                    cost = reader.GetDecimal(reader.GetOrdinal("cost")),
                                    start_date = reader.IsDBNull(reader.GetOrdinal("start_date")) ? null : reader.GetString(reader.GetOrdinal("start_date")),
                                    end_date = reader.IsDBNull(reader.GetOrdinal("end_date")) ? null : reader.GetString(reader.GetOrdinal("end_date")),
                                    work_hours = reader.GetDecimal(reader.GetOrdinal("work_hours")),
                                    admin_id = reader.GetInt32(reader.GetOrdinal("admin_id")),
                                    admin_name = reader.GetString(reader.GetOrdinal("admin_name")),
                                    manager_id = reader.GetInt32(reader.GetOrdinal("manager_id")),
                                    manager_name = reader.GetString(reader.GetOrdinal("manager_name")),
                                    engineer_id = reader.GetInt32(reader.GetOrdinal("engineer_id")),
                                    engineer_name = reader.GetString(reader.GetOrdinal("engineer_name")),
                                    active_flag = reader.GetBoolean(reader.GetOrdinal("active_flag"))
                                };

                                list.Add(data);
                            }
                        }

                        // Get the total count from the output parameter
                        totalCount = (int)command.Parameters["@total_count"].Value;
                    }
                }

                response.success = true;
                response.message = list.Count > 0 ? "Data Fetched Successfully" : "No Records Found";
                response.data = list;
                response.total = totalCount;
                response.page = request.page;
                response.page_size = request.page_size;
            }
            catch (Exception ex)
            {
                response.success = false;
                response.message = ex.Message;
            }

            return response;
        }
        public async Task<APIResponseDTO> Delete(string ids, ExtractTokenDto token)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            try
            {
                var idsToDelete = ids.Split(',').Select(id => int.Parse(id)).ToList();

                var data = await _context.Project.Where(x => x.delete_flag == false && idsToDelete.Contains(x.id) && x.company_id == token.CompanyId).ToListAsync();

                if (data.Any())
                {
                    foreach (var record in data)
                    {
                        record.delete_flag = true;
                        record.updated_by = token.UserId;
                        record.updated_date = DateTime.Now;
                    }

                    _context.Project.UpdateRange(data);
                    await _context.SaveChangesAsync();

                    aPIResponseDTO.success = true;
                    aPIResponseDTO.message = "Project deleted successfully.";
                }
                else
                {
                    aPIResponseDTO.message = "No matching data found to delete.";
                }
            }
            catch (Exception ex)
            {
                aPIResponseDTO.message = "Failed deleting details";
                return aPIResponseDTO;
            }

            return aPIResponseDTO;
        }
        public async Task<APIResponseDTO> ActiveInactive(int id, ExtractTokenDto token)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            try
            {
                var data = await _context.Project.Where(x => x.id == id).FirstOrDefaultAsync();

                if (data != null)
                {
                    data.active_flag = data.active_flag == false ? true : false;
                    data.updated_by = token.UserId;
                    data.updated_date = DateTime.Now;
                    _context.Project.Update(data);
                    await _context.SaveChangesAsync();

                    aPIResponseDTO.success = true;
                    aPIResponseDTO.message = "Project " + (data.active_flag == true ? "Activated" : "Inactivated") + " successfully.";
                }
                else
                {
                    aPIResponseDTO.message = "No matching data found.";
                }
            }
            catch (Exception ex)
            {
                aPIResponseDTO.message = "Failed saving details";
                return aPIResponseDTO;
            }

            return aPIResponseDTO;
        }
        public async Task<APIResponseDTO> GetProjectUserMappingById(int id)
        {
            APIResponseDTO response = new APIResponseDTO();
            try
            {
                var list = new List<dynamic>();

                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (var command = new SqlCommand("get_project_user_mapping_by_id", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 120;
                        command.Parameters.AddWithValue("@project_id", id);

                        await connection.OpenAsync();

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var data = new
                                {
                                    id = reader.GetInt32(reader.GetOrdinal("id")),
                                    user_id = reader.GetInt32(reader.GetOrdinal("user_id")),
                                    name = reader.GetString(reader.GetOrdinal("name"))
                                };

                                list.Add(data);
                            }
                        }
                    }
                }

                response.success = true;
                response.message = list.Count > 0 ? "Data Fetched Successfully" : "No Records Found";
                response.data = list;
            }
            catch (Exception ex)
            {
                response.success = false;
                response.message = ex.Message;
            }

            return response;
        }
        public async Task<APIResponseDTO> GetUsersDropdownList(ExtractTokenDto token)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            try
            {
                var pageList = await (from a in _context.Users
                                      join b in _context.Designation on a.designation_id equals b.id
                                      where a.company_id == token.CompanyId && a.active_flag == true && a.delete_flag == false
                                      select new
                                      {
                                          a.id,
                                          name = a.name + " (" + b.name + ")"
                                      }).ToListAsync();

                if (pageList.Count > 0)
                {
                    aPIResponseDTO.success = true;
                    aPIResponseDTO.message = "Page data fetched successfully";
                    aPIResponseDTO.data = pageList;
                }
                else
                {
                    aPIResponseDTO.success = true;
                    aPIResponseDTO.message = "No records found";
                }
            }
            catch (Exception ex)
            {
                aPIResponseDTO.success = false;
                aPIResponseDTO.message = ex.Message;
            }

            return aPIResponseDTO;
        }
    }
}