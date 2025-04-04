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
    public class ManpowerRepository : IManpowerRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        public ManpowerRepository(ApplicationDbContext context, IMapper mapper, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<APIResponseDTO> Add(ManpowerDto model, ExtractTokenDto token)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            try
            {
                var nameExists = await _context.Manpower.AnyAsync(x => x.name.ToLower() == model.name.ToLower() && x.id != model.id && x.company_id == token.CompanyId);
                if (nameExists)
                {
                    aPIResponseDTO.message = "Manpower Name already exists";
                    return aPIResponseDTO;
                }

                var codeExists = await _context.Manpower.AnyAsync(x => x.code.ToLower() == model.code.ToLower() && x.id != model.id && x.company_id == token.CompanyId);
                if (codeExists)
                {
                    aPIResponseDTO.message = "Manpower Code already exists";
                    return aPIResponseDTO;
                }

                if (model.manpower_project_mapping.Count() == 0)
                {
                    aPIResponseDTO.message = "Manpower Mapping required";
                    return aPIResponseDTO;
                }

                var dmo = _mapper.Map<Manpower>(model);

                if (dmo.id > 0)
                {
                    var existingManpower = await _context.Manpower.AsNoTracking().FirstOrDefaultAsync(x => x.id == dmo.id);

                    if (existingManpower == null)
                    {
                        aPIResponseDTO.message = "Manpower details not found";
                        return aPIResponseDTO;
                    }

                    dmo.company_id = token.CompanyId;
                    dmo.created_by = existingManpower.created_by;
                    dmo.created_date = existingManpower.created_date;
                    dmo.updated_by = token.UserId;
                    dmo.updated_date = DateTime.Now;
                    _context.Manpower.Update(dmo);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    dmo.company_id = token.CompanyId;
                    dmo.created_by = token.UserId;
                    dmo.created_date = DateTime.Now;
                    dmo.updated_date = null;
                    await _context.Manpower.AddAsync(dmo);
                    await _context.SaveChangesAsync();
                }

                foreach (var mapping in model.manpower_project_mapping)
                {
                    mapping.manpower_id = dmo.id;
                }

                var response = await SaveManpowerProjectMapping(model.manpower_project_mapping);

                if (!response)
                {
                    aPIResponseDTO.success = false;
                    aPIResponseDTO.message = "Failed to save Manpower mapping";
                    return aPIResponseDTO;
                }

                aPIResponseDTO.success = true;
                aPIResponseDTO.message = "Manpower saved successfully";
                return aPIResponseDTO;
            }
            catch (Exception ex)
            {
                aPIResponseDTO.message = $"Failed saving details: {ex.Message}";
                return aPIResponseDTO;
            }
        }
        public async Task<bool> SaveManpowerProjectMapping(List<Manpower_Project_Mapping> requestList)
        {
            try
            {
                // Extract manpower_id and project_id pairs from the requestList
                var requestPairs = requestList.Select(request => new { request.manpower_id, request.project_id }).ToHashSet();

                // Fetch only the relevant existing mappings from the database
                var manpowerIds = requestList.Select(r => r.manpower_id).ToHashSet();
                var existingMappings = await _context.Manpower_Project_Mapping.Where(mapping => manpowerIds.Contains(mapping.manpower_id)).ToListAsync();

                // Prepare lists for new mappings and updates
                var newMappings = new List<Manpower_Project_Mapping>();
                var updatedMappings = new List<Manpower_Project_Mapping>();

                foreach (var existing in existingMappings)
                {
                    // Check if the existing mapping is in the requestList
                    var isInRequest = requestPairs.Contains(new { existing.manpower_id, existing.project_id });

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
                var existingPairs = existingMappings.Select(existing => new { existing.manpower_id, existing.project_id }).ToHashSet();

                foreach (var request in requestList)
                {
                    if (!existingPairs.Contains(new { request.manpower_id, request.project_id }))
                    {
                        newMappings.Add(request);
                    }
                }

                // Apply updates to existing records
                if (updatedMappings.Any())
                {
                    _context.Manpower_Project_Mapping.UpdateRange(updatedMappings);
                }

                // Add new mappings to the database
                if (newMappings.Any())
                {
                    _context.Manpower_Project_Mapping.AddRange(newMappings);
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
                    using (var command = new SqlCommand("get_manpower_list", connection))
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
                                    designation_id = reader.GetInt32(reader.GetOrdinal("designation_id")),
                                    designation_name = reader.GetString(reader.GetOrdinal("designation_name")),
                                    engineer_id = reader.GetInt32(reader.GetOrdinal("engineer_id")),
                                    engineer_name = reader.GetString(reader.GetOrdinal("engineer_name")),
                                    charge_hand_id = reader.IsDBNull(reader.GetOrdinal("charge_hand_id")) ? 0 : reader.GetInt32(reader.GetOrdinal("charge_hand_id")),
                                    charge_hand_name = reader.IsDBNull(reader.GetOrdinal("charge_hand_name")) ? null : reader.GetString(reader.GetOrdinal("charge_hand_name")),
                                    gang_leader_id = reader.IsDBNull(reader.GetOrdinal("gang_leader_id")) ? 0 : reader.GetInt32(reader.GetOrdinal("gang_leader_id")),
                                    gang_leader_name = reader.IsDBNull(reader.GetOrdinal("gang_leader_name")) ? null : reader.GetString(reader.GetOrdinal("gang_leader_name")),
                                    subcontractor_id = reader.IsDBNull(reader.GetOrdinal("subcontractor_id")) ? 0 : reader.GetInt32(reader.GetOrdinal("subcontractor_id")),
                                    subcontractor_name = reader.IsDBNull(reader.GetOrdinal("subcontractor_name")) ? null : reader.GetString(reader.GetOrdinal("subcontractor_name")),
                                    rating = reader.IsDBNull(reader.GetOrdinal("rating")) ? 0 : reader.GetDecimal(reader.GetOrdinal("rating")),
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

                var data = await _context.Manpower.Where(x => x.delete_flag == false && idsToDelete.Contains(x.id) && x.company_id == token.CompanyId).ToListAsync();

                if (data.Any())
                {
                    foreach (var record in data)
                    {
                        record.delete_flag = true;
                        record.updated_by = token.UserId;
                        record.updated_date = DateTime.Now;
                    }

                    _context.Manpower.UpdateRange(data);
                    await _context.SaveChangesAsync();

                    aPIResponseDTO.success = true;
                    aPIResponseDTO.message = "Manpower deleted successfully.";
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
                var data = await _context.Manpower.Where(x => x.id == id).FirstOrDefaultAsync();

                if (data != null)
                {
                    data.active_flag = data.active_flag == false ? true : false;
                    data.updated_by = token.UserId;
                    data.updated_date = DateTime.Now;
                    _context.Manpower.Update(data);
                    await _context.SaveChangesAsync();

                    aPIResponseDTO.success = true;
                    aPIResponseDTO.message = "Manpower " + (data.active_flag == true ? "Activated" : "Inactivated") + " successfully.";
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
        public async Task<APIResponseDTO> GetManpowerProjectMappingById(int id)
        {
            APIResponseDTO response = new APIResponseDTO();
            try
            {
                var list = new List<dynamic>();

                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (var command = new SqlCommand("get_manpower_project_mapping_by_id", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 120;
                        command.Parameters.AddWithValue("@manpower_id", id);

                        await connection.OpenAsync();

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var data = new
                                {
                                    id = reader.GetInt32(reader.GetOrdinal("id")),
                                    project_id = reader.GetInt32(reader.GetOrdinal("project_id")),
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
    }
}