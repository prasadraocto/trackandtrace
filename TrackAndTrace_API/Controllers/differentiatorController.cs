using TrackAndTrace_API.Interface;
using TrackAndTrace_API.Models.RequestModel;
using TrackAndTrace_API.Models.ResponseModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TrackAndTrace_API.Helpers;
using TrackAndTrace_API.Models;
using Azure.Core;
using ExcelDataReader;
using Hangfire;
using TrackAndTrace_API.Models.DBModel;

namespace TrackAndTrace_API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class differentiatorController : ControllerBase
    {
        private readonly IDifferentiatorRepository _interface;
        private readonly ApplicationDbContext _context;
        public differentiatorController(IDifferentiatorRepository Interface, ApplicationDbContext context)
        {
            _interface = Interface;
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult> Add([FromBody] DifferentiatorDto model)
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

        [HttpPost("bulk_upload/{import_name}/{material_id}")]
        public async Task<ActionResult<APIResponseDTO>> BulkUpload(IFormFile file, [FromRoute] string import_name, [FromRoute] int material_id)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();
            var token = Utils.ExtractTokenDetails(HttpContext, _context);

            if (token == null)
                return Unauthorized();

            if (file == null)
            {
                aPIResponseDTO.message = "File is null";
                return aPIResponseDTO;
            }

            var allowedExtensions = new[] { ".xlsx", ".xls", ".xlsb", ".csv" };
            var extension = Path.GetExtension(file.FileName);
            if (!allowedExtensions.Contains(extension))
            {
                aPIResponseDTO.message = $"Extension must be one of: {string.Join(", ", allowedExtensions)}";
                return aPIResponseDTO;
            }

            try
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                using var stream = new MemoryStream();
                file.CopyTo(stream);
                stream.Position = 0;

                IExcelDataReader reader = extension == ".csv" ? ExcelReaderFactory.CreateCsvReader(stream) : ExcelReaderFactory.CreateReader(stream);

                DifferentiatorJsonResponse response = new DifferentiatorJsonResponse();
                bool headerRead = false;

                using (reader)
                {
                    while (reader.Read())
                    {
                        if (!headerRead)
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                if (reader.GetValue(i) != null)
                                    response.Columns.Add(reader.GetValue(i).ToString().Trim());
                            }
                            headerRead = true;
                        }
                        else
                        {
                            for (int i = 0; i < response.Columns.Count; i++)
                            {
                                if (response.Data.Count <= i)
                                    response.Data.Add(new DifferentiatorData { DifferentiatorName = response.Columns[i], Value = new List<string>() });

                                if (!string.IsNullOrEmpty(reader.GetValue(i)?.ToString().Trim()))
                                {
                                    var cellValue = reader.GetValue(i)?.ToString().Trim() ?? string.Empty;
                                    if (!response.Data[i].Value.Contains(cellValue))
                                    {
                                        response.Data[i].Value.Add(cellValue);
                                    }
                                }
                            }
                        }
                    }
                }

                var jobId = BackgroundJob.Enqueue(() => _interface.ImportDifferentiator(import_name, response, token, material_id));
                aPIResponseDTO.success = true;
                aPIResponseDTO.message = "File is valid. Import started.";
                aPIResponseDTO.data = jobId;

                return Ok(aPIResponseDTO);
            }
            catch (Exception ex)
            {
                aPIResponseDTO.message = $"Error processing file: {ex.Message}";
                return aPIResponseDTO;
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetDifferentiatorMappingById(int id)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            var token = Utils.ExtractTokenDetails(HttpContext, _context);

            if (token != null)
            {
                aPIResponseDTO = await _interface.GetDifferentiatorMappingById(id);
            }
            else
            {
                return Unauthorized();
            }

            return Ok(aPIResponseDTO);
        }

        [HttpGet("differentiator_mapping_by_material/{id}")]
        public async Task<ActionResult> GetDifferentiatorMappingByMaterialId(int id)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            var token = Utils.ExtractTokenDetails(HttpContext, _context);

            if (token != null)
            {
                aPIResponseDTO = await _interface.GetDifferentiatorMappingByMaterialId(id);
            }
            else
            {
                return Unauthorized();
            }

            return Ok(aPIResponseDTO);
        }
    }
}