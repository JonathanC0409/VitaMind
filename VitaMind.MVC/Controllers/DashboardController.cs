using ConsumeCRUD;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using VitaMind.Models;

namespace VitaMind.MVC.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DashboardController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var rol = HttpContext.Session.GetString("Rol");

            if (rol != "Administrador")
            {
                return RedirectToAction("Login", "Auth");
            }

            var registros = CRUD<RegistroSalud>.GetAll();
            var usuarios = CRUD<Usuario>.GetAll();
            var consultas = CRUD<ConsultaAgente>.GetAll();
            var errores = CRUD<ErrorIngreso>.GetAll();
            var busquedas = CRUD<BusquedaSemantica>.GetAll();
            var logs = CRUD<LogOperacion>.GetAll();

            var totalRegistros = registros.Count;
            var totalUsuarios = usuarios.Count;
            var totalConsultas = consultas.Count;
            var totalErrores = errores.Count;

            var latenciaPromedio = registros.Any()
                ? registros.Average(r => r.LatenciaMs)
                : 0;

            var tiempoConsultaPromedio = consultas.Any()
                ? consultas.Average(c => c.TiempoConsultaMs)
                : 0;

            var relevanciaPromedio = busquedas.Any()
                ? busquedas.Average(b => b.SimilitudPromedio)
                : 0;

            var registrosSimilares = registros.Count(r => r.EsSimilar);

            var vectores = registros.Count(r => r.DimensionEmbedding == 1536);

            var insercionesFallidas = errores.Count(e =>
                e.TipoError.Contains("inserción", StringComparison.OrdinalIgnoreCase) ||
                e.CampoAfectado == "n8n" ||
                e.CampoAfectado == "API");

            var intentosInsercion = totalRegistros + insercionesFallidas;

            var tasaExito = intentosInsercion > 0
                ? Math.Round((decimal)totalRegistros / intentosInsercion * 100, 2)
                : 100;

            ViewBag.TotalRegistros = totalRegistros;
            ViewBag.TotalUsuarios = totalUsuarios;
            ViewBag.TotalConsultas = totalConsultas;
            ViewBag.Errores = totalErrores;
            ViewBag.LatenciaPromedio = Math.Round(latenciaPromedio, 2);
            ViewBag.TasaExito = tasaExito;
            ViewBag.TiempoConsulta = Math.Round(tiempoConsultaPromedio, 2);
            ViewBag.Relevancia = Math.Round(relevanciaPromedio, 4);
            ViewBag.RegistrosSimilares = registrosSimilares;
            ViewBag.Vectores = vectores;
            ViewBag.TotalLogs = logs.Count;

            GuardarMetrica("Total de registros ingresados", totalRegistros, "registros", "Cantidad total de registros almacenados.");
            GuardarMetrica("Total de usuarios", totalUsuarios, "usuarios", "Cantidad total de usuarios registrados.");
            GuardarMetrica("Total de consultas al agente", totalConsultas, "consultas", "Cantidad de preguntas realizadas al agente.");
            GuardarMetrica("Total de errores", totalErrores, "errores", "Cantidad total de errores registrados.");
            GuardarMetrica("Latencia promedio de inserción", (decimal)latenciaPromedio, "ms", "Tiempo promedio de inserción de registros.");
            GuardarMetrica("Tasa de éxito de inserción", tasaExito, "%", "Porcentaje de inserciones exitosas.");
            GuardarMetrica("Tiempo promedio de consulta semántica", (decimal)tiempoConsultaPromedio, "ms", "Tiempo promedio de respuesta del agente.");
            GuardarMetrica("Relevancia promedio de recuperación", relevanciaPromedio, "similitud", "Promedio de similitud en búsquedas semánticas.");
            GuardarMetrica("Registros similares detectados", registrosSimilares, "registros", "Cantidad de registros marcados como similares.");
            GuardarMetrica("Uso de almacenamiento vectorial", vectores, "vectores", "Cantidad de embeddings almacenados con dimensión 1536.");

            await VerificarServicios();

            RegistrarLog(
                Guid.TryParse(HttpContext.Session.GetString("IdUsuario"), out var idAdmin) ? idAdmin : null,
                "Acceso dashboard",
                "El administrador consultó el dashboard de métricas.",
                "Exitosa",
                0,
                "MVC/Dashboard"
            );

            return View();
        }

        private void GuardarMetrica(string nombre, decimal valor, string unidad, string descripcion)
        {
            try
            {
                var metrica = new MetricaSistema
                {
                    IdMetrica = Guid.NewGuid(),
                    NombreMetrica = nombre,
                    ValorNumerico = valor,
                    Unidad = unidad,
                    Descripcion = descripcion,
                    FechaCalculo = DateTime.UtcNow
                };

                CRUD<MetricaSistema>.Create(metrica);
            }
            catch
            {
                // Evita romper el dashboard si no se puede guardar la métrica.
            }
        }

        private async Task VerificarServicios()
        {
            await VerificarServicioHttp("API .NET", "https://localhost:7042/api/Usuarios");
            await VerificarServicioHttp("n8n", "http://localhost:5678");
        }

        private async Task VerificarServicioHttp(string nombreServicio, string url)
        {
            var reloj = Stopwatch.StartNew();

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(3);

                var response = await client.GetAsync(url);

                reloj.Stop();

                GuardarEstadoServicio(
                    nombreServicio,
                    response.IsSuccessStatusCode ? "Activo" : "Error",
                    response.IsSuccessStatusCode
                        ? $"Servicio disponible. Tiempo: {reloj.ElapsedMilliseconds} ms"
                        : $"Servicio respondió con estado {(int)response.StatusCode}"
                );
            }
            catch (Exception ex)
            {
                reloj.Stop();

                GuardarEstadoServicio(
                    nombreServicio,
                    "Error",
                    $"No se pudo conectar. Detalle: {ex.Message}"
                );
            }
        }

        private void GuardarEstadoServicio(string nombreServicio, string estado, string mensaje)
        {
            try
            {
                var servicio = new ServicioEstado
                {
                    IdServicio = Guid.NewGuid(),
                    NombreServicio = nombreServicio,
                    Estado = estado,
                    UltimoChequeo = DateTime.UtcNow,
                    Mensaje = mensaje
                };

                CRUD<ServicioEstado>.Create(servicio);
            }
            catch
            {
                // Evita romper el dashboard si falla el monitoreo.
            }
        }

        private void RegistrarLog(Guid? idUsuario, string tipoOperacion, string descripcion, string estado, int tiempoMs, string origen)
        {
            try
            {
                var log = new LogOperacion
                {
                    IdLog = Guid.NewGuid(),
                    IdUsuario = idUsuario,
                    TipoOperacion = tipoOperacion,
                    Descripcion = descripcion,
                    Estado = estado,
                    TiempoRespuestaMs = tiempoMs,
                    Origen = origen,
                    FechaHora = DateTime.UtcNow
                };

                CRUD<LogOperacion>.Create(log);
            }
            catch
            {
                // Evita romper el dashboard si falla el log.
            }
        }
    }
}