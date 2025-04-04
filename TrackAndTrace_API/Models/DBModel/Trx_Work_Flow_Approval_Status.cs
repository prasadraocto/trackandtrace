namespace TrackAndTrace_API.Models.DBModel
{
    public class Trx_Work_Flow_Approval_Status
    {
        public int id { get; set; }
        public int request_id { get; set; }
        public int wf_project_user_id { get; set; }
        public int user_id { get; set; }
        public int order_id { get; set; }
        public bool is_supersede { get; set; }
        public string? status { get; set; }
        public int created_by { get; set; }
        public DateTime created_date { get; set; }
        public int? updated_by { get; set; }
        public DateTime? updated_date { get; set; }
    }
}