using System.ComponentModel.DataAnnotations;

namespace TodoApi.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; } = null!;

        [Required]
        // Em um banco real, garantir unicidade
        public string Email { get; set; } = null!;

        [Required]
        public string HashSenha { get; set; } = null!;
    }
} 