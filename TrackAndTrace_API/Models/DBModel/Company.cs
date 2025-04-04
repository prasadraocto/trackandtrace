namespace TrackAndTrace_API.Models.DBModel
{
    public class Company
    {
        public int id { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public string? phone { get; set; }
        public string? logo { get; set; }
        public bool active_flag { get; set; }
        public bool delete_flag { get; set; } = false;
        public DateTime created_date { get; set; }
        public DateTime? updated_date { get; set; }
    }
}
