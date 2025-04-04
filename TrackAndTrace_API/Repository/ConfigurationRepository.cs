using TrackAndTrace_API.Interface;
using TrackAndTrace_API.Models;
using TrackAndTrace_API.Models.DBModel;
using TrackAndTrace_API.Models.RequestModel;
using TrackAndTrace_API.Models.ResponseModel;
using AutoMapper;
using Azure.Core;
using Azure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.Design;
using System.Data;
using System.Linq.Expressions;

namespace TrackAndTrace_API.Repository
{
    public class ConfigurationRepository : IConfigurationRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        public ConfigurationRepository(ApplicationDbContext context, IConfiguration Configuration, IMapper mapper)
        {
            _context = context;
            _configuration = Configuration;
            _mapper = mapper;
        }

        public async Task<APIResponseDTO> AddMenu(MenuDto menu)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            try
            {
                var exists = await _context.Menu.Where(x => x.name.ToLower() == menu.name.ToLower() && x.id != menu.id).FirstOrDefaultAsync();

                if (exists != null)
                {
                    aPIResponseDTO.message = "Menu already exists";
                    return aPIResponseDTO;
                }

                var dmo = _mapper.Map<Menu>(menu);
                if (dmo.id > 0)
                {
                    var updateMenu = await _context.Menu.Where(x => x.id == dmo.id).FirstOrDefaultAsync();

                    if (updateMenu != null)
                    {
                        updateMenu.name = menu.name;
                        updateMenu.description = menu.description;
                        updateMenu.icon = menu.icon;
                        updateMenu.updated_date = DateTime.Now;
                        _context.Update(updateMenu);
                        _context.SaveChanges();
                    }
                }
                else
                {
                    dmo.created_date = DateTime.Now;
                    dmo.updated_date = DateTime.Now;
                    await _context.AddAsync(dmo);
                    await _context.SaveChangesAsync();
                }

                if (dmo.id > 0)
                {
                    aPIResponseDTO.success = true;
                    aPIResponseDTO.message = "Menu saved successfully";
                    return aPIResponseDTO;
                }
            }
            catch (Exception ex)
            {
                aPIResponseDTO.message = "Failed saving details";
                return aPIResponseDTO;
            }

            return aPIResponseDTO;
        }
        public async Task<APIResponseDTO> GetMenuList(CommonRequestDto request)
        {
            APIResponseDTO response = new APIResponseDTO();
            try
            {
                int totalCount = 0;

                var menuList = new List<dynamic>();

                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (var command = new SqlCommand("get_menu_list", connection))
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
                                var menu = new
                                {
                                    id = reader.GetInt32(reader.GetOrdinal("id")),
                                    name = reader.GetString(reader.GetOrdinal("name")),
                                    description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString(reader.GetOrdinal("description")),
                                    icon = reader.IsDBNull(reader.GetOrdinal("icon")) ? null : reader.GetString(reader.GetOrdinal("icon"))
                                };

                                menuList.Add(menu);
                            }
                        }

                        // Get the total count from the output parameter
                        totalCount = (int)command.Parameters["@total_count"].Value;
                    }
                }

                response.success = true;
                response.message = menuList.Count > 0 ? "Data Fetched Successfully" : "No Records Found";
                response.data = menuList;
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
        public async Task<APIResponseDTO> DeleteMenu(string ids)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            try
            {
                var idsToDelete = ids.Split(',').Select(id => int.Parse(id)).ToList();

                var menus = await _context.Menu.Where(menu => idsToDelete.Contains(menu.id)).ToListAsync();

                if (menus.Any())
                {
                    var menuPageMappings = await _context.Menu_Page_Mapping.Where(mapping => idsToDelete.Contains(mapping.menu_id)).ToListAsync();

                    _context.Menu_Page_Mapping.RemoveRange(menuPageMappings);

                    await _context.SaveChangesAsync();

                    _context.Menu.RemoveRange(menus);
                    await _context.SaveChangesAsync();

                    aPIResponseDTO.success = true;
                    aPIResponseDTO.message = "Menus and their associated pages deleted successfully.";
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
        public async Task<APIResponseDTO> GetMenuDropdownList()
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            try
            {
                var menuList = await (from a in _context.Menu
                                      select new
                                      {
                                          a.id,
                                          a.name,
                                      }).ToListAsync();

                if (menuList.Count > 0)
                {
                    aPIResponseDTO.success = true;
                    aPIResponseDTO.message = "Menu data fetched successfully";
                    aPIResponseDTO.data = menuList;
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
        public async Task<APIResponseDTO> AddPage(PageDto page)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            try
            {
                var existsByName = await _context.Page.Where(x => x.name == page.name && x.id != page.id).FirstOrDefaultAsync();

                var existsByUrl = await _context.Page.Where(x => x.url == page.url && x.id != page.id).FirstOrDefaultAsync();

                if (existsByName != null)
                {
                    aPIResponseDTO.message = "Page name already exists";
                    return aPIResponseDTO;
                }

                if (existsByUrl != null)
                {
                    aPIResponseDTO.message = "Page URL already exists";
                    return aPIResponseDTO;
                }

                var dmo = _mapper.Map<Page>(page);
                if (dmo.id > 0)
                {
                    var updatePage = await _context.Page.Where(x => x.id == dmo.id).FirstOrDefaultAsync();

                    if (updatePage != null)
                    {
                        updatePage.name = page.name;
                        updatePage.description = page.description;
                        updatePage.url = page.url;
                        updatePage.updated_date = DateTime.Now;
                        _context.Update(updatePage);
                        _context.SaveChanges();
                    }
                }
                else
                {
                    dmo.created_date = DateTime.Now;
                    dmo.updated_date = DateTime.Now;
                    await _context.AddAsync(dmo);
                    await _context.SaveChangesAsync();
                }

                if (dmo.id > 0)
                {
                    aPIResponseDTO.success = true;
                    aPIResponseDTO.message = "Page saved successfully";
                    return aPIResponseDTO;
                }
            }
            catch (Exception ex)
            {
                aPIResponseDTO.message = "Failed saving details";
                return aPIResponseDTO;
            }

            return aPIResponseDTO;
        }
        public async Task<APIResponseDTO> GetPageList(CommonRequestDto request)
        {
            APIResponseDTO response = new APIResponseDTO();

            try
            {
                int totalCount = 0;

                var pageList = new List<dynamic>();

                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (var command = new SqlCommand("get_page_list", connection))
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
                                var page = new
                                {
                                    id = reader.GetInt32(reader.GetOrdinal("id")),
                                    name = reader.GetString(reader.GetOrdinal("name")),
                                    description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString(reader.GetOrdinal("description")),
                                    url = reader.IsDBNull(reader.GetOrdinal("url")) ? null : reader.GetString(reader.GetOrdinal("url"))
                                };

                                pageList.Add(page);
                            }
                        }

                        // Get the total count from the output parameter
                        totalCount = (int)command.Parameters["@total_count"].Value;
                    }
                }

                response.success = true;
                response.message = pageList.Count > 0 ? "Data Fetched Successfully" : "No Records Found";
                response.data = pageList;
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
        public async Task<APIResponseDTO> DeletePage(string ids)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            var idsToDelete = ids.Split(',').Select(id => int.Parse(id)).ToList();

            var pages = await _context.Page.Where(page => idsToDelete.Contains(page.id)).ToListAsync();

            if (pages.Any())
            {
                var menuPageMappings = await _context.Menu_Page_Mapping.Where(mapping => idsToDelete.Contains(mapping.page_id)).ToListAsync();

                _context.Menu_Page_Mapping.RemoveRange(menuPageMappings);
                await _context.SaveChangesAsync();

                _context.Page.RemoveRange(pages);
                await _context.SaveChangesAsync();

                aPIResponseDTO.success = true;
                aPIResponseDTO.message = "Pages and their associated menu mappings deleted successfully.";
            }
            else
            {
                aPIResponseDTO.message = "No matching pages found to delete.";
            }

            return aPIResponseDTO;
        }
        public async Task<APIResponseDTO> AddMenuPage(MenuPageMappingDto model)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            try
            {
                var currentPageIds = model.pages.Select(p => p.page_id).ToList();

                var existingMappings = _context.Menu_Page_Mapping.Where(x => x.menu_id == model.menu_id).ToList();

                var pagesToDelete = existingMappings.Where(x => !currentPageIds.Contains(x.page_id)).ToList();

                if (pagesToDelete.Any())
                {
                    _context.Menu_Page_Mapping.RemoveRange(pagesToDelete);
                    await _context.SaveChangesAsync();
                }

                foreach (var existingMapping in existingMappings)
                {
                    var newPage = model.pages.FirstOrDefault(p => p.page_id == existingMapping.page_id);
                    if (newPage != null)
                    {
                        if (existingMapping.mapping_order != newPage.mapping_order)
                        {
                            existingMapping.mapping_order = newPage.mapping_order;

                            _context.Menu_Page_Mapping.UpdateRange(existingMapping);
                            await _context.SaveChangesAsync();
                        }
                    }
                }

                var newItems = model.pages.Where(p => !existingMappings.Any(em => em.page_id == p.page_id))
                    .Select(item => new MenuPageMapping
                    {
                        menu_id = model.menu_id,
                        page_id = item.page_id,
                        mapping_order = item.mapping_order,
                        created_date = DateTime.Now
                    }).ToList();

                if (newItems.Any())
                {
                    await _context.AddRangeAsync(newItems);
                    await _context.SaveChangesAsync();
                }

                aPIResponseDTO.success = true;
                aPIResponseDTO.message = "Menu Page mapping saved successfully";
                return aPIResponseDTO;
            }
            catch (Exception ex)
            {
                aPIResponseDTO.message = "Failed to save data: " + ex.Message;
                return aPIResponseDTO;
            }
        }
        public async Task<APIResponseDTO> GetMenuPageList(CommonRequestDto request)
        {
            APIResponseDTO response = new APIResponseDTO();

            try
            {
                int totalCount = 0;

                var menupageList = new List<dynamic>();

                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (var command = new SqlCommand("get_menu_page_list", connection))
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
                                var page = new
                                {
                                    id = reader.GetInt32(reader.GetOrdinal("id")),
                                    menu_id = reader.GetInt32(reader.GetOrdinal("menu_id")),
                                    menu_name = reader.GetString(reader.GetOrdinal("menu_name")),
                                    page_id = reader.GetInt32(reader.GetOrdinal("page_id")),
                                    page_name = reader.GetString(reader.GetOrdinal("page_name")),
                                    url = reader.GetString(reader.GetOrdinal("url")),
                                    mapping_order = reader.GetInt32(reader.GetOrdinal("mapping_order"))
                                };

                                menupageList.Add(page);
                            }
                        }

                        // Get the total count from the output parameter
                        totalCount = (int)command.Parameters["@total_count"].Value;
                    }
                }

                response.success = true;
                response.message = menupageList.Count > 0 ? "Data Fetched Successfully" : "No Records Found";
                response.data = menupageList;
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
        public async Task<APIResponseDTO> DeleteMenuPage(int id)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            var data = await _context.Menu_Page_Mapping.FindAsync(id);

            if (data != null)
            {
                _context.Menu_Page_Mapping.Remove(data);
                await _context.SaveChangesAsync();
            }

            aPIResponseDTO.success = true;
            aPIResponseDTO.message = "Menu Page removed successfully";

            return aPIResponseDTO;
        }
        public async Task<APIResponseDTO> GetPageDropdownList(int menu_id)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            try
            {
                var pageList = await (from a in _context.Page
                                      join mpm in _context.Menu_Page_Mapping on a.id equals mpm.page_id into mapping
                                      from m in mapping.DefaultIfEmpty()
                                      where m == null || m.menu_id == menu_id
                                      select new
                                      {
                                          a.id,
                                          a.name,
                                          isSelected = m != null && m.menu_id == menu_id,
                                          mapping_order = m != null ? m.mapping_order : 0
                                      }).ToListAsync();

                if (pageList.Count > 0)
                {
                    aPIResponseDTO.success = true;
                    aPIResponseDTO.message = "Page data fetched successfully";
                    aPIResponseDTO.data = pageList;
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
        public async Task<APIResponseDTO> AddCompanyRoleMenuPage(CompanyRoleMenuPageMappingDto model)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();
            List<string> errorMessages = new List<string>();

            try
            {
                var currentPageIds = model.pages.Select(p => p.page_id).ToList();

                var existingMappings = _context.Company_Role_Menu_Page_Mapping.Where(x => x.company_id == model.company_id && x.menu_id == model.menu_id).ToList();

                var pagesToDelete = existingMappings.Where(x => !currentPageIds.Contains(x.page_id)).ToList();

                if (pagesToDelete.Any())
                {
                    _context.Company_Role_Menu_Page_Mapping.RemoveRange(pagesToDelete);
                    await _context.SaveChangesAsync();
                }

                foreach (var existingMapping in existingMappings)
                {
                    var newPage = model.pages.FirstOrDefault(p => p.page_id == existingMapping.page_id);
                    if (newPage != null)
                    {
                        if (existingMapping.mapping_order != newPage.mapping_order)
                        {
                            existingMapping.mapping_order = newPage.mapping_order;
                            _context.Company_Role_Menu_Page_Mapping.UpdateRange(existingMapping);
                            await _context.SaveChangesAsync();
                        }
                    }
                }

                var newItems = model.pages.Where(item =>
                {
                    var duplicateMapping = _context.Company_Role_Menu_Page_Mapping.FirstOrDefault(x => x.company_id == model.company_id && x.menu_id == model.menu_id && x.page_id == item.page_id && x.role_id != model.role_id);

                    if (duplicateMapping != null)
                    {
                        errorMessages.Add($"\nThe page with ID {item.page_id} is already mapped to another role in the same menu.");
                        return false;
                    }

                    return true;
                }).Select(item => new CompanyRoleMenuPageMapping
                {
                    company_id = model.company_id,
                    role_id = model.role_id,
                    menu_id = model.menu_id,
                    page_id = item.page_id,
                    mapping_order = item.mapping_order,
                    created_date = DateTime.Now
                }).ToList();

                if (newItems.Any())
                {
                    await _context.AddRangeAsync(newItems);
                    await _context.SaveChangesAsync();

                    aPIResponseDTO.success = true;
                    errorMessages.Insert(0, "Company Role Menu Page mapping saved successfully");
                    aPIResponseDTO.message = string.Join(" ", errorMessages);
                }

                return aPIResponseDTO;
            }
            catch (Exception ex)
            {
                aPIResponseDTO.message = "Failed to save data: " + ex.Message;
                return aPIResponseDTO;
            }
        }
        public async Task<APIResponseDTO> GetCompanyRoleMenuPageList(CommonRequestDto request)
        {
            APIResponseDTO response = new APIResponseDTO();

            try
            {
                int totalCount = 0;

                var menupageList = new List<dynamic>();

                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (var command = new SqlCommand("get_company_role_menu_page_list", connection))
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
                                var page = new
                                {
                                    id = reader.GetInt32(reader.GetOrdinal("id")),
                                    company_id = reader.GetInt32(reader.GetOrdinal("company_id")),
                                    company_name = reader.GetString(reader.GetOrdinal("company_name")),
                                    role_id = reader.GetInt32(reader.GetOrdinal("role_id")),
                                    role_name = reader.GetString(reader.GetOrdinal("role_name")),
                                    menu_id = reader.GetInt32(reader.GetOrdinal("menu_id")),
                                    menu_name = reader.GetString(reader.GetOrdinal("menu_name")),
                                    page_id = reader.GetInt32(reader.GetOrdinal("page_id")),
                                    page_name = reader.GetString(reader.GetOrdinal("page_name")),
                                    mapping_order = reader.GetInt32(reader.GetOrdinal("mapping_order"))
                                };

                                menupageList.Add(page);
                            }
                        }

                        // Get the total count from the output parameter
                        totalCount = (int)command.Parameters["@total_count"].Value;
                    }
                }

                response.success = true;
                response.message = menupageList.Count > 0 ? "Data Fetched Successfully" : "No Records Found";
                response.data = menupageList;
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
        public async Task<APIResponseDTO> DeleteCompanyRoleMenuPage(int id)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            var data = await _context.Company_Role_Menu_Page_Mapping.FindAsync(id);

            if (data != null)
            {
                _context.Company_Role_Menu_Page_Mapping.Remove(data);
                await _context.SaveChangesAsync();
            }

            aPIResponseDTO.success = true;
            aPIResponseDTO.message = "Menu Page removed successfully";

            return aPIResponseDTO;
        }
    }
}