using TrackAndTrace_API.Models.RequestModel;
using TrackAndTrace_API.Models.ResponseModel;

namespace TrackAndTrace_API.Interface
{
    public interface IMeetingRepository
    {
        Task<APIResponseDTO> Add(MeetingDto model, ExtractTokenDto token);
        Task<APIResponseDTO> GetList(string start_date, string end_date, ExtractTokenDto token);
        Task<APIResponseDTO> Delete(int id, ExtractTokenDto token);
        Task<APIResponseDTO> GetAttendeeMappingById(int id);
        Task<APIResponseDTO> UpdateAttendeeTask(int id, AttendeeTaskDto model, ExtractTokenDto token);
        Task<APIResponseDTO> GetAttendeeTaskList(int id);
    }
}