using TrackAndTrace_API.Models.DBModel;

namespace TrackAndTrace_API.Models.RequestModel
{
    public class CommonParamDto
    {
        public int id { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public bool active_flag { get; set; } = true;
    }
}