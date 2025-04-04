using TrackAndTrace_API.Models.DBModel;

namespace TrackAndTrace_API.Models.RequestModel
{
    public class ProjectDto : CommonParamDto
    {
        public string? description { get; set; }
        public string? address { get; set; }
        public string client { get; set; }
        public decimal? cost { get; set; } = 0;
        public DateTime? start_date { get; set; }
        public DateTime? end_date { get; set; }
        public decimal work_hours { get; set; } = 0;
        public int admin_id { get; set; }
        public int manager_id { get; set; }
        public int engineer_id { get; set; }
        public string? logo { get; set; }
        public List<Project_User_Mapping> project_user_mapping { get; set; }
    }
}