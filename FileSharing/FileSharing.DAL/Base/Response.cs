using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSharing.DAL.Base
{
    public class Response<T> : IBaseResponse<T>
    {
        public T Data { get; set; }
        public IEnumerable<string> Errors { get; set; }
        public bool IsSuccessful { get; set; }

        public Response(T Data, IEnumerable<string> Errors, bool IsSuccessful) 
        {
            this.Data = Data;
            this.Errors = Errors;
            this.IsSuccessful = IsSuccessful;
        }   
    }
}
