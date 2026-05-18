using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pgvector;
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
            return await _context.BusquedaSemanticas
                .AsNoTracking()
                .ToListAsync();
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

        // ============================================================
        // POST: api/BusquedaSemanticas/buscar
        // Búsqueda semántica real + almacenamiento de métricas
        // ============================================================
        [HttpPost("buscar")]
        public async Task<IActionResult> Buscar([FromBody] BuscarSemanticaRequest request)
        {
            var reloj = Stopwatch.StartNew();

            try
            {
                if (request.IdUsuario == Guid.Empty)
                {
                    await RegistrarError(
                        null,
                        "IdUsuario",
                        "Campo obligatorio",
                        "No se recibió el IdUsuario para realizar la búsqueda semántica.",
                        $"Consulta: {request.Consulta}"
                    );

                    return BadRequest("El IdUsuario es obligatorio.");
                }

                if (string.IsNullOrWhiteSpace(request.Consulta))
                {
                    await RegistrarError(
                        request.IdUsuario,
                        "Consulta",
                        "Campo obligatorio",
                        "La consulta semántica no puede estar vacía.",
                        "Consulta vacía"
                    );

                    return BadRequest("La consulta semántica no puede estar vacía.");
                }

                var top = request.Top <= 0 ? 5 : request.Top;

                // 1. Generar embedding temporal de la consulta
                var embeddingConsulta = GenerarEmbeddingTemporal(request.Consulta);

                // 2. Obtener registros del usuario con embedding
                var registros = await _context.RegistrosSalud
                    .Where(r => r.IdUsuario == request.IdUsuario && r.Embedding != null)
                    .AsNoTracking()
                    .ToListAsync();

                var resultadosCalculados = new List<ResultadoCalculado>();

                foreach (var registro in registros)
                {
                    if (registro.Embedding == null)
                    {
                        continue;
                    }

                    var similitud = CalcularSimilitudCoseno(
                        embeddingConsulta.Memory.ToArray(),
                        registro.Embedding.Memory.ToArray()
                    );

                    resultadosCalculados.Add(new ResultadoCalculado
                    {
                        IdRegistro = registro.IdRegistro,
                        Categoria = registro.Categoria,
                        Descripcion = registro.Descripcion,
                        FechaRegistro = registro.FechaRegistro,
                        NivelSimilitud = similitud
                    });
                }

                var resultadosOrdenados = resultadosCalculados
                    .OrderByDescending(r => r.NivelSimilitud)
                    .Take(top)
                    .ToList();

                reloj.Stop();

                var totalResultados = resultadosOrdenados.Count;
                var similitudMaxima = totalResultados > 0
                    ? resultadosOrdenados.Max(r => r.NivelSimilitud)
                    : 0;

                var similitudPromedio = totalResultados > 0
                    ? Math.Round(resultadosOrdenados.Average(r => r.NivelSimilitud), 4)
                    : 0;

                // 3. Guardar búsqueda semántica
                var busqueda = new BusquedaSemantica
                {
                    IdBusqueda = Guid.NewGuid(),
                    IdUsuario = request.IdUsuario,
                    Consulta = request.Consulta,
                    EmbeddingConsulta = embeddingConsulta,
                    TiempoBusquedaMs = (int)reloj.ElapsedMilliseconds,
                    TotalResultados = totalResultados,
                    SimilitudMaxima = similitudMaxima,
                    SimilitudPromedio = similitudPromedio,
                    Estado = totalResultados > 0 ? "Exitosa" : "Sin resultados",
                    FechaHora = DateTime.UtcNow
                };

                _context.BusquedaSemanticas.Add(busqueda);

                // 4. Guardar resultados encontrados
                var resultadosBusqueda = new List<ResultadoBusqueda>();

                for (int i = 0; i < resultadosOrdenados.Count; i++)
                {
                    resultadosBusqueda.Add(new ResultadoBusqueda
                    {
                        IdResultado = Guid.NewGuid(),
                        IdBusqueda = busqueda.IdBusqueda,
                        IdRegistro = resultadosOrdenados[i].IdRegistro,
                        NivelSimilitud = resultadosOrdenados[i].NivelSimilitud,
                        PosicionResultado = i + 1,
                        FechaHora = DateTime.UtcNow
                    });
                }

                if (resultadosBusqueda.Any())
                {
                    _context.ResultadoBusquedas.AddRange(resultadosBusqueda);
                }

                // 5. Guardar log de operación
                var log = new LogOperacion
                {
                    IdLog = Guid.NewGuid(),
                    IdUsuario = request.IdUsuario,
                    TipoOperacion = "Búsqueda semántica",
                    Descripcion = $"El usuario realizó la búsqueda semántica: {request.Consulta}",
                    Estado = totalResultados > 0 ? "Exitoso" : "Sin resultados",
                    TiempoRespuestaMs = (int)reloj.ElapsedMilliseconds,
                    Origen = "API BusquedaSemanticas",
                    FechaHora = DateTime.UtcNow
                };

                _context.LogOperaciones.Add(log);

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    busqueda.IdBusqueda,
                    busqueda.IdUsuario,
                    busqueda.Consulta,
                    busqueda.TiempoBusquedaMs,
                    busqueda.TotalResultados,
                    busqueda.SimilitudMaxima,
                    busqueda.SimilitudPromedio,
                    busqueda.Estado,
                    Resultados = resultadosOrdenados
                });
            }
            catch (Exception ex)
            {
                reloj.Stop();

                await RegistrarError(
                    request?.IdUsuario,
                    "BusquedaSemantica",
                    "Excepción",
                    ex.Message,
                    request != null ? $"Consulta: {request.Consulta}" : "Request nulo"
                );

                return StatusCode(500, "Error al realizar la búsqueda semántica: " + ex.Message);
            }
        }

        // PUT: api/BusquedaSemanticas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBusquedaSemantica(Guid id, BusquedaSemantica busquedaSemantica)
        {
            if (id != busquedaSemantica.IdBusqueda)
            {
                return BadRequest();
            }

            busquedaSemantica.FechaHora = DateTime.SpecifyKind(busquedaSemantica.FechaHora, DateTimeKind.Utc);

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

                throw;
            }

            return NoContent();
        }

        // POST: api/BusquedaSemanticas
        [HttpPost]
        public async Task<ActionResult<BusquedaSemantica>> PostBusquedaSemantica(BusquedaSemantica busquedaSemantica)
        {
            if (busquedaSemantica.IdBusqueda == Guid.Empty)
            {
                busquedaSemantica.IdBusqueda = Guid.NewGuid();
            }

            busquedaSemantica.FechaHora = DateTime.SpecifyKind(
                busquedaSemantica.FechaHora,
                DateTimeKind.Utc
            );

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

        private Vector GenerarEmbeddingTemporal(string texto)
        {
            texto = texto.ToLower().Trim();

            var valores = new float[1536];

            for (int i = 0; i < texto.Length; i++)
            {
                int posicion = Math.Abs((texto[i] + i) % 1536);
                valores[posicion] += 1f;
            }

            float suma = valores.Sum();

            if (suma > 0)
            {
                for (int i = 0; i < valores.Length; i++)
                {
                    valores[i] = valores[i] / suma;
                }
            }

            return new Vector(valores);
        }

        private decimal CalcularSimilitudCoseno(float[] vectorA, float[] vectorB)
        {
            double productoPunto = 0;
            double magnitudA = 0;
            double magnitudB = 0;

            for (int i = 0; i < vectorA.Length; i++)
            {
                productoPunto += vectorA[i] * vectorB[i];
                magnitudA += vectorA[i] * vectorA[i];
                magnitudB += vectorB[i] * vectorB[i];
            }

            if (magnitudA == 0 || magnitudB == 0)
            {
                return 0;
            }

            double similitud = productoPunto / (Math.Sqrt(magnitudA) * Math.Sqrt(magnitudB));

            return Math.Round((decimal)similitud, 4);
        }

        private async Task RegistrarError(Guid? idUsuario, string? campo, string tipoError, string mensaje, string? datosIntentados)
        {
            try
            {
                var error = new ErrorIngreso
                {
                    IdError = Guid.NewGuid(),
                    IdUsuario = idUsuario,
                    CampoAfectado = campo,
                    TipoError = tipoError,
                    MensajeError = mensaje,
                    DatosIntentados = datosIntentados,
                    FechaHora = DateTime.UtcNow
                };

                _context.ErrorIngresos.Add(error);
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Evita que el registro de error rompa el flujo principal.
            }
        }

        public class BuscarSemanticaRequest
        {
            public Guid IdUsuario { get; set; }
            public string Consulta { get; set; } = string.Empty;
            public int Top { get; set; } = 5;
        }

        private class ResultadoCalculado
        {
            public Guid IdRegistro { get; set; }
            public string Categoria { get; set; } = string.Empty;
            public string Descripcion { get; set; } = string.Empty;
            public DateTime FechaRegistro { get; set; }
            public decimal NivelSimilitud { get; set; }
        }
    }
}