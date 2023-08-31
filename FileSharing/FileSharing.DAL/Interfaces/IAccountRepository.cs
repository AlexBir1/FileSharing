using FileSharing.DAL.Base;
using FileSharing.DAL.Entity;
using FileSharing.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSharing.DAL.Interfaces
{
    public interface IAccountRepository : IBaseRepository<Account>
    {
        Task<IBaseResponse<Account>> SetPassword(string Id, string password);
        Task<IBaseResponse<Account>> ChangeRole(string accountId, string role);
    }
}
