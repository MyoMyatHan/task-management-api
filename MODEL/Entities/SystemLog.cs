using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MODEL.Entities
{
    [Table("SystemLogs")]
    public class SystemLog
    {
        [Key]
        public Guid LogId { get; set; } = Guid.NewGuid();

        [Required, MaxLength(20)]
        public string Level { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Action { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        public string? StackTrace { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
