using TrackAndTrace_API.Models.DBModel;
using TrackAndTrace_API.Models.RequestModel;
using TrackAndTrace_API.Models.ResponseModel;
using Microsoft.AspNetCore.Mvc;

namespace TrackAndTrace_API.Interface
{
    public interface IConfigurationRepository
    {
        Task<APIResponseDTO> AddMenu(MenuDto menu);
        Task<APIResponseDTO> GetMenuList(CommonRequestDto? request);
        Task<APIResponseDTO> DeleteMenu(string id);
        Task<APIResponseDTO> GetMenuDropdownList();
        Task<APIResponseDTO> AddPage(PageDto page);
        Task<APIResponseDTO> GetPageList(CommonRequestDto? request);
        Task<APIResponseDTO> DeletePage(string id);
        Task<APIResponseDTO> AddMenuPage(MenuPageMappingDto model);
        Task<APIResponseDTO> GetMenuPageList(CommonRequestDto request);
        Task<APIResponseDTO> DeleteMenuPage(int id);
        Task<APIResponseDTO> GetPageDropdownList(int menu_id);
        Task<APIResponseDTO> AddCompanyRoleMenuPage(CompanyRoleMenuPageMappingDto model);
        Task<APIResponseDTO> GetCompanyRoleMenuPageList(CommonRequestDto request);
        Task<APIResponseDTO> DeleteCompanyRoleMenuPage(int id);
    }
}
