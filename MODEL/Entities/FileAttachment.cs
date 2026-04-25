using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MODEL.Entities
{
    [Table("FileAttachments")]
    public class FileAttachment
    {
        [Key]
        public Guid FileId { get; set; } = Guid.NewGuid();

        [ForeignKey(nameof(TaskHeader))]
        public Guid? TaskId { get; set; }

        [Required, MaxLength(260)]
        public string OriginalFileName { get; set; } = string.Empty;

        [Required, MaxLength(260)]
        public string StoredFileName { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string ContentType { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public TaskHeader? TaskHeader { get; set; }
    }
}
