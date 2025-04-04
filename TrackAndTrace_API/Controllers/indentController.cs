using TrackAndTrace_API.Interface;
using TrackAndTrace_API.Models.RequestModel;
using TrackAndTrace_API.Models.ResponseModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TrackAndTrace_API.Helpers;
using TrackAndTrace_API.Models;
using Azure.Core;
using Hangfire;
using System.Globalization;
using ExcelDataReader;
using TrackAndTrace_API.Models.DBModel;

namespace TrackAndTrace_API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class indentController : ControllerBase
    {
        private readonly IIndentRepository _interface;
        private readonly ApplicationDbContext _context;
        public indentController(IIndentRepository Interface, ApplicationDbContext context)
        {
            _interface = Interface;
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult> Add([FromBody] IndentDto model)
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
        public async Task<ActionResult> GetList([FromQuery] int project_id, [FromQuery] CommonRequestDto? request)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            var token = Utils.ExtractTokenDetails(HttpContext, _context);

            if (token != null)
            {
                aPIResponseDTO = await _interface.GetList(project_id, request, token);
            }
            else
            {
                return Unauthorized();
            }

            return Ok(aPIResponseDTO);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetIndentDetails(int id)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            var token = Utils.ExtractTokenDetails(HttpContext, _context);

            if (token != null)
            {
                aPIResponseDTO = await _interface.GetIndentDetails(id, token);
            }
            else
            {
                return Unauthorized();
            }

            return Ok(aPIResponseDTO);
        }

        [HttpPut("indent_request_status/{id}/{status}")]
        public async Task<ActionResult> UpdateIndentRequestStatus(int id, string status)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            var token = Utils.ExtractTokenDetails(HttpContext, _context);

            if (token != null)
            {
                aPIResponseDTO = await _interface.UpdateIndentRequestStatus(id, status, token);
            }
            else
            {
                return Unauthorized();
            }

            return Ok(aPIResponseDTO);
        }

        [HttpPut("indent_material_details")]
        public async Task<ActionResult> UpdateIndentMaterialDetails(UpdateIndentMaterialDto model)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            var token = Utils.ExtractTokenDetails(HttpContext, _context);

            if (token != null)
            {
                aPIResponseDTO = await _interface.UpdateIndentMaterialDetails(model);
            }
            else
            {
                return Unauthorized();
            }

            return Ok(aPIResponseDTO);
        }
    }
}