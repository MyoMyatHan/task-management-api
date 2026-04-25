using System.ComponentModel.DataAnnotations;

namespace MODEL.DTOs.TaskHeader
{
    public class UpdateTaskHeaderDto
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        [RegularExpression("^(Low|Medium|High)$", ErrorMessage = "Priority must be Low, Medium, or High.")]
        public string Priority { get; set; } = "Medium";

        [Required]
        [RegularExpression("^(Pending|InProgress|Done)$", ErrorMessage = "Status must be Pending, InProgress, or Done.")]
        public string Status { get; set; } = "Pending";

        public DateTime? DueDate { get; set; }

        [MaxLength(100)]
        public string? AssignedTo { get; set; }
    }
}
