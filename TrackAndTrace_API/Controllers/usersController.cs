using TrackAndTrace_API.Interface;
using TrackAndTrace_API.Models.RequestModel;
using TrackAndTrace_API.Models.ResponseModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using TrackAndTrace_API.Helpers;
using TrackAndTrace_API.Models;

namespace TrackAndTrace_API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class usersController : ControllerBase
    {
        private readonly IUsersRepository _interface;
        private readonly ApplicationDbContext _context;
        private readonly IDesignationRepository _interfaceDesignation;
        public usersController(IUsersRepository Interface, ApplicationDbContext context, IDesignationRepository interfaceDesignation)
        {
            _interface = Interface;
            _context = context;
            _interfaceDesignation = interfaceDesignation;
        }

        [HttpPost]
        public async Task<ActionResult> Add([FromBody] UsersDto model)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            var token = Utils.ExtractTokenDetails(HttpContext, _context);

            if (token != null)
            {
                token.CompanyId = model.company_id > 0 ? model.company_id : token.CompanyId;

                if (model.designation_id == 0 && model.company_id > 0)
                {
                    var designation_id = await _context.Designation.Where(x => x.code == "CA" && x.company_id == model.company_id).Select(x => x.id).FirstOrDefaultAsync();

                    if (designation_id == 0)
                    {
                        DesignationDto designationDTO = new DesignationDto
                        {
                            id = 0,
                            code = "CA",
                            name = "Company Admin",
                            role_id = _context.Roles.Where(x => x.name == "COMPANY_ADMIN").Select(x => x.id).First(),
                            active_flag = true
                        };

                        var result = await _interfaceDesignation.Add(designationDTO, token);
                        if (result.success == true && result.data != null)
                        {
                            model.designation_id = Convert.ToInt32(result.data);
                        }
                    }
                    else
                    {
                        model.designation_id = designation_id;
                    }
                }

                if (model.designation_id != 0)
                {
                    aPIResponseDTO = await _interface.Add(model, token);
                }
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

        [HttpGet("company_drop_down_list")]
        public async Task<ActionResult> GetCompanyDropDownList()
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            var token = Utils.ExtractTokenDetails(HttpContext, _context);

            if (token != null)
            {
                aPIResponseDTO = await _interface.GetCompanyDropDownList(token);
            }
            else
            {
                return Unauthorized();
            }

            return Ok(aPIResponseDTO);
        }

        [HttpGet("extract_password")]
        public async Task<ActionResult> ExtractPassword([FromBody] string password)
        {
            var extractedPassword = Utils.Decrypt(password);

            return Ok(extractedPassword);
        }
    }
}