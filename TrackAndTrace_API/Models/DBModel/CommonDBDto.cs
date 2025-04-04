namespace TrackAndTrace_API.Models.DBModel
{
    public class CommonDBDto
    {
        public int id { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public int company_id { get; set; }
        public bool active_flag { get; set; }
        public bool delete_flag { get; set; } = false;
        public int created_by { get; set; }
        public DateTime created_date { get; set; }
        public int? updated_by { get; set; }
        public DateTime? updated_date { get; set; }
    }
}
