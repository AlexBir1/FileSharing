using FileSharing.DAL.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSharing.DAL.Interfaces
{
    public interface IFilesRepository : IBaseRepository<Entity.FileInfo>
    {
        Task<IBaseResponse<IEnumerable<Entity.FileInfo>>> SelectRandom(int quantity);
        Task<IBaseResponse<IEnumerable<Entity.FileInfo>>> SelectMostDownloaded(int quantity);
        Task<IBaseResponse<IEnumerable<Entity.FileInfo>>> SelectLastUploaded(int quantity);
    }
}
