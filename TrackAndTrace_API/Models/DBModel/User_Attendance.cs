namespace TrackAndTrace_API.Models.DBModel
{
    public class User_Attendance
    {
        public int id { get; set; }
        public int user_id { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string address { get; set; }
        public string image { get; set; }
        public string attendance_type { get; set; }
        public string attendance_timestamp { get; set; }
        public int company_id { get; set; }
        public bool delete_flag { get; set; } = false;
        public int created_by { get; set; }
        public DateTime created_date { get; set; }
        public int? updated_by { get; set; }
        public DateTime? updated_date { get; set; }
    }
}