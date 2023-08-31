using FileSharing.DAL.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSharing.DAL.Interfaces
{
    public interface IUnitOfWork
    {
        public IAccountRepository Accounts { get; }
        public IFilesRepository Files { get; }
        public ICategoryRepository Categories { get; }
        public SettingsService Settings { get; }
        public bool CanConnect { get; }
        public Task CommitAsync();
    }
}
