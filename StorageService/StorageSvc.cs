using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace StorageService
{
    public class StorageSvc : IStorageSvc
    {
        private static readonly string _basePath = @"C:\PokerStorage\";

        public Task<byte[]> Download()
        {
            throw new NotImplementedException();
        }

        public Task<bool> Upload(IFormFile file)
        {
            throw new NotImplementedException();
        }

        public async Task<string> UploadPic(IFormFile file, string userId)
        {
            string filePath = @$"{_basePath}{userId}\{file.FileName}";

            if (!Directory.Exists(string.Format(_basePath, userId)))
                Directory.CreateDirectory(string.Concat(_basePath, userId));

            if (File.Exists(filePath))
                return filePath;
            else if (file != null && file.FileName != null) 
                using (var stream = File.Create(filePath))
                    await file.CopyToAsync(stream);

            return filePath;
        }
    }
}
