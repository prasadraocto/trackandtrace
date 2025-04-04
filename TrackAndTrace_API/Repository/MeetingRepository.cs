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
    public class MeetingRepository : IMeetingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        public MeetingRepository(ApplicationDbContext context, IMapper mapper, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<APIResponseDTO> Add(MeetingDto model, ExtractTokenDto token)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            try
            {
                var dmo = _mapper.Map<Meeting>(model);

                if (dmo.id > 0)
                {
                    var existingMeeting = await _context.Meeting.AsNoTracking().FirstOrDefaultAsync(x => x.id == dmo.id);

                    if (existingMeeting == null)
                    {
                        aPIResponseDTO.message = "Meeting details not found";
                        return aPIResponseDTO;
                    }

                    dmo.status = existingMeeting.status;
                    dmo.company_id = token.CompanyId;
                    dmo.created_by = existingMeeting.created_by;
                    dmo.created_date = existingMeeting.created_date;
                    dmo.updated_by = token.UserId;
                    dmo.updated_date = DateTime.Now;
                    _context.Meeting.Update(dmo);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    dmo.status = "scheduled";
                    dmo.company_id = token.CompanyId;
                    dmo.created_by = token.UserId;
                    dmo.created_date = DateTime.Now;
                    dmo.updated_date = null;
                    await _context.Meeting.AddAsync(dmo);
                    await _context.SaveChangesAsync();

                    model.id = dmo.id;
                }

                bool mappingResponse = await SaveAttendeeMapping(model);

                aPIResponseDTO.success = true;
                aPIResponseDTO.message = "Meeting saved successfully";
                return aPIResponseDTO;
            }
            catch (Exception ex)
            {
                aPIResponseDTO.message = $"Failed saving details: {ex.Message}";
                return aPIResponseDTO;
            }
        }
        private async Task<bool> SaveAttendeeMapping(MeetingDto meetingDto)
        {
            try
            {
                // Retrieve existing mappings for the given meeting
                var existingMappings = await _context.Meeting_Attendee_Detail.Where(mapping => mapping.meeting_id == meetingDto.id).ToListAsync();

                // Get the attendee IDs from the incoming DTO
                var incomingAttendeeIds = meetingDto.attendee_mapping.Select(pm => pm.attendee_id).ToList();

                // Remove mappings not present in the incoming model
                var mappingsToRemove = existingMappings.Where(existingMapping => !incomingAttendeeIds.Contains(existingMapping.attendee_id)).ToList();
                _context.RemoveRange(mappingsToRemove);

                // Add or update mappings from the incoming model
                var newMappings = meetingDto.attendee_mapping
                    .Where(incomingMapping => !existingMappings.Any(em => em.attendee_id == incomingMapping.attendee_id))
                    .Select(incomingMapping => new Meeting_Attendee_Detail
                    {
                        meeting_id = meetingDto.id,
                        attendee_id = incomingMapping.attendee_id
                    }).ToList();

                await _context.AddRangeAsync(newMappings);

                // Save changes to the database
                var changesSaved = await _context.SaveChangesAsync();

                return changesSaved > 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<APIResponseDTO> GetList(string start_date, string end_date, ExtractTokenDto token)
        {
            APIResponseDTO response = new APIResponseDTO();
            try
            {
                var list = new List<MeetingResponseDTO>();

                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (var command = new SqlCommand("get_meeting_list", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 120;
                        command.Parameters.AddWithValue("@company_id", token.CompanyId);
                        command.Parameters.AddWithValue("@start_date", start_date);
                        command.Parameters.AddWithValue("@end_date", end_date);

                        await connection.OpenAsync();

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var data = new MeetingResponseDTO
                                {
                                    id = reader.GetInt32(reader.GetOrdinal("id")),
                                    title = reader.GetString(reader.GetOrdinal("title")),
                                    agenda = reader.GetString(reader.GetOrdinal("agenda")),
                                    meeting_date = reader.GetDateTime(reader.GetOrdinal("meeting_date")),
                                    start_time = reader.GetString(reader.GetOrdinal("start_time")),
                                    end_time = reader.GetString(reader.GetOrdinal("end_time")),
                                    color = reader.GetString(reader.GetOrdinal("color")),
                                    status = reader.GetString(reader.GetOrdinal("status"))
                                };

                                if (data.status != "completed")
                                {
                                    if (DateTime.Now.ToUniversalTime() >= Convert.ToDateTime(data.end_time))
                                    {
                                        var updateStatus = await _context.Meeting.FindAsync(data.id);

                                        if (updateStatus != null)
                                        {
                                            data.status = "completed";
                                            updateStatus.status = data.status;
                                            _context.Meeting.Update(updateStatus);
                                            await _context.SaveChangesAsync();
                                        }
                                    }
                                }

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
        public async Task<APIResponseDTO> Delete(int id, ExtractTokenDto token)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            try
            {
                var data = await _context.Meeting.Where(x => x.delete_flag == false && x.id == id && x.company_id == token.CompanyId).FirstOrDefaultAsync();

                if (data != null)
                {
                    data.delete_flag = true;
                    data.updated_by = token.UserId;
                    data.updated_date = DateTime.Now;

                    _context.Meeting.Update(data);
                    await _context.SaveChangesAsync();

                    aPIResponseDTO.success = true;
                    aPIResponseDTO.message = "Meeting deleted successfully.";
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
        public async Task<APIResponseDTO> GetAttendeeMappingById(int id)
        {
            APIResponseDTO response = new APIResponseDTO();
            try
            {
                var list = await (from a in _context.Meeting_Attendee_Detail
                                  join b in _context.Users on a.attendee_id equals b.id
                                  where a.meeting_id == id && b.active_flag == true && b.delete_flag == false
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
        public async Task<APIResponseDTO> UpdateAttendeeTask(int id, AttendeeTaskDto model, ExtractTokenDto token)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            try
            {
                var dataToDelete = await (from a in _context.Meeting_Attendee_Detail
                                          join b in _context.Meeting_Assigned_Task on a.id equals b.meeting_attendee_id
                                          where a.meeting_id == id
                                          && !model.attendee_task.Select(y => y.id).Contains(b.id)
                                          select b).ToListAsync();

                _context.Meeting_Assigned_Task.RemoveRange(dataToDelete);
                await _context.SaveChangesAsync();

                foreach (var item in model.attendee_task)
                {
                    var dmo = _mapper.Map<Meeting_Assigned_Task>(item);
                    dmo.meeting_attendee_id = await _context.Meeting_Attendee_Detail.Where(x => x.meeting_id == id && x.attendee_id == item.attendee_id).Select(x => x.id).FirstOrDefaultAsync();
                    
                    if (dmo.id > 0)
                    {
                        var existingMeeting = await _context.Meeting_Assigned_Task.AsNoTracking().FirstOrDefaultAsync(x => x.id == dmo.id);

                        if (existingMeeting == null)
                        {
                            aPIResponseDTO.message = "Attendee Task details not found";
                            return aPIResponseDTO;
                        }

                        dmo.status = existingMeeting.status;
                        dmo.company_id = token.CompanyId;
                        dmo.created_by = existingMeeting.created_by;
                        dmo.created_date = existingMeeting.created_date;
                        dmo.updated_by = token.UserId;
                        dmo.updated_date = DateTime.Now;
                        _context.Meeting_Assigned_Task.Update(dmo);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        dmo.status = "pending";
                        dmo.company_id = token.CompanyId;
                        dmo.created_by = token.UserId;
                        dmo.created_date = DateTime.Now;
                        dmo.updated_date = null;
                        await _context.Meeting_Assigned_Task.AddAsync(dmo);
                        await _context.SaveChangesAsync();
                    }
                }

                aPIResponseDTO.success = true;
                aPIResponseDTO.message = "Attendee Task saved successfully";
                return aPIResponseDTO;
            }
            catch (Exception ex)
            {
                aPIResponseDTO.message = $"Failed saving details: {ex.Message}";
                return aPIResponseDTO;
            }
        }
        public async Task<APIResponseDTO> GetAttendeeTaskList(int id)
        {
            APIResponseDTO response = new APIResponseDTO();
            try
            {
                var list = await (from a in _context.Meeting_Attendee_Detail
                                  join b in _context.Meeting_Assigned_Task on a.id equals b.meeting_attendee_id
                                  where a.meeting_id == id
                                  select new
                                  {
                                      b.id,
                                      a.attendee_id,
                                      b.task,
                                      b.due_date,
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