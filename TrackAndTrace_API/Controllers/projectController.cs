using TrackAndTrace_API.Interface;
using TrackAndTrace_API.Models.RequestModel;
using TrackAndTrace_API.Models.ResponseModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TrackAndTrace_API.Helpers;
using TrackAndTrace_API.Models;
using Azure.Core;

namespace TrackAndTrace_API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class projectController : ControllerBase
    {
        private readonly IProjectRepository _interface;
        private readonly ApplicationDbContext _context;
        public projectController(IProjectRepository Interface, ApplicationDbContext context)
        {
            _interface = Interface;
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult> Add([FromBody] ProjectDto model)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            var token = Utils.ExtractTokenDetails(HttpContext, _context);

            if (token != null)
            {
                aPIResponseDTO = await _interface.Add(model, token);
            }
            else
            {
                return Unauthorized();
            }

            return Ok(aPIResponseDTO);
        }

        [HttpGet]
        public async Task<ActionResult> GetList([FromQuery] CommonRequestDto? request)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            var token = Utils.ExtractTokenDetails(HttpContext, _context);

            if (token != null)
            {
                aPIResponseDTO = await _interface.GetList(request, token);
            }
            else
            {
                return Unauthorized();
            }

            return Ok(aPIResponseDTO);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            var token = Utils.ExtractTokenDetails(HttpContext, _context);

            if (token != null)
            {
                aPIResponseDTO = await _interface.Delete(id, token);
            }
            else
            {
                return Unauthorized();
            }

            return Ok(aPIResponseDTO);
        }

        [HttpPut("active_inactive/{id}")]
        public async Task<ActionResult> ActiveInactive(int id)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            var token = Utils.ExtractTokenDetails(HttpContext, _context);

            if (token != null)
            {
                aPIResponseDTO = await _interface.ActiveInactive(id, token);
            }
            else
            {
                return Unauthorized();
            }

            return Ok(aPIResponseDTO);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetProjectUserMappingById(int id)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            var token = Utils.ExtractTokenDetails(HttpContext, _context);

            if (token != null)
            {
                aPIResponseDTO = await _interface.GetProjectUserMappingById(id);
            }
            else
            {
                return Unauthorized();
            }

            return Ok(aPIResponseDTO);
        }

        [HttpGet("users_drop_down_list")]
        public async Task<ActionResult> GetUsersDropdownList()
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            var token = Utils.ExtractTokenDetails(HttpContext, _context);

            if (token != null)
            {
                aPIResponseDTO = await _interface.GetUsersDropdownList(token);
            }
            else
            {
                return Unauthorized();
            }

            return Ok(aPIResponseDTO);
        }

    }
}