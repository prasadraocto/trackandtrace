namespace TrackAndTrace_API.Models.DBModel
{
    public class Users : CommonDBDto
    {
        public Guid uuid { get; set; }
        public string email { get; set; }
        public string? phone { get; set; }
        public string password { get; set; }
        public int designation_id { get; set; }
        public int? device_user_id { get; set; }
    }
}
