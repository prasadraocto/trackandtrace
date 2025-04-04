namespace TrackAndTrace_API.Models.DBModel
{
    public class Specification_Differentiator_Mapping
    {
        public int id { get; set; }
        public int specification_id { get; set; }
        public int differentiator_id { get; set; }
        public string value { get; set; }
    }
}