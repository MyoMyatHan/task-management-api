using Microsoft.AspNetCore.Http;
using MODEL.ApplicationConfig;

namespace BAL.IServices
{
    public interface IFileService
    {
        Task<ResponseModel> UploadFileAsync(IFormFile file, Guid? taskId);
        Task<ResponseModel> DownloadFileAsync(Guid fileId);
        Task<ResponseModel> DeleteFileAsync(Guid fileId);
    }
}
