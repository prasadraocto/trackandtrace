namespace TrackAndTrace_API.Models.DBModel
{
    public class Meeting
    {
        public int id { get; set; }
        public string title { get; set; }
        public string agenda { get; set; }
        public DateTime meeting_date { get; set; }
        public string start_time { get; set; }
        public string end_time { get; set; }
        public string? meeting_url { get; set; } = null;
        public string status { get; set; }
        public string color { get; set; }
        public int company_id { get; set; }
        public bool delete_flag { get; set; } = false;
        public int created_by { get; set; }
        public DateTime created_date { get; set; }
        public int? updated_by { get; set; }
        public DateTime? updated_date { get; set; }
    }
}