using FileSharing.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FileSharing.Services.Base
{
    public interface IBaseService<T>
    {
        Task<ResponseModel<T>> Create(T Entity);
        Task<ResponseModel<T>> Update(string Id);
        Task<ResponseModel<T>> Delete(string Id);
        Task<ResponseModel<IEnumerable<T>>> Select();
        Task<ResponseModel<IEnumerable<T>>> Select(Expression<Func<T, bool>> expression);
    }
}
