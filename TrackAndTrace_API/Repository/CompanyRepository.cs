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
    public class CompanyRepository : ICompanyRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        public CompanyRepository(ApplicationDbContext context, IMapper mapper, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<APIResponseDTO> Add(CompanyDto model)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            try
            {
                var nameExists = await _context.Company.AnyAsync(x => x.name.ToLower() == model.name.ToLower() && x.id != model.id && x.delete_flag == false);
                if (nameExists)
                {
                    aPIResponseDTO.message = "Company Name already exists";
                    return aPIResponseDTO;
                }

                var codeExists = await _context.Company.AnyAsync(x => x.code.ToLower() == model.code.ToLower() && x.id != model.id && x.delete_flag == false);
                if (codeExists)
                {
                    aPIResponseDTO.message = "Company Code already exists";
                    return aPIResponseDTO;
                }

                var dmo = _mapper.Map<Company>(model);

                if (dmo.id > 0)
                {
                    //var existingEntry = _context.ChangeTracker.Entries<Company>().FirstOrDefault(e => e.Entity.id == dmo.id);
                    //if (existingEntry != null)
                    //{
                    //    _context.Entry(existingEntry.Entity).State = EntityState.Detached;
                    //}

                    var existingCompany = await _context.Company.AsNoTracking().FirstOrDefaultAsync(x => x.id == dmo.id && x.delete_flag == false);

                    if (existingCompany == null)
                    {
                        aPIResponseDTO.message = "Company details not found";
                        return aPIResponseDTO;
                    }

                    dmo.created_date = existingCompany.created_date;
                    dmo.updated_date = DateTime.Now;
                    _context.Company.Update(dmo);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    dmo.created_date = DateTime.Now;
                    dmo.updated_date = null;
                    await _context.Company.AddAsync(dmo);
                    await _context.SaveChangesAsync();
                }

                aPIResponseDTO.success = true;
                aPIResponseDTO.message = "Company saved successfully";
                return aPIResponseDTO;
            }
            catch (Exception ex)
            {
                aPIResponseDTO.message = $"Failed saving details: {ex.Message}";
                return aPIResponseDTO;
            }
        }
        public async Task<APIResponseDTO> GetList(CommonRequestDto request)
        {
            APIResponseDTO response = new APIResponseDTO();
            try
            {
                int totalCount = 0;
                var list = new List<dynamic>();

                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (var command = new SqlCommand("get_company_list", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 120;
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
                                    phone = reader.IsDBNull(reader.GetOrdinal("phone")) ? null : reader.GetString(reader.GetOrdinal("phone")),
                                    logo = reader.IsDBNull(reader.GetOrdinal("logo")) ? null : reader.GetString(reader.GetOrdinal("logo")),
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
        public async Task<APIResponseDTO> Delete(string ids)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            try
            {
                var idsToDelete = ids.Split(',').Select(id => int.Parse(id)).ToList();

                var data = await _context.Company.Where(x => x.delete_flag == false && idsToDelete.Contains(x.id)).ToListAsync();

                if (data.Any())
                {
                    foreach (var record in data)
                    {
                        record.delete_flag = true;
                        record.updated_date = DateTime.Now;
                    }

                    _context.Company.UpdateRange(data);
                    await _context.SaveChangesAsync();

                    aPIResponseDTO.success = true;
                    aPIResponseDTO.message = "Company deleted successfully.";
                }
                else
                {
                    aPIResponseDTO.message = "No matching menus found to delete.";
                }
            }
            catch (Exception ex)
            {
                aPIResponseDTO.message = "Failed deleting details";
                return aPIResponseDTO;
            }

            return aPIResponseDTO;
        }
        public async Task<APIResponseDTO> ActiveInactive(int id)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            try
            {
                var data = await _context.Company.Where(x => x.id == id).FirstOrDefaultAsync();

                if (data != null)
                {
                    data.active_flag = data.active_flag == false ? true : false;
                    _context.Company.Update(data);
                    await _context.SaveChangesAsync();

                    aPIResponseDTO.success = true;
                    aPIResponseDTO.message = "Company " + (data.active_flag == true ? "Activated" : "Inactivated") + " successfully.";
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
    }
}