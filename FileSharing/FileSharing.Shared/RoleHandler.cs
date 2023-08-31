using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSharing.Shared
{
    public static class RoleHandler
    {
        public static AccountRoles? GetRoleByName(string roleName)
        {
            try
            {
                var role = Enum.GetValues(typeof(AccountRoles)).Cast<AccountRoles>().First(x => x.ToString() == roleName);
                return role;
            }
            catch { return null; }
        }
        public static IEnumerable<AccountRoles> GetAllRoles()
        {
            var roles = Enum.GetValues(typeof(AccountRoles)).Cast<AccountRoles>();
            return roles;
        }
    }
}
