using TrackAndTrace_API.Models.RequestModel;
using TrackAndTrace_API.Models.ResponseModel;

namespace TrackAndTrace_API.Interface
{
    public interface ICompanyRepository
    {
        Task<APIResponseDTO> Add(CompanyDto model);
        Task<APIResponseDTO> GetList(CommonRequestDto? request);
        Task<APIResponseDTO> Delete(string id);
        Task<APIResponseDTO> ActiveInactive(int id);
    }
}
