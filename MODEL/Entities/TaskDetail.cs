using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MODEL.Entities
{
    [Table("TaskDetails")]
    public class TaskDetail
    {
        [Key]
        public Guid TaskDetailId { get; set; } = Guid.NewGuid();

        [ForeignKey(nameof(TaskHeader))]
        public Guid TaskId { get; set; }

        public int LineNo { get; set; }

        [Required, MaxLength(200)]
        public string ItemTitle { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? ItemDescription { get; set; }

        public bool IsCompleted { get; set; } = false;

        [MaxLength(500)]
        public string? Remark { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public TaskHeader TaskHeader { get; set; } = null!;
    }
}
