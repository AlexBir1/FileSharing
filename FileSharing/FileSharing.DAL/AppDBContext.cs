using FileSharing.DAL.Entity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace FileSharing.DAL
{
    public class AppDBContext : IdentityDbContext<Account>
    {
        public DbSet<Entity.FileInfo> Files { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Settings> Settings { get; set; }

        private string[] triggerNames = new string[0];
        private string[] triggerValues = new string[0];

        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {

            var files = GetSQLTriggers();
            foreach (var file in files)
            {
                var sql = File.ReadAllText(file.FullName);
                triggerValues = triggerValues.Append(sql).ToArray();
                triggerNames = triggerNames.Append(sql.Split(' ').Where(x => x.StartsWith("files_after_")).FirstOrDefault()).ToArray();
            }
            Database.EnsureCreated();

            if (Database.CanConnect())
                foreach (var item in triggerValues)
                {
                    ExecuteSQLFromFile(item);
                }
        }

        private bool ExecuteSQLFromFile(string sqlQuery)
        {
            try
            {
                Database.ExecuteSqlRaw(sqlQuery);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private System.IO.FileInfo[] GetStoredProcedures()
        {
            var rootDirectory = new DirectoryInfo(Directory.GetCurrentDirectory() + "\\Sql\\StoredProcedures");
            var sqlFiles = rootDirectory.GetFiles();
            return sqlFiles;
        }

        private System.IO.FileInfo[] GetSQLTriggers()
        {
            var rootDirectory = new DirectoryInfo(Directory.GetCurrentDirectory() + "\\Sql\\Triggers");
            var sqlFiles = rootDirectory.GetFiles();
            return sqlFiles;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Settings>(l =>
            {
                l.HasKey(l => l.Id);
                l.Property(l => l.Id).HasColumnType("int").ValueGeneratedOnAdd();
            });

            builder.Entity<Account>(l =>
            {
                l.Property(l => l.FilesDownloaded).IsRequired();
                l.Property(l => l.FilesUploaded).IsRequired();
                l.Property(l => l.TotalSizeProcessed).IsRequired();
                l.Property(l => l.RegistrationDate).IsRequired();
                l.HasMany(x => x.Settings).WithOne(x => x.Account).HasForeignKey(x => x.Account_Id);
            });

            builder.Entity<Category>(l =>
            {
                l.HasKey(l => l.Id);
                l.Property(l => l.Id).HasColumnType("int").ValueGeneratedOnAdd();
                l.Property(l => l.Title).IsRequired().HasColumnType("varchar").HasMaxLength(256);
                l.HasMany(x => x.FileInfos).WithOne(x => x.Category).HasForeignKey(x => x.Category_Id);
            });

            builder.Entity<Entity.FileInfo>();


            builder.Entity<Entity.FileInfo>(l =>
            {
                l.HasKey(l => l.Id);
                l.Property(l => l.Id).HasColumnType("int").ValueGeneratedOnAdd();
                l.Property(l => l.Title).IsRequired().HasColumnType("varchar").HasMaxLength(256);
                l.Property(l => l.Path).IsRequired().HasColumnType("varchar").HasMaxLength(256);
                l.Property(l => l.Size).IsRequired();
                l.Property(l => l.Extension).IsRequired().HasColumnType("varchar").HasMaxLength(256);
                l.Property(l => l.ContentType).IsRequired().HasColumnType("varchar").HasMaxLength(256);
                l.Property(l => l.CanBeDownloaded).IsRequired();
                if (triggerNames.Length > 0)
                    foreach (var item in triggerNames)
                    {
                        l.ToTable(x => x.HasTrigger(item));
                    }

            });

            base.OnModelCreating(builder);
        }
    }
}