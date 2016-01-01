using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTPSchubser.Helper
{
    class UploadFile
    {
        public string UploadName { get; set; }
        public string FilePath { get; set; }
        public bool IsTemp { get; set; }

        public FileInfo FileInfo { get; private set; }

        public UploadFile(string filePath, string uploadName = null, bool isTemp = false)
        {
            FilePath = filePath;
            IsTemp = isTemp;
            FileInfo = new FileInfo(FilePath);
            UploadName = uploadName == null ? FileInfo.Name : uploadName;
        }
    }
}
