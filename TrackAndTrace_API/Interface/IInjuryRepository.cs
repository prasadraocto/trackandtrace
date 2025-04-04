using TrackAndTrace_API.Models.DBModel;
using TrackAndTrace_API.Models.RequestModel;
using TrackAndTrace_API.Models.ResponseModel;

namespace TrackAndTrace_API.Interface
{
    public interface IInjuryRepository
    {
        Task<APIResponseDTO> Add(InjuryDto model, ExtractTokenDto token);
        Task<APIResponseDTO> GetList(CommonRequestDto? request, ExtractTokenDto token);
        Task<APIResponseDTO> Delete(string id, ExtractTokenDto token);
        Task<APIResponseDTO> ActiveInactive(int id, ExtractTokenDto token);
        Task<APIResponseDTO> ImportInjury(string importName, IEnumerable<Type_Injury> injuryList, ExtractTokenDto token);
    }
}
