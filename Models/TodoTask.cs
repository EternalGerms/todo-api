using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TodoApi.Models
{
    public class TodoTask
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        [Required]
        public TaskStatus Status { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }

    public enum TaskStatus
    {
        Pending,
        Done
    }
} 