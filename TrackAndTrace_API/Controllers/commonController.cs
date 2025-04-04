using TrackAndTrace_API.Interface;
using TrackAndTrace_API.Models.RequestModel;
using TrackAndTrace_API.Models.ResponseModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TrackAndTrace_API.Helpers;
using TrackAndTrace_API.Models;
using Azure.Core;
using Hangfire.Storage;
using Hangfire;

namespace TrackAndTrace_API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class commonController : ControllerBase
    {
        private readonly ICommonRepository _interface;
        private readonly ApplicationDbContext _context;
        public commonController(ICommonRepository Interface, ApplicationDbContext context)
        {
            _interface = Interface;
            _context = context;
        }

        [HttpGet("common_drop_down_list/{name}")]
        public async Task<ActionResult> GetCommonDropdownList(string name)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            var token = Utils.ExtractTokenDetails(HttpContext, _context);

            if (token != null)
            {
                aPIResponseDTO = await _interface.GetCommonDropdownList(name, token);
            }
            else
            {
                return Unauthorized();
            }

            return Ok(aPIResponseDTO);
        }

        [HttpPost("bulk_import_name")]
        public async Task<ActionResult> CreateBulkImportName()
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            var token = Utils.ExtractTokenDetails(HttpContext, _context);

            if (token != null)
            {
                aPIResponseDTO = await _interface.CreateBulkImportName(token);
            }
            else
            {
                return Unauthorized();
            }

            return Ok(aPIResponseDTO);
        }

        [HttpGet("bulk_import_details/{name}")]
        public async Task<ActionResult> GetBulkImportDetails(string name)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            var token = Utils.ExtractTokenDetails(HttpContext, _context);

            if (token != null)
            {
                aPIResponseDTO = await _interface.GetBulkImportDetails(name, token);
            }
            else
            {
                return Unauthorized();
            }

            return Ok(aPIResponseDTO);
        }

        [HttpGet("hangfire_job_status/{job_id}")]
        public async Task<APIResponseDTO> GetHangfireJobStatus(string job_id)
        {
            try
            {
                APIResponseDTO aPIResponseDTO = new APIResponseDTO();

                IStorageConnection connection = JobStorage.Current.GetConnection();
                JobData jobData = connection.GetJobData(job_id);
                if (jobData != null)
                {
                    aPIResponseDTO.message = "Job status fetched successfully";
                    aPIResponseDTO.success = true;
                    aPIResponseDTO.data = jobData.State;
                }
                else
                {
                    aPIResponseDTO.message = "Job not found";
                }

                return aPIResponseDTO;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}