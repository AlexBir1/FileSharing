using AutoMapper;
using FileSharing.DAL.Entity;
using FileSharing.DAL.Interfaces;
using FileSharing.DAL.Services;
using FileSharing.Services.Interfaces;
using FileSharing.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

namespace FileSharing.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private readonly ISettingsService _service;
        private readonly IMapper _mapper;
        

        public SettingsController(ISettingsService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet("equalizeSettingsFileWithDB")]
        public async Task<ActionResult<ResponseModel<IEnumerable<SettingsModel>>>> EqualizeSettingsFileWithDB()
        {
            try
            {
                return Ok(await _service.EqualizeSettingsFileWithDB());
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("AddSettings")]
        public async Task<ActionResult<ResponseModel<IEnumerable<SettingsModel>>>> AddSettings([FromBody] IEnumerable<SettingsModel> model)
        {
            if(ModelState.IsValid)
            {
            }
            return new ResponseModel<IEnumerable<SettingsModel>>
            {
                IsSuccessful = false,
                Errors = ModelState.Where(x => x.Value.Errors.Count > 0).SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage).ToArray(),
            };
        }

        [Authorize]
        [HttpGet("GetSettings/{account_id}")]
        public async Task<ActionResult<ResponseModel<IEnumerable<SettingsModel>>>> GetSettings(string account_id)
        {
            try
            {
                return Ok(await _service.GetAccountSettings(account_id));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [Authorize]
        [HttpPatch("SetAccountSettings")]
        public async Task<ActionResult<ResponseModel<IEnumerable<SettingsModel>>>> SetSettings([FromBody] IEnumerable<SettingsModel> model)
        {
            try
            {
                return Ok(await _service.SetAccountSettings("", model));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}
