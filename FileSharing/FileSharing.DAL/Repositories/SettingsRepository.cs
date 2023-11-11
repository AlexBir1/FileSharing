using FileSharing.DAL.Base;
using FileSharing.DAL.Entity;
using FileSharing.DAL.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSharing.DAL.Repositories
{
    public class SettingsRepository : ISettingsRepository
    {
        private readonly AppDBContext _db;
        private readonly UserManager<Account> _userManager;

        public SettingsRepository(AppDBContext db, UserManager<Account> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IBaseResponse<IEnumerable<Settings>>> AddSettings(IEnumerable<Settings> settings)
        {
            try
            {
                var accountIds = await _userManager.Users.Select(x => x.Id).ToListAsync();
                List<Settings> newSettings = new List<Settings>();
                foreach (var id in accountIds)
                {
                    foreach (var setting in settings)
                    {
                        setting.Account_Id = id;
                    }
                }
                await _db.Settings.AddRangeAsync(settings);
                return new Response<IEnumerable<Settings>>(settings, new List<string>(), true);
            }
            catch (Exception ex)
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new Response<IEnumerable<Settings>>(null, errors, false);
            }
        }

        public async Task<IBaseResponse<IEnumerable<Settings>>> EqualizeSettingsWithDB()
        {
            try
            {
                var settings = new List<Settings>();
                var defaultSettingsResult = GetDefaultSettings();

                if (defaultSettingsResult.IsSuccessful)
                    settings = defaultSettingsResult.Data.ToList();
                else
                    return new Response<IEnumerable<Settings>>(null, defaultSettingsResult.Errors.ToArray(), false);

                var settingsFromDbResult = await GetSettingKeysFromDB();
                if (settingsFromDbResult.IsSuccessful)
                {
                    if (settingsFromDbResult.Data.Count() != defaultSettingsResult.Data.Count())
                    {
                        if (settingsFromDbResult.Data.Count() < defaultSettingsResult.Data.Count())
                        {
                            var accoundIds = await _userManager.Users.Select(x => x.Id).ToListAsync();
                            var newSettingsList = new List<Settings>();
                            foreach (var item in defaultSettingsResult.Data)
                            {
                                if (settingsFromDbResult.Data.FirstOrDefault(x => x.Key == item.Key) == null)
                                {
                                    foreach (var accountId in accoundIds)
                                    {
                                        newSettingsList.Add(new Settings
                                        {
                                            Key = item.Key,
                                            Value = item.Value,
                                            Account_Id = accountId,
                                        });
                                    }

                                }
                            }
                            if (newSettingsList.Count > 0)
                                _db.Settings.AddRange(newSettingsList);
                            return new Response<IEnumerable<Settings>>(null, new List<string>(), true);
                        }
                        else
                        {
                            List<Settings> list = new List<Settings>();
                            foreach (var item in settingsFromDbResult.Data)
                            {
                                if (defaultSettingsResult.Data.FirstOrDefault(x => x.Key == item.Key) == null)
                                {
                                    list = await _db.Settings.Where(x => x.Key == item.Key).ToListAsync();


                                }
                            }
                            if (list.Count > 0)
                                _db.Settings.RemoveRange(list);

                            return new Response<IEnumerable<Settings>>(null, new List<string>(), true);
                        }
                    }
                    else
                    {
                        return new Response<IEnumerable<Settings>>(null, new List<string>(), true);
                    }
                }
                return new Response<IEnumerable<Settings>>(null, settingsFromDbResult.Errors.ToArray(), false);
            }
            catch (Exception ex)
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new Response<IEnumerable<Settings>>(null, errors, false);
            }
        }

        public IBaseResponse<IEnumerable<Settings>> GetDefaultSettings()
        {
            try
            {
                var defaultSettings = new List<Settings>();

                var file = new DirectoryInfo(Directory.GetCurrentDirectory() + "\\Settings").GetFiles().First();
                var jsonSettings = File.ReadAllText(file.FullName);
                defaultSettings = JsonConvert.DeserializeObject<IEnumerable<Settings>>(jsonSettings).ToList();

                return new Response<IEnumerable<Settings>>(defaultSettings, new List<string>(), true);
            }
            catch (Exception ex)
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new Response<IEnumerable<Settings>>(null, errors, false);
            }
        }

        public async Task<IBaseResponse<IEnumerable<Settings>>> GetSettingKeysFromDB()
        {
            try
            {
                var settings = await _db.Settings.GroupBy(x => x.Key, (a, b) =>
                    new Settings
                    {
                        Key = a,
                    }).ToListAsync();
                return new Response<IEnumerable<Settings>>(settings, new List<string>(), true);
            }
            catch (Exception ex)
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new Response<IEnumerable<Settings>>(null, errors, false);
            }
        }

        public async Task<IBaseResponse<IEnumerable<Settings>>> GetSettings(string Account_Id)
        {
            try
            {
                var settings = await _db.Settings.Where(x => x.Account_Id == Account_Id).ToListAsync();
                return new Response<IEnumerable<Settings>>(settings, new List<string>(), true);
            }
            catch (Exception ex)
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new Response<IEnumerable<Settings>>(null, errors, false);
            }
        }

        public async Task<IBaseResponse<IEnumerable<Settings>>> SetDefaultSettings(string Account_Id)
        {
            try
            {
                var accountSettings = new List<Settings>();

                var file = new DirectoryInfo(Directory.GetCurrentDirectory() + "\\Settings").GetFiles().First();
                var jsonSettings = File.ReadAllText(file.FullName);
                accountSettings = JsonConvert.DeserializeObject<IEnumerable<Settings>>(jsonSettings).ToList();

                foreach (var setting in accountSettings)
                {
                    setting.Account_Id = Account_Id;

                }
                await _db.Settings.AddRangeAsync(accountSettings);
                return new Response<IEnumerable<Settings>>(accountSettings, new List<string>(), true);
            }
            catch (Exception ex)
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new Response<IEnumerable<Settings>>(null, errors, false);
            }
        }

        public async Task<IBaseResponse<IEnumerable<Settings>>> SetSettings(IEnumerable<Settings> settings)
        {
            try
            {
                _db.Settings.UpdateRange(settings);
                return new Response<IEnumerable<Settings>>(settings, new List<string>(), true);
            }
            catch (Exception ex)
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new Response<IEnumerable<Settings>>(null, errors, false);
            }
        }
    }
}
