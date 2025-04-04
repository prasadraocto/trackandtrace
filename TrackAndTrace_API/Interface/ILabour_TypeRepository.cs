using TrackAndTrace_API.Models.RequestModel;
using TrackAndTrace_API.Models.ResponseModel;

namespace TrackAndTrace_API.Interface
{
    public interface ILabour_TypeRepository
    {
        Task<APIResponseDTO> Add(Labour_TypeDto model, ExtractTokenDto token);
        Task<APIResponseDTO> GetList(CommonRequestDto? request, ExtractTokenDto token);
        Task<APIResponseDTO> Delete(string id, ExtractTokenDto token);
        Task<APIResponseDTO> ActiveInactive(int id, ExtractTokenDto token);
    }
}
