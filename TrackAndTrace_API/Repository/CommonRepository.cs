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
using System.Collections.Generic;
using Hangfire.MemoryStorage.Database;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.Json;

namespace TrackAndTrace_API.Repository
{
    public class CommonRepository : ICommonRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        public CommonRepository(ApplicationDbContext context, IMapper mapper, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<APIResponseDTO> GetCommonDropdownList(string name, ExtractTokenDto token)
        {
            APIResponseDTO response = new APIResponseDTO();
            try
            {
                var list = new List<dynamic>();

                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (var command = new SqlCommand("get_common_drop_down_list", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 120;
                        command.Parameters.AddWithValue("@company_id", token.CompanyId);
                        command.Parameters.AddWithValue("@name", name);

                        await connection.OpenAsync();

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var data = new
                                {
                                    id = reader.GetInt32(reader.GetOrdinal("id")),
                                    code = reader.GetString(reader.GetOrdinal("code")),
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
        public async Task<APIResponseDTO> CreateBulkImportName(ExtractTokenDto token)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            try
            {
                var importName = await _context.Bulk_Import_Details.Where(x => x.company_id == token.CompanyId).OrderByDescending(x => x.id).Select(x => x.name).FirstOrDefaultAsync();

                var dmo = new Bulk_Import_Details
                {
                    company_id = token.CompanyId,
                    name = importName == null ? "IMPORT_0001" : $"IMPORT_{(int.Parse(importName.Split('_')[1]) + 1).ToString("D4")}",
                    created_by = token.UserId,
                    created_date = DateTime.Now,
                    updated_date = null
                };

                await _context.Bulk_Import_Details.AddAsync(dmo);
                await _context.SaveChangesAsync();

                aPIResponseDTO.success = true;
                aPIResponseDTO.message = "Injury saved successfully";
                aPIResponseDTO.data = dmo.name;
                return aPIResponseDTO;
            }
            catch (Exception ex)
            {
                aPIResponseDTO.message = $"Failed saving details: {ex.Message}";
                return aPIResponseDTO;
            }
        }
        public async Task<APIResponseDTO> GetBulkImportDetails(string name, ExtractTokenDto token)
        {
            APIResponseDTO response = new APIResponseDTO();
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (var command = new SqlCommand("get_bulk_import_details", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 120;
                        command.Parameters.AddWithValue("@company_id", token.CompanyId);
                        command.Parameters.AddWithValue("@import_name", name);
                        await connection.OpenAsync();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                dynamic failedRecordsList = null;
                                if (!reader.IsDBNull(reader.GetOrdinal("failed_records")))
                                {
                                    string failedRecordsJson = reader.GetString(reader.GetOrdinal("failed_records"));
                                    if (!string.IsNullOrEmpty(failedRecordsJson))
                                    {
                                        failedRecordsList = JsonSerializer.Deserialize<dynamic>(failedRecordsJson);
                                    }
                                }

                                var data = new
                                {
                                    id = reader.GetInt32(reader.GetOrdinal("id")),
                                    name = reader.GetString(reader.GetOrdinal("name")),
                                    total = reader.GetInt32(reader.GetOrdinal("total")),
                                    success = reader.GetInt32(reader.GetOrdinal("success")),
                                    failed = reader.GetInt32(reader.GetOrdinal("failed")),
                                    failed_records = failedRecordsList,
                                    status = reader.IsDBNull(reader.GetOrdinal("status")) ? null : reader.GetString(reader.GetOrdinal("status"))
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
    }
}