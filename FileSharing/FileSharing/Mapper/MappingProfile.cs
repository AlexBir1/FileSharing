using AutoMapper;
using FileSharing.DAL.Entity;
using FileSharing.Shared.Models;

namespace FileSharing.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<DAL.Entity.FileInfo, FileInfoModel>()
                .ForMember(x => x.CategoryTitle, x => x.MapFrom(x => x.Category.Title));
            CreateMap<FileInfoModel, DAL.Entity.FileInfo>()
                .ForMember(x => x.Category_Id, x => x.MapFrom(x => x.Category_Id));
            CreateMap<Account, AccountModel>();
            CreateMap<AccountModel, Account>();
            CreateMap<Category, CategoryModel>();
            CreateMap<CategoryModel, Category>();
            CreateMap<SettingsModel, Settings>();
            CreateMap<Settings, SettingsModel>();
        }
    }
}
