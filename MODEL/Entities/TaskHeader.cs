using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MODEL.Entities
{
    [Table("TaskHeaders")]
    public class TaskHeader
    {
        [Key]
        public Guid TaskId { get; set; } = Guid.NewGuid();

        [Required, MaxLength(20)]
        public string TaskCode { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required, MaxLength(20)]
        public string Priority { get; set; } = "Medium";

        [Required, MaxLength(20)]
        public string Status { get; set; } = "Pending";

        public DateTime? DueDate { get; set; }

        [MaxLength(100)]
        public string? AssignedTo { get; set; }

        [Required, MaxLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<TaskDetail> TaskDetails { get; set; } = new List<TaskDetail>();

        public ICollection<FileAttachment> FileAttachments { get; set; } = new List<FileAttachment>();
    }
}
