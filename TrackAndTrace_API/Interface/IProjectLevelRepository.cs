using TrackAndTrace_API.Models.RequestModel;
using TrackAndTrace_API.Models.ResponseModel;

namespace TrackAndTrace_API.Interface
{
    public interface IProjectLevelRepository
    {
        Task<APIResponseDTO> Add(ProjectLevelDto model, ExtractTokenDto token);
        Task<APIResponseDTO> GetProjectLevelDetailsById(int projectId);
        Task<APIResponseDTO> Delete(int id, ExtractTokenDto token);
    }
}
