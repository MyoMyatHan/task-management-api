using MODEL.DTOs.TaskDetail;
using MODEL.DTOs.File;

namespace MODEL.DTOs.TaskHeader
{
    public class TaskHeaderResponseDto
    {
        public Guid TaskId { get; set; }
        public string TaskCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public string? AssignedTo { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<TaskDetailResponseDto> TaskDetails { get; set; } = new();
        public List<FileResponseDto> FileAttachments { get; set; } = new();
    }
}
