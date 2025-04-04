using TrackAndTrace_API.Interface;
using TrackAndTrace_API.Models.RequestModel;
using TrackAndTrace_API.Models.ResponseModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TrackAndTrace_API.Helpers;
using TrackAndTrace_API.Models;
using Azure.Core;
using Newtonsoft.Json.Linq;

namespace TrackAndTrace_API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class user_attendanceController : ControllerBase
    {
        private readonly IUserAttendanceRepository _interface;
        private readonly ApplicationDbContext _context;
        public user_attendanceController(IUserAttendanceRepository Interface, ApplicationDbContext context)
        {
            _interface = Interface;
            _context = context;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Add([FromBody] UserAttendanceDto model)
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
        [Authorize]
        public async Task<ActionResult> GetList([FromQuery] CommonRequestDto? request, [FromQuery] string? from_date, [FromQuery] string? to_date, [FromQuery] int company_id)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            var token = Utils.ExtractTokenDetails(HttpContext, _context);

            if (token != null)
            {
                aPIResponseDTO = await _interface.GetList(request, from_date, to_date, company_id, token);
            }
            else
            {
                return Unauthorized();
            }

            return Ok(aPIResponseDTO);
        }

        [HttpDelete("{id}")]
        [Authorize]
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

        [HttpGet("last_attendance_detail")]
        [Authorize]
        public async Task<ActionResult> GetLastAttendanceDetail()
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            var token = Utils.ExtractTokenDetails(HttpContext, _context);

            if (token != null)
            {
                aPIResponseDTO = await _interface.GetLastAttendanceDetail(token);
            }
            else
            {
                return Unauthorized();
            }

            return Ok(aPIResponseDTO);
        }

        [HttpPut("device_attendance/{company_code}")]
        public async Task<ActionResult> UpdateDeviceAttendance(string company_code, [FromBody] List<DeviceAttendanceDto> model)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            aPIResponseDTO = await _interface.UpdateDeviceAttendance(company_code, model);

            return Ok(aPIResponseDTO);
        }

    }
}