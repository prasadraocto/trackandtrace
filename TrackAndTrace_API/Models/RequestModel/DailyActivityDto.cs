namespace TrackAndTrace_API.Models.RequestModel
{
    public class DailyActivityDto
    {
        public int daily_activity_id { get; set; }
        public int activity_id { get; set; }
        public int task_id { get; set; }
        public int sub_task_id { get; set; }
        public string? favourite_by { get; set; } = null;
        public int? activity_item_id { get; set; }
        public int project_id { get; set; }
        public int project_level_id { get; set; }
        public decimal quantity { get; set; }
        public decimal progress { get; set; }
        public int shift_id { get; set; }
        public decimal hrs_spent { get; set; }
        public int labour_type_id { get; set; }
        public int? subcontractor_id { get; set; }
        public int? weather_id { get; set; }
        public string? remarks { get; set; } = null;
        public string? status { get; set; } = null;
        public bool is_draft { get; set; } = true;
        public List<DailyActivityManpower> manpower { get; set; }
        public List<DailyActivityMaterial> material { get; set; }
        public List<DailyActivityMachinery> machinery { get; set; }
    }
    public class DailyActivityManpower
    {
        public int id { get; set; }
        public int daily_activity_id { get; set; }
        public int manpower_id { get; set; }
    }
    public class DailyActivityMaterial
    {
        public int id { get; set; }
        public int daily_activity_id { get; set; }
        public int material_id { get; set; }
        public decimal quantity { get; set; }
    }
    public class DailyActivityMachinery
    {
        public int id { get; set; }
        public int daily_activity_id { get; set; }
        public int machinery_id { get; set; }
        public decimal quantity { get; set; }
        public string start_time { get; set; }
        public string end_time { get; set; }
    }
}
