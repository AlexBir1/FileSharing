using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSharing.Shared.Models
{
    public class FileDataModel
    {
        public MemoryStream? MemoryStream {  get; set; }
        public string ContentType { get; set; }
        public string FileName { get; set; }
    }
}
