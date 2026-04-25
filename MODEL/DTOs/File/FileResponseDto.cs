namespace MODEL.DTOs.File
{
    public class FileResponseDto
    {
        public Guid FileId { get; set; }
        public Guid? TaskId { get; set; }
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
