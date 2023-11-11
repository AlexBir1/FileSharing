using AutoMapper;
using FileSharing.DAL.Entity;
using FileSharing.DAL.Interfaces;
using FileSharing.Services.Interfaces;
using FileSharing.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FileSharing.Services.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResponseModel<CategoryModel>> Create(CategoryModel Entity)
        {
            try
            {
                var Result = await _unitOfWork.Categories.Create(_mapper.Map<Category>(Entity));
                if (Result.IsSuccessful)
                {
                    await _unitOfWork.CommitAsync();
                    var newCategory = await _unitOfWork.Categories.Select(x => x.Title == Entity.Title);
                    if (newCategory.IsSuccessful)
                    {
                        return new ResponseModel<CategoryModel>
                        {
                            IsSuccessful = true,
                            Data = _mapper.Map<CategoryModel>(newCategory.Data.First()),
                        };
                    }
                    return new ResponseModel<CategoryModel>
                    {
                        IsSuccessful = false,
                        Errors = newCategory.Errors.ToArray(),
                    };
                }
                return new ResponseModel<CategoryModel>
                {
                    IsSuccessful = false,
                    Errors = Result.Errors.ToArray(),
                };
            }
            catch (Exception ex)
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new ResponseModel<CategoryModel>() 
                {
                    IsSuccessful = false,
                    Data = null,
                    Errors = errors.ToArray(),
                };
            }
        }

        public Task<ResponseModel<CategoryModel>> Delete(string Id)
        {
            throw new NotImplementedException();
        }

        public async Task<ResponseModel<IEnumerable<CategoryModel>>> Select()
        {
            try
            {
                var categoriesResult = await _unitOfWork.Categories.Select();

                if (categoriesResult.IsSuccessful)
                {
                    List<CategoryModel> categoryModels = new List<CategoryModel>();

                    foreach (var item in categoriesResult.Data)
                    {
                        categoryModels.Add(_mapper.Map<CategoryModel>(item));
                    }

                    return new ResponseModel<IEnumerable<CategoryModel>>
                    {
                        IsSuccessful = true,
                        Data = categoryModels.ToArray(),
                    };
                }
                return new ResponseModel<IEnumerable<CategoryModel>>
                {
                    IsSuccessful = false,
                    Errors = categoriesResult.Errors.ToArray(),
                };
            }
            catch (Exception ex)
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new ResponseModel<IEnumerable<CategoryModel>>()
                {
                    IsSuccessful = false,
                    Data = null,
                    Errors = errors.ToArray(),
                };
            }
        }

        public Task<ResponseModel<IEnumerable<CategoryModel>>> Select(Expression<Func<CategoryModel, bool>> expression)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseModel<CategoryModel>> Update(string Id)
        {
            throw new NotImplementedException();
        }
    }
}
