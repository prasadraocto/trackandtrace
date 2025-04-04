using TrackAndTrace_API.Models.DBModel;

namespace TrackAndTrace_API.Models.RequestModel
{
    public class IndentDto
    {
        public int id { get; set; }
        public int project_id { get; set; }
        public string indent_type { get; set; }
        public string indent_date { get; set; }
        public List<Indent_MaterialDto> indent_materials { get; set; }
    }
    public class Indent_MaterialDto
    {
        public int id { get; set; }
        public int indent_id { get; set; }
        public int material_id { get; set; }
        public decimal quantity { get; set; }
        public int brand_id { get; set; }
        public List<Indent_Material_Differentiator> indent_material_differentiators { get; set; }
    }
    public class UpdateIndentMaterialDto
    {
        public List<UpdateIndentMaterial> indent_materials { get; set; }
    }
    public class UpdateIndentMaterial
    {
        public int id { get; set; }
        public int lead_days { get; set; }
        public string delivery_date { get; set; }
        public int supply_cost { get; set; }
        public string? remarks { get; set; }
    }
}