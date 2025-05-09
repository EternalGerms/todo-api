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
    [Route("api/todos")]
    public class TodosController : ControllerBase
    {
        private readonly TodoContext _context;

        public TodosController(TodoContext context)
        {
            _context = context;
        }

        // GET: api/todos
        [HttpGet]
        public async Task<ActionResult<object>> GetTodos(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            [FromQuery] TodoApi.Models.TaskStatus? status = null,
            [FromQuery] string sort = "id",
            [FromQuery] string order = "asc")
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst(ClaimTypes.Name) ?? User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized();
            }

            // Get tasks for the current user
            var query = _context.TodoTasks.Where(t => t.UserId == userId);

            // Filter by status if provided
            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }

            // Sorting
            bool descending = order.ToLower() == "desc";
            switch (sort.ToLower())
            {
                case "title":
                    query = descending ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title);
                    break;
                case "status":
                    query = descending ? query.OrderByDescending(t => t.Status) : query.OrderBy(t => t.Status);
                    break;
                case "id":
                default:
                    query = descending ? query.OrderByDescending(t => t.Id) : query.OrderBy(t => t.Id);
                    break;
            }

            var total = await query.CountAsync();
            var tasks = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            // Map to response DTOs
            var taskResponses = tasks.Select(t => new TaskResponse
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status
            });

            return Ok(new {
                data = taskResponses,
                page,
                limit,
                total
            });
        }

        // GET: api/todos/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskResponse>> GetTask(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst(ClaimTypes.Name) ?? User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized();
            }

            var task = await _context.TodoTasks.FindAsync(id);

            if (task == null)
            {
                return NotFound();
            }

            // Ensure the user owns this task
            if (task.UserId != userId)
            {
                return Forbid();
            }

            var response = new TaskResponse
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status
            };

            return response;
        }

        // POST: api/todos
        [HttpPost]
        public async Task<ActionResult<TaskResponse>> CreateTask(CreateTaskRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst(ClaimTypes.Name) ?? User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized();
            }

            var task = new TodoTask
            {
                Title = request.Title,
                Description = request.Description,
                Status = request.Status,
                UserId = userId
            };

            _context.TodoTasks.Add(task);
            await _context.SaveChangesAsync();

            var response = new TaskResponse
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status
            };

            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, response);
        }

        // PUT: api/todos/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, UpdateTaskRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst(ClaimTypes.Name) ?? User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized();
            }

            var task = await _context.TodoTasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            // Ensure the user owns this task
            if (task.UserId != userId)
            {
                return Forbid();
            }

            // Update task properties
            task.Title = request.Title;
            task.Description = request.Description;
            task.Status = request.Status;

            _context.Entry(task).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaskExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/todos/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst(ClaimTypes.Name) ?? User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized();
            }

            var task = await _context.TodoTasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            // Ensure the user owns this task
            if (task.UserId != userId)
            {
                return Forbid();
            }

            _context.TodoTasks.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TaskExists(int id)
        {
            return _context.TodoTasks.Any(e => e.Id == id);
        }
    }
} 