namespace TrackAndTrace_API.Models.ResponseModel
{
    public class MeetingResponseDTO
    {
        public int id { get; set; }
        public string title { get; set; }
        public string agenda { get; set; }
        public DateTime meeting_date { get; set; }
        public string start_time { get; set; }
        public string end_time { get; set; }
        public string color { get; set; }
        public string status { get; set; }
    }
}
