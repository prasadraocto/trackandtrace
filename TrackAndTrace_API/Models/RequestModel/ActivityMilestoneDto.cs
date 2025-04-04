using TrackAndTrace_API.Models.DBModel;

namespace TrackAndTrace_API.Models.RequestModel
{
    public class ActivityMilestoneDto : CommonParamDto
    {
        public string start_date { get; set; }
        public string end_date { get; set; }
        public decimal budget_cost { get; set; } = 0;
        public int project_id { get; set; }
        public List<Activity_Milestone_Mapping> activity_milestone_mapping { get; set; }
    }
}