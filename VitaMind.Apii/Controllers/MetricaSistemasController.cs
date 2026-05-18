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
    public class MetricaSistemasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MetricaSistemasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/MetricaSistemas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MetricaSistema>>> GetMetricaSistema()
        {
            return await _context.MetricaSistemas.ToListAsync();
        }

        // GET: api/MetricaSistemas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MetricaSistema>> GetMetricaSistema(Guid id)
        {
            var metricaSistema = await _context.MetricaSistemas.FindAsync(id);

            if (metricaSistema == null)
            {
                return NotFound();
            }

            return metricaSistema;
        }

        // PUT: api/MetricaSistemas/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMetricaSistema(Guid id, MetricaSistema metricaSistema)
        {
            if (id != metricaSistema.IdMetrica)
            {
                return BadRequest();
            }

            _context.Entry(metricaSistema).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MetricaSistemaExists(id))
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

        // METODO AGREGADO :ANDER
        // GET: api/MetricaSistemas/dashboard-admin
        [HttpGet("dashboard-admin")]
        public async Task<IActionResult> GetDashboardAdmin()
        {
            var totalRegistros = await _context.RegistrosSalud.CountAsync();

            var totalUsuarios = await _context.Usuarios.CountAsync();

            var totalConsultas = await _context.ConsultaAgentes.CountAsync();

            var totalErrores = await _context.ErrorIngresos.CountAsync();

            var latenciaPromedio = await _context.RegistrosSalud
                .Where(r => r.EstadoInsercion == "Insertado")
                .AverageAsync(r => (double?)r.LatenciaMs) ?? 0;

            var tiempoConsultaSemantica = await _context.BusquedaSemanticas
                .Where(b => b.Estado == "Exitosa")
                .AverageAsync(b => (double?)b.TiempoBusquedaMs) ?? 0;

            var relevanciaPromedio = await _context.BusquedaSemanticas
                .Where(b => b.Estado == "Exitosa")
                .AverageAsync(b => (decimal?)b.SimilitudPromedio) ?? 0;

            var registrosSimilares = await _context.RegistrosSalud
                .CountAsync(r => r.EsSimilar);

            var vectoresAlmacenados = await _context.RegistrosSalud
                .CountAsync(r => r.DimensionEmbedding > 0);

            var totalIntentos = totalRegistros + totalErrores;

            var tasaExito = totalIntentos > 0
                ? Math.Round((double)totalRegistros / totalIntentos * 100, 2)
                : 0;


            // ==============================
            // MÉTRICAS ADICIONALES
            // ==============================

            // 1. Tiempo promedio de generación de embeddings
            var tiempoPromedioEmbedding = await _context.RegistrosSalud
                .Where(r => r.TiempoEmbeddingMs > 0)
                .AverageAsync(r => (double?)r.TiempoEmbeddingMs) ?? 0;

            // 2. Registros por usuario
            var registrosPorUsuario = await _context.Usuarios
                .Select(u => new
                {
                    usuario = u.Nombre,
                    rol = u.Rol,
                    totalRegistros = _context.RegistrosSalud.Count(r => r.IdUsuario == u.IdUsuario)
                })
                .ToListAsync();

            // 3. Consultas por tipo de usuario
            var consultasPorTipoUsuario = await _context.ConsultaAgentes
                .Include(c => c.Usuario)
                .GroupBy(c => c.Usuario != null ? c.Usuario.Rol : "Sin rol")
                .Select(g => new
                {
                    tipoUsuario = g.Key,
                    totalConsultas = g.Count()
                })
                .ToListAsync();

            // 4. Promedio de similitud semántica desde resultados de búsqueda
            var promedioSimilitudResultados = await _context.ResultadoBusquedas
                .Where(r => r.NivelSimilitud > 0)
                .AverageAsync(r => (decimal?)r.NivelSimilitud) ?? 0;

            // 5. Intentos fallidos por usuario
            var intentosFallidosPorUsuario = await _context.ErrorIngresos
                .Include(e => e.Usuario)
                .GroupBy(e => e.Usuario != null ? e.Usuario.Nombre : "Usuario no identificado")
                .Select(g => new
                {
                    usuario = g.Key,
                    totalErrores = g.Count()
                })
                .ToListAsync();

            // 6. Top 5 términos consultados al agente
            var preguntasAgente = await _context.ConsultaAgentes
                .Select(c => c.Pregunta)
                .ToListAsync();

            var palabrasIgnoradas = new HashSet<string>
{
    "que", "qué", "cual", "cuál", "como", "cómo", "los", "las", "con", "para",
    "por", "del", "mis", "tengo", "una", "uno", "hay", "son", "fue", "mi",
    "el", "la", "de", "en", "y", "a"
};

            var topTerminosConsultados = preguntasAgente
                .SelectMany(p => p.ToLower()
                    .Split(new[] { ' ', '.', ',', ';', ':', '?', '¿', '!', '¡', '\n', '\r' },
                    StringSplitOptions.RemoveEmptyEntries))
                .Where(p => p.Length > 3 && !palabrasIgnoradas.Contains(p))
                .GroupBy(p => p)
                .Select(g => new
                {
                    termino = g.Key,
                    cantidad = g.Count()
                })
                .OrderByDescending(x => x.cantidad)
                .Take(5)
                .ToList();

            // 7. Registros por fecha
            var registrosPorFecha = await _context.RegistrosSalud
                .GroupBy(r => r.FechaRegistro)
                .Select(g => new
                {
                    fecha = g.Key,
                    totalRegistros = g.Count()
                })
                .OrderBy(x => x.fecha)
                .ToListAsync();

            // 8. Estado de servicios
            var estadoServicios = await _context.ServicioEstados
                .OrderBy(s => s.NombreServicio)
                .Select(s => new
                {
                    servicio = s.NombreServicio,
                    estado = s.Estado,
                    ultimoChequeo = s.UltimoChequeo,
                    mensaje = s.Mensaje
                })
                .ToListAsync();

            // 9. Dimensión de embedding
            var dimensionEmbedding = await _context.RegistrosSalud
                .Where(r => r.DimensionEmbedding > 0)
                .Select(r => r.DimensionEmbedding)
                .FirstOrDefaultAsync();


            var dashboard = new
            {
                // ==============================
                // MÉTRICAS PRINCIPALES
                // ==============================
                totalRegistros,
                totalUsuarios,
                totalConsultas,
                errores = totalErrores,
                latenciaPromedio = Math.Round(latenciaPromedio, 2),
                tasaExito,
                tiempoConsulta = Math.Round(tiempoConsultaSemantica, 2),
                relevancia = Math.Round(relevanciaPromedio, 2),
                registrosSimilares,
                vectores = vectoresAlmacenados,
                dimensionEmbedding,

                // ==============================
                // MÉTRICAS ADICIONALES
                // ==============================
                tiempoPromedioEmbedding = Math.Round(tiempoPromedioEmbedding, 2),
                registrosPorUsuario,
                consultasPorTipoUsuario,
                promedioSimilitudResultados = Math.Round(promedioSimilitudResultados, 2),
                intentosFallidosPorUsuario,
                topTerminosConsultados,
                registrosPorFecha,
                estadoServicios
            };

            return Ok(dashboard);
        }

        // POST: api/MetricaSistemas
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<MetricaSistema>> PostMetricaSistema(MetricaSistema metricaSistema)
        {
            _context.MetricaSistemas.Add(metricaSistema);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMetricaSistema", new { id = metricaSistema.IdMetrica }, metricaSistema);
        }

        // DELETE: api/MetricaSistemas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMetricaSistema(Guid id)
        {
            var metricaSistema = await _context.MetricaSistemas.FindAsync(id);
            if (metricaSistema == null)
            {
                return NotFound();
            }

            _context.MetricaSistemas.Remove(metricaSistema);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MetricaSistemaExists(Guid id)
        {
            return _context.MetricaSistemas.Any(e => e.IdMetrica == id);
        }
    }
}
