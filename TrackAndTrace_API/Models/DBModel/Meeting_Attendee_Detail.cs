namespace TrackAndTrace_API.Models.DBModel
{
    public class Meeting_Attendee_Detail
    {
        public int id { get; set; }
        public int meeting_id { get; set; }
        public int attendee_id { get; set; }
    }
}