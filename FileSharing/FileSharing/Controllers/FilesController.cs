using AutoMapper;
using FileSharing.DAL.Base;
using FileSharing.DAL.Entity;
using FileSharing.DAL.Interfaces;
using FileSharing.Services.Interfaces;
using FileSharing.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System.Net.Mime;

namespace FileSharing.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        
        private readonly IFilesService _service;
        private readonly IMapper _mapper;

        public FilesController(IMapper mapper, IFilesService service)
        {
            _mapper = mapper;
            _service = service;
        }

        private readonly string AppDirectory = Path.Combine(Directory.GetCurrentDirectory() + "wwwroot");

        [HttpGet("equalizeFileServerWithDB")]
        public async Task<ActionResult<ResponseModel<IEnumerable<FileInfoModel>>>> EqualizeFileServerWithDB()
        {
            try
            {
                return Ok(await _service.EqualizeFileServerWithDB());
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [Authorize]
        [HttpGet("Download/{id}/{account_id}")]
        public async Task<ActionResult<string>> Download(int id, string account_id)
        {
            try
            {
                return Ok(await _service.Download(id, account_id));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("UploadFile/{account}")]
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = 2048576000)]
        public async Task<ActionResult<ResponseModel<FileInfoModel>>> Upload(IFormFile file, string account)
        {
            try
            {
                return Ok(await _service.Upload(file, account));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);

            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("UpdateDownloadStatus/{id}")]
        public async Task<ActionResult<ResponseModel<FileInfoModel>>> UpdateDownloadStatus(int id, [FromBody] bool status)
        {
            try
            {
                return Ok(await _service.UpdateDownloadStatus(id, status));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteFile/{id}")]
        public async Task<ActionResult<ResponseModel<FileInfoModel>>> DeleteFile(int id)
        {
            try
            {
                return Ok(await _service.DeleteFile(id));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [Authorize]
        [HttpGet("GetFiles")]
        public async Task<ActionResult<ResponseModel<IEnumerable<FileInfoModel>>>> GetFiles()
        {
            try
            {
                return Ok(await _service.GetFiles());
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [Authorize]
        [HttpGet("GetRandomFiles/{quantity}")]
        public async Task<ActionResult<ResponseModel<IEnumerable<FileInfoModel>>>> GetRandomFiles(int quantity)
        {
            try
            {
                return Ok(await _service.GetRandomFiles(quantity));
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [Authorize]
        [HttpGet("GetLastUploadedFiles/{quantity}")]
        public async Task<ActionResult<ResponseModel<IEnumerable<FileInfoModel>>>> GetLastUploadedFiles(int quantity)
        {
            try
            {
                return Ok(await _service.GetLastUploadedFiles(quantity));
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [Authorize]
        [HttpGet("GetMostDownloadedFiles/{quantity}")]
        public async Task<ActionResult<ResponseModel<IEnumerable<FileInfoModel>>>> GetMostDownloadedFiles(int quantity)
        {
            try
            {
                return Ok(await _service.GetMostDownloadedFiles(quantity));
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [Authorize]
        [HttpGet("GetFilesByCategory")]
        public async Task<ActionResult<ResponseModel<IEnumerable<FileInfoModel>>>> GetFilesByCategory()
        {
            try
            {
                return Ok(await _service.GetFilesByCategory());
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("UpdateCategory/{fileId}")]
        public async Task<ActionResult<ResponseModel<FileInfoModel>>> UpdateFileCategory(string fileId, [FromBody] FileInfoModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    return Ok(await _service.UpdateFileCategory(fileId, model));
                }
                catch(Exception ex)
                {
                    return BadRequest(ex);
                }
            }
            return new ResponseModel<FileInfoModel>
            {
                Data = null,
                IsSuccessful = false,
                Errors = ModelState.Where(x => x.Value.Errors.Count > 0).SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage).ToArray(),
            };
        }
    }
}
