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
using Microsoft.AspNetCore.Mvc;
using Azure;

namespace TrackAndTrace_API.Repository
{
    public class DailyActivityRepository : IDailyActivityRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        public DailyActivityRepository(ApplicationDbContext context, IMapper mapper, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<APIResponseDTO> Add(DailyActivityDto model, ExtractTokenDto token)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            try
            {
                int dailyActivityId = model.daily_activity_id;

                var activityItemId = await _context.Trx_Daily_Activity_Item.Where(x => x.delete_flag == false && x.company_id == token.CompanyId &&
                                                    x.activity_id == model.activity_id && x.task_id == model.task_id &&
                                                    x.sub_task_id == model.sub_task_id).Select(x => x.id).FirstOrDefaultAsync();

                if (activityItemId == 0)
                {
                    Trx_Daily_Activity_Item trx_Daily_Activity_Item = new Trx_Daily_Activity_Item
                    {
                        activity_id = model.activity_id,
                        task_id = model.task_id,
                        sub_task_id = model.sub_task_id,
                        company_id = token.CompanyId,
                        created_by = token.UserId,
                        created_date = DateTime.Now
                    };

                    _context.Trx_Daily_Activity_Item.Add(trx_Daily_Activity_Item);
                    await _context.SaveChangesAsync();

                    model.activity_item_id = trx_Daily_Activity_Item.id;
                }
                else
                {
                    model.activity_item_id = activityItemId;
                }

                var dmo = _mapper.Map<Trx_Daily_Activity_Details>(model);
                if (dailyActivityId > 0)
                {
                    var existing = await _context.Trx_Daily_Activity_Details.AsNoTracking().FirstOrDefaultAsync(x => x.id == dailyActivityId && x.delete_flag == false);

                    if (existing == null)
                    {
                        aPIResponseDTO.message = "Daily Activity details not found";
                        return aPIResponseDTO;
                    }

                    dmo.id = dailyActivityId;
                    dmo.company_id = token.CompanyId;
                    dmo.created_by = existing.created_by;
                    dmo.created_date = existing.created_date;
                    dmo.updated_by = token.UserId;
                    dmo.updated_date = DateTime.Now;
                    _context.Trx_Daily_Activity_Details.Update(dmo);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    dmo.company_id = token.CompanyId;
                    dmo.created_by = token.UserId;
                    dmo.created_date = DateTime.Now;
                    dmo.updated_date = null;
                    await _context.Trx_Daily_Activity_Details.AddAsync(dmo);
                    await _context.SaveChangesAsync();

                    dailyActivityId = dmo.id;
                }

                if (dailyActivityId > 0)
                {
                    await AddOrUpdateManpower(model.manpower, dailyActivityId, token);
                    await AddOrUpdateMaterial(model.material, dailyActivityId, token);
                    await AddOrUpdateMachinery(model.machinery, dailyActivityId, token);
                }

                aPIResponseDTO.success = true;
                aPIResponseDTO.message = "DailyActivity saved successfully";
                return aPIResponseDTO;
            }
            catch (Exception ex)
            {
                aPIResponseDTO.message = $"Failed saving details: {ex.Message}";
                return aPIResponseDTO;
            }
        }
        private async Task AddOrUpdateManpower(List<DailyActivityManpower> manpowerList, int dailyActivityId, ExtractTokenDto token)
        {
            var existingManpower = await _context.Trx_Daily_Activity_Manpower.Where(x => x.daily_activity_id == dailyActivityId && x.company_id == token.CompanyId && x.delete_flag == false).ToListAsync();

            var manpowerIdsInRequest = manpowerList.Select(x => x.id).ToList();

            foreach (var manpower in manpowerList)
            {
                var manpowerData = await _context.Manpower.Where(x => x.id == manpower.manpower_id).FirstOrDefaultAsync();

                if (manpowerData != null)
                {
                    var existingEntry = existingManpower.FirstOrDefault(x => x.manpower_id == manpower.manpower_id);

                    if (existingEntry != null)
                    {
                        existingEntry.manpower_id = manpower.manpower_id;
                        existingEntry.designation_id = manpowerData.designation_id;
                        existingEntry.engineer_id = manpowerData.engineer_id;
                        existingEntry.charge_hand_id = manpowerData.charge_hand_id;
                        existingEntry.gang_leader_id = manpowerData.gang_leader_id;
                        existingEntry.subcontractor_id = manpowerData.subcontractor_id;
                        existingEntry.updated_by = token.UserId;
                        existingEntry.updated_date = DateTime.Now;
                    }
                    else
                    {
                        _context.Trx_Daily_Activity_Manpower.Add(new Trx_Daily_Activity_Manpower
                        {
                            daily_activity_id = dailyActivityId,
                            manpower_id = manpower.manpower_id,
                            designation_id = manpowerData.designation_id,
                            engineer_id = manpowerData.engineer_id,
                            charge_hand_id = manpowerData.charge_hand_id,
                            gang_leader_id = manpowerData.gang_leader_id,
                            subcontractor_id = manpowerData.subcontractor_id,
                            company_id = token.CompanyId,
                            created_by = token.UserId,
                            created_date = DateTime.Now
                        });
                    }
                }
            }

            foreach (var manpower in existingManpower.Where(x => !manpowerIdsInRequest.Contains(x.id)))
            {
                manpower.delete_flag = true;
                manpower.updated_by = token.UserId;
                manpower.updated_date = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }
        private async Task AddOrUpdateMaterial(List<DailyActivityMaterial> materialList, int dailyActivityId, ExtractTokenDto token)
        {
            var existingMaterial = await _context.Trx_Daily_Activity_Material.Where(x => x.daily_activity_id == dailyActivityId && x.company_id == token.CompanyId && x.delete_flag == false).ToListAsync();

            var materialIdsInRequest = materialList.Select(x => x.id).ToList();

            foreach (var material in materialList)
            {
                var existingEntry = existingMaterial.FirstOrDefault(x => x.material_id == material.material_id);

                if (existingEntry != null)
                {
                    existingEntry.material_id = material.material_id;
                    existingEntry.quantity = material.quantity;
                    existingEntry.updated_by = token.UserId;
                    existingEntry.updated_date = DateTime.Now;
                }
                else
                {
                    _context.Trx_Daily_Activity_Material.Add(new Trx_Daily_Activity_Material
                    {
                        daily_activity_id = dailyActivityId,
                        material_id = material.material_id,
                        quantity = material.quantity,
                        company_id = token.CompanyId,
                        created_by = token.UserId,
                        created_date = DateTime.Now
                    });
                }
            }

            foreach (var material in existingMaterial.Where(x => !materialIdsInRequest.Contains(x.id)))
            {
                material.delete_flag = true;
                material.updated_by = token.UserId;
                material.updated_date = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }
        private async Task AddOrUpdateMachinery(List<DailyActivityMachinery> machineryList, int dailyActivityId, ExtractTokenDto token)
        {
            var existingMachinery = await _context.Trx_Daily_Activity_Machinery.Where(x => x.daily_activity_id == dailyActivityId && x.company_id == token.CompanyId && x.delete_flag == false).ToListAsync();

            var machineryIdsInRequest = machineryList.Select(x => x.id).ToList();

            foreach (var machinery in machineryList)
            {
                var existingEntry = existingMachinery.FirstOrDefault(x => x.machinery_id == machinery.machinery_id);

                if (existingEntry != null)
                {
                    existingEntry.machinery_id = machinery.machinery_id;
                    existingEntry.quantity = machinery.quantity;
                    existingEntry.start_time = machinery.start_time;
                    existingEntry.end_time = machinery.end_time;
                    existingEntry.updated_by = token.UserId;
                    existingEntry.updated_date = DateTime.Now;
                }
                else
                {
                    _context.Trx_Daily_Activity_Machinery.Add(new Trx_Daily_Activity_Machinery
                    {
                        daily_activity_id = dailyActivityId,
                        machinery_id = machinery.machinery_id,
                        quantity = machinery.quantity,
                        start_time = machinery.start_time,
                        end_time = machinery.end_time,
                        company_id = token.CompanyId,
                        created_by = token.UserId,
                        created_date = DateTime.Now
                    });
                }
            }

            foreach (var machinery in existingMachinery.Where(x => !machineryIdsInRequest.Contains(x.id)))
            {
                machinery.delete_flag = true;
                machinery.updated_by = token.UserId;
                machinery.updated_date = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }
        public async Task<APIResponseDTO> GetList(int projectId, DateTime? fromDate, DateTime? toDate, CommonRequestDto request, ExtractTokenDto token)
        {
            APIResponseDTO response = new APIResponseDTO();
            try
            {
                int totalCount = 0;
                var list = new List<dynamic>();

                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (var command = new SqlCommand("get_daily_activity_list", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 120;
                        command.Parameters.AddWithValue("@company_id", token.CompanyId);
                        command.Parameters.AddWithValue("@project_id", projectId);
                        command.Parameters.AddWithValue("@from_date", fromDate);
                        command.Parameters.AddWithValue("@to_date", toDate);
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
                                    daily_activity_name = reader.GetString(reader.GetOrdinal("daily_activity_name")),
                                    activity_date = reader.GetDateTime(reader.GetOrdinal("activity_date")),
                                    project_id = reader.GetInt32(reader.GetOrdinal("project_id")),
                                    project_name = reader.GetString(reader.GetOrdinal("project_name")),
                                    quantity = reader.GetDecimal(reader.GetOrdinal("quantity")),
                                    uom_name = reader.GetString(reader.GetOrdinal("uom_name")),
                                    shift_id = reader.GetInt32(reader.GetOrdinal("shift_id")),
                                    shift_name = reader.GetString(reader.GetOrdinal("shift_name")),
                                    hrs_spent = reader.GetDecimal(reader.GetOrdinal("hrs_spent")),
                                    project_level_name = reader.GetString(reader.GetOrdinal("project_level_name")),
                                    is_draft = reader.GetBoolean(reader.GetOrdinal("is_draft")),
                                    status = reader.IsDBNull(reader.GetOrdinal("status")) ? "" : reader.GetString(reader.GetOrdinal("status"))
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

                var data = await _context.Trx_Daily_Activity_Details.Where(x => x.delete_flag == false && idsToDelete.Contains(x.id) && x.company_id == token.CompanyId).ToListAsync();

                if (data.Any())
                {
                    foreach (var record in data)
                    {
                        record.delete_flag = true;
                        record.updated_by = token.UserId;
                        record.updated_date = DateTime.Now;
                    }

                    _context.Trx_Daily_Activity_Details.UpdateRange(data);
                    await _context.SaveChangesAsync();

                    aPIResponseDTO.success = true;
                    aPIResponseDTO.message = "DailyActivity deleted successfully.";
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
        public async Task<APIResponseDTO> GetSubcontractorDropdownList(int id, ExtractTokenDto token)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            try
            {
                var list = await (from a in _context.Subcontractor
                                  where a.company_id == token.CompanyId && a.labour_type_id == id && a.active_flag == true && a.delete_flag == false
                                  select new
                                  {
                                      a.id,
                                      a.name
                                  }).ToListAsync();

                if (list.Count > 0)
                {
                    aPIResponseDTO.success = true;
                    aPIResponseDTO.message = "Data fetched successfully";
                    aPIResponseDTO.data = list;
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
        public async Task<APIResponseDTO> GetManpowerDropdownList(int project_id, int shift_id, decimal hrs_spent, DateTime activity_date, decimal old_hrs_spent, string old_manpower_ids, ExtractTokenDto token)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (var command = new SqlCommand("get_daily_activity_manpower_drop_down_list", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 120;
                        command.Parameters.AddWithValue("@project_id", project_id);
                        command.Parameters.AddWithValue("@user_id", token.UserId);
                        command.Parameters.AddWithValue("@shift_id", shift_id);
                        command.Parameters.AddWithValue("@hrs_spent", hrs_spent);
                        command.Parameters.AddWithValue("@activity_date", activity_date);
                        command.Parameters.AddWithValue("@old_hrs_spent", old_hrs_spent);
                        command.Parameters.AddWithValue("@old_manpower_ids", old_manpower_ids == "0" ? "" : old_manpower_ids);

                        // Define output parameter
                        var responseMessageParam = new SqlParameter("@response_message", SqlDbType.NVarChar, 1000)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(responseMessageParam);

                        await connection.OpenAsync();

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            var manpowerList = new List<dynamic>();

                            while (await reader.ReadAsync())
                            {
                                var manpower = new
                                {
                                    manpower_id = reader.GetInt32(reader.GetOrdinal("manpower_id")),
                                    manpower_code = reader.GetString(reader.GetOrdinal("manpower_code")),
                                    manpower_name = reader.GetString(reader.GetOrdinal("manpower_name")),
                                    designation_name = reader.GetString(reader.GetOrdinal("designation_name")),
                                    remaining_work_hrs = reader.GetDecimal(reader.GetOrdinal("remaining_work_hrs")),
                                    is_valid = reader.GetInt32(reader.GetOrdinal("is_valid")) == 1
                                };

                                manpowerList.Add(manpower);
                            }

                            aPIResponseDTO.success = true;
                            aPIResponseDTO.message = "Manpower List retrieved successfully";
                            aPIResponseDTO.data = manpowerList;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                aPIResponseDTO.message = ex.Message;
            }

            return aPIResponseDTO;
        }
        public async Task<APIResponseDTO> GetMaterialDropdownList(int id, ExtractTokenDto token)
        {
            APIResponseDTO response = new APIResponseDTO();

            try
            {
                var list = new List<dynamic>();

                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (var command = new SqlCommand("get_project_material_drop_down_list", connection))
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
                                    material_id = reader.GetInt32(reader.GetOrdinal("material_id")),
                                    material_code = reader.GetString(reader.GetOrdinal("material_code")),
                                    material_name = reader.GetString(reader.GetOrdinal("material_name")),
                                    quantity = reader.GetDecimal(reader.GetOrdinal("quantity")),
                                    uom_name = reader.GetString(reader.GetOrdinal("uom_name"))
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
        public async Task<APIResponseDTO> GetMachineryDropdownList(int daily_activity_id, ExtractTokenDto token)
        {
            APIResponseDTO response = new APIResponseDTO();

            try
            {
                var list = new List<dynamic>();

                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (var command = new SqlCommand("get_daily_activity_machinery_drop_down_list", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 120;
                        command.Parameters.AddWithValue("@company_id", token.CompanyId);
                        command.Parameters.AddWithValue("@daily_activity_id", daily_activity_id);

                        await connection.OpenAsync();

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var data = new
                                {
                                    machinery_id = reader.GetInt32(reader.GetOrdinal("machinery_id")),
                                    machinery_code = reader.GetString(reader.GetOrdinal("machinery_code")),
                                    machinery_name = reader.GetString(reader.GetOrdinal("machinery_name")),
                                    quantity = reader.GetDecimal(reader.GetOrdinal("available_quantity"))
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
        public async Task<APIResponseDTO> GetDailyActivityDetailById(int id, ExtractTokenDto token)
        {
            APIResponseDTO response = new APIResponseDTO();
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (var command = new SqlCommand("get_daily_activity_detail_by_id", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 120;
                        command.Parameters.AddWithValue("@daily_activity_id", id);

                        await connection.OpenAsync();

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var data = new
                                {
                                    id = reader.GetInt32(reader.GetOrdinal("id")),
                                    activity_date = reader.GetDateTime(reader.GetOrdinal("activity_date")),
                                    activity_id = reader.GetInt32(reader.GetOrdinal("activity_id")),
                                    task_id = reader.GetInt32(reader.GetOrdinal("task_id")),
                                    sub_task_id = reader.GetInt32(reader.GetOrdinal("sub_task_id")),
                                    quantity = reader.GetDecimal(reader.GetOrdinal("quantity")),
                                    progress = reader.GetDecimal(reader.GetOrdinal("progress")),
                                    shift_id = reader.GetInt32(reader.GetOrdinal("shift_id")),
                                    hrs_spent = reader.GetDecimal(reader.GetOrdinal("hrs_spent")),
                                    labour_type_id = reader.GetInt32(reader.GetOrdinal("labour_type_id")),
                                    subcontractor_id = reader.IsDBNull(reader.GetOrdinal("subcontractor_id")) ? 0 : reader.GetInt32(reader.GetOrdinal("subcontractor_id")),
                                    weather_id = reader.IsDBNull(reader.GetOrdinal("weather_id")) ? 0 : reader.GetInt32(reader.GetOrdinal("weather_id")),
                                    remarks = reader.GetString(reader.GetOrdinal("remarks")),
                                    project_level_id = reader.GetInt32(reader.GetOrdinal("project_level_id")),
                                    is_draft = reader.GetBoolean(reader.GetOrdinal("is_draft")),
                                    status = reader.IsDBNull(reader.GetOrdinal("status")) ? "" : reader.GetString(reader.GetOrdinal("status"))
                                };

                                response.success = true;
                                response.message = data != null ? "Data Fetched Successfully" : "No Records Found";
                                response.data = data;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.success = false;
                response.message = ex.Message;
            }

            return response;
        }
        public async Task<APIResponseDTO> GetDailyActivityManpowerById(int id, ExtractTokenDto token)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            try
            {
                var list = await (from a in _context.Trx_Daily_Activity_Manpower
                                  join b in _context.Manpower on a.manpower_id equals b.id
                                  join c in _context.Designation on a.designation_id equals c.id
                                  where a.company_id == token.CompanyId && a.daily_activity_id == id && a.delete_flag == false
                                  select new
                                  {
                                      a.id,
                                      a.daily_activity_id,
                                      a.manpower_id,
                                      manpower_code = b.code,
                                      manpower_name = b.name,
                                      designation_name = c.name
                                  }).ToListAsync();

                if (list.Count > 0)
                {
                    aPIResponseDTO.success = true;
                    aPIResponseDTO.message = "Data fetched successfully";
                    aPIResponseDTO.data = list;
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
        public async Task<APIResponseDTO> GetDailyActivityMaterialById(int id, ExtractTokenDto token)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            try
            {
                var list = await (from a in _context.Trx_Daily_Activity_Material
                                  join b in _context.Material on a.material_id equals b.id
                                  join c in _context.UOM on b.uom_id equals c.id
                                  where a.company_id == token.CompanyId && a.daily_activity_id == id && a.delete_flag == false
                                  select new
                                  {
                                      a.id,
                                      a.daily_activity_id,
                                      a.material_id,
                                      material_code = b.code,
                                      material_name = b.name,
                                      uom_name = c.name,
                                      a.quantity
                                  }).ToListAsync();

                if (list.Count > 0)
                {
                    aPIResponseDTO.success = true;
                    aPIResponseDTO.message = "Data fetched successfully";
                    aPIResponseDTO.data = list;
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
        public async Task<APIResponseDTO> GetDailyActivityMachineryById(int id, ExtractTokenDto token)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            try
            {
                var list = await (from a in _context.Trx_Daily_Activity_Machinery
                                  join b in _context.Machinery on a.machinery_id equals b.id
                                  where a.company_id == token.CompanyId && a.daily_activity_id == id && a.delete_flag == false
                                  select new
                                  {
                                      a.id,
                                      a.daily_activity_id,
                                      a.machinery_id,
                                      machinery_code = b.code,
                                      machinery_name = b.name,
                                      a.quantity,
                                      a.start_time,
                                      a.end_time
                                  }).ToListAsync();

                if (list.Count > 0)
                {
                    aPIResponseDTO.success = true;
                    aPIResponseDTO.message = "Data fetched successfully";
                    aPIResponseDTO.data = list;
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