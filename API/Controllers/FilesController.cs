using BAL.IServices;
using Microsoft.AspNetCore.Mvc;
using MODEL.ApplicationConfig;

namespace API.Controllers
{
    [ApiController]
    [Route("files")]
    public class FilesController : BaseController
    {
        private readonly IFileService _fileService;
        private readonly ILogService _logService;
        private const string ControllerName = "FilesController";

        public FilesController(IFileService fileService, ILogService logService)
        {
            _fileService = fileService;
            _logService = logService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file, [FromQuery] Guid? taskId = null)
        {
            try
            {
                var result = await _fileService.UploadFileAsync(file, taskId);
                if (result.Status == APIStatus.Successful)
                    await _logService.LogInfoAsync($"{ControllerName}.Upload", $"File uploaded: {file?.FileName}");
                return ToResponse(result, created: true);
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"{ControllerName}.Upload", ex.Message, ex.StackTrace);
                return StatusCode(500, new ResponseModel { Message = ex.Message, Status = APIStatus.SystemError });
            }
        }

        [HttpGet("download/{id:guid}")]
        public async Task<IActionResult> Download(Guid id)
        {
            try
            {
                var result = await _fileService.DownloadFileAsync(id);

                if (result.Status != APIStatus.Successful)
                    return ToResponse(result);

                dynamic data = result.Data!;
                string fileName = data.OriginalFileName;
                string contentType = data.ContentType;
                byte[] fileBytes = data.FileBytes;

                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"{ControllerName}.Download", ex.Message, ex.StackTrace);
                return StatusCode(500, new ResponseModel { Message = ex.Message, Status = APIStatus.SystemError });
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _fileService.DeleteFileAsync(id);
                if (result.Status == APIStatus.Successful)
                    await _logService.LogInfoAsync($"{ControllerName}.Delete", $"File soft-deleted: {id}");
                return ToResponse(result);
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"{ControllerName}.Delete", ex.Message, ex.StackTrace);
                return StatusCode(500, new ResponseModel { Message = ex.Message, Status = APIStatus.SystemError });
            }
        }
    }
}
