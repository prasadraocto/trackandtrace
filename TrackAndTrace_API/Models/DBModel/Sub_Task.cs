namespace TrackAndTrace_API.Models.DBModel
{
    public class Sub_Task : CommonDBDto
    {
        public int activity_id { get; set; }
        public int task_id { get; set; }
        public int uom_id { get; set; }
        public bool is_prime { get; set; } = false;
        public int estimated_days { get; set; }
    }
}