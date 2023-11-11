
using FileSharing.Services.Base;
using FileSharing.Shared.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSharing.Services.Interfaces
{
    public interface IAccountService : IBaseService<AccountModel>
    {
        Task<AccountModel> RefreshToken(string accountId, IConfiguration config);
        Task<ResponseModel<AccountInfoModel>> GetAdditionalInfo(string accountId);
        Task<ResponseModel<AccountModel>> ChangeRole(AccountRoleModel model);
        Task<ResponseModel<AccountModel>> SignIn(LoginModel model, IConfiguration config);
        Task<ResponseModel<AccountModel>> SignUp(RegisterModel model, IConfiguration config);
        Task<ResponseModel<string[]>> GetAvailableRoles();
    }
}
