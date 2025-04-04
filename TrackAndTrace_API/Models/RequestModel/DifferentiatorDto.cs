using TrackAndTrace_API.Models.DBModel;

namespace TrackAndTrace_API.Models.RequestModel
{
    public class DifferentiatorDto : CommonParamDto
    {
        public int material_id { get; set; }
        public List<Differentiator_Mapping> differentiator_mapping { get; set; }
    }
    public class DifferentiatorJsonResponse
    {
        public List<string> Columns { get; set; }  // List of differentiator codes (column names)
        public List<DifferentiatorData> Data { get; set; }  // Data rows with Name and Values

        public DifferentiatorJsonResponse()
        {
            Columns = new List<string>();
            Data = new List<DifferentiatorData>();
        }
    }

    public class DifferentiatorData
    {
        public string DifferentiatorName { get; set; }  // The name of the differentiator (column)
        public List<string> Value { get; set; }  // The values associated with the differentiator
    }
}