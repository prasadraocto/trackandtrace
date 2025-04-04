namespace TrackAndTrace_API.Models.DBModel
{
    public class Trx_Daily_Activity_Item
    {
        public int id { get; set; }
        public int activity_id { get; set; }
        public int task_id { get; set; }
        public int sub_task_id { get; set; }
        public string? favourite_by { get; set; } = null;
        public int company_id { get; set; }
        public bool delete_flag { get; set; } = false;
        public int created_by { get; set; }
        public DateTime created_date { get; set; }
        public int? updated_by { get; set; }
        public DateTime? updated_date { get; set; }
    }
}