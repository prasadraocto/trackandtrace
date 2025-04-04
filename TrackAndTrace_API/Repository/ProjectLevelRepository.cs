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
using Hangfire.MemoryStorage.Database;
using Azure;
using System.ComponentModel.Design;
using System.Collections.Generic;

namespace TrackAndTrace_API.Repository
{
    public class ProjectLevelRepository : IProjectLevelRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        public ProjectLevelRepository(ApplicationDbContext context, IMapper mapper, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<APIResponseDTO> Add(ProjectLevelDto model, ExtractTokenDto token)
        {
            APIResponseDTO response = new APIResponseDTO();

            try
            {
                var projectLevel = _mapper.Map<Project_Level_Mapping>(model);

                if (projectLevel.id > 0)
                {
                    response = await UpdateProjectLevelAsync(projectLevel, token);
                }
                else
                {
                    response = await AddNewProjectLevelAsync(projectLevel, token);
                }

                if (response.success)
                {
                    response.message = "Project Level saved successfully";
                }
                return response;
            }
            catch (Exception ex)
            {
                return new APIResponseDTO
                {
                    message = $"Failed saving details: {ex.Message}"
                };
            }
        }
        private async Task<APIResponseDTO> UpdateProjectLevelAsync(Project_Level_Mapping projectLevel, ExtractTokenDto token)
        {
            var existingLevel = await _context.Project_Level_Mapping.AsNoTracking().FirstOrDefaultAsync(x => x.id == projectLevel.id);

            if (existingLevel == null)
            {
                return new APIResponseDTO { message = "Project Level details not found" };
            }

            if (await IsCodeOrNameDuplicateAsync(projectLevel, token, excludeId: projectLevel.id))
            {
                return new APIResponseDTO
                {
                    message = $"Project Level Code '{projectLevel.code}' or Name '{projectLevel.name}' already exists!"
                };
            }

            projectLevel.created_by = existingLevel.created_by;
            projectLevel.created_date = existingLevel.created_date;
            projectLevel.updated_by = token.UserId;
            projectLevel.updated_date = DateTime.Now;

            _context.Project_Level_Mapping.Update(projectLevel);
            await _context.SaveChangesAsync();

            return new APIResponseDTO { success = true };
        }
        private async Task<APIResponseDTO> AddNewProjectLevelAsync(Project_Level_Mapping projectLevel, ExtractTokenDto token)
        {
            bool isDuplicate = projectLevel.parent_id == 0
                ? await IsRootLevelDuplicateAsync(projectLevel, token)
                : await IsChildLevelDuplicateAsync(projectLevel, token);

            if (isDuplicate)
            {
                return new APIResponseDTO
                {
                    message = $"Duplicate combination exists for Level Code: {projectLevel.code} and Level Name: {projectLevel.name}"
                };
            }

            projectLevel.created_by = token.UserId;
            projectLevel.created_date = DateTime.Now;

            await _context.Project_Level_Mapping.AddAsync(projectLevel);
            await _context.SaveChangesAsync();

            return new APIResponseDTO { success = true };
        }
        private async Task<bool> IsCodeOrNameDuplicateAsync(Project_Level_Mapping projectLevel, ExtractTokenDto token, int excludeId = 0)
        {
            return await _context.Project_Level_Mapping
                .AnyAsync(x => !x.delete_flag &&
                               x.project_id == projectLevel.project_id &&
                               (x.code == projectLevel.code || x.name == projectLevel.name) &&
                               x.id != excludeId);
        }
        private async Task<bool> IsRootLevelDuplicateAsync(Project_Level_Mapping projectLevel, ExtractTokenDto token)
        {
            return await (from a in _context.Project_Level_Mapping
                          join b in _context.Project on a.project_id equals b.id
                          where !a.delete_flag &&
                                a.code == projectLevel.code &&
                                a.name == projectLevel.name &&
                                a.parent_id == 0 &&
                                a.project_id == projectLevel.project_id &&
                                b.company_id == token.CompanyId
                          select a)
                          .AnyAsync();
        }
        private async Task<bool> IsChildLevelDuplicateAsync(Project_Level_Mapping projectLevel, ExtractTokenDto token)
        {
            return await (from a in _context.Project_Level_Mapping
                          join b in _context.Project on a.project_id equals b.id
                          where !a.delete_flag &&
                                a.code == projectLevel.code &&
                                a.name == projectLevel.name &&
                                a.project_id == projectLevel.project_id &&
                                (a.parent_id == projectLevel.parent_id || a.id == projectLevel.parent_id) &&
                                b.company_id == token.CompanyId
                          select a)
                          .AnyAsync();
        }
        public async Task<APIResponseDTO> GetProjectLevelDetailsById(int projectId)
        {
            APIResponseDTO response = new APIResponseDTO();
            try
            {
                // First, get all records for the project
                var allLevels = await _context.Project_Level_Mapping.Where(x => x.project_id == projectId && !x.delete_flag)
                    .Select(x => new ProjectLevelResponseDTO
                    {
                        id = x.id,
                        parent_id = x.parent_id,
                        code = x.code,
                        name = x.name,
                        project_id = x.project_id,
                        children = new List<ProjectLevelResponseDTO>()
                    }).ToListAsync();

                // Build hierarchical structure starting from root level (parent_id = 0)
                var hierarchicalResult = BuildHierarchy(allLevels);

                response.success = true;
                response.message = hierarchicalResult.Any() ? "Data Fetched Successfully" : "No Records Found";
                response.data = hierarchicalResult;
            }
            catch (Exception ex)
            {
                response.success = false;
                response.message = ex.Message;
            }
            return response;
        }
        private List<ProjectLevelResponseDTO> BuildHierarchy(List<ProjectLevelResponseDTO> allLevels, int parentId = 0)
        {
            var items = allLevels.Where(x => x.parent_id == parentId).ToList();
            foreach (var item in items)
            {
                item.children = BuildHierarchy(allLevels, item.id);
            }
            return items;
        }
        public async Task<APIResponseDTO> Delete(int id, ExtractTokenDto token)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            try
            {
                var data = await _context.Project_Level_Mapping.Where(x => x.delete_flag == false && x.id == id).FirstOrDefaultAsync();

                if (data != null)
                {
                    data.delete_flag = true;
                    data.updated_by = token.UserId;
                    data.updated_date = DateTime.Now;

                    _context.Project_Level_Mapping.Update(data);
                    await _context.SaveChangesAsync();

                    aPIResponseDTO.success = true;
                    aPIResponseDTO.message = "Project Level deleted successfully.";
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
    }
}