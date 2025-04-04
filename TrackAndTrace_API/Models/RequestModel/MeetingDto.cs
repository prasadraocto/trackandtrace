using TrackAndTrace_API.Models.DBModel;

namespace TrackAndTrace_API.Models.RequestModel
{
    public class MeetingDto
    {
        public int id { get; set; }
        public string title { get; set; }
        public string agenda { get; set; }
        public DateTime meeting_date { get; set; }
        public string start_time { get; set; }
        public string end_time { get; set; }
        public string? meeting_url { get; set; } = null;
        public string color { get; set; }
        public List<Meeting_Attendee_Detail> attendee_mapping { get; set; }
    }
    public class AttendeeTaskDto
    {
        public List<MeetingAssignedTaskDto> attendee_task { get; set; }
    }
    public class MeetingAssignedTaskDto
    {
        public int id { get; set; }
        public int attendee_id { get; set; }
        public string task { get; set; }
        public DateTime due_date { get; set; }
    }
}