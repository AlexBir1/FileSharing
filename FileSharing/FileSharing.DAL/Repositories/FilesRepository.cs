using FileSharing.DAL.Base;
using FileSharing.DAL.Entity;
using FileSharing.DAL.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FileSharing.DAL.Repositories
{
    public class FilesRepository : IFilesRepository
    {
        private readonly AppDBContext _db;
        private readonly UserManager<Account> _userManager;

        public FilesRepository(AppDBContext db, UserManager<Account> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IBaseResponse<Entity.FileInfo>> Create(Entity.FileInfo Entity)
        {
            try
            {
                var Result = _db.Files.Add(Entity);
                if (Result != null)
                {
                    return new Response<Entity.FileInfo>(Result.Entity, new List<string>(), true);
                }
                return new Response<Entity.FileInfo>(Result.Entity, new List<string>(), false);
            }
            catch (Exception ex)
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new Response<Entity.FileInfo>(null, errors, false);
            }
        }

        public async Task<IBaseResponse<Entity.FileInfo>> Delete(string Id)
        {
            try
            {
                _db.Files.Remove(new Entity.FileInfo() { Id = int.Parse(Id) });
                return new Response<Entity.FileInfo>(null, new List<string>(), true);
            }
            catch (Exception ex)
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new Response<Entity.FileInfo>(null, errors, false);
            }
        }

        public async Task<IBaseResponse<IEnumerable<Entity.FileInfo>>> SelectRandom(int quantity)
        {
            try
            {
                var files = await _db.Files
                    .Include(x => x.Category)
                    .OrderBy(x => Guid.NewGuid())
                    .Take(quantity)
                    .ToListAsync();

                return new Response<IEnumerable<Entity.FileInfo>>(files, new List<string>(), true);
            }
            catch (Exception ex)
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new Response<IEnumerable<Entity.FileInfo>>(new List<Entity.FileInfo>(), errors, false);
            }
        }

        public async Task<IBaseResponse<IEnumerable<Entity.FileInfo>>> Select()
        {
            try
            {
                var Files = await _db.Files.Include(x => x.Category).ToListAsync();
                return new Response<IEnumerable<Entity.FileInfo>>(Files, new List<string>(), true);
            }
            catch (Exception ex)
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new Response<IEnumerable<Entity.FileInfo>>(new List<Entity.FileInfo>(), errors, false);
            }

        }

        public async Task<IBaseResponse<IEnumerable<Entity.FileInfo>>> Select(Expression<Func<Entity.FileInfo>> expression)
        {
            throw new NotImplementedException();
        }

        public async Task<IBaseResponse<IEnumerable<Entity.FileInfo>>> Select(Expression<Func<Entity.FileInfo, bool>> expression)
        {
            try
            {
                var Files = await _db.Files.Include(x => x.Category).Where(expression).AsNoTracking().ToListAsync();
                return new Response<IEnumerable<Entity.FileInfo>>(Files, new List<string>(), true);
            }
            catch (Exception ex)
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new Response<IEnumerable<Entity.FileInfo>>(new List<Entity.FileInfo>(), errors, false);
            }
        }

        public async Task<IBaseResponse<Entity.FileInfo>> Update(string Id, Entity.FileInfo Entity)
        {
            try
            {
                var fileFromDb = await _db.Files.AsNoTracking().FirstOrDefaultAsync(x => x.Id == int.Parse(Id));
                if (fileFromDb != null)
                {
                    _db.Files.Update(Entity);
                    return new Response<Entity.FileInfo>(Entity, new List<string>(), true);
                }
                return new Response<Entity.FileInfo>(Entity, new List<string>() { new string("File not found") }, false);
            }
            catch (Exception ex)
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new Response<Entity.FileInfo>(Entity, errors, false);
            }
        }

        public async Task<IBaseResponse<IEnumerable<Entity.FileInfo>>> SelectMostDownloaded(int quantity)
        {
            try
            {
                var files = await _db.Files
                    .Include(x => x.Category)
                    .OrderByDescending(x => x.DownloadCount)
                    .Take(quantity)
                    .ToListAsync();

                return new Response<IEnumerable<Entity.FileInfo>>(files, new List<string>(), true);
            }
            catch (Exception ex)
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new Response<IEnumerable<Entity.FileInfo>>(new List<Entity.FileInfo>(), errors, false);
            }
        }

        public async Task<IBaseResponse<IEnumerable<Entity.FileInfo>>> SelectLastUploaded(int quantity)
        {
            try
            {
                var files = await _db.Files
                    .Include(x => x.Category)
                    .OrderByDescending(x=>x.UploadDate)
                    .Take(quantity)
                    .ToListAsync();

                return new Response<IEnumerable<Entity.FileInfo>>(files, new List<string>(), true);
            }
            catch (Exception ex)
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new Response<IEnumerable<Entity.FileInfo>>(new List<Entity.FileInfo>(), errors, false);
            }
        }
    }
}
