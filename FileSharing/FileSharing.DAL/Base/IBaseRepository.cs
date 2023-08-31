using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FileSharing.DAL.Base
{
    public interface IBaseRepository<T>
    {
        Task<IBaseResponse<T>> Create(T Entity, IEnumerable<CRUDOptions> Options);
        Task<IBaseResponse<T>> Update(string Id, T Entity, IEnumerable<CRUDOptions> Options);
        Task<IBaseResponse<T>> Delete(string Id);
        Task<IBaseResponse<IEnumerable<T>>> Select();
        Task<IBaseResponse<IEnumerable<T>>> Select(Expression<Func<T>> expression);
        Task<IBaseResponse<IEnumerable<T>>> Select(Expression<Func<T,bool>> expression);

    }
}
