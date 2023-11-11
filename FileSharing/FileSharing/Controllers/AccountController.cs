using AutoMapper;
using FileSharing.DAL.Base;
using FileSharing.DAL.Entity;
using FileSharing.DAL.Interfaces;
using FileSharing.DAL.Repositories;
using FileSharing.DAL.Services;
using FileSharing.Services.Interfaces;
using FileSharing.Shared;
using FileSharing.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using System.Security.Claims;
using System.Security.Principal;

namespace FileSharing.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IAccountService _service;

        public AccountController(IConfiguration config, IAccountService service)
        {
            _config = config;
            _service = service;
        }

        [Authorize]
        [HttpGet("refreshTken")]
        public async Task<ActionResult<AccountModel>> RefreshToken()
        {
            return Ok(await _service.RefreshToken(User.FindFirstValue(ClaimTypes.NameIdentifier), _config));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("GetAccounts")]
        public async Task<ActionResult<ResponseModel<AccountModel[]>>> GetAccounts()
        {
            try
            {
                var accounts = await _service.Select();
                if (accounts.IsSuccessful)
                {
                    return new ResponseModel<AccountModel[]>
                    {
                        IsSuccessful = true,
                        Errors = null,
                        Data = accounts.Data.ToArray()
                    };
                }
                return new ResponseModel<AccountModel[]>
                {
                    IsSuccessful = false,
                    Errors = accounts.Errors.ToArray()
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [Authorize]
        [HttpGet("AdditionalInfo/{accountId}")]
        public async Task<ActionResult<ResponseModel<AccountInfoModel>>> GetAdditionalInfo(string accountId)
        {
            try
            {
                return Ok(await _service.GetAdditionalInfo(accountId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [Authorize]
        [HttpGet("GetAccount")]
        public async Task<ActionResult<ResponseModel<AccountModel>>> GetAccount(string accountId)
        {
            return BadRequest();
        }

        [Authorize]
        [HttpPatch("UpdateAccount")]
        public async Task<ActionResult<ResponseModel<AccountModel>>> UpdateAccount([FromBody] AccountModel model)
        {
            try
            {

                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteAccount/{accountId}")]
        public async Task<ActionResult<ResponseModel<AccountModel>>> DeleteAccount(string accountId)
        {
            try
            {
                return Ok(await _service.Delete(accountId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("ChangeRole")]
        public async Task<ActionResult<ResponseModel<AccountModel>>> ChangeRole(AccountRoleModel model)
        {
            try
            {
                return Ok(await _service.ChangeRole(model));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet("GetAvailableRoles")]
        public async Task<ActionResult<ResponseModel<string[]>>> GetAvailableRoles()
        {
            try
            {
                return Ok(await _service.GetAvailableRoles());
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("SignIn")]
        public async Task<ActionResult<ResponseModel<AccountModel>>> SignIn([FromBody] LoginModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    return Ok(await _service.SignIn(model, _config));
                }
                catch (Exception ex)
                {
                    return BadRequest(ex);
                }
            }
            return new ResponseModel<AccountModel>
            {
                Data = null,
                IsSuccessful = false,
                Errors = ModelState.Where(x => x.Value.Errors.Count > 0).SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage).ToArray(),
            };
        }

        [HttpPost("SignUp")]
        public async Task<ActionResult<ResponseModel<AccountModel>>> SignUp([FromBody] RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    return Ok(await _service.SignUp(model, _config));
                }
                catch (Exception ex)
                {
                    return BadRequest(ex);
                }
            }
            return new ResponseModel<AccountModel>
            {
                Data = null,
                IsSuccessful = false,
                Errors = ModelState.Where(x => x.Value.Errors.Count > 0).SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage).ToArray(),
            };
        }
    }
}
