namespace TrackAndTrace_API.Models.RequestModel
{
    public class TokenModel
    {
        public int CompanyId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string RoleName { get; set; }
    }
}
