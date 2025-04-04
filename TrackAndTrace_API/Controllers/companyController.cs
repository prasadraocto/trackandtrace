using TrackAndTrace_API.Interface;
using TrackAndTrace_API.Models.RequestModel;
using TrackAndTrace_API.Models.ResponseModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TrackAndTrace_API.Helpers;

namespace TrackAndTrace_API.Controllers
{
    [ApiController]
    //[Authorize]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class companyController : ControllerBase
    {
        private readonly ICompanyRepository _interface;
        public companyController(ICompanyRepository Interface)
        {
            _interface = Interface;
        }

        [HttpPost]
        public async Task<ActionResult> Add([FromBody] CompanyDto model)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            aPIResponseDTO = await _interface.Add(model);

            return Ok(aPIResponseDTO);
        }

        [HttpGet]
        public async Task<ActionResult> GetList([FromQuery] CommonRequestDto? request)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            aPIResponseDTO = await _interface.GetList(request);

            return Ok(aPIResponseDTO);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            aPIResponseDTO = await _interface.Delete(id);

            return Ok(aPIResponseDTO);
        }

        [HttpPut("active_inactive/{id}")]
        public async Task<ActionResult> ActiveInactive(int id)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            aPIResponseDTO = await _interface.ActiveInactive(id);

            return Ok(aPIResponseDTO);
        }
    }
}