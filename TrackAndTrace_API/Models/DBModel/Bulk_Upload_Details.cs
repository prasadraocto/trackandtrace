namespace TrackAndTrace_API.Models.DBModel
{
    public class Bulk_Import_Details
    {
        public int id { get; set; }
        public string name { get; set; }
        public int company_id { get; set; }
        public int total { get; set; } = 0;
        public int success { get; set; } = 0;
        public int failed { get; set; } = 0;
        public string? failed_records { get; set; } = null;
        public string? status { get; set; } = null;
        public int created_by { get; set; }
        public DateTime created_date { get; set; }
        public int? updated_by { get; set; }
        public DateTime? updated_date { get; set; }
    }
}