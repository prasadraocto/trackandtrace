using TrackAndTrace_API.Models.DBModel;

namespace TrackAndTrace_API.Models.RequestModel
{
    public class CompanyDto : CommonParamDto
    {
        public string? phone { get; set; }
        public string? logo { get; set; }
    }
}
