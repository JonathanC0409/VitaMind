using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VitaMind.Models;
using System.Diagnostics;
using Pgvector;

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
            var registros = await _context.RegistrosSalud
                .AsNoTracking()
                .Select(r => new RegistroSalud
                {
                    IdRegistro = r.IdRegistro,
                    IdUsuario = r.IdUsuario,
                    Categoria = r.Categoria,
                    Descripcion = r.Descripcion,
                    Intensidad = r.Intensidad,
                    Observacion = r.Observacion,
                    TipoEntrada = r.TipoEntrada,
                    FechaRegistro = r.FechaRegistro,
                    HoraRegistro = r.HoraRegistro,
                    EstadoInsercion = r.EstadoInsercion,
                    LatenciaMs = r.LatenciaMs,
                    TiempoEmbeddingMs = r.TiempoEmbeddingMs,

                    // No enviar el vector al MVC
                    Embedding = null,

                    DimensionEmbedding = r.DimensionEmbedding,
                    EsSimilar = r.EsSimilar,
                    SimilitudDetectada = r.SimilitudDetectada,
                    IdRegistroSimilar = r.IdRegistroSimilar,
                    FechaCreacion = r.FechaCreacion,

                    // Evitar relaciones/ciclos
                    Usuario = null,
                    RegistroSimilar = null,
                    ResultadosBusqueda = null
                })
                .ToListAsync();

            return Ok(registros);
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
        [HttpPost]
        public async Task<ActionResult<RegistroSalud>> PostRegistroSalud(RegistroSalud registroSalud)
        {
            var relojTotal = Stopwatch.StartNew();

            try
            {
                // Generar ID si viene vacío
                if (registroSalud.IdRegistro == Guid.Empty)
                {
                    registroSalud.IdRegistro = Guid.NewGuid();
                }

                // Fechas en UTC para PostgreSQL
                registroSalud.FechaRegistro = DateTime.UtcNow.Date;
                registroSalud.HoraRegistro = DateTime.UtcNow.TimeOfDay;
                registroSalud.FechaCreacion = DateTime.UtcNow;

                // Valores por defecto
                registroSalud.TipoEntrada = string.IsNullOrWhiteSpace(registroSalud.TipoEntrada)
                    ? "Formulario"
                    : registroSalud.TipoEntrada;

                registroSalud.EstadoInsercion = "Procesando";
                registroSalud.DimensionEmbedding = 1536;

                // ==============================
                // 1. GENERAR EMBEDDING
                // ==============================
                var relojEmbedding = Stopwatch.StartNew();

                registroSalud.Embedding = GenerarEmbeddingTemporal(registroSalud.Descripcion);

                relojEmbedding.Stop();

                registroSalud.TiempoEmbeddingMs = (int)relojEmbedding.ElapsedMilliseconds;

                // ==============================
                // 2. BUSCAR REGISTRO SIMILAR
                // ==============================
                var registrosAnteriores = await _context.RegistrosSalud
                    .Where(x => x.IdUsuario == registroSalud.IdUsuario && x.Embedding != null)
                    .AsNoTracking()
                    .ToListAsync();

                decimal mayorSimilitud = 0;
                Guid? idMasSimilar = null;

                foreach (var anterior in registrosAnteriores)
                {
                    if (anterior.Embedding == null || registroSalud.Embedding == null)
                        continue;

                    decimal similitud = CalcularSimilitudCoseno(
                        registroSalud.Embedding.Memory.ToArray(),
                        anterior.Embedding.Memory.ToArray()
                    );

                    if (similitud > mayorSimilitud)
                    {
                        mayorSimilitud = similitud;
                        idMasSimilar = anterior.IdRegistro;
                    }
                }

                registroSalud.SimilitudDetectada = mayorSimilitud;
                registroSalud.IdRegistroSimilar = idMasSimilar;

                // Si es mayor o igual a 80%, se considera similar
                registroSalud.EsSimilar = mayorSimilitud >= 0.80m;

                // ==============================
                // 3. FINALIZAR INSERCIÓN
                // ==============================
                registroSalud.EstadoInsercion = "Insertado";

                relojTotal.Stop();
                registroSalud.LatenciaMs = (int)relojTotal.ElapsedMilliseconds;

                // ==============================
                // 4. GUARDAR REGISTRO + LOG
                // ==============================
                _context.RegistrosSalud.Add(registroSalud);

                var log = new LogOperacion
                {
                    IdLog = Guid.NewGuid(),
                    IdUsuario = registroSalud.IdUsuario,
                    TipoOperacion = "Registro de salud",
                    Descripcion = $"El usuario registró información en la categoría {registroSalud.Categoria}.",
                    Estado = "Exitoso",
                    TiempoRespuestaMs = registroSalud.LatenciaMs,
                    Origen = "Formulario cliente / API RegistrosSalud",
                    FechaHora = DateTime.UtcNow
                };

                _context.LogOperaciones.Add(log);

                await _context.SaveChangesAsync();

                return CreatedAtAction(
                    "GetRegistroSalud",
                    new { id = registroSalud.IdRegistro },
                    registroSalud
                );
            }
            catch
            {
                relojTotal.Stop();
                registroSalud.LatenciaMs = (int)relojTotal.ElapsedMilliseconds;
                registroSalud.EstadoInsercion = "Error";

                throw;
            }
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

            // Normalizar para que no crezca demasiado
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
                return 0;

            double similitud = productoPunto / (Math.Sqrt(magnitudA) * Math.Sqrt(magnitudB));

            return Math.Round((decimal)similitud, 4);
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