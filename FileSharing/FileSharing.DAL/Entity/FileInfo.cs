using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSharing.DAL.Entity
{
    public class FileInfo
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Extension { set; get; } = string.Empty;
        public long Size { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public bool CanBeDownloaded { get; set; } = true;
        public int DownloadCount { get; set; } = 0;
        public DateTime UploadDate { get; set; }
        public DateTime LastDownloadDate { get; set; }

        public void IncrementDownloadCount()
        {
            this.DownloadCount++;
        }

        public int Category_Id { get; set; }
        public Category Category { get; set; }
    }
}
