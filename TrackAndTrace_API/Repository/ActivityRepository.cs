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
    public class ActivityRepository : IActivityRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        public ActivityRepository(ApplicationDbContext context, IMapper mapper, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<APIResponseDTO> Add(ActivityDto model, ExtractTokenDto token)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            try
            {
                var nameExists = await _context.Activity.AnyAsync(x => x.name.ToLower() == model.name.ToLower() && x.id != model.id && x.company_id == token.CompanyId);
                if (nameExists)
                {
                    aPIResponseDTO.message = "Activity Name already exists";
                    return aPIResponseDTO;
                }

                var codeExists = await _context.Activity.AnyAsync(x => x.code.ToLower() == model.code.ToLower() && x.id != model.id && x.company_id == token.CompanyId);
                if (codeExists)
                {
                    aPIResponseDTO.message = "Activity Code already exists";
                    return aPIResponseDTO;
                }

                var dmo = _mapper.Map<TrackAndTrace_API.Models.DBModel.Activity>(model);

                if (dmo.id > 0)
                {
                    var existingActivity = await _context.Activity.AsNoTracking().FirstOrDefaultAsync(x => x.id == dmo.id);

                    if (existingActivity == null)
                    {
                        aPIResponseDTO.message = "Activity details not found";
                        return aPIResponseDTO;
                    }

                    dmo.company_id = token.CompanyId;
                    dmo.created_by = existingActivity.created_by;
                    dmo.created_date = existingActivity.created_date;
                    dmo.updated_by = token.UserId;
                    dmo.updated_date = DateTime.Now;
                    _context.Activity.Update(dmo);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    dmo.company_id = token.CompanyId;
                    dmo.created_by = token.UserId;
                    dmo.created_date = DateTime.Now;
                    dmo.updated_date = null;
                    await _context.Activity.AddAsync(dmo);
                    await _context.SaveChangesAsync();

                    model.id = dmo.id;
                }

                bool mappingResponse = await SaveActivityProjectMapping(model);

                aPIResponseDTO.success = true;
                aPIResponseDTO.message = "Activity saved successfully";
                return aPIResponseDTO;
            }
            catch (Exception ex)
            {
                aPIResponseDTO.message = $"Failed saving details: {ex.Message}";
                return aPIResponseDTO;
            }
        }
        private async Task<bool> SaveActivityProjectMapping(ActivityDto activityDto)
        {
            // Retrieve existing mappings for the given activity
            var existingMappings = await _context.Activity_Project_Mapping.Where(mapping => mapping.activity_id == activityDto.id).ToListAsync();

            // Get the project IDs from the incoming DTO
            var incomingProjectIds = activityDto.project_mapping.Select(pm => pm.project_id).ToList();

            // Remove mappings not present in the incoming model
            var mappingsToRemove = existingMappings.Where(existingMapping => !incomingProjectIds.Contains(existingMapping.project_id)).ToList();
            _context.RemoveRange(mappingsToRemove);

            // Check if activity is used in Task and Subtask and remove mappings
            var taskMappingsToRemove = await _context.Task_Project_Mapping
                .Where(taskMapping => _context.Task
                    .Any(task => task.id == taskMapping.task_id && task.activity_id == activityDto.id)
                    && !incomingProjectIds.Contains(taskMapping.project_id))
                .ToListAsync();

            _context.RemoveRange(taskMappingsToRemove);

            var subTaskMappingsToRemove = await _context.Sub_Task_Project_Mapping
                .Where(subTaskMapping => _context.Sub_Task
                    .Any(subTask => subTask.id == subTaskMapping.sub_task_id && subTask.activity_id == activityDto.id)
                    && !incomingProjectIds.Contains(subTaskMapping.project_id))
                .ToListAsync();

            _context.RemoveRange(subTaskMappingsToRemove);

            // Add or update mappings from the incoming model
            var newMappings = activityDto.project_mapping
                .Where(incomingMapping => !existingMappings.Any(em => em.project_id == incomingMapping.project_id))
                .Select(incomingMapping => new Activity_Project_Mapping
                {
                    activity_id = activityDto.id,
                    project_id = incomingMapping.project_id
                })
                .ToList();

            await _context.AddRangeAsync(newMappings);

            // Save changes to the database
            var changesSaved = await _context.SaveChangesAsync();

            return changesSaved > 0;
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
                    using (var command = new SqlCommand("get_activity_list", connection))
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

                var data = await _context.Activity.Where(x => x.delete_flag == false && idsToDelete.Contains(x.id) && x.company_id == token.CompanyId).ToListAsync();

                if (data.Any())
                {
                    foreach (var record in data)
                    {
                        record.delete_flag = true;
                        record.updated_by = token.UserId;
                        record.updated_date = DateTime.Now;
                    }

                    _context.Activity.UpdateRange(data);
                    await _context.SaveChangesAsync();

                    aPIResponseDTO.success = true;
                    aPIResponseDTO.message = "Activity deleted successfully.";
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
                var data = await _context.Activity.Where(x => x.id == id).FirstOrDefaultAsync();

                if (data != null)
                {
                    data.active_flag = data.active_flag == false ? true : false;
                    data.updated_by = token.UserId;
                    data.updated_date = DateTime.Now;
                    _context.Activity.Update(data);
                    await _context.SaveChangesAsync();

                    aPIResponseDTO.success = true;
                    aPIResponseDTO.message = "Activity " + (data.active_flag == true ? "Activated" : "Inactivated") + " successfully.";
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
        public async Task<APIResponseDTO> GetProjectMappingById(int id)
        {
            APIResponseDTO response = new APIResponseDTO();
            try
            {
                var list = await (from a in _context.Activity_Project_Mapping
                                  join b in _context.Project on a.project_id equals b.id
                                  where a.activity_id == id && b.active_flag == true && b.delete_flag == false
                                  select new
                                  {
                                      b.id,
                                      b.name
                                  }).ToListAsync();

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