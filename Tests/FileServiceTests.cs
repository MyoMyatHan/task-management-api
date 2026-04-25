using BAL.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MODEL.ApplicationConfig;
using MODEL.Entities;
using Moq;
using REPOSITORY.Repositories.IRepositories;
using REPOSITORY.UnitOfWork;
using Xunit;

namespace Tests
{
    public class FileServiceTests : IDisposable
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<ITaskHeaderRepository> _taskHeaders;
        private readonly Mock<IFileAttachmentRepository> _fileAttachments;
        private readonly string _tempDir;
        private readonly FileService _service;

        public FileServiceTests()
        {
            _taskHeaders     = new Mock<ITaskHeaderRepository>();
            _fileAttachments = new Mock<IFileAttachmentRepository>();

            _unitOfWork = new Mock<IUnitOfWork>();
            _unitOfWork.Setup(u => u.TaskHeaders).Returns(_taskHeaders.Object);
            _unitOfWork.Setup(u => u.FileAttachments).Returns(_fileAttachments.Object);

            // Use a real temp directory so the service can write files
            _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDir);

            var config = new Mock<IConfiguration>();
            config.Setup(c => c["FileStorage:UploadPath"]).Returns(_tempDir);

            _service = new FileService(_unitOfWork.Object, config.Object);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, recursive: true);
        }

        // ──────────────────────────────────────────────
        // Helpers
        // ──────────────────────────────────────────────

        private static Mock<IFormFile> CreateFormFile(
            string fileName,
            string contentType,
            long sizeBytes = 1024)
        {
            var file = new Mock<IFormFile>();
            file.Setup(f => f.FileName).Returns(fileName);
            file.Setup(f => f.ContentType).Returns(contentType);
            file.Setup(f => f.Length).Returns(sizeBytes);
            file.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return file;
        }

        // ──────────────────────────────────────────────
        // UploadFileAsync — validation
        // ──────────────────────────────────────────────

        [Fact]
        public async Task UploadFileAsync_NullFile_ReturnsError()
        {
            var result = await _service.UploadFileAsync(null!, null);

            Assert.Equal(APIStatus.Error, result.Status);
        }

        [Fact]
        public async Task UploadFileAsync_EmptyFile_ReturnsError()
        {
            var file = new Mock<IFormFile>();
            file.Setup(f => f.Length).Returns(0);

            var result = await _service.UploadFileAsync(file.Object, null);

            Assert.Equal(APIStatus.Error, result.Status);
        }

        [Fact]
        public async Task UploadFileAsync_FileTooLarge_ReturnsError()
        {
            var file = CreateFormFile("big.pdf", "application/pdf", sizeBytes: 11 * 1024 * 1024);

            var result = await _service.UploadFileAsync(file.Object, null);

            Assert.Equal(APIStatus.Error, result.Status);
            Assert.Contains("size", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task UploadFileAsync_DisallowedContentType_ReturnsError()
        {
            var file = CreateFormFile("virus.exe", "application/x-msdownload");

            var result = await _service.UploadFileAsync(file.Object, null);

            Assert.Equal(APIStatus.Error, result.Status);
            Assert.Contains("not allowed", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task UploadFileAsync_TaskIdGiven_TaskNotFound_ReturnsNotFound()
        {
            var file = CreateFormFile("report.pdf", "application/pdf");
            _taskHeaders.Setup(r => r.GetByGuid(It.IsAny<Guid>())).ReturnsAsync((TaskHeader?)null);

            var result = await _service.UploadFileAsync(file.Object, Guid.NewGuid());

            Assert.Equal(APIStatus.NotFound, result.Status);
        }

        // ──────────────────────────────────────────────
        // UploadFileAsync — success
        // ──────────────────────────────────────────────

        [Fact]
        public async Task UploadFileAsync_ValidFile_SavesRecordAndReturnsSuccessful()
        {
            var file = CreateFormFile("report.pdf", "application/pdf");
            _fileAttachments.Setup(r => r.Add(It.IsAny<FileAttachment>())).Returns(Task.CompletedTask);
            _unitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _service.UploadFileAsync(file.Object, null);

            Assert.Equal(APIStatus.Successful, result.Status);
            _fileAttachments.Verify(r => r.Add(It.IsAny<FileAttachment>()), Times.Once);
            _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UploadFileAsync_ValidFile_LinkedToTask_ReturnsSuccessful()
        {
            var taskId = Guid.NewGuid();
            var task = new TaskHeader { TaskId = taskId, IsActive = true };
            var file = CreateFormFile("doc.docx",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document");

            _taskHeaders.Setup(r => r.GetByGuid(taskId)).ReturnsAsync(task);
            _fileAttachments.Setup(r => r.Add(It.IsAny<FileAttachment>())).Returns(Task.CompletedTask);
            _unitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _service.UploadFileAsync(file.Object, taskId);

            Assert.Equal(APIStatus.Successful, result.Status);
        }

        // ──────────────────────────────────────────────
        // DownloadFileAsync
        // ──────────────────────────────────────────────

        [Fact]
        public async Task DownloadFileAsync_AttachmentNotFound_ReturnsNotFound()
        {
            _fileAttachments.Setup(r => r.GetActiveByIdAsync(It.IsAny<Guid>()))
                            .ReturnsAsync((FileAttachment?)null);

            var result = await _service.DownloadFileAsync(Guid.NewGuid());

            Assert.Equal(APIStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task DownloadFileAsync_FileNotOnDisk_ReturnsNotFound()
        {
            var attachment = new FileAttachment
            {
                FileId = Guid.NewGuid(),
                FilePath = Path.Combine(_tempDir, "missing_file.pdf"),
                OriginalFileName = "missing_file.pdf",
                ContentType = "application/pdf",
                IsActive = true
            };
            _fileAttachments.Setup(r => r.GetActiveByIdAsync(attachment.FileId)).ReturnsAsync(attachment);

            var result = await _service.DownloadFileAsync(attachment.FileId);

            Assert.Equal(APIStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task DownloadFileAsync_ValidFile_ReturnsBytesSuccessfully()
        {
            var storedPath = Path.Combine(_tempDir, "test.pdf");
            await File.WriteAllBytesAsync(storedPath, new byte[] { 1, 2, 3, 4 });

            var attachment = new FileAttachment
            {
                FileId = Guid.NewGuid(),
                FilePath = storedPath,
                OriginalFileName = "report.pdf",
                ContentType = "application/pdf",
                IsActive = true
            };
            _fileAttachments.Setup(r => r.GetActiveByIdAsync(attachment.FileId)).ReturnsAsync(attachment);

            var result = await _service.DownloadFileAsync(attachment.FileId);

            Assert.Equal(APIStatus.Successful, result.Status);
            Assert.NotNull(result.Data);
        }

        // ──────────────────────────────────────────────
        // DeleteFileAsync
        // ──────────────────────────────────────────────

        [Fact]
        public async Task DeleteFileAsync_AttachmentNotFound_ReturnsNotFound()
        {
            _fileAttachments.Setup(r => r.GetByGuid(It.IsAny<Guid>())).ReturnsAsync((FileAttachment?)null);

            var result = await _service.DeleteFileAsync(Guid.NewGuid());

            Assert.Equal(APIStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task DeleteFileAsync_InactiveAttachment_ReturnsNotFound()
        {
            var attachment = new FileAttachment { FileId = Guid.NewGuid(), IsActive = false };
            _fileAttachments.Setup(r => r.GetByGuid(attachment.FileId)).ReturnsAsync(attachment);

            var result = await _service.DeleteFileAsync(attachment.FileId);

            Assert.Equal(APIStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task DeleteFileAsync_ValidAttachment_SoftDeletesAndReturnsSuccessful()
        {
            var attachment = new FileAttachment { FileId = Guid.NewGuid(), IsActive = true };
            _fileAttachments.Setup(r => r.GetByGuid(attachment.FileId)).ReturnsAsync(attachment);
            _unitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _service.DeleteFileAsync(attachment.FileId);

            Assert.Equal(APIStatus.Successful, result.Status);
            Assert.False(attachment.IsActive);
            _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
    }
}
