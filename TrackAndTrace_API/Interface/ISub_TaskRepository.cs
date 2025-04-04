using TrackAndTrace_API.Models.RequestModel;
using TrackAndTrace_API.Models.ResponseModel;

namespace TrackAndTrace_API.Interface
{
    public interface ISub_TaskRepository
    {
        Task<APIResponseDTO> Add(Sub_TaskDto model, ExtractTokenDto token);
        Task<APIResponseDTO> GetList(CommonRequestDto? request, ExtractTokenDto token);
        Task<APIResponseDTO> Delete(string id, ExtractTokenDto token);
        Task<APIResponseDTO> ActiveInactive(int id, ExtractTokenDto token);
        Task<APIResponseDTO> GetProjectDropdownList(int activity_id, int task_id, ExtractTokenDto token);
        Task<APIResponseDTO> GetActivityTaskSubTaskDropDownList(int activity_id, int task_id, int project_id, ExtractTokenDto token);
        Task<APIResponseDTO> SaveSub_TaskProjectMapping(Sub_Task_Project_MappingDto dto, ExtractTokenDto token);
        Task<APIResponseDTO> GetSubTaskProjectmappingList(int id, CommonRequestDto request, ExtractTokenDto token);
    }
}