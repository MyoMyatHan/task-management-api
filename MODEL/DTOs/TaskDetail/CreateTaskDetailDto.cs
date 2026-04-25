using System.ComponentModel.DataAnnotations;

namespace MODEL.DTOs.TaskDetail
{
    public class CreateTaskDetailDto
    {
        [Required, MaxLength(200)]
        public string ItemTitle { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? ItemDescription { get; set; }

        [MaxLength(500)]
        public string? Remark { get; set; }
    }
}
