using TrackAndTrace_API.Models.DBModel;

namespace TrackAndTrace_API.Models.RequestModel
{
    public class SpaceManagementDto : CommonParamDto
    {
        public int? capacity { get; set; } = 0;
    }
}