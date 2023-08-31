using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSharing.DAL.Base
{
    public interface IBaseResponse<T>
    {
        public T Data { get; set; }
        public IEnumerable<string> Errors { get; set; }
        public bool IsSuccessful { get; set; }  
    }
}
