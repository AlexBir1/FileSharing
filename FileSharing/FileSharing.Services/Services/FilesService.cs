using AutoMapper;
using FileSharing.DAL.Entity;
using FileSharing.DAL.Interfaces;
using FileSharing.Services.Interfaces;
using FileSharing.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSharing.Services.Services
{
    public class FilesService : IFilesService
    {
        private string ForbiddenSymbols = ";,:][{}?!/";
        private readonly string AppDirectory = Directory.GetCurrentDirectory();
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public FilesService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResponseModel<FileInfoModel>> DeleteFile(int id)
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
                        List<string> errors = new List<string>()
                        {
                            new string(ex.Message),
                        };
                        return new ResponseModel<FileInfoModel>()
                        {
                            IsSuccessful = false,
                            Data = null,
                            Errors = errors.ToArray(),
                        };
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
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new ResponseModel<FileInfoModel>()
                {
                    IsSuccessful = false,
                    Data = null,
                    Errors = errors.ToArray(),
                };
            }
        }

        public async Task<FileDataModel> Download(int id, string account_id)
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
                    await _unitOfWork.Files.Update(file.Id.ToString(), file);

                    var accounts = await _unitOfWork.Accounts.Select(x => x.Id == account_id);

                    if (accounts.IsSuccessful)
                    {
                        var account = accounts.Data.FirstOrDefault();
                        account.IncrementDownloadedCounter();
                        account.AddTotalSize(file.Size);
                        await _unitOfWork.Accounts.Update(account.Id, account);
                    }

                    await _unitOfWork.CommitAsync();

                    memory.Position = 0;
                    string contentType = "APPLICATION/octet-stream";
                    var fileName = Path.GetFileName(path);

                    return new FileDataModel
                    {
                        ContentType = contentType,
                        FileName = fileName,
                        MemoryStream = memory
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<ResponseModel<IEnumerable<FileInfoModel>>> EqualizeFileServerWithDB()
        {
            List<FileInfoModel> files = new List<FileInfoModel>();
            try
            {
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
                            await _unitOfWork.Files.Create(newFile);
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
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new ResponseModel<IEnumerable<FileInfoModel>>()
                {
                    Data = files.ToArray(),
                    IsSuccessful = true,
                    Errors = new string[0],
                };
            }
        }

        public async Task<ResponseModel<IEnumerable<FileInfoModel>>> GetFiles()
        {
            try
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
            catch(Exception ex)
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new ResponseModel<IEnumerable<FileInfoModel>>()
                {
                    IsSuccessful = false,
                    Errors = errors.ToArray(),
                    Data = null,
                };
            }
        }

        public async Task<ResponseModel<IEnumerable<FileInfoModel>>> GetFilesByCategory()
        {
            try
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
            catch (Exception ex)
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new ResponseModel<IEnumerable<FileInfoModel>>()
                {
                    IsSuccessful = false,
                    Errors = errors.ToArray(),
                    Data = null,
                };
            }
        }

        public async Task<ResponseModel<IEnumerable<FileInfoModel>>> GetLastUploadedFiles(int quantity)
        {
            try
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
            catch (Exception ex)
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new ResponseModel<IEnumerable<FileInfoModel>>()
                {
                    IsSuccessful = false,
                    Errors = errors.ToArray(),
                    Data = null,
                };
            }
        }

        public async Task<ResponseModel<IEnumerable<FileInfoModel>>> GetMostDownloadedFiles(int quantity)
        {
            try
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
            catch (Exception ex)
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new ResponseModel<IEnumerable<FileInfoModel>>()
                {
                    IsSuccessful = false,
                    Errors = errors.ToArray(),
                    Data = null,
                };
            }
        }

        public async Task<ResponseModel<IEnumerable<FileInfoModel>>> GetRandomFiles(int quantity)
        {
            try
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
                    Errors = new string[] { new string("No files") },
                    Data = new FileInfoModel[0],
                };
            }
            catch (Exception ex)
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new ResponseModel<IEnumerable<FileInfoModel>>()
                {
                    IsSuccessful = false,
                    Errors = errors.ToArray(),
                    Data = null,
                };
            }
        }

        public async Task<ResponseModel<FileInfoModel>> UpdateDownloadStatus(int id, bool status)
        {
            try
            {
                var fileResult = await _unitOfWork.Files.Select(x => x.Id == id);
                if (fileResult.IsSuccessful)
                {
                    var newFile = fileResult.Data.FirstOrDefault();
                    newFile.CanBeDownloaded = status;

                    var Result = await _unitOfWork.Files.Update(newFile.Id.ToString(), newFile);
                    await _unitOfWork.CommitAsync();

                    var newReturn = new ResponseModel<FileInfoModel>()
                    {
                        IsSuccessful = fileResult.IsSuccessful,
                        Errors = fileResult.Errors.ToArray(),
                        Data = _mapper.Map<DAL.Entity.FileInfo, FileInfoModel>(Result.Data)
                    };

                    return newReturn;
                }
                List<string> errors = new List<string>()
                {
                    new string("File does not exist"),
                };
                return new ResponseModel<FileInfoModel>()
                {
                    IsSuccessful = false,
                    Data = null,
                    Errors = errors.ToArray(),
                };
            }
            catch (Exception ex)
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new ResponseModel<FileInfoModel>()
                {
                    IsSuccessful = false,
                    Data = null,
                    Errors = errors.ToArray(),
                };
            }
        }

        public async Task<ResponseModel<FileInfoModel>> UpdateFileCategory(string fileId, FileInfoModel model)
        {
            var fileToUpdate = _mapper.Map<DAL.Entity.FileInfo>(model);
            try
            {
                var Result = await _unitOfWork.Files.Update(fileId, fileToUpdate);
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
            catch (Exception ex)
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new ResponseModel<FileInfoModel>()
                {
                    IsSuccessful = false,
                    Data = null,
                    Errors = errors.ToArray(),
                };
            }
        }

        public async Task<ResponseModel<FileInfoModel>> Upload(IFormFile file, string account)
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

                if (!defaultCategoryResult.IsSuccessful)
                {
                    await _unitOfWork.Categories.Create(new Category() { Title = "Unsorted" });
                    await _unitOfWork.CommitAsync();
                }

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
                });

                await _unitOfWork.CommitAsync();

                return new ResponseModel<FileInfoModel>
                {
                    IsSuccessful = true,
                    Data = _mapper.Map<FileInfoModel>(result.Data),
                };
            }
            catch (Exception ex)
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new ResponseModel<FileInfoModel>()
                {
                    IsSuccessful = false,
                    Data = null,
                    Errors = errors.ToArray(),
                };
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
    }
}
