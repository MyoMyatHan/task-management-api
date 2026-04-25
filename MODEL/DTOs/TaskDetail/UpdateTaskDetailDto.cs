using System.ComponentModel.DataAnnotations;

namespace MODEL.DTOs.TaskDetail
{
    public class UpdateTaskDetailDto
    {
        [Required, MaxLength(200)]
        public string ItemTitle { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? ItemDescription { get; set; }

        public bool IsCompleted { get; set; }

        [MaxLength(500)]
        public string? Remark { get; set; }
    }
}
