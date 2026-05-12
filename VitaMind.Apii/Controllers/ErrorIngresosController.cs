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
    public class ErrorIngresosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ErrorIngresosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ErrorIngresos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ErrorIngreso>>> GetErrorIngreso()
        {
            return await _context.ErrorIngresos.ToListAsync();
        }

        // GET: api/ErrorIngresos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ErrorIngreso>> GetErrorIngreso(Guid id)
        {
            var errorIngreso = await _context.ErrorIngresos.FindAsync(id);

            if (errorIngreso == null)
            {
                return NotFound();
            }

            return errorIngreso;
        }

        // PUT: api/ErrorIngresos/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutErrorIngreso(Guid id, ErrorIngreso errorIngreso)
        {
            if (id != errorIngreso.IdError)
            {
                return BadRequest();
            }

            _context.Entry(errorIngreso).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ErrorIngresoExists(id))
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

        // POST: api/ErrorIngresos
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ErrorIngreso>> PostErrorIngreso(ErrorIngreso errorIngreso)
        {
            _context.ErrorIngresos.Add(errorIngreso);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetErrorIngreso", new { id = errorIngreso.IdError }, errorIngreso);
        }

        // DELETE: api/ErrorIngresos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteErrorIngreso(Guid id)
        {
            var errorIngreso = await _context.ErrorIngresos.FindAsync(id);
            if (errorIngreso == null)
            {
                return NotFound();
            }

            _context.ErrorIngresos.Remove(errorIngreso);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ErrorIngresoExists(Guid id)
        {
            return _context.ErrorIngresos.Any(e => e.IdError == id);
        }
    }
}
