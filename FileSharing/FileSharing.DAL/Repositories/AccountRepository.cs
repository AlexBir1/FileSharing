using FileSharing.DAL.Base;
using FileSharing.DAL.Entity;
using FileSharing.DAL.Interfaces;
using FileSharing.DAL.Services;
using FileSharing.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace FileSharing.DAL.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AppDBContext _db;
        private readonly UserManager<Account> _userManager;

        public AccountRepository(AppDBContext db, UserManager<Account> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IBaseResponse<Account>> ChangeRole(string accountId, string role)
        {
            try
            {
                var account = await _userManager.FindByIdAsync(accountId);
                var roles = await _userManager.GetRolesAsync(account);

                if(roles.Any())
                    await _userManager.RemoveFromRoleAsync(account, roles.First());

                var accountRole = RoleHandler.GetRoleByName(role);
                if (accountRole != null)
                {
                    await _userManager.AddToRoleAsync(account, accountRole.ToString());
                    return new Response<Account>(account, new List<string>(), true);
                }
                return new Response<Account>(account, new List<string>() { new string("Role does not exist") }, false);
            }
            catch (Exception ex)
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new Response<Account>(null, errors, false);
            }
        }

        public async Task<IBaseResponse<Account>> Create(Account Entity)
        {
            try
            {
                var Result = await _userManager.CreateAsync(Entity);

                Account newAccount = await _userManager.FindByNameAsync(Entity.UserName);

                if (Result.Succeeded)
                {
                    return new Response<Account>(Entity, new List<string>(), true);
                }

                List<string> Errors = new List<string>();
                foreach (var i in Result.Errors)
                {
                    Errors.Add(i.Description);
                }

                return new Response<Account>(Entity, Errors, false);
            }
            catch (Exception ex)
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new Response<Account>(Entity, errors, false);
            }
        }

        public async Task<IBaseResponse<Account>> Delete(string Id)
        {
            try
            {
                var Account = await _userManager.FindByIdAsync(Id);
                var Result = await _userManager.DeleteAsync(Account);
                return new Response<Account>(Account, new List<string>(), false);
            }
            catch (Exception ex)
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new Response<Account>(null, errors, false);
            }
        }

        public async Task<IBaseResponse<string[]>> GetAccountRoles(string accountId)
        {
            try
            {
                var account = await _userManager.FindByIdAsync(accountId);
                var roles = await _userManager.GetRolesAsync(account);

                return new Response<string[]>(roles.ToArray(), null, true);
            }
            catch(Exception ex) 
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new Response<string[]>(new string[0], errors, false);
            }
        }

        public async Task<IBaseResponse<IEnumerable<Account>>> Select()
        {
            try
            {
                var Result = await _userManager.Users.ToListAsync();
                if (Result.Count == 0)
                {
                    List<string> errors = new List<string>()
                {
                    new string("No accounts were found"),
                };
                    return new Response<IEnumerable<Account>>(Result, errors, false);
                }
                return new Response<IEnumerable<Account>>(Result, new List<string>(), true);
            }
            catch (Exception ex)
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new Response<IEnumerable<Account>>(new List<Account>(), errors, false);
            }
        }

        public async Task<IBaseResponse<IEnumerable<Account>>> Select(Expression<Func<Account, bool>> func)
        {
            try
            {
                var Result = await _userManager.Users.Where(func).ToListAsync();
                if (Result.Count == 0)
                {
                    List<string> errors = new List<string>()
                {
                    new string("No accounts were found"),
                };
                    return new Response<IEnumerable<Account>>(Result, errors, false);
                }
                return new Response<IEnumerable<Account>>(Result, new List<string>(), true);
            }
            catch (Exception ex)
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new Response<IEnumerable<Account>>(new List<Account>(), errors, false);
            }
        }

        public Task<IBaseResponse<IEnumerable<Account>>> Select(Expression<Func<Account>> expression)
        {
            throw new NotImplementedException();
        }

        public async Task<IBaseResponse<Account>> SetPassword(string Id, string password)
        {
            try
            {
                var account = await _userManager.FindByIdAsync(Id);

                if (account == null)
                    return new Response<Account>(null, new List<string>() { new string("Account does not exist") }, false);

                var Result = await _userManager.AddPasswordAsync(account, password);

                if (Result.Succeeded)
                {
                    return new Response<Account>(account, new List<string>(), true);
                }
                return new Response<Account>(account, Result.Errors.Select(x => x.Description).ToList(), false);
            }
            catch (Exception ex)
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new Response<Account>(null, errors, false);
            }
        }

        public async Task<IBaseResponse<Account>> Update(string Id, Account Entity)
        {
            try
            {
                var Account = await _userManager.FindByIdAsync(Id);
                if (Account != null)
                {
                    var Result = await _userManager.UpdateAsync(Entity);
                    if (Result.Succeeded)
                    {
                        return new Response<Account>(Account, new List<string>(), true);
                    }
                    return new Response<Account>(Account, Result.Errors.Select(x => x.Description).ToList(), false);
                }

                return new Response<Account>(Account, new List<string>() { new string("Cannot update account that does not exist") }, false);
            }
            catch (Exception ex)
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new Response<Account>(null, errors, false);
            }
        }

        public async Task<IBaseResponse<Account>> UpdateFileActivity(string accountId, long addToTotalSize)
        {
            try
            {
                var account = await _userManager.FindByIdAsync(accountId);

                if (account != null)
                {
                    account.AddTotalSize(addToTotalSize);
                    account.IncrementUploadedCounter();

                    var Result = await _userManager.UpdateAsync(account);
                    if (Result.Succeeded)
                    {
                        return new Response<Account>(account, new List<string>(), true);
                    }
                    return new Response<Account>(account, Result.Errors.Select(x => x.Description).ToList(), false);
                }

                return new Response<Account>(account, new List<string>() { new string("Cannot update account that does not exist") }, false);
            }
            catch (Exception ex)
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new Response<Account>(null, errors, false);
            }
        }
    }
}
