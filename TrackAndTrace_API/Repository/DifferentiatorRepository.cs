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

namespace TrackAndTrace_API.Repository
{
    public class DifferentiatorRepository : IDifferentiatorRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        public DifferentiatorRepository(ApplicationDbContext context, IMapper mapper, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<APIResponseDTO> Add(DifferentiatorDto model, ExtractTokenDto token)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            try
            {
                var nameExists = await _context.Differentiator.AnyAsync(x => x.name.ToLower() == model.name.ToLower() && x.id != model.id && x.company_id == token.CompanyId);
                if (nameExists)
                {
                    aPIResponseDTO.message = "Differentiator Name already exists";
                    return aPIResponseDTO;
                }

                var codeExists = await _context.Differentiator.AnyAsync(x => x.code.ToLower() == model.code.ToLower() && x.id != model.id && x.company_id == token.CompanyId);
                if (codeExists)
                {
                    aPIResponseDTO.message = "Differentiator Code already exists";
                    return aPIResponseDTO;
                }

                var dmo = _mapper.Map<Differentiator>(model);

                if (dmo.id > 0)
                {
                    var existingDifferentiator = await _context.Differentiator.AsNoTracking().FirstOrDefaultAsync(x => x.id == dmo.id);

                    if (existingDifferentiator == null)
                    {
                        aPIResponseDTO.message = "Differentiator details not found";
                        return aPIResponseDTO;
                    }

                    dmo.company_id = token.CompanyId;
                    dmo.created_by = existingDifferentiator.created_by;
                    dmo.created_date = existingDifferentiator.created_date;
                    dmo.updated_by = token.UserId;
                    dmo.updated_date = DateTime.Now;
                    _context.Differentiator.Update(dmo);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    dmo.company_id = token.CompanyId;
                    dmo.created_by = token.UserId;
                    dmo.created_date = DateTime.Now;
                    dmo.updated_date = null;
                    await _context.Differentiator.AddAsync(dmo);
                    await _context.SaveChangesAsync();
                }

                foreach (var mapping in model.differentiator_mapping)
                {
                    mapping.differentiator_id = dmo.id;
                }

                var response = await SaveDifferentiatorMapping(model.differentiator_mapping);

                aPIResponseDTO.success = true;
                aPIResponseDTO.message = "Differentiator saved successfully";
                return aPIResponseDTO;
            }
            catch (Exception ex)
            {
                aPIResponseDTO.message = $"Failed saving details: {ex.Message}";
                return aPIResponseDTO;
            }
        }
        public async Task<bool> SaveDifferentiatorMapping(List<Differentiator_Mapping> requestList)
        {
            try
            {
                var existingMappings = await _context.Differentiator_Mapping.Where(m => requestList.Select(r => r.differentiator_id).Contains(m.differentiator_id)).ToListAsync();

                // Find mappings to remove (present in DB but not in the request list)
                var mappingsToRemove = existingMappings.Where(m => !requestList.Any(r => r.differentiator_id == m.differentiator_id && r.value == m.value)).ToList();

                // Find mappings to add (present in request but not in DB)
                var mappingsToAdd = requestList.Where(r => !existingMappings.Any(m => m.differentiator_id == r.differentiator_id && m.value == r.value)).ToList();

                if (mappingsToRemove.Any())
                {
                    _context.Differentiator_Mapping.RemoveRange(mappingsToRemove);
                    await _context.SaveChangesAsync();
                }

                if (mappingsToAdd.Any())
                {
                    await _context.Differentiator_Mapping.AddRangeAsync(mappingsToAdd);
                    await _context.SaveChangesAsync();
                }

                return true;
            }
            catch
            {
                return false;
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
                    using (var command = new SqlCommand("get_differentiator_list", connection))
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
                                    material_id = reader.GetInt32(reader.GetOrdinal("material_id")),
                                    material_name = reader.GetString(reader.GetOrdinal("material_name")),
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

                var data = await _context.Differentiator.Where(x => x.delete_flag == false && idsToDelete.Contains(x.id) && x.company_id == token.CompanyId).ToListAsync();

                if (data.Any())
                {
                    foreach (var record in data)
                    {
                        record.delete_flag = true;
                        record.updated_by = token.UserId;
                        record.updated_date = DateTime.Now;
                    }

                    _context.Differentiator.UpdateRange(data);
                    await _context.SaveChangesAsync();

                    aPIResponseDTO.success = true;
                    aPIResponseDTO.message = "Differentiator deleted successfully.";
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
                var data = await _context.Differentiator.Where(x => x.id == id).FirstOrDefaultAsync();

                if (data != null)
                {
                    data.active_flag = data.active_flag == false ? true : false;
                    data.updated_by = token.UserId;
                    data.updated_date = DateTime.Now;
                    _context.Differentiator.Update(data);
                    await _context.SaveChangesAsync();

                    aPIResponseDTO.success = true;
                    aPIResponseDTO.message = "Differentiator " + (data.active_flag == true ? "Activated" : "Inactivated") + " successfully.";
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
        public async Task ImportDifferentiator(string importName, DifferentiatorJsonResponse jsonResponse, ExtractTokenDto token, int materialId)
        {
            if (jsonResponse == null || jsonResponse.Columns.Count == 0 || jsonResponse.Data.Count == 0)
            {
                throw new Exception("No valid data to import.");
            }

            int importId = await _context.Bulk_Import_Details.Where(b => b.name == importName).Select(b => b.id).FirstOrDefaultAsync();

            if (importId == 0)
            {
                throw new Exception("Invalid Import Name");
            }

            var existingDifferentiators = await _context.Differentiator.Where(d => d.material_id == materialId && jsonResponse.Columns.Contains(d.code)).ToDictionaryAsync(d => d.code, d => d.id);

            List<Differentiator> newDifferentiators = new List<Differentiator>();
            List<Differentiator_Mapping> differentiatorMappings = new List<Differentiator_Mapping>();

            foreach (var column in jsonResponse.Columns)
            {
                int differentiatorId;
                if (!existingDifferentiators.TryGetValue(column, out differentiatorId))
                {
                    var newDifferentiator = new Differentiator
                    {
                        code = column,
                        name = column,
                        company_id = token.CompanyId,
                        active_flag = true,
                        delete_flag = false,
                        created_by = token.UserId,
                        created_date = DateTime.Now,
                        material_id = materialId
                    };

                    newDifferentiators.Add(newDifferentiator);
                }
            }

            if (newDifferentiators.Any())
            {
                await _context.Differentiator.AddRangeAsync(newDifferentiators);
                await _context.SaveChangesAsync();

                var newlyAddedDifferentiators = await _context.Differentiator.Where(d => d.material_id == materialId && jsonResponse.Columns.Contains(d.code)).ToDictionaryAsync(d => d.code, d => d.id);

                existingDifferentiators = newlyAddedDifferentiators;
            }

            foreach (var differentiatorData in jsonResponse.Data)
            {
                if (existingDifferentiators.TryGetValue(differentiatorData.DifferentiatorName, out int differentiatorId))
                {
                    differentiatorMappings.AddRange(
                        differentiatorData.Value
                        .Where(value => !string.IsNullOrEmpty(value))
                        .Select(value => new Differentiator_Mapping
                        {
                            differentiator_id = differentiatorId,
                            value = value
                        }));
                }
            }

            if (differentiatorMappings.Any())
            {
                await _context.Differentiator_Mapping.AddRangeAsync(differentiatorMappings);
                await _context.SaveChangesAsync();
            }

            var bulkImportSuccess = await _context.Bulk_Import_Details.FindAsync(importId);
            if (bulkImportSuccess != null)
            {
                bulkImportSuccess.total = jsonResponse.Data.Count;
                bulkImportSuccess.success = jsonResponse.Data.Count;
                bulkImportSuccess.failed = 0;
                bulkImportSuccess.failed_records = null;
                bulkImportSuccess.status = "completed";
                bulkImportSuccess.updated_by = token.UserId;
                bulkImportSuccess.updated_date = DateTime.Now;

                await _context.SaveChangesAsync();
            }
        }
        public async Task<APIResponseDTO> GetDifferentiatorMappingById(int id)
        {
            APIResponseDTO response = new APIResponseDTO();
            try
            {
                var list = await _context.Differentiator_Mapping.Where(x => x.differentiator_id == id).ToListAsync();

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
        public async Task<APIResponseDTO> GetDifferentiatorMappingByMaterialId(int id)
        {
            APIResponseDTO response = new APIResponseDTO();
            try
            {
                var list = await (from a in _context.Differentiator
                                  where a.material_id == id && a.active_flag == true && a.delete_flag == false
                                  select new
                                  {
                                      differentiator_id = a.id,
                                      differentiator_name = a.name,
                                      differentiator_values = _context.Differentiator_Mapping.Where(x => x.differentiator_id == a.id).ToList(),
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