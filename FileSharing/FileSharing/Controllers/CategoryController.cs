using AutoMapper;
using FileSharing.DAL.Base;
using FileSharing.DAL.Entity;
using FileSharing.DAL.Interfaces;
using FileSharing.DAL.Services;
using FileSharing.Services.Interfaces;
using FileSharing.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FileSharing.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _service;
        private readonly IMapper _mapper;

        public CategoryController(IMapper mapper, ICategoryService service)
        {
            _mapper = mapper;
            _service = service;
        }

        [HttpGet("GetCategories")]
        [Authorize]
        public async Task<ActionResult<ResponseModel<IEnumerable<CategoryModel>>>> GetCategories()
        {
            try
            {
                return Ok(await _service.Select());
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
                return Ok(await _service.Create(model));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}
