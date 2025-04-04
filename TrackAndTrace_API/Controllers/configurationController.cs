using TrackAndTrace_API.Helpers;
using TrackAndTrace_API.Interface;
using TrackAndTrace_API.Models;
using TrackAndTrace_API.Models.DBModel;
using TrackAndTrace_API.Models.RequestModel;
using TrackAndTrace_API.Models.ResponseModel;
using TrackAndTrace_API.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TrackAndTrace_API.Controllers
{
    [ApiController]
    //[Authorize]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class configurationController : ControllerBase
    {
        private readonly IConfigurationRepository _interface;
        public configurationController(IConfigurationRepository Interface)
        {
            _interface = Interface;
        }

        [HttpPost("menu")]
        public async Task<ActionResult> Menu([FromBody] MenuDto model)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            aPIResponseDTO = await _interface.AddMenu(model);

            return Ok(aPIResponseDTO);
        }

        [HttpGet("menu")]
        public async Task<ActionResult> GetMenuList([FromQuery] CommonRequestDto? request)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            aPIResponseDTO = await _interface.GetMenuList(request);

            return Ok(aPIResponseDTO);
        }

        [HttpDelete("menu/{id}")]
        public async Task<ActionResult> DeleteMenu(string id)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            aPIResponseDTO = await _interface.DeleteMenu(id);

            return Ok(aPIResponseDTO);
        }

        [HttpGet("menu_dropdown")]
        public async Task<ActionResult> GetMenuDropdownList()
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            aPIResponseDTO = await _interface.GetMenuDropdownList();

            return Ok(aPIResponseDTO);
        }

        [HttpPost("page")]
        public async Task<ActionResult> Page([FromBody] PageDto model)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            aPIResponseDTO = await _interface.AddPage(model);

            return Ok(aPIResponseDTO);
        }

        [HttpGet("page")]
        public async Task<ActionResult> GetPageList([FromQuery] CommonRequestDto? request)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            aPIResponseDTO = await _interface.GetPageList(request);

            return Ok(aPIResponseDTO);
        }

        [HttpDelete("page/{id}")]
        public async Task<ActionResult> DeletePage(string id)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            aPIResponseDTO = await _interface.DeletePage(id);

            return Ok(aPIResponseDTO);
        }

        [HttpPost("menu_page")]
        public async Task<ActionResult> MenuPage([FromBody] MenuPageMappingDto model)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            aPIResponseDTO = await _interface.AddMenuPage(model);

            return Ok(aPIResponseDTO);
        }

        [HttpGet("menu_page")]
        public async Task<ActionResult> GetMenuPageList([FromQuery] CommonRequestDto? request)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            aPIResponseDTO = await _interface.GetMenuPageList(request);

            return Ok(aPIResponseDTO);
        }

        [HttpDelete("menu_page/{id}")]
        public async Task<ActionResult> DeleteMenuPage(int id)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            aPIResponseDTO = await _interface.DeleteMenuPage(id);

            return Ok(aPIResponseDTO);
        }

        [HttpGet("page_dropdown/{menu_id}")]
        public async Task<ActionResult> GetPageDropdownList(int menu_id)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            aPIResponseDTO = await _interface.GetPageDropdownList(menu_id);

            return Ok(aPIResponseDTO);
        }

        [HttpPost("company_role_menu_page")]
        public async Task<ActionResult> CompanyRoleMenuPage([FromBody] CompanyRoleMenuPageMappingDto model)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            aPIResponseDTO = await _interface.AddCompanyRoleMenuPage(model);

            return Ok(aPIResponseDTO);
        }

        [HttpGet("company_role_menu_page")]
        public async Task<ActionResult> GetCompanyRoleMenuPageList([FromQuery] CommonRequestDto? request)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            aPIResponseDTO = await _interface.GetCompanyRoleMenuPageList(request);

            return Ok(aPIResponseDTO);
        }

        [HttpDelete("company_role_menu_page/{id}")]
        public async Task<ActionResult> DeleteCompanyRoleMenuPage(int id)
        {
            APIResponseDTO aPIResponseDTO = new APIResponseDTO();

            aPIResponseDTO = await _interface.DeleteCompanyRoleMenuPage(id);

            return Ok(aPIResponseDTO);
        }
    }
}