using FileSharing.DAL.Entity;
using FileSharing.DAL.Interfaces;
using FileSharing.DAL.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSharing.DAL.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        public IAccountRepository Accounts { get; private set; }
        public IFilesRepository Files { get; private set; }
        public ICategoryRepository Categories { get; private set; }
        public bool CanConnect { get; private set; }
        public ISettingsRepository Settings { get; private set; }
        public JWTService JWT { get; private set; }

        public AppDBContext _db { get; private set; }

        private readonly UserManager<Account> _userManager;

        public UnitOfWork(AppDBContext db, UserManager<Account> userManager)
        {
            _db = db;
            _userManager = userManager;
            CanConnect = _db.Database.CanConnect();
            Files = new FilesRepository(db, _userManager);
            Categories = new CategoryRepository(db);
            Settings = new SettingsRepository(db, _userManager);
            Accounts = new AccountRepository(db, _userManager);
            JWT = new JWTService(userManager);
        }

        public async Task CommitAsync()
        {
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                
            }
        }
    }
}
