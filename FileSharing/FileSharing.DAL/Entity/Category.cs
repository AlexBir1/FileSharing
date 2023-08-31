using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSharing.DAL.Entity
{
    public class Category
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int ElementCount { get; set; }

        public ICollection<FileInfo> FileInfos { get; set; } 

        public void IncrementElementCount()
        {
            ElementCount++;
        }

        public void DecrementElementCount()
        {
            ElementCount--;
        }
    }
}
