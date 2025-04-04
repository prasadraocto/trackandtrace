namespace TrackAndTrace_API.Models.DBModel
{
    public class Meeting_Assigned_Task
    {
        public int id { get; set; }
        public int meeting_attendee_id { get; set; }
        public string task { get; set; }
        public DateTime due_date { get; set; }
        public string status { get; set; }
        public int company_id { get; set; }
        public int created_by { get; set; }
        public DateTime created_date { get; set; }
        public int? updated_by { get; set; }
        public DateTime? updated_date { get; set; }
    }
}