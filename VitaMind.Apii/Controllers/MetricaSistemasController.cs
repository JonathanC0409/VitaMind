using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            return await _context.MetricaSistemas
                .AsNoTracking()
                .ToListAsync();
        }

        // GET: api/MetricaSistemas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MetricaSistema>> GetMetricaSistema(Guid id)
        {
            var metricaSistema = await _context.MetricaSistemas
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdMetrica == id);

            if (metricaSistema == null)
            {
                return NotFound();
            }

            return metricaSistema;
        }

        // PUT: api/MetricaSistemas/5
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

                throw;
            }

            return NoContent();
        }

        // ============================================================
        // DASHBOARD COMPLETO
        // Se mantiene por compatibilidad con la vista anterior.
        // ============================================================
        // GET: api/MetricaSistemas/dashboard-admin
        [HttpGet("dashboard-admin")]
        public async Task<IActionResult> GetDashboardAdmin()
        {
            var totalRegistros = await _context.RegistrosSalud
                .AsNoTracking()
                .CountAsync();

            var totalUsuarios = await _context.Usuarios
                .AsNoTracking()
                .CountAsync();

            var totalConsultas = await _context.ConsultaAgentes
                .AsNoTracking()
                .CountAsync();

            var totalErrores = await _context.ErrorIngresos
                .AsNoTracking()
                .CountAsync();

            var latenciaPromedio = await _context.RegistrosSalud
                .AsNoTracking()
                .Where(r => r.EstadoInsercion == "Insertado")
                .AverageAsync(r => (double?)r.LatenciaMs) ?? 0;

            var tiempoConsultaSemantica = await _context.BusquedaSemanticas
                .AsNoTracking()
                .Where(b => b.Estado == "Exitosa")
                .AverageAsync(b => (double?)b.TiempoBusquedaMs) ?? 0;

            var relevanciaPromedio = await _context.BusquedaSemanticas
                .AsNoTracking()
                .Where(b => b.Estado == "Exitosa")
                .AverageAsync(b => (decimal?)b.SimilitudPromedio) ?? 0;

            var registrosSimilares = await _context.RegistrosSalud
                .AsNoTracking()
                .CountAsync(r => r.EsSimilar);

            var vectoresAlmacenados = await _context.RegistrosSalud
                .AsNoTracking()
                .CountAsync(r => r.DimensionEmbedding > 0);

            var totalIntentos = totalRegistros + totalErrores;

            var tasaExito = totalIntentos > 0
                ? Math.Round((double)totalRegistros / totalIntentos * 100, 2)
                : 0;

            var tiempoPromedioEmbedding = await _context.RegistrosSalud
                .AsNoTracking()
                .Where(r => r.TiempoEmbeddingMs > 0)
                .AverageAsync(r => (double?)r.TiempoEmbeddingMs) ?? 0;

            var registrosPorUsuario = await _context.Usuarios
                .AsNoTracking()
                .Select(u => new
                {
                    usuario = u.Nombre,
                    rol = u.Rol,
                    totalRegistros = _context.RegistrosSalud
                        .AsNoTracking()
                        .Count(r => r.IdUsuario == u.IdUsuario)
                })
                .ToListAsync();

            var consultasPorTipoUsuario = await _context.ConsultaAgentes
                .AsNoTracking()
                .Join(
                    _context.Usuarios.AsNoTracking(),
                    consulta => consulta.IdUsuario,
                    usuario => usuario.IdUsuario,
                    (consulta, usuario) => new
                    {
                        usuario.Rol
                    }
                )
                .GroupBy(x => x.Rol)
                .Select(g => new
                {
                    tipoUsuario = g.Key,
                    totalConsultas = g.Count()
                })
                .ToListAsync();

            var promedioSimilitudResultados = await _context.ResultadoBusquedas
                .AsNoTracking()
                .Where(r => r.NivelSimilitud > 0)
                .AverageAsync(r => (decimal?)r.NivelSimilitud) ?? 0;

            var intentosFallidosPorUsuario = await _context.ErrorIngresos
                .AsNoTracking()
                .GroupJoin(
                    _context.Usuarios.AsNoTracking(),
                    error => error.IdUsuario,
                    usuario => usuario.IdUsuario,
                    (error, usuarios) => new
                    {
                        Usuario = usuarios.Select(u => u.Nombre).FirstOrDefault() ?? "Usuario no identificado"
                    }
                )
                .GroupBy(x => x.Usuario)
                .Select(g => new
                {
                    usuario = g.Key,
                    totalErrores = g.Count()
                })
                .ToListAsync();

            var preguntasAgente = await _context.ConsultaAgentes
                .AsNoTracking()
                .OrderByDescending(c => c.FechaHora)
                .Select(c => c.Pregunta)
                .Take(100)
                .ToListAsync();

            var topTerminosConsultados = CalcularTopTerminos(preguntasAgente);

            var registrosPorFecha = await ObtenerRegistrosPorFecha();

            var estadoServicios = await _context.ServicioEstados
                .AsNoTracking()
                .OrderBy(s => s.NombreServicio)
                .Select(s => new
                {
                    servicio = s.NombreServicio,
                    estado = s.Estado,
                    ultimoChequeo = s.UltimoChequeo,
                    mensaje = s.Mensaje
                })
                .ToListAsync();

            var dimensionEmbedding = await _context.RegistrosSalud
                .AsNoTracking()
                .Where(r => r.DimensionEmbedding > 0)
                .Select(r => r.DimensionEmbedding)
                .FirstOrDefaultAsync();

            var dashboard = new
            {
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

        // ============================================================
        // NUEVOS ENDPOINTS SEPARADOS PARA OPTIMIZAR EL DASHBOARD
        // ============================================================

        // GET: api/MetricaSistemas/resumen-admin
        [HttpGet("resumen-admin")]
        public async Task<IActionResult> GetResumenAdmin()
        {
            var totalRegistros = await _context.RegistrosSalud
                .AsNoTracking()
                .CountAsync();

            var totalUsuarios = await _context.Usuarios
                .AsNoTracking()
                .CountAsync();

            var totalConsultas = await _context.ConsultaAgentes
                .AsNoTracking()
                .CountAsync();

            var totalErrores = await _context.ErrorIngresos
                .AsNoTracking()
                .CountAsync();

            var vectores = await _context.RegistrosSalud
                .AsNoTracking()
                .CountAsync(r => r.DimensionEmbedding > 0);

            var totalIntentos = totalRegistros + totalErrores;

            var tasaExito = totalIntentos > 0
                ? Math.Round((double)totalRegistros / totalIntentos * 100, 2)
                : 0;

            return Ok(new
            {
                totalRegistros,
                totalUsuarios,
                totalConsultas,
                errores = totalErrores,
                tasaExito,
                vectores
            });
        }

        // GET: api/MetricaSistemas/registros
        [HttpGet("registros")]
        public async Task<IActionResult> GetMetricasRegistros()
        {
            var totalRegistros = await _context.RegistrosSalud
                .AsNoTracking()
                .CountAsync();

            var registrosPorUsuario = await _context.Usuarios
                .AsNoTracking()
                .Select(u => new
                {
                    usuario = u.Nombre,
                    rol = u.Rol,
                    totalRegistros = _context.RegistrosSalud
                        .AsNoTracking()
                        .Count(r => r.IdUsuario == u.IdUsuario)
                })
                .ToListAsync();

            var registrosPorFecha = await ObtenerRegistrosPorFecha();

            return Ok(new
            {
                totalRegistros,
                registrosPorUsuario,
                registrosPorFecha
            });
        }

        // GET: api/MetricaSistemas/rendimiento
        [HttpGet("rendimiento")]
        public async Task<IActionResult> GetMetricasRendimiento()
        {
            var latenciaPromedio = await _context.RegistrosSalud
                .AsNoTracking()
                .Where(r => r.EstadoInsercion == "Insertado")
                .AverageAsync(r => (double?)r.LatenciaMs) ?? 0;

            var tiempoPromedioEmbedding = await _context.RegistrosSalud
                .AsNoTracking()
                .Where(r => r.TiempoEmbeddingMs > 0)
                .AverageAsync(r => (double?)r.TiempoEmbeddingMs) ?? 0;

            var vectores = await _context.RegistrosSalud
                .AsNoTracking()
                .CountAsync(r => r.DimensionEmbedding > 0);

            var dimensionEmbedding = await _context.RegistrosSalud
                .AsNoTracking()
                .Where(r => r.DimensionEmbedding > 0)
                .Select(r => r.DimensionEmbedding)
                .FirstOrDefaultAsync();

            return Ok(new
            {
                latenciaPromedio = Math.Round(latenciaPromedio, 2),
                tiempoPromedioEmbedding = Math.Round(tiempoPromedioEmbedding, 2),
                vectores,
                dimensionEmbedding
            });
        }

        // GET: api/MetricaSistemas/calidad
        [HttpGet("calidad")]
        public async Task<IActionResult> GetMetricasCalidad()
        {
            var totalRegistros = await _context.RegistrosSalud
                .AsNoTracking()
                .CountAsync();

            var totalErrores = await _context.ErrorIngresos
                .AsNoTracking()
                .CountAsync();

            var registrosSimilares = await _context.RegistrosSalud
                .AsNoTracking()
                .CountAsync(r => r.EsSimilar);

            var totalIntentos = totalRegistros + totalErrores;

            var tasaExito = totalIntentos > 0
                ? Math.Round((double)totalRegistros / totalIntentos * 100, 2)
                : 0;

            var intentosFallidosPorUsuario = await _context.ErrorIngresos
                .AsNoTracking()
                .GroupJoin(
                    _context.Usuarios.AsNoTracking(),
                    error => error.IdUsuario,
                    usuario => usuario.IdUsuario,
                    (error, usuarios) => new
                    {
                        Usuario = usuarios.Select(u => u.Nombre).FirstOrDefault() ?? "Usuario no identificado"
                    }
                )
                .GroupBy(x => x.Usuario)
                .Select(g => new
                {
                    usuario = g.Key,
                    totalErrores = g.Count()
                })
                .ToListAsync();

            return Ok(new
            {
                errores = totalErrores,
                tasaExito,
                registrosSimilares,
                intentosFallidosPorUsuario
            });
        }

        // GET: api/MetricaSistemas/semantica
        [HttpGet("semantica")]
        public async Task<IActionResult> GetMetricasSemantica()
        {
            var tiempoConsulta = await _context.BusquedaSemanticas
                .AsNoTracking()
                .Where(b => b.Estado == "Exitosa")
                .AverageAsync(b => (double?)b.TiempoBusquedaMs) ?? 0;

            var relevancia = await _context.BusquedaSemanticas
                .AsNoTracking()
                .Where(b => b.Estado == "Exitosa")
                .AverageAsync(b => (decimal?)b.SimilitudPromedio) ?? 0;

            var promedioSimilitudResultados = await _context.ResultadoBusquedas
                .AsNoTracking()
                .Where(r => r.NivelSimilitud > 0)
                .AverageAsync(r => (decimal?)r.NivelSimilitud) ?? 0;

            var totalBusquedas = await _context.BusquedaSemanticas
                .AsNoTracking()
                .CountAsync();

            var totalResultados = await _context.ResultadoBusquedas
                .AsNoTracking()
                .CountAsync();

            return Ok(new
            {
                tiempoConsulta = Math.Round(tiempoConsulta, 2),
                relevancia = Math.Round(relevancia, 2),
                promedioSimilitudResultados = Math.Round(promedioSimilitudResultados, 2),
                totalBusquedas,
                totalResultados
            });
        }

        // GET: api/MetricaSistemas/agente
        [HttpGet("agente")]
        public async Task<IActionResult> GetMetricasAgente()
        {
            var totalConsultas = await _context.ConsultaAgentes
                .AsNoTracking()
                .CountAsync();

            var consultasPorTipoUsuario = await _context.ConsultaAgentes
                .AsNoTracking()
                .Join(
                    _context.Usuarios.AsNoTracking(),
                    consulta => consulta.IdUsuario,
                    usuario => usuario.IdUsuario,
                    (consulta, usuario) => new
                    {
                        usuario.Rol
                    }
                )
                .GroupBy(x => x.Rol)
                .Select(g => new
                {
                    tipoUsuario = g.Key,
                    totalConsultas = g.Count()
                })
                .ToListAsync();

            var preguntasAgente = await _context.ConsultaAgentes
                .AsNoTracking()
                .OrderByDescending(c => c.FechaHora)
                .Select(c => c.Pregunta)
                .Take(100)
                .ToListAsync();

            var topTerminosConsultados = CalcularTopTerminos(preguntasAgente);

            return Ok(new
            {
                totalConsultas,
                consultasPorTipoUsuario,
                topTerminosConsultados
            });
        }

        // GET: api/MetricaSistemas/servicios
        [HttpGet("servicios")]
        public async Task<IActionResult> GetMetricasServicios()
        {
            var estadoServicios = await _context.ServicioEstados
                .AsNoTracking()
                .OrderBy(s => s.NombreServicio)
                .Select(s => new
                {
                    servicio = s.NombreServicio,
                    estado = s.Estado,
                    ultimoChequeo = s.UltimoChequeo,
                    mensaje = s.Mensaje
                })
                .ToListAsync();

            return Ok(new
            {
                estadoServicios
            });
        }

        // POST: api/MetricaSistemas
        [HttpPost]
        public async Task<ActionResult<MetricaSistema>> PostMetricaSistema(MetricaSistema metricaSistema)
        {
            if (metricaSistema.IdMetrica == Guid.Empty)
            {
                metricaSistema.IdMetrica = Guid.NewGuid();
            }

            metricaSistema.FechaCalculo = DateTime.SpecifyKind(
                metricaSistema.FechaCalculo,
                DateTimeKind.Utc
            );

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

        // ============================================================
        // MÉTODOS AUXILIARES
        // ============================================================

        private async Task<List<object>> ObtenerRegistrosPorFecha()
        {
            var fechasRegistros = await _context.RegistrosSalud
                .AsNoTracking()
                .Select(r => r.FechaRegistro)
                .ToListAsync();

            var registrosPorFecha = fechasRegistros
                .GroupBy(f => f.Date)
                .Select(g => new
                {
                    fecha = g.Key,
                    totalRegistros = g.Count()
                })
                .OrderBy(x => x.fecha)
                .Cast<object>()
                .ToList();

            return registrosPorFecha;
        }

        private List<object> CalcularTopTerminos(List<string> preguntasAgente)
        {
            var palabrasIgnoradas = new HashSet<string>
            {
                "que", "qué", "cual", "cuál", "como", "cómo", "los", "las", "con", "para",
                "por", "del", "mis", "tengo", "una", "uno", "hay", "son", "fue", "mi",
                "el", "la", "de", "en", "y", "a"
            };

            var topTerminosConsultados = preguntasAgente
                .Where(p => !string.IsNullOrWhiteSpace(p))
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
                .Cast<object>()
                .ToList();

            return topTerminosConsultados;
        }
    }
}