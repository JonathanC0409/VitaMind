using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
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
        private readonly IConfiguration _configuration;

        public ServicioEstadosController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: api/ServicioEstados
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServicioEstado>>> GetServicioEstado()
        {
            return await _context.ServicioEstados
                .OrderBy(s => s.NombreServicio)
                .ToListAsync();
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

        // ============================================================
        // GET: api/ServicioEstados/verificar
        // Verifica automáticamente el estado de los servicios principales
        // ============================================================
        [HttpGet("verificar")]
        public async Task<IActionResult> VerificarServicios()
        {
            var ahora = DateTime.UtcNow;

            // 1. API .NET
            await CrearOActualizarServicio(
                "API .NET",
                "Activo",
                ahora,
                "API funcionando correctamente."
            );

            // 2. Supabase / PostgreSQL / pgVector
            try
            {
                await _context.Usuarios.AsNoTracking().Take(1).AnyAsync();

                await CrearOActualizarServicio(
                    "Supabase Vector",
                    "Activo",
                    ahora,
                    "Base de datos PostgreSQL/Supabase disponible correctamente."
                );
            }
            catch (Exception ex)
            {
                await CrearOActualizarServicio(
                    "Supabase Vector",
                    "Error",
                    ahora,
                    "No se pudo conectar con Supabase/PostgreSQL: " + ex.Message
                );
            }

            // 3. n8n
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(3);

                var response = await client.GetAsync("http://localhost:5678");

                await CrearOActualizarServicio(
                    "n8n",
                    "Activo",
                    ahora,
                    "n8n responde correctamente en localhost:5678."
                );
            }
            catch (Exception ex)
            {
                await CrearOActualizarServicio(
                    "n8n",
                    "Error",
                    ahora,
                    "n8n no está disponible en localhost:5678: " + ex.Message
                );
            }

            // 4. OpenAI API
            var apiKey =
                _configuration["OPENAI_API_KEY"] ??
                Environment.GetEnvironmentVariable("OPENAI_API_KEY");

            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                await CrearOActualizarServicio(
                    "OpenAI API",
                    "Activo",
                    ahora,
                    "Clave de OpenAI configurada correctamente en el entorno."
                );
            }
            else
            {
                await CrearOActualizarServicio(
                    "OpenAI API",
                    "No configurado",
                    ahora,
                    "No se encontró OPENAI_API_KEY en la configuración del backend."
                );
            }

            await _context.SaveChangesAsync();

            var servicios = await _context.ServicioEstados
                .OrderBy(s => s.NombreServicio)
                .ToListAsync();

            return Ok(servicios);
        }

        // PUT: api/ServicioEstados/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutServicioEstado(Guid id, ServicioEstado servicioEstado)
        {
            if (id != servicioEstado.IdServicio)
            {
                return BadRequest();
            }

            servicioEstado.UltimoChequeo = DateTime.SpecifyKind(
                servicioEstado.UltimoChequeo,
                DateTimeKind.Utc
            );

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

                throw;
            }

            return NoContent();
        }

        // POST: api/ServicioEstados
        [HttpPost]
        public async Task<ActionResult<ServicioEstado>> PostServicioEstado(ServicioEstado servicioEstado)
        {
            if (servicioEstado.IdServicio == Guid.Empty)
            {
                servicioEstado.IdServicio = Guid.NewGuid();
            }

            servicioEstado.UltimoChequeo = DateTime.SpecifyKind(
                servicioEstado.UltimoChequeo,
                DateTimeKind.Utc
            );

            _context.ServicioEstados.Add(servicioEstado);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                "GetServicioEstado",
                new { id = servicioEstado.IdServicio },
                servicioEstado
            );
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

        private async Task CrearOActualizarServicio(
            string nombreServicio,
            string estado,
            DateTime ultimoChequeo,
            string mensaje)
        {
            var servicio = await _context.ServicioEstados
                .FirstOrDefaultAsync(s => s.NombreServicio == nombreServicio);

            if (servicio == null)
            {
                servicio = new ServicioEstado
                {
                    IdServicio = Guid.NewGuid(),
                    NombreServicio = nombreServicio,
                    Estado = estado,
                    UltimoChequeo = ultimoChequeo,
                    Mensaje = mensaje
                };

                _context.ServicioEstados.Add(servicio);
            }
            else
            {
                servicio.Estado = estado;
                servicio.UltimoChequeo = ultimoChequeo;
                servicio.Mensaje = mensaje;
            }
        }

        private bool ServicioEstadoExists(Guid id)
        {
            return _context.ServicioEstados.Any(e => e.IdServicio == id);
        }
    }
}