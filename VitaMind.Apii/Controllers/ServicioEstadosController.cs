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
    public class ServicioEstadosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ServicioEstadosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ServicioEstados
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServicioEstado>>> GetServicioEstado()
        {
            return await _context.ServicioEstados.ToListAsync();
        }

        // GET: api/ServicioEstados/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ServicioEstado>> GetServicioEstado(Guid id)
        {
            var servicioEstado = await _context.ServicioEstados.FindAsync(id);

            if (servicioEstado == null)
            {
                return NotFound();
            }

            return servicioEstado;
        }

        // PUT: api/ServicioEstados/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutServicioEstado(Guid id, ServicioEstado servicioEstado)
        {
            if (id != servicioEstado.IdServicio)
            {
                return BadRequest();
            }

            _context.Entry(servicioEstado).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServicioEstadoExists(id))
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

        // POST: api/ServicioEstados
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ServicioEstado>> PostServicioEstado(ServicioEstado servicioEstado)
        {
            _context.ServicioEstados.Add(servicioEstado);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetServicioEstado", new { id = servicioEstado.IdServicio }, servicioEstado);
        }

        // DELETE: api/ServicioEstados/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServicioEstado(Guid id)
        {
            var servicioEstado = await _context.ServicioEstados.FindAsync(id);
            if (servicioEstado == null)
            {
                return NotFound();
            }

            _context.ServicioEstados.Remove(servicioEstado);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ServicioEstadoExists(Guid id)
        {
            return _context.ServicioEstados.Any(e => e.IdServicio == id);
        }
    }
}
