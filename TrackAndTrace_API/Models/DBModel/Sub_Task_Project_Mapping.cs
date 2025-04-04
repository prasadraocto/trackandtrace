namespace TrackAndTrace_API.Models.DBModel
{
    public class Sub_Task_Project_Mapping
    {
        public int id { get; set; }
        public int sub_task_id { get; set; }
        public int project_id { get; set; }
        public string start_date { get; set; }
        public int execution_days { get; set; } = 0;
        public string end_date { get; set; }
        public decimal installation_cost { get; set; } = 0;
        public int manpower_count { get; set; } = 0;
        public int created_by { get; set; }
        public DateTime created_date { get; set; }
        public int? updated_by { get; set; }
        public DateTime? updated_date { get; set; }
    }
}