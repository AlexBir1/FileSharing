using AutoMapper;
using FileSharing.DAL.Entity;
using FileSharing.DAL.Interfaces;
using FileSharing.DAL.Services;
using FileSharing.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FileSharing.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        

        public SettingsController(IUnitOfWork unitOfWork, IMapper mapper, SettingsService settingsService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet("equalizeSettingsFileWithDB")]
        public async Task<ActionResult<ResponseModel<IEnumerable<SettingsModel>>>> EqualizeSettingsFileWithDB()
        {
            try
            {
                var result = await _unitOfWork.Settings.EqualizeSettingsWithDB();
                if (result.IsSuccessful)
                {
                    await _unitOfWork.CommitAsync();
                    return new ResponseModel<IEnumerable<SettingsModel>>()
                    {
                        IsSuccessful = result.IsSuccessful,
                    };
                }
                return new ResponseModel<IEnumerable<SettingsModel>>()
                {
                    IsSuccessful = result.IsSuccessful,
                    Errors = result.Errors.ToArray(),
                };
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
                var settingsResult = await _unitOfWork.Settings.GetSettings(account_id);
                if (settingsResult.IsSuccessful)
                {
                    var settingsList = new List<SettingsModel>();
                    foreach (var item in settingsResult.Data)
                    {
                        var setting = _mapper.Map<SettingsModel>(item);
                        settingsList.Add(setting);
                    }
                    return new ResponseModel<IEnumerable<SettingsModel>>
                    {
                        IsSuccessful = true,
                        Data = settingsList,
                    };
                }
                return new ResponseModel<IEnumerable<SettingsModel>>
                {
                    IsSuccessful = false,
                    Errors = settingsResult.Errors.ToArray(),
                };
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
                var settingsList = new List<Settings>();
                foreach (var item in model)
                {
                    var setting = _mapper.Map<Settings>(item);
                    settingsList.Add(setting);
                }
                var Result = await _unitOfWork.Settings.SetSettings(settingsList);
                if (Result.IsSuccessful)
                {
                    await _unitOfWork.CommitAsync();

                    var settings = new List<SettingsModel>();
                    foreach (var item in settingsList)
                    {
                        settings.Add(_mapper.Map<SettingsModel>(item));
                    }

                    return new ResponseModel<IEnumerable<SettingsModel>>
                    {
                        IsSuccessful = true,
                        Data = settings.ToArray()
                    };
                }
                return new ResponseModel<IEnumerable<SettingsModel>>
                {
                    IsSuccessful = false,
                    Errors = Result.Errors.ToArray(),
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}
