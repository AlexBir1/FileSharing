using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSharing.DAL.Entity
{
    public class Settings
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public bool Value { get; set; }

        public string Account_Id { get; set; }
        public virtual Account Account { get; set; }
    }
}
