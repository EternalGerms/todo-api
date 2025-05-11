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
        public StatusTarefa Status { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }

    public enum StatusTarefa
    {
        Pendente,
        Concluida
    }

    // DTOs para uso nos controllers traduzidos
    public class RequisicaoCriarTarefa
    {
        [Required]
        public string Titulo { get; set; } = null!;
        public string Descricao { get; set; } = null!;
        [Required]
        public StatusTarefa Status { get; set; }
    }

    public class RequisicaoAtualizarTarefa
    {
        [Required]
        public string Titulo { get; set; } = null!;
        public string Descricao { get; set; } = null!;
        [Required]
        public StatusTarefa Status { get; set; }
    }

    public class RespostaTarefa
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = null!;
        public string Descricao { get; set; } = null!;
        public StatusTarefa Status { get; set; }
    }
} 