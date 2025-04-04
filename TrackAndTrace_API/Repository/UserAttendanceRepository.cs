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
using TrackAndTrace_API.Helpers;

namespace TrackAndTrace_API.Repository
{
    public class UserAttendanceRepository : IUserAttendanceRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        public UserAttendanceRepository(ApplicationDbContext context, IMapper mapper, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<APIResponseDTO> Add(UserAttendanceDto model, ExtractTokenDto token)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            try
            {
                var dmo = _mapper.Map<User_Attendance>(model);

                if (dmo.id > 0)
                {
                    var existingUserAttendance = await _context.User_Attendance.AsNoTracking().FirstOrDefaultAsync(x => x.id == dmo.id);

                    if (existingUserAttendance == null)
                    {
                        aPIResponseDTO.message = "User Attendance details not found";
                        return aPIResponseDTO;
                    }

                    dmo.user_id = existingUserAttendance.user_id;
                    dmo.company_id = token.CompanyId;
                    dmo.created_by = existingUserAttendance.created_by;
                    dmo.created_date = existingUserAttendance.created_date;
                    dmo.updated_by = token.UserId;
                    dmo.updated_date = DateTime.Now;
                    _context.User_Attendance.Update(dmo);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    var companyCode = await _context.Company.Where(x => x.id == token.CompanyId).Select(x => x.code).FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(companyCode))
                        dmo.image = await Common.SaveDocument(dmo.image, null, _configuration["AzureBlob:Attendance"].ToString(), _configuration, companyCode, token);

                    if (dmo.image.Contains("blob.core"))
                    {
                        dmo.user_id = token.UserId;
                        dmo.company_id = token.CompanyId;
                        dmo.created_by = token.UserId;
                        dmo.created_date = DateTime.Now;
                        dmo.updated_date = null;
                        await _context.User_Attendance.AddAsync(dmo);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        aPIResponseDTO.message = $"Failed saving details: {dmo.image}";
                        return aPIResponseDTO;
                    }
                }

                aPIResponseDTO.success = true;
                aPIResponseDTO.message = "Attendance saved successfully";
                return aPIResponseDTO;
            }
            catch (Exception ex)
            {
                aPIResponseDTO.message = $"Failed saving details: {ex.Message}";
                return aPIResponseDTO;
            }
        }
        public async Task<APIResponseDTO> GetList(CommonRequestDto request, string from_date, string to_date, int company_id, ExtractTokenDto token)
        {
            APIResponseDTO response = new APIResponseDTO();
            try
            {
                int totalCount = 0;
                var list = new List<dynamic>();

                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (var command = new SqlCommand("get_user_attendance_list", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 120;
                        command.Parameters.AddWithValue("@user_id", token.UserId);
                        command.Parameters.AddWithValue("@company_id", company_id == 0 ? token.CompanyId : company_id);
                        command.Parameters.AddWithValue("@page", request.page);
                        command.Parameters.AddWithValue("@page_size", request.page_size);
                        command.Parameters.AddWithValue("@search_query", request.search_query);
                        command.Parameters.AddWithValue("@sort_column", request.sort_column);
                        command.Parameters.AddWithValue("@sort_direction", request.sort_direction);
                        command.Parameters.AddWithValue("@is_export", request.is_export);
                        command.Parameters.AddWithValue("@from_date", from_date);
                        command.Parameters.AddWithValue("@to_date", to_date);

                        command.Parameters.Add("@total_count", SqlDbType.Int).Direction = ParameterDirection.Output;

                        await connection.OpenAsync();

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var data = new
                                {
                                    id = reader.GetInt32(reader.GetOrdinal("id")),
                                    user_id = reader.GetInt32(reader.GetOrdinal("user_id")),
                                    user_code = reader.GetString(reader.GetOrdinal("user_code")),
                                    user_name = reader.GetString(reader.GetOrdinal("user_name")),
                                    designation_id = reader.GetInt32(reader.GetOrdinal("designation_id")),
                                    designation_name = reader.GetString(reader.GetOrdinal("designation_name")),
                                    latitude = reader.GetString(reader.GetOrdinal("latitude")),
                                    longitude = reader.GetString(reader.GetOrdinal("longitude")),
                                    address = reader.GetString(reader.GetOrdinal("address")),
                                    image = reader.GetString(reader.GetOrdinal("image")),
                                    attendance_type = reader.GetString(reader.GetOrdinal("attendance_type")),
                                    attendance_date = reader.GetString(reader.GetOrdinal("attendance_date"))
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

                var data = await _context.User_Attendance.Where(x => x.delete_flag == false && idsToDelete.Contains(x.id) && x.company_id == token.CompanyId).ToListAsync();

                if (data.Any())
                {
                    foreach (var record in data)
                    {
                        record.delete_flag = true;
                        record.updated_by = token.UserId;
                        record.updated_date = DateTime.Now;
                    }

                    _context.User_Attendance.UpdateRange(data);
                    await _context.SaveChangesAsync();

                    aPIResponseDTO.success = true;
                    aPIResponseDTO.message = "UserAttendance deleted successfully.";
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
        public async Task<APIResponseDTO> GetLastAttendanceDetail(ExtractTokenDto token)
        {
            APIResponseDTO response = new APIResponseDTO();
            try
            {
                var data = await _context.User_Attendance.Where(x => x.user_id == token.UserId).OrderByDescending(x => x.id).FirstOrDefaultAsync();

                response.success = true;
                response.message = "Data Fetched Successfully";
                response.data = data;
            }
            catch (Exception ex)
            {
                response.success = false;
                response.message = ex.Message;
            }

            return response;
        }
        public async Task<APIResponseDTO> UpdateDeviceAttendance(string company_code, List<DeviceAttendanceDto> model)
        {
            APIResponseDTO response = new APIResponseDTO();
            try
            {
                int companyId = await _context.Company.Where(x => x.code == company_code).Select(x => x.id).FirstOrDefaultAsync();

                if (companyId > 0)
                {
                    var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(_configuration.GetConnectionString("DefaultConnection")).Options;

                    using (var dbContext = new ApplicationDbContext(options))
                    {
                        // Fetch all users with valid device_user_id
                        var users = dbContext.Users.Where(u => u.device_user_id != null && u.device_user_id != 0 && u.company_id == companyId).ToList();

                        var userIds = users.Select(u => u.id).ToList();

                        // Fetch all user attendance for the fetched users
                        var userAttendance = dbContext.User_Attendance.Where(ua => userIds.Contains(ua.user_id)).ToList();

                        var lastAttendanceTimestamps = userAttendance
                            .GroupBy(ua => ua.user_id)
                            .ToDictionary(
                                g => g.Key,
                                g => g.OrderByDescending(ua => ua.attendance_timestamp).FirstOrDefault()?.attendance_timestamp
                            );

                        var newAttendances = new List<User_Attendance>();
                        foreach (var entry in model)
                        {
                            var user = users.FirstOrDefault(u => u.device_user_id == entry.device_user_id);
                            if (user == null) continue;

                            DateTime deviceAttendanceTime = DateTime.Parse(entry.attendance_timestamp).ToUniversalTime();
                            string formattedDeviceAttendanceTime = deviceAttendanceTime.ToString("o"); // ISO 8601 format

                            // Get last attendance record
                            lastAttendanceTimestamps.TryGetValue(user.id, out var lastAttendanceTimeStr);
                            DateTime? lastAttendanceTime = lastAttendanceTimeStr != null
                                ? DateTime.Parse(lastAttendanceTimeStr).ToUniversalTime()
                                : (DateTime?)null;

                            // Mark previous day's last log as "check-out"
                            if (lastAttendanceTime.HasValue && lastAttendanceTime.Value.Date < deviceAttendanceTime.Date)
                            {
                                var lastAttendanceRecord = userAttendance.FirstOrDefault(ua => ua.user_id == user.id &&
                                    DateTime.Parse(ua.attendance_timestamp).ToUniversalTime() == lastAttendanceTime.Value);

                                if (lastAttendanceRecord != null)
                                {
                                    lastAttendanceRecord.attendance_type = "check-out";
                                }
                            }

                            // Insert only if not exists
                            bool exists = userAttendance.Any(ua => ua.user_id == user.id && DateTime.Parse(ua.attendance_timestamp).ToUniversalTime().ToString("o") == formattedDeviceAttendanceTime);
                            if (!exists)
                            {
                                newAttendances.Add(new User_Attendance
                                {
                                    user_id = user.id,
                                    latitude = entry.latitude,
                                    longitude = entry.longitude,
                                    address = entry.address,
                                    attendance_timestamp = formattedDeviceAttendanceTime,
                                    attendance_type = "check-in",
                                    company_id = companyId,
                                    image = "",
                                    created_by = user.id,
                                    created_date = DateTime.UtcNow,
                                    delete_flag = false
                                });
                            }
                        }

                        if (newAttendances.Any())
                        {
                            await dbContext.User_Attendance.AddRangeAsync(newAttendances);
                            await dbContext.SaveChangesAsync();
                        }
                    }
                }

                response.success = true;
                response.message = "Data Synced Successfully";
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