using AutoMapper;
using FileSharing.DAL.Base;
using FileSharing.DAL.Entity;
using FileSharing.DAL.Interfaces;
using FileSharing.Models;
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
        private string ForbiddenSymbols = ";,:][{}?!/";
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public FilesController(IUnitOfWork unitOfWork, UserManager<Account> userManager, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        private readonly string AppDirectory = Path.Combine(Directory.GetCurrentDirectory() + "wwwroot");

        [HttpGet("equalizeFileServerWithDB")]
        public async Task<ActionResult<ResponseModel<IEnumerable<FileInfoModel>>>> EqualizeFileServerWithDB()
        {
            List<FileInfoModel> files = new List<FileInfoModel>();
            try
            {
                if (!_unitOfWork.CanConnect)
                {
                    return BadRequest("Error while connecting to the database");
                }
                var filesInDatabase = await _unitOfWork.Files.Select();
                System.IO.FileInfo[] filesOnFileServer;

                DirectoryInfo fileServerDirectory = new DirectoryInfo(AppDirectory);
                filesOnFileServer = fileServerDirectory.GetFiles();

                if (filesOnFileServer.Length != filesInDatabase.Data.Count())
                {
                    Category defaultCategory = new Category();
                    var defaultCategoryResult = await _unitOfWork.Categories.Select(x => x.Title == "Unsorted");
                    if (defaultCategoryResult.IsSuccessful)
                    {
                        defaultCategory = defaultCategoryResult.Data.FirstOrDefault();
                    }

                    for (int i = 0; i < filesOnFileServer.Length; i++)
                    {

                        if (filesInDatabase.Data.FirstOrDefault(x => x.Title == filesOnFileServer[i].Name) == null)
                        {
                            string contentType;
                            var a = new FileExtensionContentTypeProvider().TryGetContentType(filesOnFileServer[i].FullName, out contentType);
                            var newFile = new DAL.Entity.FileInfo()
                            {
                                Id = 0,
                                Title = filesOnFileServer[i].Name,
                                Extension = filesOnFileServer[i].Extension.ToLower(),
                                Size = filesOnFileServer[i].Length,
                                Path = filesOnFileServer[i].FullName,
                                ContentType = contentType == null ? "application/octet-stream" : contentType,
                                CanBeDownloaded = true,
                                DownloadCount = 0,
                                UploadDate = DateTime.Now,
                                Category_Id = defaultCategory.Id
                            };
                            await _unitOfWork.Files.Create(newFile, new List<CRUDOptions>());
                            await _unitOfWork.CommitAsync();
                            files.Add(_mapper.Map<DAL.Entity.FileInfo, FileInfoModel>(newFile));
                        }
                    }
                }
                return new ResponseModel<IEnumerable<FileInfoModel>>()
                {
                    Data = files.ToArray(),
                    IsSuccessful = true,
                    Errors = new string[0],
                };
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
                var filesResult = await _unitOfWork.Files.Select(l => l.Id == id);
                var file = filesResult.IsSuccessful == true ? filesResult.Data.FirstOrDefault() : null;


                if (file != null)
                {

                    var path = Path.Combine(AppDirectory, file.Title);
                    var memory = new MemoryStream();

                    using (var stream = new FileStream(path, FileMode.Open))
                    {

                        await stream.CopyToAsync(memory);

                    }

                    file.LastDownloadDate = DateTime.Now;
                    file.IncrementDownloadCount();
                    await _unitOfWork.Files.Update(file.Id.ToString(), file, new List<CRUDOptions>());

                    var accounts = await _unitOfWork.Accounts.Select(x => x.Id == account_id);

                    if (accounts.IsSuccessful)
                    {
                        var account = accounts.Data.FirstOrDefault();
                        account.IncrementDownloadedCounter();
                        account.AddTotalSize(file.Size);
                        await _unitOfWork.Accounts.Update(account.Id, account, new List<CRUDOptions>());
                    }

                    await _unitOfWork.CommitAsync();

                    memory.Position = 0;
                    string contentType = "APPLICATION/octet-stream";
                    var fileName = Path.GetFileName(path);
                    var a = File(memory, contentType, fileName);
                    return a;
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private bool CheckForbiddenSymbols(string fileName)
        {
            foreach (var i in ForbiddenSymbols)
            {
                if (fileName.Contains(i))
                    return true;
            }
            return false;
        }

        [Authorize]
        [HttpPost("UploadFile/{account}")]
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = 2048576000)]
        public async Task<ActionResult<ResponseModel<FileInfoModel>>> Upload(IFormFile file, string account)
        {
            try
            {

                if (!Directory.Exists(AppDirectory))
                    Directory.CreateDirectory(AppDirectory);

                string fileTitle = file.FileName;

                if (CheckForbiddenSymbols(fileTitle) == true)
                {
                    return new ResponseModel<FileInfoModel>
                    {
                        IsSuccessful = false,
                        Errors = new string[] { new string("Filename contains forbidden symbols: " + ForbiddenSymbols) },
                    };
                }

                var defaultCategoryResult = await _unitOfWork.Categories.GetDefaultDirectory();

                if (defaultCategoryResult.IsSuccessful)
                {
                    string filePath = Path.Combine(AppDirectory, file.FileName);

                    FileInfoModel model = new FileInfoModel()
                    {
                        Id = 0,
                        Title = fileTitle,
                        Path = filePath,
                        Extension = Path.GetExtension(file.FileName).ToLower(),
                        Size = file.Length,
                        ContentType = file.ContentType,
                        CanBeDownloaded = true,
                        UploadDate = DateTime.Now,
                        Category_Id = defaultCategoryResult.Data.Id,
                        CategoryTitle = defaultCategoryResult.Data.Title,
                    };


                    string fileName = string.Empty;
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                        fileName = stream.Name;
                    }


                    var result = await _unitOfWork.Files.Create(new DAL.Entity.FileInfo()
                    {
                        Id = 0,
                        Title = fileName.Split('\\').Last(),
                        Size = model.Size,
                        Path = fileName,
                        Extension = model.Extension,
                        ContentType = file.ContentType,
                        CanBeDownloaded = model.CanBeDownloaded,
                        DownloadCount = 0,
                        UploadDate = model.UploadDate,
                        Category_Id = defaultCategoryResult.Data.Id,
                    }, new List<CRUDOptions>() { new CRUDOptions("account", account) });

                    await _unitOfWork.CommitAsync();

                    return new ResponseModel<FileInfoModel>
                    {
                        IsSuccessful = true,
                        Data = _mapper.Map<FileInfoModel>(result.Data),
                    };
                }
                return BadRequest();
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
                var fileResult = await _unitOfWork.Files.Select(x => x.Id == id);
                if (fileResult.IsSuccessful)
                {
                    if (fileResult.Data.Count() == 0)
                        return NotFound();

                    var newFile = fileResult.Data.FirstOrDefault();
                    newFile.CanBeDownloaded = status;

                    var Result = await _unitOfWork.Files.Update(newFile.Id.ToString(), newFile, new List<CRUDOptions>());
                    await _unitOfWork.CommitAsync();

                    var newReturn = new ResponseModel<FileInfoModel>()
                    {
                        IsSuccessful = fileResult.IsSuccessful,
                        Errors = fileResult.Errors.ToArray(),
                        Data = _mapper.Map<DAL.Entity.FileInfo, FileInfoModel>(Result.Data)
                    };

                    return newReturn;
                }
                return NotFound(fileResult);
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
                FileInfoModel deletedFile;
                string fileFullName = string.Empty;
                var fileResult = await _unitOfWork.Files.Select(x => x.Id == id);

                if (fileResult.IsSuccessful)
                {
                    var file = fileResult.Data.First();
                    deletedFile = _mapper.Map<FileInfoModel>(file);
                    fileFullName = file.Path;

                    try
                    {
                        await _unitOfWork.Files.Delete(file.Id.ToString());
                        await _unitOfWork.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(ex);
                    }

                    DirectoryInfo di = new DirectoryInfo(AppDirectory);
                    foreach (var item in di.GetFiles())
                    {
                        if (item.FullName == fileFullName)
                        {
                            item.Delete();
                            break;
                        }
                    }
                    return new ResponseModel<FileInfoModel>
                    {
                        IsSuccessful = true,
                        Data = deletedFile,
                    };
                }
                return new ResponseModel<FileInfoModel>
                {
                    IsSuccessful = false,
                    Errors = fileResult.Errors.ToArray()
                };
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
            var files = await _unitOfWork.Files.Select();
            if (files.IsSuccessful)
            {
                var filesData = files.Data.Select(x => new FileInfoModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    Size = x.Size,
                    Extension = x.Extension,
                    Path = x.Path,
                    ContentType = x.ContentType,
                    CanBeDownloaded = x.CanBeDownloaded,
                    LastDownloadDate = x.LastDownloadDate,
                    CategoryTitle = x.Category.Title,
                    Category_Id = x.Category.Id,
                    UploadDate = x.UploadDate,
                }).ToArray();
                return new ResponseModel<IEnumerable<FileInfoModel>>()
                {
                    IsSuccessful = true,
                    Errors = new string[] { },
                    Data = filesData
                };
            }
            return new ResponseModel<IEnumerable<FileInfoModel>>()
            {
                IsSuccessful = true,
                Errors = new string[] { new string("Something went wrong") },
                Data = new FileInfoModel[0],
            };
        }

        [Authorize]
        [HttpGet("GetRandomFiles/{quantity}")]
        public async Task<ActionResult<ResponseModel<IEnumerable<FileInfoModel>>>> GetRandomFiles(int quantity)
        {
            var files = await _unitOfWork.Files.SelectRandom(quantity);
            if (files.IsSuccessful)
            {
                var filesData = files.Data.Select(x => new FileInfoModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    Size = x.Size,
                    Extension = x.Extension,
                    Path = x.Path,
                    ContentType = x.ContentType,
                    CanBeDownloaded = x.CanBeDownloaded,
                    LastDownloadDate = x.LastDownloadDate,
                    CategoryTitle = x.Category.Title,
                    Category_Id = x.Category.Id,
                    UploadDate = x.UploadDate,
                }).ToArray();
                return new ResponseModel<IEnumerable<FileInfoModel>>()
                {
                    IsSuccessful = true,
                    Errors = new string[] { },
                    Data = filesData
                };
            }
            return new ResponseModel<IEnumerable<FileInfoModel>>()
            {
                IsSuccessful = true,
                Errors = new string[] { new string("Something went wrong") },
                Data = new FileInfoModel[0],
            };
        }

        [Authorize]
        [HttpGet("GetLastUploadedFiles/{quantity}")]
        public async Task<ActionResult<ResponseModel<IEnumerable<FileInfoModel>>>> GetLastUploadedFiles(int quantity)
        {
            var files = await _unitOfWork.Files.SelectLastUploaded(quantity);
            if (files.IsSuccessful)
            {
                var filesData = files.Data.Select(x => new FileInfoModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    Size = x.Size,
                    Extension = x.Extension,
                    Path = x.Path,
                    ContentType = x.ContentType,
                    CanBeDownloaded = x.CanBeDownloaded,
                    LastDownloadDate = x.LastDownloadDate,
                    CategoryTitle = x.Category.Title,
                    Category_Id = x.Category.Id,
                    UploadDate = x.UploadDate,
                }).ToArray();
                return new ResponseModel<IEnumerable<FileInfoModel>>()
                {
                    IsSuccessful = true,
                    Errors = new string[] { },
                    Data = filesData
                };
            }
            return new ResponseModel<IEnumerable<FileInfoModel>>()
            {
                IsSuccessful = true,
                Errors = new string[] { new string("Something went wrong") },
                Data = new FileInfoModel[0],
            };
        }

        [Authorize]
        [HttpGet("GetMostDownloadedFiles/{quantity}")]
        public async Task<ActionResult<ResponseModel<IEnumerable<FileInfoModel>>>> GetMostDownloadedFiles(int quantity)
        {
            var files = await _unitOfWork.Files.SelectMostDownloaded(quantity);
            if (files.IsSuccessful)
            {
                var filesData = files.Data.Select(x => new FileInfoModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    Size = x.Size,
                    Extension = x.Extension,
                    Path = x.Path,
                    ContentType = x.ContentType,
                    CanBeDownloaded = x.CanBeDownloaded,
                    LastDownloadDate = x.LastDownloadDate,
                    DownloadCount = x.DownloadCount,
                    CategoryTitle = x.Category.Title,
                    Category_Id = x.Category.Id,
                    UploadDate = x.UploadDate,
                }).ToArray();
                return new ResponseModel<IEnumerable<FileInfoModel>>()
                {
                    IsSuccessful = true,
                    Errors = new string[] { },
                    Data = filesData
                };
            }
            return new ResponseModel<IEnumerable<FileInfoModel>>()
            {
                IsSuccessful = true,
                Errors = new string[] { new string("Something went wrong") },
                Data = new FileInfoModel[0],
            };
        }

        [Authorize]
        [HttpGet("GetFilesByCategory")]
        public async Task<ActionResult<ResponseModel<IEnumerable<FileInfoModel>>>> GetFilesByCategory()
        {
            var files = await _unitOfWork.Files.Select();
            if (files.IsSuccessful)
            {
                var filesData = files.Data.Select(x => new FileInfoModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    Size = x.Size,
                    Extension = x.Extension,
                    Path = x.Path,
                    ContentType = x.ContentType,
                    CanBeDownloaded = x.CanBeDownloaded,
                    LastDownloadDate = x.LastDownloadDate,
                    CategoryTitle = x.Category.Title,
                    Category_Id = x.Category.Id,
                    UploadDate = x.UploadDate,
                }).ToArray();
                return new ResponseModel<IEnumerable<FileInfoModel>>()
                {
                    IsSuccessful = true,
                    Errors = new string[] { },
                    Data = filesData
                };
            }
            return new ResponseModel<IEnumerable<FileInfoModel>>()
            {
                IsSuccessful = true,
                Errors = new string[] { new string("Something went wrong") },
                Data = new FileInfoModel[0],
            };
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("UpdateCategory/{fileId}")]
        public async Task<ActionResult<ResponseModel<FileInfoModel>>> UpdateFileCategory(string fileId, [FromBody] FileInfoModel model)
        {
            if (ModelState.IsValid)
            {
                var fileToUpdate = _mapper.Map<DAL.Entity.FileInfo>(model);
                try
                {
                    var Result = await _unitOfWork.Files.Update(fileId, fileToUpdate, new List<CRUDOptions>());
                    if (Result.IsSuccessful)
                    {
                        await _unitOfWork.CommitAsync();
                        return new ResponseModel<FileInfoModel>
                        {
                            IsSuccessful = true,
                            Data = model
                        };
                    }
                    return new ResponseModel<FileInfoModel>
                    {
                        IsSuccessful = false,
                        Errors = Result.Errors.ToArray(),
                    };
                }
                catch(Exception ex)
                {

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
