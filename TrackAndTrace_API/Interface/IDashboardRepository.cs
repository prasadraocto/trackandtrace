using TrackAndTrace_API.Models.RequestModel;
using TrackAndTrace_API.Models.ResponseModel;

namespace TrackAndTrace_API.Interface
{
    public interface IDashboardRepository
    {
        Task<APIResponseDTO> GetList(int project_id, ExtractTokenDto token);
    }
}
