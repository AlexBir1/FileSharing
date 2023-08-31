using FileSharing.DAL.Base;
using FileSharing.DAL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSharing.DAL.Interfaces
{
    public interface ICategoryRepository : IBaseRepository<Category>
    {
        Task<IBaseResponse<Category>> GetDefaultDirectory();
    }
}
