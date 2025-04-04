namespace TrackAndTrace_API.Models.DBModel
{
    public class Trx_Daily_Activity_Details
    {
        public int id { get; set; }
        public int activity_item_id { get; set; }
        public int project_id { get; set; }
        public int project_level_id { get; set; }
        public decimal quantity { get; set; }
        public decimal progress { get; set; }
        public int shift_id { get; set; }
        public decimal hrs_spent { get; set; }
        public int labour_type_id { get; set; }
        public int? subcontractor_id { get; set; }
        public int? weather_id { get; set; }
        public string? remarks { get; set; } = null;
        public string? status { get; set; } = null;
        public bool is_draft { get; set; } = true;
        public int company_id { get; set; }
        public bool delete_flag { get; set; } = false;
        public int created_by { get; set; }
        public DateTime created_date { get; set; }
        public int? updated_by { get; set; }
        public DateTime? updated_date { get; set; }
    }
}