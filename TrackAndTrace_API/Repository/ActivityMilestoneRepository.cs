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
    public class ActivityMilestoneRepository : IActivityMilestoneRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        public ActivityMilestoneRepository(ApplicationDbContext context, IMapper mapper, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<APIResponseDTO> Add(ActivityMilestoneDto model, ExtractTokenDto token)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            try
            {
                var nameExists = await _context.Activity_Milestone.AnyAsync(x => x.name.ToLower() == model.name.ToLower() && x.id != model.id && x.project_id == model.project_id && x.company_id == token.CompanyId);
                if (nameExists)
                {
                    aPIResponseDTO.message = "Activity Milestone Name already exists";
                    return aPIResponseDTO;
                }

                var codeExists = await _context.Activity_Milestone.AnyAsync(x => x.code.ToLower() == model.code.ToLower() && x.id != model.id && x.project_id == model.project_id && x.company_id == token.CompanyId);
                if (codeExists)
                {
                    aPIResponseDTO.message = "Activity Milestone Code already exists";
                    return aPIResponseDTO;
                }

                var dmo = _mapper.Map<Activity_Milestone>(model);

                if (dmo.id > 0)
                {
                    var existingActivityMilestone = await _context.Activity_Milestone.AsNoTracking().FirstOrDefaultAsync(x => x.id == dmo.id);

                    if (existingActivityMilestone == null)
                    {
                        aPIResponseDTO.message = "ActivityMilestone details not found";
                        return aPIResponseDTO;
                    }

                    dmo.company_id = token.CompanyId;
                    dmo.created_by = existingActivityMilestone.created_by;
                    dmo.created_date = existingActivityMilestone.created_date;
                    dmo.updated_by = token.UserId;
                    dmo.updated_date = DateTime.Now;
                    _context.Activity_Milestone.Update(dmo);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    dmo.company_id = token.CompanyId;
                    dmo.created_by = token.UserId;
                    dmo.created_date = DateTime.Now;
                    dmo.updated_date = null;
                    await _context.Activity_Milestone.AddAsync(dmo);
                    await _context.SaveChangesAsync();

                    model.id = dmo.id;
                }

                foreach (var item in model.activity_milestone_mapping)
                {
                    item.activity_milestone_id = model.id;
                }

                bool mapptingStatus = await SaveActivityMilestoneMapping(model);

                aPIResponseDTO.success = true;
                aPIResponseDTO.message = "Activity Milestone saved successfully";
                return aPIResponseDTO;
            }
            catch (Exception ex)
            {
                aPIResponseDTO.message = $"Failed saving details: {ex.Message}";
                return aPIResponseDTO;
            }
        }
        private async Task<bool> SaveActivityMilestoneMapping(ActivityMilestoneDto dto)
        {
            if (dto == null || dto.activity_milestone_mapping == null)
                throw new ArgumentNullException(nameof(dto));

            // Retrieve existing mappings from the database for the given ActivityMilestone
            var existingMappings = await _context.Activity_Milestone_Mapping.Where(m => m.activity_milestone_id == dto.id).ToListAsync();

            // Use both activity_milestone_id and activity_id for comparison
            var incomingMappings = dto.activity_milestone_mapping;

            // Identify mappings to update: Match both activity_milestone_id and activity_id
            var mappingsToUpdate = incomingMappings
                .Where(incoming => existingMappings.Any(existing =>
                    existing.activity_milestone_id == incoming.activity_milestone_id &&
                    existing.activity_id == incoming.activity_id))
                .ToList();

            // Identify mappings to add: New combinations of activity_milestone_id and activity_id
            var mappingsToAdd = incomingMappings
                .Where(incoming => !existingMappings.Any(existing =>
                    existing.activity_milestone_id == incoming.activity_milestone_id &&
                    existing.activity_id == incoming.activity_id))
                .ToList();

            // Identify mappings to remove: Existing mappings not in the incoming list
            var mappingsToRemove = existingMappings
                .Where(existing => !incomingMappings.Any(incoming =>
                    existing.activity_milestone_id == incoming.activity_milestone_id &&
                    existing.activity_id == incoming.activity_id))
                .ToList();

            // Update existing mappings
            foreach (var mappingToUpdate in mappingsToUpdate)
            {
                var existingMapping = existingMappings.FirstOrDefault(m =>
                    m.activity_milestone_id == mappingToUpdate.activity_milestone_id &&
                    m.activity_id == mappingToUpdate.activity_id);

                if (existingMapping != null)
                {
                    existingMapping.start_date = mappingToUpdate.start_date;
                    existingMapping.end_date = mappingToUpdate.end_date;
                    existingMapping.cost = mappingToUpdate.cost;
                }
            }

            // Remove mappings that are not in the incoming data
            _context.Activity_Milestone_Mapping.RemoveRange(mappingsToRemove);

            // Add new mappings
            var newMappings = mappingsToAdd.Select(m => new Activity_Milestone_Mapping
            {
                activity_milestone_id = dto.id,
                activity_id = m.activity_id,
                start_date = m.start_date,
                end_date = m.end_date,
                cost = m.cost
            });
            await _context.Activity_Milestone_Mapping.AddRangeAsync(newMappings);

            // Save changes to the database
            var changesSaved = await _context.SaveChangesAsync();

            return changesSaved > 0;
        }
        public async Task<APIResponseDTO> GetList(int project_id, CommonRequestDto request, ExtractTokenDto token)
        {
            APIResponseDTO response = new APIResponseDTO();
            try
            {
                int totalCount = 0;
                var list = new List<dynamic>();

                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (var command = new SqlCommand("get_activity_milestone_list", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 120;
                        command.Parameters.AddWithValue("@project_id", project_id);
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
                                    start_date = reader.GetString(reader.GetOrdinal("start_date")),
                                    end_date = reader.GetString(reader.GetOrdinal("end_date")),
                                    budget_cost = reader.GetDecimal(reader.GetOrdinal("budget_cost")),
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

                var data = await _context.Activity_Milestone.Where(x => x.delete_flag == false && idsToDelete.Contains(x.id) && x.company_id == token.CompanyId).ToListAsync();

                if (data.Any())
                {
                    foreach (var record in data)
                    {
                        record.delete_flag = true;
                        record.updated_by = token.UserId;
                        record.updated_date = DateTime.Now;
                    }

                    _context.Activity_Milestone.UpdateRange(data);
                    await _context.SaveChangesAsync();

                    aPIResponseDTO.success = true;
                    aPIResponseDTO.message = "Activity Milestone deleted successfully.";
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
                var data = await _context.Activity_Milestone.Where(x => x.id == id).FirstOrDefaultAsync();

                if (data != null)
                {
                    data.active_flag = data.active_flag == false ? true : false;
                    data.updated_by = token.UserId;
                    data.updated_date = DateTime.Now;
                    _context.Activity_Milestone.Update(data);
                    await _context.SaveChangesAsync();

                    aPIResponseDTO.success = true;
                    aPIResponseDTO.message = "Activity Milestone " + (data.active_flag == true ? "Activated" : "Inactivated") + " successfully.";
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
        public async Task<APIResponseDTO> GetActivityMilestoneMapping(int id)
        {
            APIResponseDTO response = new APIResponseDTO();
            try
            {
                var list = await (from a in _context.Activity_Milestone_Mapping
                                  join b in _context.Activity on a.activity_id equals b.id
                                  where a.activity_milestone_id == id

                                  select new
                                  {
                                      a.id,
                                      a.activity_id,
                                      a.start_date,
                                      a.end_date,
                                      a.cost,
                                      activity_name = b.name
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
        public async Task<APIResponseDTO> GetActivityDropdownMilestoneList(int project_id, ExtractTokenDto token)
        {
            APIResponseDTO response = new APIResponseDTO();
            try
            {
                var list = new List<dynamic>();

                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (var command = new SqlCommand("get_activity_drop_down_milestone_list", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 120;
                        command.Parameters.AddWithValue("@project_id", project_id);
                        command.Parameters.AddWithValue("@company_id", token.CompanyId);

                        await connection.OpenAsync();

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var data = new
                                {
                                    activity_id = reader.GetInt32(reader.GetOrdinal("activity_id")),
                                    activity_name = reader.GetString(reader.GetOrdinal("activity_name")),
                                    start_date = reader.GetString(reader.GetOrdinal("start_date")),
                                    end_date = reader.GetString(reader.GetOrdinal("end_date")),
                                    cost = reader.GetDecimal(reader.GetOrdinal("cost")),
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