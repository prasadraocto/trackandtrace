namespace TrackAndTrace_API.Models.DBModel
{
    public class Activity_Milestone_Mapping
    {
        public int id { get; set; }
        public int activity_milestone_id { get; set; }
        public int activity_id { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
        public decimal cost { get; set; } = 0;
    }
}