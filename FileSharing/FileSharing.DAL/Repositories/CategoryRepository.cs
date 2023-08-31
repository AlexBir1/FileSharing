using FileSharing.DAL.Base;
using FileSharing.DAL.Entity;
using FileSharing.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FileSharing.DAL.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDBContext _db;

        public CategoryRepository(AppDBContext db)
        {
            _db = db;
        }

        public async Task<IBaseResponse<Category>> Create(Category Entity, IEnumerable<CRUDOptions> Options)
        {
            try
            {
                var catogery = await _db.Categories.Where(x => x.Title == Entity.Title).FirstOrDefaultAsync();
                if (catogery == null)
                {
                    _db.Categories.Add(Entity);
                    return new Response<Category>(Entity, new List<string>(), true);
                }
                else
                {
                    List<string> errors = new List<string>()
                {
                    new string("Category exists"),
                };
                    return new Response<Category>(Entity, errors, false);
                }
            }
            catch (Exception ex)
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new Response<Category>(Entity, errors, false);
            }
        }

        public async Task<IBaseResponse<Category>> Delete(string Id)
        {
            try
            {
                var catogery = await _db.Categories.FirstOrDefaultAsync(x => x.Id == int.Parse(Id));
                if (catogery != null)
                {
                    _db.Categories.Remove(catogery);
                    return new Response<Category>(catogery, new List<string>(), true);
                }
                else
                {
                    List<string> errors = new List<string>()
                {
                    new string("Category does not exist"),
                };
                    return new Response<Category>(catogery, errors, false);
                }
            }
            catch (Exception ex)
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new Response<Category>(null, errors, false);
            }
        }

        public async Task<IBaseResponse<Category>> GetDefaultDirectory()
        {
            try
            {
                var category = await _db.Categories.FirstAsync(x => x.Title == "Unsorted");
                return new Response<Category>(category, new List<string>(), true);
            }
            catch (Exception ex)
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new Response<Category>(null, errors, false);
            }
        }

        public async Task<IBaseResponse<IEnumerable<Category>>> Select()
        {
            try
            {
                var categories = await _db.Categories.AsNoTracking().ToListAsync();
                if (categories.Count > 0)
                    return new Response<IEnumerable<Category>>(categories, new List<string>(), true);
                else
                {
                    List<string> errors = new List<string>()
                    {
                        new string("There are no categories in the database."),
                    };
                    return new Response<IEnumerable<Category>>(categories, errors, false);
                }
            }
            catch (Exception ex)
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new Response<IEnumerable<Category>>(null, errors, false);
            }
        }

        public async Task<IBaseResponse<IEnumerable<Category>>> Select(Expression<Func<Category>> expression)
        {
            throw new NotImplementedException();
        }

        public async Task<IBaseResponse<IEnumerable<Category>>> Select(Expression<Func<Category, bool>> expression)
        {
            try
            {
                var categories = await _db.Categories.Where(expression).AsNoTracking().ToListAsync();
                if (categories.Count > 0)
                    return new Response<IEnumerable<Category>>(categories, new List<string>(), true);
                else
                {
                    List<string> errors = new List<string>()
                    {
                        new string("There are no categories in the database."),
                    };
                    return new Response<IEnumerable<Category>>(categories, errors, false);
                }
            }
            catch (Exception ex)
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new Response<IEnumerable<Category>>(null, errors, false);
            }
        }

        public async Task<IBaseResponse<Category>> Update(string Id, Category Entity, IEnumerable<CRUDOptions> Options)
        {
            try
            {
                var category = await _db.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == int.Parse(Id));

                if (category == null)
                {
                    List<string> errors = new List<string>()
                {
                    new string("Category does not exist"),
                };
                    return new Response<Category>(null, errors, false);
                }

                category.Title = Entity.Title;

                _db.Categories.Update(category);

                return new Response<Category>(category, new List<string>(), true);
            }
            catch (Exception ex)
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new Response<Category>(null, errors, false);
            }
        }
    }
}
