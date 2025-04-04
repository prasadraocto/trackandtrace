using TrackAndTrace_API.Models.DBModel;

namespace TrackAndTrace_API.Models.RequestModel
{
    public class UsersDto : CommonParamDto
    {
        public string email { get; set; }
        public string? phone { get; set; }
        public string password { get; set; }
        public int company_id { get; set; }
        public int designation_id { get; set; }
    }
}