using Microsoft.AspNetCore.Identity;

namespace FileSharing.DAL.Entity
{
    public class Account : IdentityUser
    {
        public DateTime RegistrationDate { get; set; } = DateTime.Now; 

        public int FilesUploaded { get; set; } = 0;

        public int FilesDownloaded { get; set; } = 0;

        public long TotalSizeProcessed { get; set; } = 0;

        public ICollection<Settings> Settings { get; set; }

        public void IncrementUploadedCounter()
        {
            this.FilesUploaded++;
        }
        public void IncrementDownloadedCounter()
        {
            this.FilesDownloaded++;
        }
        public void AddTotalSize(long size)
        {
            this.TotalSizeProcessed += size;
        }
    }
}