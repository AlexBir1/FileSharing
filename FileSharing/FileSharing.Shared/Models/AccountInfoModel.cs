namespace FileSharing.Shared.Models
{
    public class AccountInfoModel
    {
        public int FilesUploaded { get; set; }
        public int FilesDownloaded { get; set; }
        public long TotalSizeProcessed { get; set; }
    }
}
