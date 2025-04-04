using TrackAndTrace_API.Models.DBModel;

namespace TrackAndTrace_API.Models.RequestModel
{
    public class WarehouseDto : CommonParamDto
    {
        public string? address { get; set; }
    }
}