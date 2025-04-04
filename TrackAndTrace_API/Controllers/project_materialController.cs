using TrackAndTrace_API.Interface;
using TrackAndTrace_API.Models.RequestModel;
using TrackAndTrace_API.Models.ResponseModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TrackAndTrace_API.Helpers;
using TrackAndTrace_API.Models;

namespace TrackAndTrace_API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class project_materialController : ControllerBase
    {
        private readonly IProjectMaterialRepository _interface;
        private readonly ApplicationDbContext _context;
        public project_materialController(IProjectMaterialRepository Interface, ApplicationDbContext context)
        {
            _interface = Interface;
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetList(int id, [FromQuery] CommonRequestDto? request)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            var token = Utils.ExtractTokenDetails(HttpContext, _context);

            if (token != null)
            {
                aPIResponseDTO = await _interface.GetList(id, request, token);
            }
            else
            {
                return Unauthorized();
            }

            return Ok(aPIResponseDTO);
        }
    }
}