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
    public class injuryController : ControllerBase
    {
        private readonly IInjuryRepository _interface;
        private readonly ApplicationDbContext _context;
        public injuryController(IInjuryRepository Interface, ApplicationDbContext context)
        {
            _interface = Interface;
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult> Add([FromBody] InjuryDto model)
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

        [HttpPost("bulk_upload/{import_name}")]
        public async Task<ActionResult<APIResponseDTO>> BulkUpload(IFormFile file, [FromRoute] string import_name)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            var token = Utils.ExtractTokenDetails(HttpContext, _context);

            if (token != null)
            {

                var readingStarted = false;
                string result = string.Empty;

                if (file == null)
                {
                    aPIResponseDTO.message = "file is null";
                    return aPIResponseDTO;
                }

                var allowedExtensions = new[] { ".xlsx", ".xls", ".xlsb", ".csv" };
                var extension = Path.GetExtension(file.FileName);
                if (!allowedExtensions.Contains(extension))
                {
                    aPIResponseDTO.message = $"extension must contains {allowedExtensions.ToArray()}";
                    return aPIResponseDTO;
                }

                bool dataReadingStarted = false;
                string firstCellValue = string.Empty;

                try
                {
                    // Directly create the list of Type_Injury
                    var typeInjuryList = new List<Type_Injury>();

                    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                    using (var stream = new MemoryStream())
                    {
                        file.CopyTo(stream);

                        IExcelDataReader reader;
                        if (extension == ".csv")
                        {
                            reader = ExcelReaderFactory.CreateCsvReader(stream);
                        }
                        else
                        {
                            reader = ExcelReaderFactory.CreateReader(stream);
                        }

                        using (reader)
                        {
                            do
                            {
                                while (reader.Read())
                                {
                                    if ((reader.GetString(0) == "Code" || reader.GetString(1) == "Name") || !readingStarted)
                                    {
                                        if (reader.GetValue(0).ToString().Trim() != "Code" || reader.GetValue(1).ToString().Trim() != "Name")
                                        {
                                            aPIResponseDTO.message = "Please upload valid file";
                                            return aPIResponseDTO;
                                        }
                                        else
                                        {
                                            readingStarted = true;
                                            continue;
                                        }
                                    }

                                    //to validate if the fisrst cell value after retop reading is a valid value or not?
                                    if (dataReadingStarted == true && string.IsNullOrEmpty(reader.GetValue(0).ToString().Trim()))
                                    {
                                        aPIResponseDTO.message = "Invalid file: Data rows required";
                                        return aPIResponseDTO;
                                    }
                                    else
                                    {
                                        //reading column
                                        dataReadingStarted = true;

                                        typeInjuryList.Add(new Type_Injury
                                        {
                                            code = reader.GetValue(0) == null ? null : reader.GetValue(0).ToString(),
                                            name = reader.GetValue(1) == null ? null : reader.GetValue(1).ToString()
                                        });
                                    }
                                }

                            } while (reader.NextResult());
                        }
                    }

                    if (dataReadingStarted == true)
                    {
                        var jobId = BackgroundJob.Enqueue(() => _interface.ImportInjury(import_name, typeInjuryList, token));
                        aPIResponseDTO.success = true;
                        aPIResponseDTO.message = "File is Valid. Import Started";
                        aPIResponseDTO.data = jobId;
                    }

                    return Ok(aPIResponseDTO);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else
            {
                return Unauthorized();
            }
        }
    }
}