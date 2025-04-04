using TrackAndTrace_API.Models.DBModel;
using TrackAndTrace_API.Models.RequestModel;
using TrackAndTrace_API.Models.ResponseModel;

namespace TrackAndTrace_API.Interface
{
    public interface ISpecificationRepository
    {
        Task<APIResponseDTO> Add(SpecificationDto model, ExtractTokenDto token);
        Task<APIResponseDTO> GetList(CommonRequestDto? request, ExtractTokenDto token);
        Task<APIResponseDTO> Delete(string id, ExtractTokenDto token);
        Task<APIResponseDTO> ActiveInactive(int id, ExtractTokenDto token);
        Task<APIResponseDTO> GetSpecificationDifferentiatorMappingById(int id);
        Task ImportSpecification(JsonResponse jsonResponse, string prefix, string importName, ExtractTokenDto token);
    }
}
