using TrackAndTrace_API.Models.DBModel;
using TrackAndTrace_API.Models.RequestModel;
using TrackAndTrace_API.Models.ResponseModel;

namespace TrackAndTrace_API.Interface
{
    public interface IIndentRepository
    {
        Task<APIResponseDTO> Add(IndentDto model, ExtractTokenDto token);
        Task<APIResponseDTO> GetList(int project_id, CommonRequestDto? request, ExtractTokenDto token);
        Task<APIResponseDTO> GetIndentDetails(int id, ExtractTokenDto token);
        Task<APIResponseDTO> UpdateIndentRequestStatus(int request_id, string status, ExtractTokenDto token);
        Task<APIResponseDTO> UpdateIndentMaterialDetails(UpdateIndentMaterialDto model);
    }
}