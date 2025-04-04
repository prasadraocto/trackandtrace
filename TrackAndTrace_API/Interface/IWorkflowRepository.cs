using TrackAndTrace_API.Models.RequestModel;
using TrackAndTrace_API.Models.ResponseModel;

namespace TrackAndTrace_API.Interface
{
    public interface IWorkflowRepository
    {
        Task<APIResponseDTO> Add(WorkflowDto model, ExtractTokenDto token);
        Task<APIResponseDTO> GetList(CommonRequestDto? request, ExtractTokenDto token);
        Task<APIResponseDTO> Delete(int work_flow_id, int project_id, ExtractTokenDto token);
        Task<APIResponseDTO> GetWFProjectUserMappingById(int work_flow_id, int project_id);
        Task<APIResponseDTO> GetWFPendingRequest(CommonRequestDto request, ExtractTokenDto token);
    }
}
