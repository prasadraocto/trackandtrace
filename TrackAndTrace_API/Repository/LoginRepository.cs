using TrackAndTrace_API.Helpers;
using TrackAndTrace_API.Interface;
using TrackAndTrace_API.Models;
using TrackAndTrace_API.Models.RequestModel;
using TrackAndTrace_API.Models.ResponseModel;
using Microsoft.EntityFrameworkCore;

namespace TrackAndTrace_API.Repository
{
    public class LoginRepository : ILoginRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtHelper _jwtHelper;

        public LoginRepository(ApplicationDbContext context, JwtHelper jwtHelper)
        {
            _context = context;
            _jwtHelper = jwtHelper;
        }
        public async Task<APIResponseDTO> AuthenticateUser(LoginDto? model, string email)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            var user = await (from a in _context.Users
                              join b in _context.Designation on a.designation_id equals b.id
                              join c in _context.Roles on b.role_id equals c.id
                              where (model != null ? (a.email == model.Email && a.password == Utils.Encrypt(model.Password)) : a.email == email) &&
                                    a.active_flag == true && a.delete_flag == false &&
                                    b.active_flag == true && b.delete_flag == false

                              select new
                              {
                                  a.company_id,
                                  user_id = a.id,
                                  user_name = a.name,
                                  a.uuid,
                                  a.email,
                                  role_id = c.id,
                                  role_name = c.name,
                                  designation_id = b.id,
                                  designation_name = b.name,
                                  projects =
                                              c.name == "PROJECT_ADMIN" ? _context.Project.Where(x => x.company_id == a.company_id && x.admin_id == a.id && x.active_flag == true && x.delete_flag == false).Select(x => new
                                              {
                                                  x.id,
                                                  x.name
                                              }).ToList() :
                                              c.name == "GENERAL_STAFF" ? (from d in _context.Project_User_Mapping
                                                                           join e in _context.Project on d.project_id equals e.id
                                                                           where d.user_id == a.id && d.delete_flag == false &&
                                                                                 e.active_flag == true && e.delete_flag == false
                                                                           select new
                                                                           {
                                                                               e.id,
                                                                               e.name
                                                                           }).ToList() : null
                              }).FirstOrDefaultAsync();

            if (user == null)
            {
                aPIResponseDTO.message = "Incorrect Email or Password";
                return aPIResponseDTO;
            }
            else
            {
                TokenModel tokenModel = new TokenModel
                {
                    CompanyId = user.company_id,
                    UserId = user.user_id,
                    UserName = user.user_name,
                    Email = user.email
                };

                var token = _jwtHelper.GenerateToken(tokenModel);

                if (!string.IsNullOrEmpty(token))
                {
                    LoginResponseDto loginResponseDto = new LoginResponseDto
                    {
                        name = user.user_name,
                        email = user.email,
                        role = user.role_name,
                        token = token,
                        designation = user.designation_name,
                        projects = user.projects
                    };

                    aPIResponseDTO.success = true;
                    aPIResponseDTO.message = "Logged in! Let's get started.";
                    aPIResponseDTO.data = loginResponseDto;
                }
                else
                {
                    aPIResponseDTO.message = "Error occured during login";
                }
            }

            return aPIResponseDTO;
        }
    }
}
