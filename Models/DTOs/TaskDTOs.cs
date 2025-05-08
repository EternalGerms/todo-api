using System.ComponentModel.DataAnnotations;
using TodoApi.Models;

namespace TodoApi.Models.DTOs
{
    public class CreateTaskRequest
    {
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public TodoApi.Models.TaskStatus Status { get; set; } = TodoApi.Models.TaskStatus.Pending;
    }

    public class UpdateTaskRequest
    {
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        [Required]
        public TodoApi.Models.TaskStatus Status { get; set; }
    }

    public class TaskResponse
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public TodoApi.Models.TaskStatus Status { get; set; }
    }
} 