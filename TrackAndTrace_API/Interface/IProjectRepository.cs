using TrackAndTrace_API.Models.RequestModel;
using TrackAndTrace_API.Models.ResponseModel;

namespace TrackAndTrace_API.Interface
{
    public interface IProjectRepository
    {
        Task<APIResponseDTO> Add(ProjectDto model, ExtractTokenDto token);
        Task<APIResponseDTO> GetList(CommonRequestDto? request, ExtractTokenDto token);
        Task<APIResponseDTO> Delete(string id, ExtractTokenDto token);
        Task<APIResponseDTO> ActiveInactive(int id, ExtractTokenDto token);
        Task<APIResponseDTO> GetProjectUserMappingById(int id);
        Task<APIResponseDTO> GetUsersDropdownList(ExtractTokenDto token);
    }
}
