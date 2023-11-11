using FileSharing.DAL.Base;
using FileSharing.DAL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSharing.DAL.Interfaces
{
    public interface ISettingsRepository
    {
        Task<IBaseResponse<IEnumerable<Settings>>> AddSettings(IEnumerable<Settings> settings);
        Task<IBaseResponse<IEnumerable<Settings>>> SetDefaultSettings(string Account_Id);
        IBaseResponse<IEnumerable<Settings>> GetDefaultSettings();
        Task<IBaseResponse<IEnumerable<Settings>>> SetSettings(IEnumerable<Settings> settings);
        Task<IBaseResponse<IEnumerable<Settings>>> GetSettings(string Account_Id);
        Task<IBaseResponse<IEnumerable<Settings>>> EqualizeSettingsWithDB();
        Task<IBaseResponse<IEnumerable<Settings>>> GetSettingKeysFromDB();
    }
}
