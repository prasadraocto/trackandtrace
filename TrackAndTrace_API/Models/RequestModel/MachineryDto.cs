using TrackAndTrace_API.Models.DBModel;

namespace TrackAndTrace_API.Models.RequestModel
{
    public class MachineryDto : CommonParamDto
    {
        public decimal quantity { get; set; }
        public bool in_house { get; set; }
    }
}