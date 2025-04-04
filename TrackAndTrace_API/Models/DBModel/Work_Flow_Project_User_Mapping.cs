namespace TrackAndTrace_API.Models.DBModel
{
    public class Work_Flow_Project_User_Mapping
    {
        public int id { get; set; }
        public int work_flow_id { get; set; }
        public int project_id { get; set; }
        public int user_id { get; set; }
        public int order_id { get; set; }
        public bool is_supersede { get; set; } = false;
        public bool delete_flag { get; set; } = false;
        public int created_by { get; set; }
        public DateTime created_date { get; set; }
        public int? updated_by { get; set; }
        public DateTime? updated_date { get; set; }
    }
}