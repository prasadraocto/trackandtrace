using TrackAndTrace_API.Models.DBModel;
using TrackAndTrace_API.Models.RequestModel;
using TrackAndTrace_API.Models.ResponseModel;

namespace TrackAndTrace_API.Interface
{
    public interface IDifferentiatorRepository
    {
        Task<APIResponseDTO> Add(DifferentiatorDto model, ExtractTokenDto token);
        Task<APIResponseDTO> GetList(CommonRequestDto? request, ExtractTokenDto token);
        Task<APIResponseDTO> Delete(string id, ExtractTokenDto token);
        Task<APIResponseDTO> ActiveInactive(int id, ExtractTokenDto token);
        Task ImportDifferentiator(string importName, DifferentiatorJsonResponse jsonResponse, ExtractTokenDto token, int materialId);
        Task<APIResponseDTO> GetDifferentiatorMappingById(int id);
        Task<APIResponseDTO> GetDifferentiatorMappingByMaterialId(int id);
    }
}
