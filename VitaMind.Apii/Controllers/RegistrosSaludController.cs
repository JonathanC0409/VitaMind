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
    public class RegistrosSaludController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RegistrosSaludController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/RegistrosSalud
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RegistroSalud>>> GetRegistroSalud()
        {
            return await _context.RegistrosSalud.ToListAsync();
        }

        // GET: api/RegistrosSalud/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RegistroSalud>> GetRegistroSalud(Guid id)
        {
            var registroSalud = await _context.RegistrosSalud.FindAsync(id);

            if (registroSalud == null)
            {
                return NotFound();
            }

            return registroSalud;
        }

        // PUT: api/RegistrosSalud/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRegistroSalud(Guid id, RegistroSalud registroSalud)
        {
            if (id != registroSalud.IdRegistro)
            {
                return BadRequest();
            }

            // Asegurar fechas en UTC
            registroSalud.FechaRegistro = DateTime.SpecifyKind(registroSalud.FechaRegistro, DateTimeKind.Utc);
            registroSalud.FechaCreacion = DateTime.SpecifyKind(registroSalud.FechaCreacion, DateTimeKind.Utc);

            _context.Entry(registroSalud).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RegistroSaludExists(id))
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

        // POST: api/RegistrosSalud
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<RegistroSalud>> PostRegistroSalud(RegistroSalud registroSalud)
        {
            // Generar ID si viene vacío
            if (registroSalud.IdRegistro == Guid.Empty)
            {
                registroSalud.IdRegistro = Guid.NewGuid();
            }

            // Fechas en UTC para evitar error con PostgreSQL timestamp with time zone
            registroSalud.FechaRegistro = DateTime.UtcNow.Date;
            registroSalud.HoraRegistro = DateTime.UtcNow.TimeOfDay;
            registroSalud.FechaCreacion = DateTime.UtcNow;

            // Valores por defecto
            registroSalud.TipoEntrada = string.IsNullOrWhiteSpace(registroSalud.TipoEntrada)
                ? "Formulario"
                : registroSalud.TipoEntrada;

            registroSalud.EstadoInsercion = string.IsNullOrWhiteSpace(registroSalud.EstadoInsercion)
                ? "Insertado"
                : registroSalud.EstadoInsercion;

            registroSalud.DimensionEmbedding = 1536;

            // Por ahora puede ir vacío hasta conectar n8n/OpenAI
            registroSalud.Embedding = null;

            _context.RegistrosSalud.Add(registroSalud);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRegistroSalud", new { id = registroSalud.IdRegistro }, registroSalud);
        }

        // DELETE: api/RegistrosSalud/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRegistroSalud(Guid id)
        {
            var registroSalud = await _context.RegistrosSalud.FindAsync(id);
            if (registroSalud == null)
            {
                return NotFound();
            }

            _context.RegistrosSalud.Remove(registroSalud);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RegistroSaludExists(Guid id)
        {
            return _context.RegistrosSalud.Any(e => e.IdRegistro == id);
        }
    }
}
