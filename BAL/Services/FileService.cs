using BAL.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MODEL.ApplicationConfig;
using MODEL.DTOs.File;
using MODEL.Entities;
using REPOSITORY.UnitOfWork;

namespace BAL.Services
{
    public class FileService : IFileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly string _uploadDirectory;

        private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/png", "image/gif", "image/webp",
            "application/pdf",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/vnd.ms-excel",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "text/plain"
        };

        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

        public FileService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _uploadDirectory = configuration["FileStorage:UploadPath"]
                               ?? Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

            Directory.CreateDirectory(_uploadDirectory);
        }        

        public async Task<ResponseModel> UploadFileAsync(IFormFile file, Guid? taskId)
        {
            var validationError = ValidateFile(file);
            if (validationError != null)
                return validationError;

            if (taskId.HasValue)
            {
                var taskError = await ValidateTaskExistsAsync(taskId.Value);
                if (taskError != null)
                    return taskError;
            }

            var attachment = await SaveFileAsync(file, taskId);
            await _unitOfWork.FileAttachments.Add(attachment);
            await _unitOfWork.SaveChangesAsync();

            return new ResponseModel
            {
                Status = APIStatus.Successful,
                Message = Messages.AddSucess,
                Data = MapToDto(attachment)
            };
        }

        public async Task<ResponseModel> DownloadFileAsync(Guid fileId)
        {
            var attachment = await _unitOfWork.FileAttachments.GetActiveByIdAsync(fileId);

            if (attachment == null)
                return NotFound("File not found.");

            if (!File.Exists(attachment.FilePath))
                return NotFound("File not found on server.");

            var fileBytes = await File.ReadAllBytesAsync(attachment.FilePath);

            return new ResponseModel
            {
                Status = APIStatus.Successful,
                Message = Messages.Successful,
                Data = new
                {
                    attachment.OriginalFileName,
                    attachment.ContentType,
                    FileBytes = fileBytes
                }
            };
        }

        public async Task<ResponseModel> DeleteFileAsync(Guid fileId)
        {
            var attachment = await _unitOfWork.FileAttachments.GetByGuid(fileId);

            if (attachment == null || !attachment.IsActive)
                return NotFound("File not found.");

            attachment.IsActive = false;
            _unitOfWork.FileAttachments.Update(attachment);
            await _unitOfWork.SaveChangesAsync();

            return new ResponseModel { Status = APIStatus.Successful, Message = Messages.DeleteSucess };
        }

        private static ResponseModel? ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Error("File is empty or missing.");

            if (file.Length > MaxFileSizeBytes)
                return Error($"File size exceeds the maximum allowed size of {MaxFileSizeBytes / (1024 * 1024)} MB.");

            var originalFileName = Path.GetFileName(file.FileName);
            if (string.IsNullOrWhiteSpace(originalFileName) || originalFileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                return Error("Invalid file name.");

            if (!AllowedContentTypes.Contains(file.ContentType))
                return Error("File type is not allowed.");

            return null;
        }

        private async Task<ResponseModel?> ValidateTaskExistsAsync(Guid taskId)
        {
            var task = await _unitOfWork.TaskHeaders.GetByGuid(taskId);
            if (task == null || !task.IsActive)
                return NotFound("Task not found.");

            return null;
        }

        private async Task<FileAttachment> SaveFileAsync(IFormFile file, Guid? taskId)
        {
            var originalFileName = Path.GetFileName(file.FileName);
            var extension        = Path.GetExtension(originalFileName);
            var storedFileName   = $"{Guid.NewGuid()}{extension}";
            var filePath         = Path.Combine(_uploadDirectory, storedFileName);
            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return new FileAttachment
            {
                FileId           = Guid.NewGuid(),
                TaskId           = taskId,
                OriginalFileName = originalFileName,
                StoredFileName   = storedFileName,
                FilePath         = filePath,
                ContentType      = file.ContentType,
                FileSize         = file.Length,
                UploadedAt       = DateTime.UtcNow,
                IsActive         = true
            };
        }

        private static FileResponseDto MapToDto(FileAttachment f) => new()
        {
            FileId           = f.FileId,
            TaskId           = f.TaskId,
            OriginalFileName = f.OriginalFileName,
            ContentType      = f.ContentType,
            FileSize         = f.FileSize,
            UploadedAt       = f.UploadedAt
        };
        
        private static ResponseModel Error(string message) =>
            new() { Status = APIStatus.Error, Message = message };

        private static ResponseModel NotFound(string message) =>
            new() { Status = APIStatus.NotFound, Message = message };
    }
}

