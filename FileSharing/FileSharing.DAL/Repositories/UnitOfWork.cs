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
        public SettingsService Settings { get; private set; }

        public AppDBContext _db { get; private set; }

        private readonly UserManager<Account> _userManager;

        public UnitOfWork(AppDBContext db, UserManager<Account> userManager)
        {
            _db = db;
            _userManager = userManager;
            CanConnect = _db.Database.CanConnect();
            Files = new FilesRepository(_db, _userManager);
            Categories = new CategoryRepository(_db);
            Settings = new SettingsService(_db, _userManager);
            Accounts = new AccountRepository(_db, _userManager, Settings);
        }

        public async Task CommitAsync()
        {
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _db.ChangeTracker.Clear();
                foreach (var entry in ex.Entries)
                {
                    _db.Attach(entry);
                    _db.SaveChanges();
                }
            }
        }
    }
}
