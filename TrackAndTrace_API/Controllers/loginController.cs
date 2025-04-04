using TrackAndTrace_API.Helpers;
using TrackAndTrace_API.Interface;
using TrackAndTrace_API.Models;
using TrackAndTrace_API.Models.RequestModel;
using TrackAndTrace_API.Models.ResponseModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TrackAndTrace_API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class loginController : ControllerBase
    {
        private readonly ILoginRepository _interface;
        public loginController(ILoginRepository Interface)
        {
            _interface = Interface;
        }

        [HttpPost]
        public async Task<ActionResult> Login([FromBody] LoginDto model)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            aPIResponseDTO = await _interface.AuthenticateUser(model, "");

            return Ok(aPIResponseDTO);
        }

        [HttpPost("authenticate_user/{email}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult> AuthenticateUser(string email)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            aPIResponseDTO = await _interface.AuthenticateUser(null, email);

            return Ok(aPIResponseDTO);
        }
    }
}