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
    public class sub_taskController : ControllerBase
    {
        private readonly ISub_TaskRepository _interface;
        private readonly ApplicationDbContext _context;
        public sub_taskController(ISub_TaskRepository Interface, ApplicationDbContext context)
        {
            _interface = Interface;
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult> Add([FromBody] Sub_TaskDto model)
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

        [HttpGet("project_drop_down_list")]
        public async Task<ActionResult> GetProjectDropdownList(int activity_id, int task_id)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            var token = Utils.ExtractTokenDetails(HttpContext, _context);

            if (token != null)
            {
                aPIResponseDTO = await _interface.GetProjectDropdownList(activity_id, task_id, token);
            }
            else
            {
                return Unauthorized();
            }

            return Ok(aPIResponseDTO);
        }

        [HttpGet("activity_task_sub_task_drop_down_list")]
        public async Task<ActionResult> GetActivityTaskSubTaskDropDownList(int activity_id, int task_id, int project_id)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            var token = Utils.ExtractTokenDetails(HttpContext, _context);

            if (token != null)
            {
                aPIResponseDTO = await _interface.GetActivityTaskSubTaskDropDownList(activity_id, task_id, project_id, token);
            }
            else
            {
                return Unauthorized();
            }

            return Ok(aPIResponseDTO);
        }

        [HttpPut("project_mapping")]
        public async Task<ActionResult> SaveSub_TaskProjectMapping([FromBody] Sub_Task_Project_MappingDto model)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            var token = Utils.ExtractTokenDetails(HttpContext, _context);

            if (token != null)
            {
                aPIResponseDTO = await _interface.SaveSub_TaskProjectMapping(model, token);
            }
            else
            {
                return Unauthorized();
            }

            return Ok(aPIResponseDTO);
        }

        [HttpGet("project_mapping/{id}")]
        public async Task<ActionResult> GetSubTaskProjectmappingList(int id, [FromQuery] CommonRequestDto? request)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            var token = Utils.ExtractTokenDetails(HttpContext, _context);

            if (token != null)
            {
                aPIResponseDTO = await _interface.GetSubTaskProjectmappingList(id, request, token);
            }
            else
            {
                return Unauthorized();
            }

            return Ok(aPIResponseDTO);
        }
    }
}