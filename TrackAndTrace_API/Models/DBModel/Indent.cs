namespace TrackAndTrace_API.Models.DBModel
{
    public class Indent
    {
        public int id { get; set; }
        public int project_id { get; set; }
        public int request_id { get; set; }
        public string indent_type { get; set; }
        public string indent_no { get; set; }
        public string indent_date { get; set; }
        public string status { get; set; }
        public string? po_number { get; set; } = null;
        public string? po_file_url { get; set; } = null;
        public int created_by { get; set; }
        public DateTime created_date { get; set; }
        public int? updated_by { get; set; }
        public DateTime? updated_date { get; set; }
    }
}