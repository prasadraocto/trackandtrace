using TrackAndTrace_API.Models.RequestModel;
using TrackAndTrace_API.Models.ResponseModel;

namespace TrackAndTrace_API.Interface
{
    public interface IActivityMilestoneRepository
    {
        Task<APIResponseDTO> Add(ActivityMilestoneDto model, ExtractTokenDto token);
        Task<APIResponseDTO> GetList(int project_id, CommonRequestDto? request, ExtractTokenDto token);
        Task<APIResponseDTO> Delete(string id, ExtractTokenDto token);
        Task<APIResponseDTO> ActiveInactive(int id, ExtractTokenDto token);
        Task<APIResponseDTO> GetActivityMilestoneMapping(int id);
        Task<APIResponseDTO> GetActivityDropdownMilestoneList(int project_id, ExtractTokenDto token);
    }
}