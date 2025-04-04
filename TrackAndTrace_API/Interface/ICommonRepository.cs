using TrackAndTrace_API.Models.RequestModel;
using TrackAndTrace_API.Models.ResponseModel;

namespace TrackAndTrace_API.Interface
{
    public interface ICommonRepository
    {
        Task<APIResponseDTO> GetCommonDropdownList(string name, ExtractTokenDto token);
        Task<APIResponseDTO> CreateBulkImportName(ExtractTokenDto token);
        Task<APIResponseDTO> GetBulkImportDetails(string name, ExtractTokenDto token);
    }
}
