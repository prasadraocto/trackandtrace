namespace TrackAndTrace_API.Models.DBModel
{
    public class Trx_Daily_Activity_Manpower
    {
        public int id { get; set; }
        public int daily_activity_id { get; set; }
        public int manpower_id { get; set; }
        public int designation_id { get; set; }
        public int engineer_id { get; set; }
        public int? charge_hand_id { get; set; }
        public int? gang_leader_id { get; set; }
        public int? subcontractor_id { get; set; }
        public int company_id { get; set; }
        public bool delete_flag { get; set; } = false;
        public int created_by { get; set; }
        public DateTime created_date { get; set; }
        public int? updated_by { get; set; }
        public DateTime? updated_date { get; set; }
    }
}