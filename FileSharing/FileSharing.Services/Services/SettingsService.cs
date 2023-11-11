using AutoMapper;
using FileSharing.DAL.Entity;
using FileSharing.DAL.Interfaces;
using FileSharing.Services.Interfaces;
using FileSharing.Shared.Models;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FileSharing.Services.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SettingsService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResponseModel<SettingsModel>> Create(SettingsModel Entity)
        {
            throw new NotImplementedException();
        }

        public async Task<ResponseModel<SettingsModel>> Delete(string Id)
        {
            throw new NotImplementedException();
        }

        public async Task<ResponseModel<IEnumerable<SettingsModel>>> EqualizeSettingsFileWithDB()
        {
            try
            {
                var result = await _unitOfWork.Settings.EqualizeSettingsWithDB();
                if (result.IsSuccessful)
                {
                    await _unitOfWork.CommitAsync();
                    return new ResponseModel<IEnumerable<SettingsModel>>()
                    {
                        IsSuccessful = result.IsSuccessful,
                    };
                }
                return new ResponseModel<IEnumerable<SettingsModel>>()
                {
                    IsSuccessful = result.IsSuccessful,
                    Errors = result.Errors.ToArray(),
                };
            }
            catch (Exception ex)
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new ResponseModel<IEnumerable<SettingsModel>>()
                {
                    IsSuccessful = false,
                    Data = null,
                    Errors = errors.ToArray(),
                };
            }
        }

        public async Task<ResponseModel<IEnumerable<SettingsModel>>> GetAccountSettings(string accountId)
        {
            try
            {
                var settingsResult = await _unitOfWork.Settings.GetSettings(accountId);
                if (settingsResult.IsSuccessful)
                {
                    var settingsList = _mapper.Map<IEnumerable<SettingsModel>>(settingsResult.Data);

                    return new ResponseModel<IEnumerable<SettingsModel>>
                    {
                        IsSuccessful = true,
                        Data = settingsList,
                    };
                }
                return new ResponseModel<IEnumerable<SettingsModel>>
                {
                    IsSuccessful = false,
                    Errors = settingsResult.Errors.ToArray(),
                };
            }
            catch (Exception ex)
            {
                List<string> errors = new List<string>()
                {
                    new string(ex.Message),
                };
                return new ResponseModel<IEnumerable<SettingsModel>>()
                {
                    IsSuccessful = false,
                    Data = null,
                    Errors = errors.ToArray(),
                };
            }
        }

        public async Task<ResponseModel<IEnumerable<SettingsModel>>> Select()
        {
            throw new NotImplementedException();
        }

        public async Task<ResponseModel<IEnumerable<SettingsModel>>> Select(Expression<Func<SettingsModel, bool>> expression)
        {
            throw new NotImplementedException();
        }

        public async Task<ResponseModel<IEnumerable<SettingsModel>>> SetAccountSettings(string accountId, IEnumerable<SettingsModel> models)
        {
            try
            {
                var settingsList = _mapper.Map<IEnumerable<Settings>>(models);

                var Result = await _unitOfWork.Settings.SetSettings(settingsList);
                if (Result.IsSuccessful)
                {
                    await _unitOfWork.CommitAsync();

                    var settings = _mapper.Map<IEnumerable<SettingsModel>>(settingsList);

                    return new ResponseModel<IEnumerable<SettingsModel>>
                    {
                        IsSuccessful = true,
                        Data = settings.ToArray()
                    };
                }
                return new ResponseModel<IEnumerable<SettingsModel>>
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
                return new ResponseModel<IEnumerable<SettingsModel>>()
                {
                    IsSuccessful = false,
                    Data = null,
                    Errors = errors.ToArray(),
                };
            }
        }

        public async Task<ResponseModel<SettingsModel>> Update(string Id)
        {
            throw new NotImplementedException();
        }
    }
}
