using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VitaMind.Models;

namespace VitaMind.Apii.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogOperacionesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LogOperacionesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/LogOperaciones
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LogOperacion>>> GetLogOperacion()
        {
            return await _context.LogOperaciones.ToListAsync();
        }

        // GET: api/LogOperaciones/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LogOperacion>> GetLogOperacion(Guid id)
        {
            var logOperacion = await _context.LogOperaciones.FindAsync(id);

            if (logOperacion == null)
            {
                return NotFound();
            }

            return logOperacion;
        }

        // PUT: api/LogOperaciones/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLogOperacion(Guid id, LogOperacion logOperacion)
        {
            if (id != logOperacion.IdLog)
            {
                return BadRequest();
            }

            _context.Entry(logOperacion).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LogOperacionExists(id))
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

        // POST: api/LogOperaciones
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<LogOperacion>> PostLogOperacion(LogOperacion logOperacion)
        {
            _context.LogOperaciones.Add(logOperacion);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLogOperacion", new { id = logOperacion.IdLog }, logOperacion);
        }

        // DELETE: api/LogOperaciones/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLogOperacion(Guid id)
        {
            var logOperacion = await _context.LogOperaciones.FindAsync(id);
            if (logOperacion == null)
            {
                return NotFound();
            }

            _context.LogOperaciones.Remove(logOperacion);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LogOperacionExists(Guid id)
        {
            return _context.LogOperaciones.Any(e => e.IdLog == id);
        }
    }
}
