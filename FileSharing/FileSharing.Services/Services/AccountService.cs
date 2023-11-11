using AutoMapper;
using FileSharing.DAL.Base;
using FileSharing.DAL.Entity;
using FileSharing.DAL.Interfaces;
using FileSharing.DAL.Services;
using FileSharing.Services.Interfaces;
using FileSharing.Shared;
using FileSharing.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FileSharing.Services.Services
{
    public class AccountService : IAccountService
    {
        private readonly IConfiguration _config;
        private readonly IUnitOfWork _unit;
        private readonly IMapper _mapper;

        public AccountService(IConfiguration config, IUnitOfWork unit, IMapper mapper)
        {
            _config = config;
            _unit = unit;
            _mapper = mapper;
        }

        public async Task<ResponseModel<AccountModel>> ChangeRole(AccountRoleModel model)
        {
            try
            {
                var result = await _unit.Accounts.ChangeRole(model.accountId, model.role.ToString());
                if (result.IsSuccessful)
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
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new ResponseModel<AccountModel>()
                {
                    IsSuccessful = false,
                    Data = null,
                    Errors = errors.ToArray(),
                };
            }
        }

        public async Task<ResponseModel<AccountModel>> Create(AccountModel Entity)
        {
            throw new NotImplementedException();
        }

        public async Task<ResponseModel<AccountModel>> Delete(string Id)
        {
            try
            {
                var result = await _unit.Accounts.Delete(Id);
                if (result.IsSuccessful)
                {
                    return new ResponseModel<AccountModel>()
                    {
                        IsSuccessful = result.IsSuccessful,
                        Data = _mapper.Map<AccountModel>(result.Data),
                        Errors = null,
                    };
                }
                return new ResponseModel<AccountModel>()
                {
                    IsSuccessful = result.IsSuccessful,
                    Data = null,
                    Errors = result.Errors.ToArray(),
                };
            }
            catch (Exception ex)
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new ResponseModel<AccountModel>()
                {
                    IsSuccessful = false,
                    Data = null,
                    Errors = errors.ToArray(),
                };
            }
        }

        public async Task<ResponseModel<AccountInfoModel>> GetAdditionalInfo(string accountId)
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
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new ResponseModel<AccountInfoModel>()
                {
                    IsSuccessful = false,
                    Data = null,
                    Errors = errors.ToArray(),
                };
            }
        }

        public async Task<ResponseModel<string[]>> GetAvailableRoles()
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
            catch (Exception ex)
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new ResponseModel<string[]>()
                {
                    IsSuccessful = false,
                    Data = null,
                    Errors = errors.ToArray(),
                };
            }
        }

        public async Task<AccountModel> RefreshToken(string accountId, IConfiguration config)
        {
            try
            {
                var Accounts = await _unit.Accounts.Select(x => x.Id == accountId);
                var rolesList = await _unit.Accounts.GetAccountRoles(accountId);

                var account = Accounts.Data.First();

                return new AccountModel()
                {
                    Id = account.Id,
                    Email = account.Email,
                    JWTToken = await _unit.JWT.CreateToken(new JWTTokenProperty(_config["JWT:Key"], _config["JWT:Issuer"], int.Parse(_config["JWT:ExpiresInDays"]), account)),
                    RegistrationDate = account.RegistrationDate,
                    Username = account.UserName,
                    Roles = rolesList.Data.ToArray(),
                };
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<ResponseModel<IEnumerable<AccountModel>>> Select()
        {
            throw new NotImplementedException();
        }

        public async Task<ResponseModel<IEnumerable<AccountModel>>> Select(Expression<Func<AccountModel, bool>> expression)
        {
            throw new NotImplementedException();
        }

        public async Task<ResponseModel<AccountModel>> SignIn(LoginModel model, IConfiguration config)
        {
            try
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

                var rolesList = await _unit.Accounts.GetAccountRoles(account.Id);

                var accountModel = new AccountModel()
                {
                    Id = account.Id,
                    Email = account.Email,
                    Username = account.UserName,
                    RegistrationDate = account.RegistrationDate,
                    JWTToken = await _unit.JWT.CreateToken(new JWTTokenProperty(_config["JWT:Key"], _config["JWT:Issuer"], int.Parse(_config["JWT:ExpiresInDays"]), account)),
                    Roles = rolesList.Data.ToArray(),
                };
                return new ResponseModel<AccountModel>
                {
                    Data = accountModel,
                    IsSuccessful = true,
                    Errors = new string[] { }
                };
            }
            catch(Exception ex)
            {
                return new ResponseModel<AccountModel>
                {
                    Data = null,
                    IsSuccessful = false,
                    Errors = new string[]
                    {
                        ex.Message
                    }
                };
            }
        }

        public async Task<ResponseModel<AccountModel>> SignUp(RegisterModel model, IConfiguration config)
        {
            try
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

                    var Result = await _unit.Accounts.Create(newAccount);
                    if (Result.IsSuccessful)
                    {
                        await _unit.Accounts.SetPassword(Result.Data.Id, model.Password);
                        await _unit.Accounts.ChangeRole(Result.Data.Id, AccountRoles.User.ToString());
                        var rolesList = await _unit.Accounts.GetAccountRoles(Result.Data.Id);

                        return new ResponseModel<AccountModel>()
                        {
                            Data = new AccountModel()
                            {
                                Id = Result.Data.Id,
                                Email = Result.Data.Email,
                                Username = Result.Data.UserName,
                                JWTToken = await _unit.JWT.CreateToken(new JWTTokenProperty(_config["JWT:Key"], _config["JWT:Issuer"], int.Parse(_config["JWT:ExpiresInDays"]), Result.Data)),
                                RegistrationDate = Result.Data.RegistrationDate,
                                Roles = rolesList.Data.ToArray()
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
            catch(Exception ex)
            {
                return new ResponseModel<AccountModel>
                {
                    Data = null,
                    IsSuccessful = false,
                    Errors = new string[]
                    {
                        ex.Message
                    }
                };
            }
        }

        public async Task<ResponseModel<AccountModel>> Update(string Id)
        {
            throw new NotImplementedException();
        }
    }
}
