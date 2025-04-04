using TrackAndTrace_API.Models.DBModel;

namespace TrackAndTrace_API.Models.RequestModel
{
    public class Sub_TaskDto : CommonParamDto
    {
        public int activity_id { get; set; }
        public int task_id { get; set; }
        public int uom_id { get; set; }
        public bool is_prime { get; set; } = false;
        public int estimated_days { get; set; }
    }
    public class Sub_Task_Project_MappingDto
    {
        public int id { get; set; }
        public int sub_task_id { get; set; }
        public int project_id { get; set; }
        public string start_date { get; set; }
        public int execution_days { get; set; } = 0;
        public string end_date { get; set; }
        public decimal installation_cost { get; set; } = 0;
        public int manpower_count { get; set; } = 0;
    }
}