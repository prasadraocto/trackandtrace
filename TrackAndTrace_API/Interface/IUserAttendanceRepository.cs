using Microsoft.AspNetCore.Mvc;
using TrackAndTrace_API.Models.RequestModel;
using TrackAndTrace_API.Models.ResponseModel;

namespace TrackAndTrace_API.Interface
{
    public interface IUserAttendanceRepository
    {
        Task<APIResponseDTO> Add(UserAttendanceDto model, ExtractTokenDto token);
        Task<APIResponseDTO> GetList(CommonRequestDto? request, string from_date, string to_date, int company_id, ExtractTokenDto token);
        Task<APIResponseDTO> Delete(string id, ExtractTokenDto token);
        Task<APIResponseDTO> GetLastAttendanceDetail(ExtractTokenDto token);
        Task<APIResponseDTO> UpdateDeviceAttendance(string company_code, List<DeviceAttendanceDto> model);
    }
}