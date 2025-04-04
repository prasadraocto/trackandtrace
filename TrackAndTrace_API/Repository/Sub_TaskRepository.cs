using TrackAndTrace_API.Interface;
using TrackAndTrace_API.Models;
using TrackAndTrace_API.Models.RequestModel;
using TrackAndTrace_API.Models.ResponseModel;
using Microsoft.EntityFrameworkCore;
using TrackAndTrace_API.Models.DBModel;
using AutoMapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace TrackAndTrace_API.Repository
{
    public class Sub_TaskRepository: ISub_TaskRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        public Sub_TaskRepository(ApplicationDbContext context, IMapper mapper, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<APIResponseDTO> Add(Sub_TaskDto model, ExtractTokenDto token)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            try
            {
                var nameExists = await _context.Sub_Task.AnyAsync(x => x.name.ToLower() == model.name.ToLower() && x.id != model.id && x.activity_id == model.activity_id && x.task_id == model.task_id && x.company_id == token.CompanyId);
                if (nameExists)
                {
                    aPIResponseDTO.message = "Sub_Task Name already exists";
                    return aPIResponseDTO;
                }

                var codeExists = await _context.Sub_Task.AnyAsync(x => x.code.ToLower() == model.code.ToLower() && x.id != model.id && x.activity_id == model.activity_id && x.task_id == model.task_id && x.company_id == token.CompanyId);
                if (codeExists)
                {
                    aPIResponseDTO.message = "Sub_Task Code already exists";
                    return aPIResponseDTO;
                }

                var dmo = _mapper.Map<Models.DBModel.Sub_Task>(model);

                if (dmo.id > 0)
                {
                    var existingSub_Task = await _context.Sub_Task.AsNoTracking().FirstOrDefaultAsync(x => x.id == dmo.id);

                    if (existingSub_Task == null)
                    {
                        aPIResponseDTO.message = "Sub_Task details not found";
                        return aPIResponseDTO;
                    }

                    dmo.company_id = token.CompanyId;
                    dmo.created_by = existingSub_Task.created_by;
                    dmo.created_date = existingSub_Task.created_date;
                    dmo.updated_by = token.UserId;
                    dmo.updated_date = DateTime.Now;
                    _context.Sub_Task.Update(dmo);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    dmo.company_id = token.CompanyId;
                    dmo.created_by = token.UserId;
                    dmo.created_date = DateTime.Now;
                    dmo.updated_date = null;
                    await _context.Sub_Task.AddAsync(dmo);
                    await _context.SaveChangesAsync();
                }

                aPIResponseDTO.success = true;
                aPIResponseDTO.message = "Sub_Task saved successfully";
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
                    using (var command = new SqlCommand("get_sub_task_list", connection))
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
                                    activity_id = reader.GetInt32(reader.GetOrdinal("activity_id")),
                                    activity_name = reader.GetString(reader.GetOrdinal("activity_name")),
                                    task_id = reader.GetInt32(reader.GetOrdinal("task_id")),
                                    task_name = reader.GetString(reader.GetOrdinal("task_name")),
                                    uom_id = reader.GetInt32(reader.GetOrdinal("uom_id")),
                                    uom_name = reader.GetString(reader.GetOrdinal("uom_name")),
                                    is_prime = reader.GetBoolean(reader.GetOrdinal("is_prime")),
                                    estimated_days = reader.GetInt32(reader.GetOrdinal("estimated_days")),
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

                var data = await _context.Sub_Task.Where(x => x.delete_flag == false && idsToDelete.Contains(x.id) && x.company_id == token.CompanyId).ToListAsync();

                if (data.Any())
                {
                    foreach (var record in data)
                    {
                        record.delete_flag = true;
                        record.updated_by = token.UserId;
                        record.updated_date = DateTime.Now;
                    }

                    _context.Sub_Task.UpdateRange(data);
                    await _context.SaveChangesAsync();

                    aPIResponseDTO.success = true;
                    aPIResponseDTO.message = "Sub_Task deleted successfully.";
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
                var data = await _context.Sub_Task.Where(x => x.id == id).FirstOrDefaultAsync();

                if (data != null)
                {
                    data.active_flag = data.active_flag == false ? true : false;
                    data.updated_by = token.UserId;
                    data.updated_date = DateTime.Now;
                    _context.Sub_Task.Update(data);
                    await _context.SaveChangesAsync();

                    aPIResponseDTO.success = true;
                    aPIResponseDTO.message = "Sub_Task " + (data.active_flag == true ? "Activated" : "Inactivated") + " successfully.";
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
        public async Task<APIResponseDTO> GetProjectDropdownList(int activity_id, int task_id, ExtractTokenDto token)
        {
            APIResponseDTO response = new APIResponseDTO();
            try
            {
                var list = new List<dynamic>();

                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (var command = new SqlCommand("get_project_drop_down_list", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 120;
                        command.Parameters.AddWithValue("@company_id", token.CompanyId);
                        command.Parameters.AddWithValue("@activity_id", activity_id);
                        command.Parameters.AddWithValue("@task_id", task_id);

                        await connection.OpenAsync();

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var data = new
                                {
                                    id = reader.GetInt32(reader.GetOrdinal("id")),
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
        public async Task<APIResponseDTO> GetActivityTaskSubTaskDropDownList(int activity_id, int task_id, int project_id, ExtractTokenDto token)
        {
            APIResponseDTO response = new APIResponseDTO();
            try
            {
                var list = new List<dynamic>();

                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (var command = new SqlCommand("get_activity_task_sub_task_drop_down_list", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 120;
                        command.Parameters.AddWithValue("@company_id", token.CompanyId);
                        command.Parameters.AddWithValue("@activity_id", activity_id);
                        command.Parameters.AddWithValue("@task_id", task_id);
                        command.Parameters.AddWithValue("@project_id", project_id);

                        await connection.OpenAsync();

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var data = new
                                {
                                    drop_down_name = reader.GetString(reader.GetOrdinal("drop_down_name")),
                                    id = reader.GetInt32(reader.GetOrdinal("id")),
                                    name = reader.GetString(reader.GetOrdinal("name")),
                                    uom_id = reader.IsDBNull(reader.GetOrdinal("uom_id")) ? 0 : reader.GetInt32(reader.GetOrdinal("uom_id")),
                                    uom_name = reader.IsDBNull(reader.GetOrdinal("uom_name")) ? "" : reader.GetString(reader.GetOrdinal("uom_name")),
                                    start_date = reader.IsDBNull(reader.GetOrdinal("start_date")) ? "" : reader.GetString(reader.GetOrdinal("start_date")),
                                    end_date = reader.IsDBNull(reader.GetOrdinal("end_date")) ? "" : reader.GetString(reader.GetOrdinal("end_date")),
                                    cost = reader.IsDBNull(reader.GetOrdinal("cost")) ? 0 : reader.GetDecimal(reader.GetOrdinal("cost")),
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
        public async Task<APIResponseDTO> SaveSub_TaskProjectMapping(Sub_Task_Project_MappingDto dto, ExtractTokenDto token)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            var dmo = _mapper.Map<Sub_Task_Project_Mapping>(dto);
            try
            {
                if (dmo.id > 0)
                {
                    var existingSub_Task_Project_Mapping = await _context.Sub_Task_Project_Mapping.AsNoTracking().FirstOrDefaultAsync(x => x.id == dmo.id);
                    if (existingSub_Task_Project_Mapping == null)
                    {
                        aPIResponseDTO.message = "Mapping details not found";
                        return aPIResponseDTO;
                    }
                    dmo.created_by = existingSub_Task_Project_Mapping.created_by;
                    dmo.created_date = existingSub_Task_Project_Mapping.created_date;
                    dmo.updated_by = token.UserId;
                    dmo.updated_date = DateTime.Now;
                    _context.Sub_Task_Project_Mapping.Update(dmo);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    dmo.created_by = token.UserId;
                    dmo.created_date = DateTime.Now;
                    dmo.updated_date = null;
                    await _context.Sub_Task_Project_Mapping.AddAsync(dmo);
                    await _context.SaveChangesAsync();
                }

                aPIResponseDTO.success = true;
                aPIResponseDTO.message = "Mapping saved successfully";
                return aPIResponseDTO;
            }
            catch (Exception ex)
            {
                aPIResponseDTO.message = $"Failed saving details: {ex.Message}";
                return aPIResponseDTO;
            }
        }
        public async Task<APIResponseDTO> GetSubTaskProjectmappingList(int id, CommonRequestDto request, ExtractTokenDto token)
        {
            APIResponseDTO response = new APIResponseDTO();
            try
            {
                int totalCount = 0;
                var list = new List<dynamic>();

                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (var command = new SqlCommand("get_sub_task_project_mapping_list", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 120;
                        command.Parameters.AddWithValue("@project_id", id);
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
                                    sub_task_id = reader.GetInt32(reader.GetOrdinal("sub_task_id")),
                                    code = reader.GetString(reader.GetOrdinal("code")),
                                    name = reader.GetString(reader.GetOrdinal("name")),
                                    cost = reader.GetDecimal(reader.GetOrdinal("cost")),
                                    start_date = reader.GetString(reader.GetOrdinal("start_date")),
                                    end_date = reader.GetString(reader.GetOrdinal("end_date")),
                                    activity_id = reader.GetInt32(reader.GetOrdinal("activity_id")),
                                    activity_name = reader.GetString(reader.GetOrdinal("activity_name")),
                                    task_id = reader.GetInt32(reader.GetOrdinal("task_id")),
                                    task_name = reader.GetString(reader.GetOrdinal("task_name")),
                                    uom_id = reader.GetInt32(reader.GetOrdinal("uom_id")),
                                    uom_name = reader.GetString(reader.GetOrdinal("uom_name")),
                                    is_prime = reader.GetBoolean(reader.GetOrdinal("is_prime"))
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
    }
}