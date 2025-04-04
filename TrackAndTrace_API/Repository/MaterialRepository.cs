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
    public class MaterialRepository: IMaterialRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        public MaterialRepository(ApplicationDbContext context, IMapper mapper, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<APIResponseDTO> Add(MaterialDto model, ExtractTokenDto token)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            try
            {
                var nameExists = await _context.Material.AnyAsync(x => x.name.ToLower() == model.name.ToLower() && x.id != model.id && x.company_id == token.CompanyId);
                if (nameExists)
                {
                    aPIResponseDTO.message = "Material Name already exists";
                    return aPIResponseDTO;
                }

                var codeExists = await _context.Material.AnyAsync(x => x.code.ToLower() == model.code.ToLower() && x.id != model.id && x.company_id == token.CompanyId);
                if (codeExists)
                {
                    aPIResponseDTO.message = "Material Code already exists";
                    return aPIResponseDTO;
                }

                var dmo = _mapper.Map<Material>(model);

                if (dmo.id > 0)
                {
                    var existingMaterial = await _context.Material.AsNoTracking().FirstOrDefaultAsync(x => x.id == dmo.id);

                    if (existingMaterial == null)
                    {
                        aPIResponseDTO.message = "Material details not found";
                        return aPIResponseDTO;
                    }

                    dmo.company_id = token.CompanyId;
                    dmo.created_by = existingMaterial.created_by;
                    dmo.created_date = existingMaterial.created_date;
                    dmo.updated_by = token.UserId;
                    dmo.updated_date = DateTime.Now;
                    _context.Material.Update(dmo);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    dmo.company_id = token.CompanyId;
                    dmo.created_by = token.UserId;
                    dmo.created_date = DateTime.Now;
                    dmo.updated_date = null;
                    await _context.Material.AddAsync(dmo);
                    await _context.SaveChangesAsync();
                }

                foreach (var mapping in model.material_brand_mapping)
                {
                    mapping.material_id = dmo.id;
                }

                var response = await SaveMaterialBrandMapping(model.material_brand_mapping);


                aPIResponseDTO.success = true;
                aPIResponseDTO.message = "Material saved successfully";
                return aPIResponseDTO;
            }
            catch (Exception ex)
            {
                aPIResponseDTO.message = $"Failed saving details: {ex.Message}";
                return aPIResponseDTO;
            }
        }
        public async Task<bool> SaveMaterialBrandMapping(List<Material_Brand_Mapping> requestList)
        {
            try
            {
                var materialIds = requestList.Select(r => r.material_id).ToList();

                var existingMappings = await _context.Material_Brand_Mapping.Where(m => materialIds.Contains(m.material_id)).ToListAsync();

                // Remove mappings not in the request list
                _context.Material_Brand_Mapping.RemoveRange(existingMappings.Where(m => !requestList.Any(r => r.material_id == m.material_id && r.brand_id == m.brand_id)));
                await _context.SaveChangesAsync();

                // Add new mappings that don't exist
                var newMappings = requestList.Where(r => !existingMappings.Any(m => m.material_id == r.material_id && m.brand_id == r.brand_id)).ToList();

                if (newMappings.Any())
                    await _context.Material_Brand_Mapping.AddRangeAsync(newMappings);

                await _context.SaveChangesAsync();
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
                    using (var command = new SqlCommand("get_material_list", connection))
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
                                    description = reader.GetString(reader.GetOrdinal("description")),
                                    cost = reader.GetDecimal(reader.GetOrdinal("cost")),
                                    uom_id = reader.GetInt32(reader.GetOrdinal("uom_id")),
                                    uom_name = reader.GetString(reader.GetOrdinal("uom_name")),
                                    type = reader.GetString(reader.GetOrdinal("type")),
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

                var data = await _context.Material.Where(x => x.delete_flag == false && idsToDelete.Contains(x.id) && x.company_id == token.CompanyId).ToListAsync();

                if (data.Any())
                {
                    foreach (var record in data)
                    {
                        record.delete_flag = true;
                        record.updated_by = token.UserId;
                        record.updated_date = DateTime.Now;
                    }

                    _context.Material.UpdateRange(data);
                    await _context.SaveChangesAsync();

                    aPIResponseDTO.success = true;
                    aPIResponseDTO.message = "Material deleted successfully.";
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
                var data = await _context.Material.Where(x => x.id == id).FirstOrDefaultAsync();

                if (data != null)
                {
                    data.active_flag = data.active_flag == false ? true : false;
                    data.updated_by = token.UserId;
                    data.updated_date = DateTime.Now;
                    _context.Material.Update(data);
                    await _context.SaveChangesAsync();

                    aPIResponseDTO.success = true;
                    aPIResponseDTO.message = "Material " + (data.active_flag == true ? "Activated" : "Inactivated") + " successfully.";
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
        public async Task<APIResponseDTO> GetBrandMappingById(int id)
        {
            APIResponseDTO response = new APIResponseDTO();
            try
            {
                var list = await (from a in _context.Material_Brand_Mapping
                                  join b in _context.Brand on a.brand_id equals b.id
                                  where a.material_id == id && b.active_flag == true && b.delete_flag == false
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
        public async Task<APIResponseDTO> GetMaterialDropdown(ExtractTokenDto token)
        {
            APIResponseDTO response = new APIResponseDTO();
            try
            {
                var list = await (from a in _context.Material
                                  join b in _context.UOM on a.uom_id equals b.id
                                  where a.company_id == token.CompanyId && a.active_flag == true && 
                                        a.delete_flag == false && b.delete_flag == false
                                  select new
                                  {
                                      a.id,
                                      a.code,
                                      a.name,
                                      a.type,
                                      a.uom_id,
                                      uom_name = b.name
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