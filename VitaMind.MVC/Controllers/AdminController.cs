using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace VitaMind.MVC.Controllers
{
    public class AdminController : Controller
    {
        private readonly string apiMetricas = "https://localhost:7042/api/MetricaSistemas";

        private bool EsAdministrador()
        {
            var rol = HttpContext.Session.GetString("Rol");
            return rol == "Administrador";
        }

        private async Task<JObject?> ObtenerJson(string endpoint)
        {
            try
            {
                /*
                    Este handler se usa para desarrollo local.
                    Ayuda cuando el MVC tiene problemas al consumir HTTPS local
                    por certificados de desarrollo.
                    No usar así en producción.
                */
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };

                using var client = new HttpClient(handler);
                client.Timeout = TimeSpan.FromSeconds(8);

                var response = await client.GetAsync(endpoint);

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.ErrorDetalle =
                        $"La API respondió con estado {(int)response.StatusCode} - {response.ReasonPhrase}";
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(json))
                {
                    ViewBag.ErrorDetalle = "La API respondió vacío.";
                    return null;
                }

                return JObject.Parse(json);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorDetalle = "Error al conectar con la API: " + ex.Message;
                return null;
            }
        }

        public async Task<IActionResult> Dashboard()
        {
            if (!EsAdministrador())
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.Nombre = HttpContext.Session.GetString("Nombre");

            var data = await ObtenerJson($"{apiMetricas}/resumen-admin");

            if (data == null)
            {
                ViewBag.Error = "No se pudieron cargar las métricas principales.";
                return View();
            }

            ViewBag.TotalRegistros = data["totalRegistros"]?.ToObject<int>() ?? 0;
            ViewBag.TotalUsuarios = data["totalUsuarios"]?.ToObject<int>() ?? 0;
            ViewBag.TotalConsultas = data["totalConsultas"]?.ToObject<int>() ?? 0;
            ViewBag.Errores = data["errores"]?.ToObject<int>() ?? 0;
            ViewBag.TasaExito = data["tasaExito"]?.ToObject<decimal>() ?? 0;
            ViewBag.Vectores = data["vectores"]?.ToObject<int>() ?? 0;

            return View();
        }

        public async Task<IActionResult> MetricasRegistros()
        {
            if (!EsAdministrador())
            {
                return RedirectToAction("Login", "Auth");
            }

            var data = await ObtenerJson($"{apiMetricas}/registros");

            if (data == null)
            {
                ViewBag.Error = "No se pudieron cargar las métricas de registros.";
                return View();
            }

            ViewBag.TotalRegistros = data["totalRegistros"]?.ToObject<int>() ?? 0;
            ViewBag.RegistrosPorUsuario = data["registrosPorUsuario"];
            ViewBag.RegistrosPorFecha = data["registrosPorFecha"];

            return View();
        }

        public async Task<IActionResult> MetricasRendimiento()
        {
            if (!EsAdministrador())
            {
                return RedirectToAction("Login", "Auth");
            }

            var data = await ObtenerJson($"{apiMetricas}/rendimiento");

            if (data == null)
            {
                ViewBag.Error = "No se pudieron cargar las métricas de rendimiento.";
                return View();
            }

            ViewBag.LatenciaPromedio = data["latenciaPromedio"]?.ToObject<decimal>() ?? 0;
            ViewBag.TiempoPromedioEmbedding = data["tiempoPromedioEmbedding"]?.ToObject<decimal>() ?? 0;
            ViewBag.Vectores = data["vectores"]?.ToObject<int>() ?? 0;
            ViewBag.DimensionEmbedding = data["dimensionEmbedding"]?.ToObject<int>() ?? 0;

            return View();
        }

        public async Task<IActionResult> MetricasCalidad()
        {
            if (!EsAdministrador())
            {
                return RedirectToAction("Login", "Auth");
            }

            var data = await ObtenerJson($"{apiMetricas}/calidad");

            if (data == null)
            {
                ViewBag.Error = "No se pudieron cargar las métricas de calidad.";
                return View();
            }

            ViewBag.Errores = data["errores"]?.ToObject<int>() ?? 0;
            ViewBag.TasaExito = data["tasaExito"]?.ToObject<decimal>() ?? 0;
            ViewBag.RegistrosSimilares = data["registrosSimilares"]?.ToObject<int>() ?? 0;
            ViewBag.IntentosFallidosPorUsuario = data["intentosFallidosPorUsuario"];

            return View();
        }

        public async Task<IActionResult> MetricasSemantica()
        {
            if (!EsAdministrador())
            {
                return RedirectToAction("Login", "Auth");
            }

            var data = await ObtenerJson($"{apiMetricas}/semantica");

            if (data == null)
            {
                ViewBag.Error = "No se pudieron cargar las métricas semánticas.";
                return View();
            }

            ViewBag.TiempoConsulta = data["tiempoConsulta"]?.ToObject<decimal>() ?? 0;
            ViewBag.Relevancia = data["relevancia"]?.ToObject<decimal>() ?? 0;
            ViewBag.PromedioSimilitudResultados = data["promedioSimilitudResultados"]?.ToObject<decimal>() ?? 0;
            ViewBag.TotalBusquedas = data["totalBusquedas"]?.ToObject<int>() ?? 0;
            ViewBag.TotalResultados = data["totalResultados"]?.ToObject<int>() ?? 0;

            return View();
        }

        public async Task<IActionResult> MetricasAgente()
        {
            if (!EsAdministrador())
            {
                return RedirectToAction("Login", "Auth");
            }

            var data = await ObtenerJson($"{apiMetricas}/agente");

            if (data == null)
            {
                ViewBag.Error = "No se pudieron cargar las métricas del agente.";
                return View();
            }

            ViewBag.TotalConsultas = data["totalConsultas"]?.ToObject<int>() ?? 0;
            ViewBag.ConsultasPorTipoUsuario = data["consultasPorTipoUsuario"];
            ViewBag.TopTerminosConsultados = data["topTerminosConsultados"];

            return View();
        }

        public async Task<IActionResult> MetricasServicios()
        {
            if (!EsAdministrador())
            {
                return RedirectToAction("Login", "Auth");
            }

            var data = await ObtenerJson($"{apiMetricas}/servicios");

            if (data == null)
            {
                ViewBag.Error = "No se pudieron cargar las métricas de servicios.";
                return View();
            }

            ViewBag.EstadoServicios = data["estadoServicios"];

            return View();
        }
    }
}