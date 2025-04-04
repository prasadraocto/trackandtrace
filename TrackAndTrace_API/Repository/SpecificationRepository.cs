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
using static TrackAndTrace_API.Helpers.Utils;
using Newtonsoft.Json;

namespace TrackAndTrace_API.Repository
{
    public class SpecificationRepository : ISpecificationRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        public SpecificationRepository(ApplicationDbContext context, IMapper mapper, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<APIResponseDTO> Add(SpecificationDto model, ExtractTokenDto token)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            try
            {
                var nameExists = await _context.Specification.AnyAsync(x => x.name.ToLower() == model.name.ToLower() && x.id != model.id && x.company_id == token.CompanyId);
                if (nameExists)
                {
                    aPIResponseDTO.message = "Specification Name already exists";
                    return aPIResponseDTO;
                }

                var codeExists = await _context.Specification.AnyAsync(x => x.code.ToLower() == model.code.ToLower() && x.id != model.id && x.company_id == token.CompanyId);
                if (codeExists)
                {
                    aPIResponseDTO.message = "Specification Code already exists";
                    return aPIResponseDTO;
                }

                if (model.specification_differentiator_mapping.Count() == 0)
                {
                    aPIResponseDTO.message = "Specification Mapping required";
                    return aPIResponseDTO;
                }

                var dmo = _mapper.Map<Specification>(model);

                if (dmo.id > 0)
                {
                    var existingSpecification = await _context.Specification.AsNoTracking().FirstOrDefaultAsync(x => x.id == dmo.id);

                    if (existingSpecification == null)
                    {
                        aPIResponseDTO.message = "Specification details not found";
                        return aPIResponseDTO;
                    }

                    dmo.company_id = token.CompanyId;
                    dmo.created_by = existingSpecification.created_by;
                    dmo.created_date = existingSpecification.created_date;
                    dmo.updated_by = token.UserId;
                    dmo.updated_date = DateTime.Now;
                    _context.Specification.Update(dmo);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    dmo.company_id = token.CompanyId;
                    dmo.created_by = token.UserId;
                    dmo.created_date = DateTime.Now;
                    dmo.updated_date = null;
                    await _context.Specification.AddAsync(dmo);
                    await _context.SaveChangesAsync();
                }

                foreach (var mapping in model.specification_differentiator_mapping)
                {
                    mapping.specification_id = dmo.id;
                }

                var response = await SaveSpecificationDifferentiatorMapping(model.specification_differentiator_mapping);

                if (!response)
                {
                    aPIResponseDTO.success = false;
                    aPIResponseDTO.message = "Failed to save Specification mapping";
                    return aPIResponseDTO;
                }

                aPIResponseDTO.success = true;
                aPIResponseDTO.message = "Specification saved successfully";
                return aPIResponseDTO;
            }
            catch (Exception ex)
            {
                aPIResponseDTO.message = $"Failed saving details: {ex.Message}";
                return aPIResponseDTO;
            }
        }
        public async Task<bool> SaveSpecificationDifferentiatorMapping(List<Specification_Differentiator_Mapping> requestList)
        {
            try
            {
                var existingMappings = await _context.Specification_Differentiator_Mapping.Where(m => requestList.Select(r => r.specification_id).Contains(m.specification_id)).ToListAsync();

                var mappingsToRemove = existingMappings.Where(m => !requestList.Any(r => r.specification_id == m.specification_id && r.differentiator_id == m.differentiator_id)).ToList();

                if (mappingsToRemove.Any())
                {
                    _context.Specification_Differentiator_Mapping.RemoveRange(mappingsToRemove);
                    await _context.SaveChangesAsync();
                }

                foreach (var requestMapping in requestList)
                {
                    var existingMapping = existingMappings.FirstOrDefault(m => m.specification_id == requestMapping.specification_id && m.differentiator_id == requestMapping.differentiator_id);

                    if (existingMapping != null)
                    {
                        existingMapping.value = requestMapping.value;
                    }
                    else
                    {
                        _context.Specification_Differentiator_Mapping.Add(requestMapping);
                    }
                }
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
                    using (var command = new SqlCommand("get_specification_list", connection))
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

                var data = await _context.Specification.Where(x => x.delete_flag == false && idsToDelete.Contains(x.id) && x.company_id == token.CompanyId).ToListAsync();

                if (data.Any())
                {
                    foreach (var record in data)
                    {
                        record.delete_flag = true;
                        record.updated_by = token.UserId;
                        record.updated_date = DateTime.Now;
                    }

                    _context.Specification.UpdateRange(data);
                    await _context.SaveChangesAsync();

                    aPIResponseDTO.success = true;
                    aPIResponseDTO.message = "Specification deleted successfully.";
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
                var data = await _context.Specification.Where(x => x.id == id).FirstOrDefaultAsync();

                if (data != null)
                {
                    data.active_flag = data.active_flag == false ? true : false;
                    data.updated_by = token.UserId;
                    data.updated_date = DateTime.Now;
                    _context.Specification.Update(data);
                    await _context.SaveChangesAsync();

                    aPIResponseDTO.success = true;
                    aPIResponseDTO.message = "Specification " + (data.active_flag == true ? "Activated" : "Inactivated") + " successfully.";
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
        public async Task<APIResponseDTO> GetSpecificationDifferentiatorMappingById(int id)
        {
            APIResponseDTO response = new APIResponseDTO();
            try
            {
                var list = new List<dynamic>();

                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (var command = new SqlCommand("get_specification_differentiator_mapping_by_id", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 120;
                        command.Parameters.AddWithValue("@specification_id", id);

                        await connection.OpenAsync();

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var data = new
                                {
                                    id = reader.GetInt32(reader.GetOrdinal("id")),
                                    differentiator_id = reader.GetInt32(reader.GetOrdinal("differentiator_id")),
                                    name = reader.GetString(reader.GetOrdinal("name")),
                                    value = reader.GetString(reader.GetOrdinal("value")),
                                    specification_code = reader.GetString(reader.GetOrdinal("specification_code"))
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
        public async Task ImportSpecification(JsonResponse jsonResponse, string prefix, string importName, ExtractTokenDto token)
        {
            int importId = await _context.Bulk_Import_Details
                .Where(b => b.name == importName)
                .Select(b => b.id)
                .FirstOrDefaultAsync();

            if (importId == 0)
            {
                throw new Exception("Invalid Import Name");
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // Step 1: Verify Differentiator Existence
                    var existingDifferentiators = await _context.Differentiator
                        .Where(d => d.company_id == token.CompanyId)
                        .Select(d => new { d.code, d.id })
                        .ToDictionaryAsync(d => d.code, d => d.id);

                    var missingDifferentiators = jsonResponse.Columns
                        .Where(col => !existingDifferentiators.ContainsKey(col))
                        .Select(col => new { Remark = $"{col} does not exist" })
                        .ToList();

                    if (missingDifferentiators.Any())
                    {
                        var bulkImport = await _context.Bulk_Import_Details
                            .FirstOrDefaultAsync(x => x.id == importId);

                        if (bulkImport != null)
                        {
                            bulkImport.status = "completed";
                            bulkImport.failed_records = JsonConvert.SerializeObject(missingDifferentiators);
                            bulkImport.updated_by = token.UserId;
                            bulkImport.updated_date = DateTime.Now;
                            // Update the bulk import record
                            _context.Bulk_Import_Details.Update(bulkImport);
                            await _context.SaveChangesAsync();
                        }

                        // Commit the transaction and return
                        await transaction.CommitAsync();
                        return;
                    }

                    // Step 2: Get the latest Specification Code with the given prefix
                    var lastSpec = await _context.Specification
                        .Where(s => s.company_id == token.CompanyId && s.code.StartsWith(prefix))
                        .OrderByDescending(s => s.code)
                        .Select(s => s.code)
                        .FirstOrDefaultAsync();

                    int newSpecNumber = lastSpec != null ? int.Parse(lastSpec.Split('-').Last()) + 1 : 1;

                    // Step 3: Insert Specification and get ID for each row
                    var newSpecifications = new List<Specification>();
                    var mappingEntries = new List<Specification_Differentiator_Mapping>();

                    foreach (var row in jsonResponse.Data)
                    {
                        string newSpecCode = $"{prefix}-{newSpecNumber:D4}";
                        newSpecNumber++;

                        var newSpec = new Specification
                        {
                            code = newSpecCode,
                            name = newSpecCode,
                            company_id = token.CompanyId,
                            active_flag = true,
                            delete_flag = false,
                            created_by = token.UserId,
                            created_date = DateTime.UtcNow
                        };
                        _context.Specification.Add(newSpec);
                        await _context.SaveChangesAsync();

                        newSpecifications.Add(newSpec);

                        mappingEntries.AddRange(row.Select(data => new Specification_Differentiator_Mapping
                        {
                            specification_id = newSpec.id,
                            differentiator_id = existingDifferentiators[data.Name],
                            value = data.Value
                        }));
                    }

                    _context.Specification_Differentiator_Mapping.AddRange(mappingEntries);
                    await _context.SaveChangesAsync();

                    // Step 5: Update Bulk Import Details
                    var bulkImportSuccess = await _context.Bulk_Import_Details.FindAsync(importId);
                    bulkImportSuccess.total = jsonResponse.Data.Count;
                    bulkImportSuccess.success = jsonResponse.Data.Count;
                    bulkImportSuccess.failed = 0;
                    bulkImportSuccess.failed_records = null;
                    bulkImportSuccess.status = "completed";
                    bulkImportSuccess.updated_by = token.UserId;
                    bulkImportSuccess.updated_date = DateTime.Now;
                    await _context.SaveChangesAsync();

                    // Commit the transaction
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    // Rollback transaction if there's an error
                    await transaction.RollbackAsync();
                    throw ex;
                }
            }
        }

    }
}