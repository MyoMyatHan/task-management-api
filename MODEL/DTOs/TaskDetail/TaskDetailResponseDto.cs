namespace MODEL.DTOs.TaskDetail
{
    public class TaskDetailResponseDto
    {
        public Guid TaskDetailId { get; set; }
        public Guid TaskId { get; set; }
        public int LineNo { get; set; }
        public string ItemTitle { get; set; } = string.Empty;
        public string? ItemDescription { get; set; }
        public bool IsCompleted { get; set; }
        public string? Remark { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
