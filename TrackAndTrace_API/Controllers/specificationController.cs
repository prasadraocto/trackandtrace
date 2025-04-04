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
    public class specificationController : ControllerBase
    {
        private readonly ISpecificationRepository _interface;
        private readonly ApplicationDbContext _context;
        public specificationController(ISpecificationRepository Interface, ApplicationDbContext context)
        {
            _interface = Interface;
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult> Add([FromBody] SpecificationDto model)
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
        public async Task<ActionResult> GetSpecificationDifferentiatorMappingById(int id)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            var token = Utils.ExtractTokenDetails(HttpContext, _context);

            if (token != null)
            {
                aPIResponseDTO = await _interface.GetSpecificationDifferentiatorMappingById(id);
            }
            else
            {
                return Unauthorized();
            }

            return Ok(aPIResponseDTO);
        }

        [HttpPost("bulk_upload/{import_name}/{prefix}")]
        public async Task<ActionResult<APIResponseDTO>> BulkUpload(IFormFile file, [FromRoute] string import_name, [FromRoute] string prefix)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();
            var token = Utils.ExtractTokenDetails(HttpContext, _context);
            if (token == null)
            {
                return Unauthorized();
            }

            if (file == null)
            {
                aPIResponseDTO.message = "File is null";
                return aPIResponseDTO;
            }

            var allowedExtensions = new[] { ".xlsx", ".xls", ".xlsb", ".csv" };
            var extension = Path.GetExtension(file.FileName);
            if (!allowedExtensions.Contains(extension))
            {
                aPIResponseDTO.message = $"Invalid file extension. Allowed: {string.Join(", ", allowedExtensions)}";
                return aPIResponseDTO;
            }

            try
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);
                    stream.Position = 0;

                    IExcelDataReader reader = extension == ".csv" ? ExcelReaderFactory.CreateCsvReader(stream) : ExcelReaderFactory.CreateReader(stream);

                    using (reader)
                    {
                        List<string> columnNames = new List<string>();
                        List<List<SpecificationData>> dataRows = new List<List<SpecificationData>>();
                        bool isHeaderRow = true;

                        while (reader.Read())
                        {
                            if (isHeaderRow)
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    columnNames.Add(reader.GetValue(i)?.ToString()?.Trim() ?? "Column" + i);
                                }
                                isHeaderRow = false;
                            }
                            else
                            {
                                List<SpecificationData> rowData = new List<SpecificationData>();
                                for (int i = 0; i < columnNames.Count; i++)
                                {
                                    rowData.Add(new SpecificationData
                                    {
                                        Name = columnNames[i],
                                        Value = reader.GetValue(i)?.ToString()?.Trim() ?? string.Empty
                                    });
                                }
                                dataRows.Add(rowData);
                            }
                        }

                        var jsonResponse = new JsonResponse
                        {
                            Columns = columnNames,
                            Data = dataRows
                        };

                        // Enqueue the import task using Background Job
                        var jobId = BackgroundJob.Enqueue(() => _interface.ImportSpecification(jsonResponse, prefix, import_name, token));

                        aPIResponseDTO.success = true;
                        aPIResponseDTO.message = "File is Valid. Import Started";
                        aPIResponseDTO.data = jobId;  // Optionally, you can add the jobId to the response if needed
                    }
                }
            }
            catch (Exception ex)
            {
                aPIResponseDTO.message = "An error occurred while processing the file: " + ex.Message;
                return aPIResponseDTO;
            }

            return Ok(aPIResponseDTO);
        }
    }
}