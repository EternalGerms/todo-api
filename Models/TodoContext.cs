using Microsoft.EntityFrameworkCore;

namespace TodoApi.Models
{
    public class TodoContext : DbContext
    {
        public TodoContext(DbContextOptions<TodoContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<TodoTask> TodoTasks { get; set; }
    }
} 