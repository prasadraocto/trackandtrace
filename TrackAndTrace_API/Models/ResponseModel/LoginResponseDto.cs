namespace TrackAndTrace_API.Models.ResponseModel
{
    public class LoginResponseDto
    {
        public string name { get; set; }
        public string email { get; set; }
        public string role { get; set; }
        public string designation { get; set; }
        public string token { get; set; }
        public object? projects { get; set; }
    }
    public class ExtractTokenDto
    {
        public int CompanyId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
    }
}
