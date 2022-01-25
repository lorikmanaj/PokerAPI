using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StorageService
{
    public interface IStorageSvc
    {
        Task<byte[]> Download();
        Task<bool> Upload(IFormFile file);
        Task<string> UploadPic(IFormFile file, string userId);
    }
}
