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
    public class ConsultaAgentesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ConsultaAgentesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ConsultaAgentes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConsultaAgente>>> GetConsultaAgente()
        {
            return await _context.ConsultaAgentes.ToListAsync();
        }

        // GET: api/ConsultaAgentes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ConsultaAgente>> GetConsultaAgente(Guid id)
        {
            var consultaAgente = await _context.ConsultaAgentes.FindAsync(id);

            if (consultaAgente == null)
            {
                return NotFound();
            }

            return consultaAgente;
        }

        // PUT: api/ConsultaAgentes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutConsultaAgente(Guid id, ConsultaAgente consultaAgente)
        {
            if (id != consultaAgente.IdConsulta)
            {
                return BadRequest();
            }

            _context.Entry(consultaAgente).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ConsultaAgenteExists(id))
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

        // POST: api/ConsultaAgentes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ConsultaAgente>> PostConsultaAgente(ConsultaAgente consultaAgente)
        {
            _context.ConsultaAgentes.Add(consultaAgente);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetConsultaAgente", new { id = consultaAgente.IdConsulta }, consultaAgente);
        }

        // DELETE: api/ConsultaAgentes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConsultaAgente(Guid id)
        {
            var consultaAgente = await _context.ConsultaAgentes.FindAsync(id);
            if (consultaAgente == null)
            {
                return NotFound();
            }

            _context.ConsultaAgentes.Remove(consultaAgente);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ConsultaAgenteExists(Guid id)
        {
            return _context.ConsultaAgentes.Any(e => e.IdConsulta == id);
        }
    }
}
