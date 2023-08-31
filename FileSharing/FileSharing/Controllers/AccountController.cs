using AutoMapper;
using FileSharing.DAL.Base;
using FileSharing.DAL.Entity;
using FileSharing.DAL.Interfaces;
using FileSharing.DAL.Repositories;
using FileSharing.DAL.Services;
using FileSharing.Models;
using FileSharing.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Principal;

namespace FileSharing.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly SignInManager<Account> _signInManager;
        private readonly JWTService _jwtService;
        private readonly UserManager<Account> _userManager;
        private readonly IUnitOfWork _unit;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;

        public AccountController(SignInManager<Account> signInManager, JWTService jwtService, IUnitOfWork unit,
            IConfiguration config, UserManager<Account> userManager, RoleManager<IdentityRole> roleManager, IMapper mapper)
        {
            _signInManager = signInManager;
            _jwtService = jwtService;
            _unit = unit;
            _config = config;
            _userManager = userManager;
            _mapper = mapper;
        }

        [Authorize]
        [HttpGet("refreshTken")]
        public async Task<ActionResult<AccountModel>> RefreshToken()
        {
            var Account = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var rolesList = await _userManager.GetRolesAsync(Account);
            return new AccountModel()
            {
                Id = Account.Id,
                Email = Account.Email,
                JWTToken = await _jwtService.CreateToken(new JWTTokenProperty(_config["JWT:Key"], _config["JWT:Issuer"], int.Parse(_config["JWT:ExpiresInDays"]), Account)),
                RegistrationDate = Account.RegistrationDate,
                Username = Account.UserName,
                Roles = rolesList.ToArray(),
            };
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("GetAccounts")]
        public async Task<ActionResult<ResponseModel<AccountModel[]>>> GetAccounts()
        {
            try
            {
                var accounts = await _unit.Accounts.Select();
                if (accounts.IsSuccessful)
                {
                    var accountModels = new List<AccountModel>();
                    foreach (var account in accounts.Data)
                    {
                        accountModels.Add(_mapper.Map<AccountModel>(account));
                        var roles = await _userManager.GetRolesAsync(account);
                        accountModels.First(x => x.Id == account.Id).Roles = roles.ToArray();
                    }
                    return new ResponseModel<AccountModel[]>
                    {
                        IsSuccessful = true,
                        Data = accountModels.ToArray()
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
                var accountResult = await _unit.Accounts.Select(x => x.Id == accountId);
                if (accountResult.IsSuccessful)
                {
                    var account = accountResult.Data.First();
                    var accountInfoModel = new AccountInfoModel()
                    {
                        FilesDownloaded = account.FilesDownloaded,
                        FilesUploaded = account.FilesUploaded,
                        TotalSizeProcessed = account.TotalSizeProcessed,
                    };
                    return new ResponseModel<AccountInfoModel>()
                    {
                        Data = accountInfoModel,
                        IsSuccessful = true,
                    };
                }
                return new ResponseModel<AccountInfoModel>()
                {
                    Errors = new string[] { new string("No account additional info") },
                    IsSuccessful = false,
                };
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
            var accounts = await _unit.Accounts.Select(x => x.Id == accountId);
            if (accounts.Data.Count() > 0)
            {
                var account = accounts.Data.FirstOrDefault();
                var rolesList = await _userManager.GetRolesAsync(account);
                return new ResponseModel<AccountModel>()
                {
                    Data = new AccountModel()
                    {
                        Id = account.Id,
                        Email = account.Email,
                        RegistrationDate = account.RegistrationDate,
                        Username = account.UserName,
                        JWTToken = await _jwtService.CreateToken(new JWTTokenProperty(_config["JWT:Key"], _config["JWT:Issuer"], int.Parse(_config["JWT:ExpiresInDays"]), account)),
                        Roles = rolesList.ToArray(),
                    },
                    IsSuccessful = true,
                };
            }
            return new ResponseModel<AccountModel>
            {
                IsSuccessful = false,
                Errors = new string[] { new string("No such account") }
            };
        }

        [Authorize]
        [HttpPatch("UpdateAccount")]
        public async Task<ActionResult<ResponseModel<AccountModel>>> UpdateAccount([FromBody] AccountModel model)
        {
            try
            {
                var account = await _unit.Accounts.Select(l => l.Id == model.Id);

                if (account.IsSuccessful)
                {
                    var updatedAccount = account.Data.FirstOrDefault();

                    if (model.Username == updatedAccount.UserName || model.Email == updatedAccount.Email)
                        return new ResponseModel<AccountModel>
                        {
                            IsSuccessful = false,
                            Data = null,
                            Errors = new string[] { new string("Either old email or username equals to its new value.") },
                        };

                    string token = await _userManager.GenerateChangeEmailTokenAsync(updatedAccount, model.Email);
                    await _userManager.ChangeEmailAsync(updatedAccount, model.Email, token);
                    await _userManager.SetUserNameAsync(updatedAccount, model.Username);
                    await _unit.CommitAsync();

                    var newAccount = await _unit.Accounts.Select(l => l.Id == model.Id);

                    return new ResponseModel<AccountModel>
                    {
                        IsSuccessful = true,
                        Data = null,
                    };
                }
                return new ResponseModel<AccountModel>
                {
                    IsSuccessful = false,
                    Data = null,
                    Errors = account.Errors.ToArray(),
                };

            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPatch("SetPassword/{id}")]
        public async Task<ActionResult<ResponseModel<AccountModel>>> SetPassword(string id, [FromBody] string newPassword)
        {
            try
            {
                var accountResult = await _unit.Accounts.Select(x => x.Id == id);
                if (accountResult.IsSuccessful)
                {
                    var account = accountResult.Data.FirstOrDefault();
                    var setPasswordResult = await _unit.Accounts.SetPassword(id, newPassword);
                    if (setPasswordResult.IsSuccessful)
                    {
                        return new ResponseModel<AccountModel>
                        {
                            IsSuccessful = true,
                            Data = _mapper.Map<AccountModel>(account),
                        };
                    }
                    return new ResponseModel<AccountModel>
                    {
                        IsSuccessful = false,
                        Errors = setPasswordResult.Errors.ToArray(),
                    };
                }
                return new ResponseModel<AccountModel>
                {
                    IsSuccessful = false,
                    Errors = accountResult.Errors.ToArray(),
                };
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
                var accountResult = await _unit.Accounts.Select(a => a.Id == accountId);
                if (accountResult.IsSuccessful)
                {
                    var account = accountResult.Data.First();
                    var removalResult = await _userManager.DeleteAsync(account);

                    if (removalResult.Succeeded)
                        return new ResponseModel<AccountModel>
                        {
                            IsSuccessful = true,
                            Data = _mapper.Map<AccountModel>(account),
                        };
                    return new ResponseModel<AccountModel>
                    {
                        IsSuccessful = false,
                        Errors = removalResult.Errors.Select(x => x.Description).ToArray(),
                    };
                }
                return new ResponseModel<AccountModel>
                {
                    IsSuccessful = false,
                    Errors = accountResult.Errors.ToArray(),
                };
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

                var result = await _unit.Accounts.ChangeRole(model.accountId, model.role.ToString());
                if(result.IsSuccessful)
                {
                    var account = _mapper.Map<AccountModel>(result.Data);
                    account.Roles = new string[1] { new string(model.role) };
                    return new ResponseModel<AccountModel>
                    {
                        Data = account,
                        IsSuccessful = true,
                    };
                }
                return new ResponseModel<AccountModel>
                {
                    Errors = result.Errors.ToArray(),
                    IsSuccessful = false,
                };
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
                var roles = RoleHandler.GetAllRoles();
                return new ResponseModel<string[]> 
                {
                    IsSuccessful = true,
                    Data = roles.Select(x => x.ToString()).ToArray(),
                };
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("SignIn")]
        public async Task<ActionResult<ResponseModel<AccountModel>>> SignIn([FromBody] LoginModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var accounts = await _unit.Accounts.Select(x => x.UserName == model.someid || x.Email == model.someid);
                    if (accounts.Data.Count() == 0)
                    {
                        return new ResponseModel<AccountModel>
                        {
                            Data = null,
                            IsSuccessful = false,
                            Errors = new string[] { new string("Account not found") }
                        };
                    }

                    var account = accounts.Data.FirstOrDefault();

                    if (string.IsNullOrEmpty(account.PasswordHash))
                    {
                        return new ResponseModel<AccountModel>
                        {
                            Data = null,
                            IsSuccessful = false,
                            Errors = new string[] { new string("Password is not set. Cannot sign in.") }
                        };
                    }

                    var a = await _signInManager.CheckPasswordSignInAsync(account, model.Password, false);
                    var rolesList = await _userManager.GetRolesAsync(account);

                    var accountModel = new AccountModel()
                    {
                        Id = account.Id,
                        Email = account.Email,
                        Username = account.UserName,
                        RegistrationDate = account.RegistrationDate,
                        JWTToken = await _jwtService.CreateToken(new JWTTokenProperty(_config["JWT:Key"], _config["JWT:Issuer"], int.Parse(_config["JWT:ExpiresInDays"]), account)),
                        Roles = rolesList.ToArray(),
                    };
                    return new ResponseModel<AccountModel>
                    {
                        Data = accountModel,
                        IsSuccessful = true,
                        Errors = new string[] { }
                    };
                }
                return new ResponseModel<AccountModel>
                {
                    Data = null,
                    IsSuccessful = false,
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }

        }

        [HttpPost("SignUp")]
        public async Task<ResponseModel<AccountModel>> SignUp([FromBody] RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var accounts = await _unit.Accounts.Select();
                var account = accounts.Data.Where(l => l.UserName == model.Username || l.Email == model.Email).FirstOrDefault();
                if (account == null)
                {
                    var newAccount = new Account()
                    {
                        UserName = model.Username,
                        Email = model.Email,
                        FilesDownloaded = 0,
                        FilesUploaded = 0,
                        TotalSizeProcessed = 0,
                    };
                    List<CRUDOptions> options = new List<CRUDOptions>()
                        {
                            new CRUDOptions("Password", model.PasswordConfirm)
                        };

                    var Result = await _unit.Accounts.Create(newAccount, options);
                    if (Result.IsSuccessful)
                    {
                        await _userManager.AddToRoleAsync(Result.Data, AccountRoles.User.ToString());
                        await _signInManager.CheckPasswordSignInAsync(Result.Data, model.PasswordConfirm, false);

                        var rolesList = await _userManager.GetRolesAsync(Result.Data);

                        return new ResponseModel<AccountModel>()
                        {
                            Data = new AccountModel()
                            {
                                Id = Result.Data.Id,
                                Email = Result.Data.Email,
                                Username = Result.Data.UserName,
                                JWTToken = await _jwtService.CreateToken(new JWTTokenProperty(_config["JWT:Key"], _config["JWT:Issuer"], int.Parse(_config["JWT:ExpiresInDays"]), Result.Data)),
                                RegistrationDate = Result.Data.RegistrationDate,
                                Roles = rolesList.ToArray()
                            },
                            IsSuccessful = true
                        };
                    }
                    return new ResponseModel<AccountModel>
                    {
                        Data = new AccountModel()
                        {
                            Id = Result.Data.Id,
                            Email = Result.Data.Email,
                            Username = Result.Data.UserName,
                        },
                        IsSuccessful = false,
                        Errors = Result.Errors.ToArray()
                    };
                }
                return new ResponseModel<AccountModel>
                {
                    Data = null,
                    IsSuccessful = false,
                    Errors = new string[]
                    {
                        new string("Account already exists."),
                    }
                };
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
