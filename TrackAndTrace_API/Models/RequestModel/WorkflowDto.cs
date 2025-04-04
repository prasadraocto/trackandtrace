using TrackAndTrace_API.Models.DBModel;

namespace TrackAndTrace_API.Models.RequestModel
{
    public class WorkflowDto
    {
        public int work_flow_id { get; set; }
        public int project_id { get; set; }
        public List<WFP_Users> users { get; set; }
    }
    public class WFP_Users
    {
        public int user_id { get; set; }
        public int order_id { get; set; }
        public bool is_supersede { get; set; } = false;
    }
}