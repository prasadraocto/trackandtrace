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
    public class UsersRepository: IUsersRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        public UsersRepository(ApplicationDbContext context, IMapper mapper, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<APIResponseDTO> Add(UsersDto model, ExtractTokenDto token)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            try
            {
                var emailExists = await _context.Users.AnyAsync(x => x.email.ToLower() == model.email.ToLower() && x.id != model.id && x.delete_flag == false);
                if (emailExists)
                {
                    aPIResponseDTO.message = "Email already exists contact administrator";
                    return aPIResponseDTO;
                }

                var nameExists = await _context.Users.AnyAsync(x => x.name.ToLower() == model.name.ToLower() && x.id != model.id && x.company_id == token.CompanyId && x.delete_flag == false);
                if (nameExists)
                {
                    aPIResponseDTO.message = "Users Name already exists";
                    return aPIResponseDTO;
                }

                var codeExists = await _context.Users.AnyAsync(x => x.code.ToLower() == model.code.ToLower() && x.id != model.id && x.company_id == token.CompanyId && x.delete_flag == false);
                if (codeExists)
                {
                    aPIResponseDTO.message = "Users Code already exists";
                    return aPIResponseDTO;
                }

                var dmo = _mapper.Map<Users>(model);

                if (dmo.id > 0)
                {
                    var existingUsers = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.id == dmo.id && x.company_id == token.CompanyId && x.delete_flag == false);

                    if (existingUsers == null)
                    {
                        aPIResponseDTO.message = "Users details not found";
                        return aPIResponseDTO;
                    }

                    dmo.uuid = existingUsers.uuid;
                    dmo.password = Utils.Encrypt(dmo.password);
                    dmo.company_id = token.CompanyId;
                    dmo.created_by = existingUsers.created_by;
                    dmo.created_date = existingUsers.created_date;
                    dmo.updated_date = DateTime.Now;
                    dmo.updated_by = token.UserId;
                    _context.Users.Update(dmo);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    dmo.uuid = Guid.NewGuid();
                    dmo.password = Utils.Encrypt(dmo.password);
                    dmo.company_id = token.CompanyId;
                    dmo.created_by = token.UserId;
                    dmo.created_date = DateTime.Now;
                    dmo.updated_date = null;
                    await _context.Users.AddAsync(dmo);
                    await _context.SaveChangesAsync();
                }

                aPIResponseDTO.success = true;
                aPIResponseDTO.message = "Users saved successfully";
                return aPIResponseDTO;
            }
            catch (Exception ex)
            {
                aPIResponseDTO.message = $"Failed saving details: {ex.Message}";
                return aPIResponseDTO;
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
                    using (var command = new SqlCommand("get_users_list", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 120;
                        command.Parameters.AddWithValue("@user_id", token.UserId);
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
                                    email = reader.GetString(reader.GetOrdinal("email")),
                                    password = Utils.Decrypt(reader.GetString(reader.GetOrdinal("password"))),
                                    phone = reader.IsDBNull(reader.GetOrdinal("phone")) ? null : reader.GetString(reader.GetOrdinal("phone")),
                                    designation_id = reader.GetInt32(reader.GetOrdinal("designation_id")),
                                    designation_name = reader.GetString(reader.GetOrdinal("designation_name")),
                                    role_id = reader.GetInt32(reader.GetOrdinal("role_id")),
                                    role_name = reader.GetString(reader.GetOrdinal("role_name")),
                                    company_id = reader.GetInt32(reader.GetOrdinal("company_id")),
                                    company_name = reader.GetString(reader.GetOrdinal("company_name")),
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

                var data = await _context.Users.Where(x => x.delete_flag == false && idsToDelete.Contains(x.id)).ToListAsync();

                if (data.Any())
                {
                    foreach (var record in data)
                    {
                        record.delete_flag = true;
                        record.updated_by = token.UserId;
                        record.updated_date = DateTime.Now;
                    }

                    _context.Users.UpdateRange(data);
                    await _context.SaveChangesAsync();

                    aPIResponseDTO.success = true;
                    aPIResponseDTO.message = "Users deleted successfully.";
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
                var data = await _context.Users.Where(x => x.id == id).FirstOrDefaultAsync();

                if (data != null)
                {
                    data.active_flag = data.active_flag == false ? true : false;
                    data.updated_by = token.UserId;
                    data.updated_date = DateTime.Now;
                    _context.Users.Update(data);
                    await _context.SaveChangesAsync();

                    aPIResponseDTO.success = true;
                    aPIResponseDTO.message = "Users " + (data.active_flag == true ? "Activated" : "Inactivated") + " successfully.";
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
        public async Task<APIResponseDTO> GetCompanyDropDownList(ExtractTokenDto token)
        {
            APIResponseDTO response = new APIResponseDTO();
            try
            {
                var list = new List<dynamic>();

                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (var command = new SqlCommand("get_company_drop_down_list", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 120;
                        command.Parameters.AddWithValue("@user_id", token.UserId);
                        await connection.OpenAsync();

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var data = new
                                {
                                    id = reader.GetInt32(reader.GetOrdinal("id")),
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
    }
}