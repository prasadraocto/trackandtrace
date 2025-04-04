using TrackAndTrace_API.Models.DBModel;

namespace TrackAndTrace_API.Models.RequestModel
{
    public class MaterialDto : CommonParamDto
    {
        public string? description { get; set; }
        public decimal cost { get; set; } = 0;
        public int uom_id { get; set; }
        public string type { get; set; }
        public List<Material_Brand_Mapping> material_brand_mapping { get; set; }
    }
}