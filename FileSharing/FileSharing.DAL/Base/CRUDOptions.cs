using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSharing.DAL.Base
{
    public class CRUDOptions
    {
        public string Id { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public object? Object { get; set; } 

        public CRUDOptions(string Id, string Value) 
        { 
            this.Id = Id;
            this.Value = Value;
        }
    }
}
