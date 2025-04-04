namespace TrackAndTrace_API.Models.DBModel
{
    public class Trx_Daily_Activity_Machinery
    {
        public int id { get; set; }
        public int daily_activity_id { get; set; }
        public int machinery_id { get; set; }
        public decimal quantity { get; set; }
        public string start_time { get; set; }
        public string end_time { get; set; }
        public int company_id { get; set; }
        public bool delete_flag { get; set; } = false;
        public int created_by { get; set; }
        public DateTime created_date { get; set; }
        public int? updated_by { get; set; }
        public DateTime? updated_date { get; set; }
    }
}