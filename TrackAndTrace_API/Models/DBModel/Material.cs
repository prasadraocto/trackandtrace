namespace TrackAndTrace_API.Models.DBModel
{
    public class Material : CommonDBDto
    {
        public string? description { get; set; }
        public decimal cost { get; set; } = 0;
        public int uom_id { get; set; }
        public string type { get; set; }
    }
}