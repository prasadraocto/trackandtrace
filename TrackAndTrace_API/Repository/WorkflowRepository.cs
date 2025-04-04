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
    public class WorkflowRepository: IWorkflowRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        public WorkflowRepository(ApplicationDbContext context, IMapper mapper, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<APIResponseDTO> Add(WorkflowDto model, ExtractTokenDto token)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            try
            {
                if (model == null || model.users == null || !model.users.Any())
                {
                    aPIResponseDTO.message = "Workflow Mapping required";
                    return aPIResponseDTO;
                }

                var existingMappings = await _context.Work_Flow_Project_User_Mapping.Where(x => x.work_flow_id == model.work_flow_id && x.project_id == model.project_id).ToListAsync();

                var dbEntitiesToAdd = new List<Work_Flow_Project_User_Mapping>();

                foreach (var mapping in model.users)
                {
                    var existingMapping = existingMappings.FirstOrDefault(x => x.user_id == mapping.user_id);

                    if (existingMapping != null)
                    {
                        if (existingMapping.order_id != mapping.order_id || existingMapping.is_supersede != mapping.is_supersede)
                        {
                            existingMapping.order_id = mapping.order_id;
                            existingMapping.is_supersede = mapping.is_supersede;
                            existingMapping.updated_by = token.UserId;
                            existingMapping.updated_date = DateTime.Now;
                        }
                    }
                    else
                    {
                        dbEntitiesToAdd.Add(new Work_Flow_Project_User_Mapping
                        {
                            work_flow_id = model.work_flow_id,
                            project_id = model.project_id,
                            user_id = mapping.user_id,
                            order_id = mapping.order_id,
                            is_supersede = mapping.is_supersede,
                            delete_flag = false, // Default to false
                            created_by = token.UserId,
                            created_date = DateTime.Now,
                            updated_by = null,
                            updated_date = null
                        });
                    }
                }

                if (dbEntitiesToAdd.Any())
                {
                    await _context.Work_Flow_Project_User_Mapping.AddRangeAsync(dbEntitiesToAdd);
                }

                await _context.SaveChangesAsync();

                aPIResponseDTO.success = true;
                aPIResponseDTO.message = "Workflow saved successfully";
                return aPIResponseDTO;
            }
            catch (Exception ex)
            {
                aPIResponseDTO.message = $"Failed saving details: {ex.Message}";
                return aPIResponseDTO;
            }
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
                    using (var command = new SqlCommand("get_work_flow_project_mapping_list", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 120;
                        command.Parameters.AddWithValue("@company_id", token.CompanyId);
                        command.Parameters.AddWithValue("@page", request.page);
                        command.Parameters.AddWithValue("@page_size", request.page_size);
                        command.Parameters.AddWithValue("@search_query", request.search_query);
                        command.Parameters.AddWithValue("@sort_column", request.sort_column == "id" ? "work_flow_id" : request.sort_column);
                        command.Parameters.AddWithValue("@sort_direction", request.sort_direction);

                        command.Parameters.Add("@total_count", SqlDbType.Int).Direction = ParameterDirection.Output;

                        await connection.OpenAsync();

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var data = new
                                {
                                    id = list.Count + 1,
                                    work_flow_id = reader.GetInt32(reader.GetOrdinal("work_flow_id")),
                                    work_flow_code = reader.GetString(reader.GetOrdinal("work_flow_code")),
                                    work_flow_name = reader.GetString(reader.GetOrdinal("work_flow_name")),
                                    project_id = reader.GetInt32(reader.GetOrdinal("project_id")),
                                    project_name = reader.GetString(reader.GetOrdinal("project_name"))
                                };

                                list.Add(data);
                            }
                        }

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
        public async Task<APIResponseDTO> Delete(int work_flow_id, int project_id, ExtractTokenDto token)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            try
            {
                var data = await _context.Work_Flow_Project_User_Mapping.Where(x => x.delete_flag == false && x.work_flow_id == work_flow_id && x.project_id == project_id).ToListAsync();

                if (data.Any())
                {
                    foreach (var record in data)
                    {
                        record.delete_flag = true;
                        record.updated_by = token.UserId;
                        record.updated_date = DateTime.Now;
                    }

                    _context.Work_Flow_Project_User_Mapping.UpdateRange(data);
                    await _context.SaveChangesAsync();

                    aPIResponseDTO.success = true;
                    aPIResponseDTO.message = "Workflow deleted successfully.";
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
        public async Task<APIResponseDTO> GetWFProjectUserMappingById(int work_flow_id, int project_id)
        {
            APIResponseDTO response = new APIResponseDTO();
            try
            {
                var list = new List<dynamic>();

                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (var command = new SqlCommand("get_work_flow_project_user_mapping_by_id", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 120;
                        command.Parameters.AddWithValue("@work_flow_id", work_flow_id);
                        command.Parameters.AddWithValue("@project_id", project_id);

                        await connection.OpenAsync();

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var data = new
                                {
                                    id = reader.GetInt32(reader.GetOrdinal("id")),
                                    name = reader.GetString(reader.GetOrdinal("name")),
                                    order_id = reader.GetInt32(reader.GetOrdinal("order_id")),
                                    is_supersede = reader.GetBoolean(reader.GetOrdinal("is_supersede"))
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
        public async Task<APIResponseDTO> GetWFPendingRequest(CommonRequestDto request, ExtractTokenDto token)
        {
            APIResponseDTO response = new APIResponseDTO();
            try
            {
                int totalCount = 0;
                var list = new List<dynamic>();

                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (var command = new SqlCommand("get_work_flow_pending_request", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 120;
                        command.Parameters.AddWithValue("@user_id", token.UserId);
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
                                    request_type = reader.GetString(reader.GetOrdinal("request_type")),
                                    project_id = reader.GetInt32(reader.GetOrdinal("project_id")),
                                    project_name = reader.GetString(reader.GetOrdinal("project_name")),
                                    request_id = reader.GetInt32(reader.GetOrdinal("request_id")),
                                    import_name = reader.GetString(reader.GetOrdinal("import_name")),
                                    current_user_id = reader.GetInt32(reader.GetOrdinal("current_user_id")),
                                    current_user_name = reader.GetString(reader.GetOrdinal("current_user_name")),
                                    next_user_id = reader.IsDBNull(reader.GetOrdinal("next_user_id")) ? 0 : reader.GetInt32(reader.GetOrdinal("next_user_id")),
                                    next_user_name = reader.IsDBNull(reader.GetOrdinal("next_user_id")) ? null : reader.GetString(reader.GetOrdinal("next_user_name")),
                                    order_id = reader.GetInt32(reader.GetOrdinal("order_id"))
                                };

                                list.Add(data);
                            }
                        }

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
    }
}