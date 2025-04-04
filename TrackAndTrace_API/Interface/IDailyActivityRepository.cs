using TrackAndTrace_API.Models.RequestModel;
using TrackAndTrace_API.Models.ResponseModel;

namespace TrackAndTrace_API.Interface
{
    public interface IDailyActivityRepository
    {
        Task<APIResponseDTO> Add(DailyActivityDto model, ExtractTokenDto token);
        Task<APIResponseDTO> GetList(int projectId, DateTime? fromDate, DateTime? toDate, CommonRequestDto? request, ExtractTokenDto token);
        Task<APIResponseDTO> Delete(string id, ExtractTokenDto token);
        Task<APIResponseDTO> GetSubcontractorDropdownList(int id, ExtractTokenDto token);
        Task<APIResponseDTO> GetManpowerDropdownList(int project_id, int shift_id, decimal hrs_spent, DateTime activity_date, decimal old_hrs_spent, string old_manpower_ids, ExtractTokenDto token);
        Task<APIResponseDTO> GetMaterialDropdownList(int id, ExtractTokenDto token);
        Task<APIResponseDTO> GetMachineryDropdownList(int daily_activity_id, ExtractTokenDto token);
        Task<APIResponseDTO> GetDailyActivityDetailById(int id, ExtractTokenDto token);
        Task<APIResponseDTO> GetDailyActivityManpowerById(int id, ExtractTokenDto token);
        Task<APIResponseDTO> GetDailyActivityMaterialById(int id, ExtractTokenDto token);
        Task<APIResponseDTO> GetDailyActivityMachineryById(int id, ExtractTokenDto token);

    }
}
