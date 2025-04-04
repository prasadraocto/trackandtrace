namespace TrackAndTrace_API.Models.DBModel
{
    public class Activity_Milestone : CommonDBDto
    {
        public string start_date { get; set; }
        public string end_date { get; set; }
        public decimal budget_cost { get; set; } = 0;
        public int project_id { get; set; }
    }
}