using Microsoft.EntityFrameworkCore;

namespace TodoApi.Models
{
    public class TodoContext : DbContext
    {
        public TodoContext(DbContextOptions<TodoContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<TodoTask> Tarefas { get; set; }
    }
} 