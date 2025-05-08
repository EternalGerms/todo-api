using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoApi.Models;
using TodoApi.Models.DTOs;

namespace TodoApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly TodoContext _context;

        public TasksController(TodoContext context)
        {
            _context = context;
        }

        // GET: api/tasks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskResponse>>> GetTasks([FromQuery] TodoApi.Models.TaskStatus? status = null)
        {
            // Get the user from HttpContext.Items that was set in the middleware
            if (HttpContext.Items["User"] is not User currentUser)
            {
                return Unauthorized();
            }

            // Get tasks for the current user
            var query = _context.TodoTasks.Where(t => t.UserId == currentUser.Id);

            // Filter by status if provided
            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }

            var tasks = await query.ToListAsync();

            // Map to response DTOs
            var taskResponses = tasks.Select(t => new TaskResponse
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status
            });

            return Ok(taskResponses);
        }

        // GET: api/tasks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskResponse>> GetTask(int id)
        {
            if (HttpContext.Items["User"] is not User currentUser)
            {
                return Unauthorized();
            }

            var task = await _context.TodoTasks.FindAsync(id);

            if (task == null)
            {
                return NotFound();
            }

            // Ensure the user owns this task
            if (task.UserId != currentUser.Id)
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

        // POST: api/tasks
        [HttpPost]
        public async Task<ActionResult<TaskResponse>> CreateTask(CreateTaskRequest request)
        {
            if (HttpContext.Items["User"] is not User currentUser)
            {
                return Unauthorized();
            }

            var task = new TodoTask
            {
                Title = request.Title,
                Description = request.Description,
                Status = request.Status,
                UserId = currentUser.Id
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

        // PUT: api/tasks/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, UpdateTaskRequest request)
        {
            if (HttpContext.Items["User"] is not User currentUser)
            {
                return Unauthorized();
            }

            var task = await _context.TodoTasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            // Ensure the user owns this task
            if (task.UserId != currentUser.Id)
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

        // DELETE: api/tasks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            if (HttpContext.Items["User"] is not User currentUser)
            {
                return Unauthorized();
            }

            var task = await _context.TodoTasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            // Ensure the user owns this task
            if (task.UserId != currentUser.Id)
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