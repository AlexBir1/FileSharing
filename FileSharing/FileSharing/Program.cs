using FileSharing.DAL;
using FileSharing.DAL.Base;
using FileSharing.DAL.Entity;
using FileSharing.DAL.Interfaces;
using FileSharing.DAL.Repositories;
using FileSharing.DAL.Services;
using FileSharing.Services.Interfaces;
using FileSharing.Services.Services;
using FileSharing.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace FileSharing
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<AppDBContext>(option =>
            {
                option.UseSqlServer(builder.Configuration.GetConnectionString("StrConnection"));
            });
            builder.Services.AddIdentityCore<Account>(options =>
            {
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 12;
                options.Password.RequireDigit = false;
            })
                .AddSignInManager<SignInManager<Account>>()
                .AddRoles<IdentityRole>()
                .AddRoleManager<RoleManager<IdentityRole>>()
                .AddDefaultTokenProviders()
            .AddEntityFrameworkStores<AppDBContext>();

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(l =>
            {
                l.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"])),
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["JWT:Issuer"],
                    ValidateAudience = false,
                };
            });

            builder.Services.AddScoped<JWTService>();

            builder.Services.AddScoped<ISettingsService, SettingsService>();
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IFilesService, FilesService>();

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            builder.Services.AddAutoMapper(typeof(Program).Assembly);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseCors(options =>
            {
                options.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().WithExposedHeaders("content-disposition");
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller}/{action=Index}/{id?}");

            using (var scope = app.Services.CreateScope())
            {
                var service = scope.ServiceProvider.GetService<IUnitOfWork>();
                if (service.CanConnect)
                {
                    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Account>>();

                    var unsortedCategory = new Category()
                    {
                        Id = 0,
                        Title = "Unsorted",
                        ElementCount = 0,
                    };

                    var categoryResult = await service.Categories.Select(x => x.Title == unsortedCategory.Title);

                    if (!categoryResult.IsSuccessful)
                    {
                        await service.Categories.Create(unsortedCategory);
                        await service.CommitAsync();
                    }

                    foreach (var item in RoleHandler.GetAllRoles())
                    {
                        if (!await roleManager.RoleExistsAsync(item.ToString()))
                        {
                            await roleManager.CreateAsync(new IdentityRole(item.ToString()));
                        }
                    }
                    var Result = await service.Accounts.Select(x => x.UserName == builder.Configuration["AdminDefaults:Username"]);
                    if (!Result.IsSuccessful)
                    {
                        string adminPassword = builder.Configuration["AdminDefaults:Password"];

                        Account admin = new Account()
                        {
                            UserName = builder.Configuration["AdminDefaults:Username"],
                            Email = builder.Configuration["AdminDefaults:Email"],
                            TotalSizeProcessed = 0,
                            FilesDownloaded = 0,
                            FilesUploaded = 0,
                        };

                        await service.Accounts.Create(admin);

                        var administrator = await userManager.FindByNameAsync(builder.Configuration["AdminDefaults:Username"]);

                        await userManager.AddPasswordAsync(administrator, adminPassword);

                        await userManager.AddToRoleAsync(administrator, AccountRoles.Admin.ToString());

                            await service.CommitAsync();

                    }
                }
            };

            app.MapFallbackToFile("index.html");

            app.Run();
        }
    }
}