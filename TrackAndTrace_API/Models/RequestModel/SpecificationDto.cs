using TrackAndTrace_API.Models.DBModel;

namespace TrackAndTrace_API.Models.RequestModel
{
    public class SpecificationDto : CommonParamDto
    {
        public List<Specification_Differentiator_Mapping> specification_differentiator_mapping { get; set; }
    }
    public class JsonResponse
    {
        public List<string> Columns { get; set; }  // List of differentiator codes (column names)
        public List<List<SpecificationData>> Data { get; set; }  // Data rows with Name and Value pairs

        public JsonResponse()
        {
            Columns = new List<string>();
            Data = new List<List<SpecificationData>>();
        }
    }

    public class SpecificationData
    {
        public string Name { get; set; }  // The name of the differentiator (column)
        public string Value { get; set; }  // The value associated with the differentiator
    }

}