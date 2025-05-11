using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoApi.Models;
using TodoApi.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace TodoApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/tarefas")]
    public class TarefasController : ControllerBase
    {
        private readonly TodoContext _contexto;

        public TarefasController(TodoContext contexto)
        {
            _contexto = contexto;
        }

        // GET: api/tarefas
        [HttpGet]
        public async Task<ActionResult<object>> ObterTarefas(
            [FromQuery] int pagina = 1,
            [FromQuery] int limite = 10,
            [FromQuery] TodoApi.Models.StatusTarefa? status = null,
            [FromQuery] string ordenarPor = "id",
            [FromQuery] string ordem = "asc")
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst(ClaimTypes.Name) ?? User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (usuarioIdClaim == null || !int.TryParse(usuarioIdClaim.Value, out int usuarioId))
            {
                return StatusCode(401, new { mensagem = "Não autorizado: Token não fornecido ou inválido." });
            }

            var consulta = _contexto.TodoTasks.Where(t => t.UserId == usuarioId);

            if (status.HasValue)
            {
                consulta = consulta.Where(t => t.Status == status.Value);
            }

            bool descendente = ordem.ToLower() == "desc";
            switch (ordenarPor.ToLower())
            {
                case "title":
                    consulta = descendente ? consulta.OrderByDescending(t => t.Title) : consulta.OrderBy(t => t.Title);
                    break;
                case "status":
                    consulta = descendente ? consulta.OrderByDescending(t => t.Status) : consulta.OrderBy(t => t.Status);
                    break;
                case "id":
                default:
                    consulta = descendente ? consulta.OrderByDescending(t => t.Id) : consulta.OrderBy(t => t.Id);
                    break;
            }

            var total = await consulta.CountAsync();
            var tarefas = await consulta
                .Skip((pagina - 1) * limite)
                .Take(limite)
                .ToListAsync();

            var respostaTarefas = tarefas.Select(t => new RespostaTarefa
            {
                Id = t.Id,
                Titulo = t.Title,
                Descricao = t.Description,
                Status = t.Status
            });

            return Ok(new {
                dados = respostaTarefas,
                pagina,
                limite,
                total
            });
        }

        // GET: api/tarefas/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<RespostaTarefa>> ObterTarefa(int id)
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst(ClaimTypes.Name) ?? User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (usuarioIdClaim == null || !int.TryParse(usuarioIdClaim.Value, out int usuarioId))
            {
                return StatusCode(401, new { mensagem = "Não autorizado: Token não fornecido ou inválido." });
            }

            var tarefa = await _contexto.TodoTasks.FindAsync(id);

            if (tarefa == null)
            {
                return NotFound(new { mensagem = "Não encontrada: Tarefa não localizada." });
            }

            if (tarefa.UserId != usuarioId)
            {
                return StatusCode(403, new { mensagem = "Proibido: Você não tem permissão para acessar esta tarefa." });
            }

            var resposta = new RespostaTarefa
            {
                Id = tarefa.Id,
                Titulo = tarefa.Title,
                Descricao = tarefa.Description,
                Status = tarefa.Status
            };

            return resposta;
        }

        // POST: api/tarefas
        [HttpPost]
        public async Task<ActionResult<RespostaTarefa>> CriarTarefa(RequisicaoCriarTarefa requisicao)
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst(ClaimTypes.Name) ?? User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (usuarioIdClaim == null || !int.TryParse(usuarioIdClaim.Value, out int usuarioId))
            {
                return StatusCode(401, new { mensagem = "Não autorizado: Token não fornecido ou inválido." });
            }

            var tarefa = new TodoTask
            {
                Title = requisicao.Titulo,
                Description = requisicao.Descricao,
                Status = requisicao.Status,
                UserId = usuarioId
            };

            _contexto.TodoTasks.Add(tarefa);
            await _contexto.SaveChangesAsync();

            var resposta = new RespostaTarefa
            {
                Id = tarefa.Id,
                Titulo = tarefa.Title,
                Descricao = tarefa.Description,
                Status = tarefa.Status
            };

            return CreatedAtAction(nameof(ObterTarefa), new { id = tarefa.Id }, resposta);
        }

        // PUT: api/tarefas/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarTarefa(int id, RequisicaoAtualizarTarefa requisicao)
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst(ClaimTypes.Name) ?? User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (usuarioIdClaim == null || !int.TryParse(usuarioIdClaim.Value, out int usuarioId))
            {
                return StatusCode(401, new { mensagem = "Não autorizado: Token não fornecido ou inválido." });
            }

            var tarefa = await _contexto.TodoTasks.FindAsync(id);
            if (tarefa == null)
            {
                return NotFound(new { mensagem = "Não encontrada: Tarefa não localizada." });
            }

            if (tarefa.UserId != usuarioId)
            {
                return StatusCode(403, new { mensagem = "Proibido: Você não tem permissão para atualizar esta tarefa." });
            }

            tarefa.Title = requisicao.Titulo;
            tarefa.Description = requisicao.Descricao;
            tarefa.Status = requisicao.Status;

            _contexto.Entry(tarefa).State = EntityState.Modified;

            try
            {
                await _contexto.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TarefaExiste(id))
                {
                    return NotFound(new { mensagem = "Não encontrada: Tarefa não localizada." });
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/tarefas/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletarTarefa(int id)
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst(ClaimTypes.Name) ?? User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (usuarioIdClaim == null || !int.TryParse(usuarioIdClaim.Value, out int usuarioId))
            {
                return StatusCode(401, new { mensagem = "Não autorizado: Token não fornecido ou inválido." });
            }

            var tarefa = await _contexto.TodoTasks.FindAsync(id);
            if (tarefa == null)
            {
                return NotFound(new { mensagem = "Não encontrada: Tarefa não localizada." });
            }

            if (tarefa.UserId != usuarioId)
            {
                return StatusCode(403, new { mensagem = "Proibido: Você não tem permissão para deletar esta tarefa." });
            }

            _contexto.TodoTasks.Remove(tarefa);
            await _contexto.SaveChangesAsync();

            return NoContent();
        }

        private bool TarefaExiste(int id)
        {
            return _contexto.TodoTasks.Any(e => e.Id == id);
        }
    }
} 