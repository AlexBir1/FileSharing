using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSharing.DAL.Entity
{
    public class JWTTokenProperty
    {
        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public int ExpiresInDays { get; set; }
        public Account? Account { get; set; }

        public JWTTokenProperty(string Key, string issuer, int expiresInDays, Account Account)
        {
            this.Key = Key;
            this.Account = Account;
            Issuer = issuer;
            ExpiresInDays = expiresInDays;
        }
    }
}
