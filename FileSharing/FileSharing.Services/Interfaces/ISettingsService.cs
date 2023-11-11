using FileSharing.Services.Base;
using FileSharing.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSharing.Services.Interfaces
{
    public interface ISettingsService : IBaseService<SettingsModel>
    {
        Task<ResponseModel<IEnumerable<SettingsModel>>> EqualizeSettingsFileWithDB();
        Task<ResponseModel<IEnumerable<SettingsModel>>> GetAccountSettings(string accountId);
        Task<ResponseModel<IEnumerable<SettingsModel>>> SetAccountSettings(string accountId, IEnumerable<SettingsModel> models);
    }
}
