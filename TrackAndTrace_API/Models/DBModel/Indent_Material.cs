namespace TrackAndTrace_API.Models.DBModel
{
    public class Indent_Material
    {
        public int id { get; set; }
        public int indent_id { get; set; }
        public int material_id { get; set; }
        public decimal quantity { get; set; }
        public int brand_id { get; set; }
        public int? lead_days { get; set; }
        public string? delivery_date { get; set; }
        public decimal? supply_cost { get; set; }
        public string? remarks { get; set; }
    }
}