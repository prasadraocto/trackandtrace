namespace TrackAndTrace_API.Models.DBModel
{
    public class Trx_Daily_Activity_Material
    {
        public int id { get; set; }
        public int daily_activity_id { get; set; }
        public int material_id { get; set; }
        public decimal quantity { get; set; } = 0;
        public int company_id { get; set; }
        public bool delete_flag { get; set; } = false;
        public int created_by { get; set; }
        public DateTime created_date { get; set; }
        public int? updated_by { get; set; }
        public DateTime? updated_date { get; set; }
    }
}