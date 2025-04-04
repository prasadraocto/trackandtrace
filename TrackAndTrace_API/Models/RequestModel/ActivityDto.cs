using TrackAndTrace_API.Models.DBModel;

namespace TrackAndTrace_API.Models.RequestModel
{
    public class ActivityDto : CommonParamDto
    {
        public int estimated_days { get; set; }
        public List<Activity_Project_Mapping> project_mapping { get; set; }
    }
}