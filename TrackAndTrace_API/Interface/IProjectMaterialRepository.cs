using TrackAndTrace_API.Models.RequestModel;
using TrackAndTrace_API.Models.ResponseModel;

namespace TrackAndTrace_API.Interface
{
    public interface IProjectMaterialRepository
    {
        Task<APIResponseDTO> GetList(int id, CommonRequestDto? request, ExtractTokenDto token);
    }
}