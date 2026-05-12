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
    public class BusquedaSemanticasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BusquedaSemanticasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/BusquedaSemanticas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BusquedaSemantica>>> GetBusquedaSemantica()
        {
            return await _context.BusquedaSemanticas.ToListAsync();
        }

        // GET: api/BusquedaSemanticas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BusquedaSemantica>> GetBusquedaSemantica(Guid id)
        {
            var busquedaSemantica = await _context.BusquedaSemanticas.FindAsync(id);

            if (busquedaSemantica == null)
            {
                return NotFound();
            }

            return busquedaSemantica;
        }

        // PUT: api/BusquedaSemanticas/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBusquedaSemantica(Guid id, BusquedaSemantica busquedaSemantica)
        {
            if (id != busquedaSemantica.IdBusqueda)
            {
                return BadRequest();
            }

            _context.Entry(busquedaSemantica).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BusquedaSemanticaExists(id))
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

        // POST: api/BusquedaSemanticas
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<BusquedaSemantica>> PostBusquedaSemantica(BusquedaSemantica busquedaSemantica)
        {
            _context.BusquedaSemanticas.Add(busquedaSemantica);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBusquedaSemantica", new { id = busquedaSemantica.IdBusqueda }, busquedaSemantica);
        }

        // DELETE: api/BusquedaSemanticas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBusquedaSemantica(Guid id)
        {
            var busquedaSemantica = await _context.BusquedaSemanticas.FindAsync(id);
            if (busquedaSemantica == null)
            {
                return NotFound();
            }

            _context.BusquedaSemanticas.Remove(busquedaSemantica);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BusquedaSemanticaExists(Guid id)
        {
            return _context.BusquedaSemanticas.Any(e => e.IdBusqueda == id);
        }
    }
}
