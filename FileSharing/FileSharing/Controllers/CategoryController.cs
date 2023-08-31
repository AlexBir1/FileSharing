using AutoMapper;
using FileSharing.DAL.Base;
using FileSharing.DAL.Entity;
using FileSharing.DAL.Interfaces;
using FileSharing.DAL.Services;
using FileSharing.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FileSharing.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet("GetCategories")]
        [Authorize]
        public async Task<ActionResult<ResponseModel<CategoryModel[]>>> GetCategories()
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

                    return new ResponseModel<CategoryModel[]>
                    {
                        IsSuccessful = true,
                        Data = categoryModels.ToArray(),
                    };
                }
                return new ResponseModel<CategoryModel[]>
                {
                    IsSuccessful = false,
                    Errors = categoriesResult.Errors.ToArray(),
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("CreateCategory")]
        [Authorize]
        public async Task<ActionResult<ResponseModel<CategoryModel>>> CreateCategory([FromBody] CategoryModel model)
        {
            try
            {
                var Result = await _unitOfWork.Categories.Create(_mapper.Map<Category>(model), new List<CRUDOptions>());
                if(Result.IsSuccessful)
                {
                    await _unitOfWork.CommitAsync();
                    var newCategory = await _unitOfWork.Categories.Select(x => x.Title == model.Title);
                    if(newCategory.IsSuccessful)
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
                return BadRequest(ex);
            }
        }
    }
}
