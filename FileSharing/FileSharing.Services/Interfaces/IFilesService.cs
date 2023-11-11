using FileSharing.Shared.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSharing.Services.Interfaces
{
    public interface IFilesService
    {
        Task<ResponseModel<IEnumerable<FileInfoModel>>> EqualizeFileServerWithDB();
        Task<FileDataModel> Download(int id, string account_id);
        Task<ResponseModel<FileInfoModel>> Upload(IFormFile file, string account);
        Task<ResponseModel<FileInfoModel>> UpdateDownloadStatus(int id, bool status);
        Task<ResponseModel<FileInfoModel>> DeleteFile(int id);
        Task<ResponseModel<IEnumerable<FileInfoModel>>> GetFiles();
        Task<ResponseModel<IEnumerable<FileInfoModel>>> GetRandomFiles(int quantity);
        Task<ResponseModel<IEnumerable<FileInfoModel>>> GetLastUploadedFiles(int quantity);
        Task<ResponseModel<IEnumerable<FileInfoModel>>> GetMostDownloadedFiles(int quantity);
        Task<ResponseModel<IEnumerable<FileInfoModel>>> GetFilesByCategory();
        Task<ResponseModel<FileInfoModel>> UpdateFileCategory(string fileId, FileInfoModel model);
    }
}
