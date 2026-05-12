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
    public class ResultadoBusquedasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ResultadoBusquedasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ResultadoBusquedas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ResultadoBusqueda>>> GetResultadoBusqueda()
        {
            return await _context.ResultadoBusquedas.ToListAsync();
        }

        // GET: api/ResultadoBusquedas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ResultadoBusqueda>> GetResultadoBusqueda(Guid id)
        {
            var resultadoBusqueda = await _context.ResultadoBusquedas.FindAsync(id);

            if (resultadoBusqueda == null)
            {
                return NotFound();
            }

            return resultadoBusqueda;
        }

        // PUT: api/ResultadoBusquedas/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutResultadoBusqueda(Guid id, ResultadoBusqueda resultadoBusqueda)
        {
            if (id != resultadoBusqueda.IdResultado)
            {
                return BadRequest();
            }

            _context.Entry(resultadoBusqueda).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ResultadoBusquedaExists(id))
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

        // POST: api/ResultadoBusquedas
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ResultadoBusqueda>> PostResultadoBusqueda(ResultadoBusqueda resultadoBusqueda)
        {
            _context.ResultadoBusquedas.Add(resultadoBusqueda);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetResultadoBusqueda", new { id = resultadoBusqueda.IdResultado }, resultadoBusqueda);
        }

        // DELETE: api/ResultadoBusquedas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResultadoBusqueda(Guid id)
        {
            var resultadoBusqueda = await _context.ResultadoBusquedas.FindAsync(id);
            if (resultadoBusqueda == null)
            {
                return NotFound();
            }

            _context.ResultadoBusquedas.Remove(resultadoBusqueda);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ResultadoBusquedaExists(Guid id)
        {
            return _context.ResultadoBusquedas.Any(e => e.IdResultado == id);
        }
    }
}
