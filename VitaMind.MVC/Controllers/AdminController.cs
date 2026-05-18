using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace VitaMind.MVC.Controllers
{
    public class AdminController : Controller
    {
        private readonly string endpointDashboard = "https://localhost:7042/api/MetricaSistemas/dashboard-admin";

        public async Task<IActionResult> Dashboard()
        {
            var rol = HttpContext.Session.GetString("Rol");

            if (rol != "Administrador")
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.Nombre = HttpContext.Session.GetString("Nombre");

            using var client = new HttpClient();

            try
            {
                await client.GetAsync("https://localhost:7042/api/ServicioEstados/verificar");
            }
            catch
            {
                // Si falla la verificación, no rompemos el dashboard.
            }

            var response = await client.GetAsync(endpointDashboard);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "No se pudieron cargar las métricas del dashboard.";

                CargarMetricasVacias();

                return View();
            }

            var json = await response.Content.ReadAsStringAsync();

            var metricas = JsonConvert.DeserializeObject<DashboardAdminDto>(json);

            ViewBag.TotalRegistros = metricas?.TotalRegistros ?? 0;
            ViewBag.TotalUsuarios = metricas?.TotalUsuarios ?? 0;
            ViewBag.TotalConsultas = metricas?.TotalConsultas ?? 0;
            ViewBag.Errores = metricas?.Errores ?? 0;
            ViewBag.LatenciaPromedio = metricas?.LatenciaPromedio ?? 0;
            ViewBag.TasaExito = metricas?.TasaExito ?? 0;
            ViewBag.TiempoConsulta = metricas?.TiempoConsulta ?? 0;
            ViewBag.Relevancia = metricas?.Relevancia ?? 0;
            ViewBag.RegistrosSimilares = metricas?.RegistrosSimilares ?? 0;
            ViewBag.Vectores = metricas?.Vectores ?? 0;

            // Métrica principal 10 más completa
            ViewBag.DimensionEmbedding = metricas?.DimensionEmbedding ?? 0;

            // Métricas adicionales
            ViewBag.TiempoPromedioEmbedding = metricas?.TiempoPromedioEmbedding ?? 0;
            ViewBag.RegistrosPorUsuario = metricas?.RegistrosPorUsuario ?? new List<RegistroPorUsuarioDto>();
            ViewBag.ConsultasPorTipoUsuario = metricas?.ConsultasPorTipoUsuario ?? new List<ConsultaPorTipoUsuarioDto>();
            ViewBag.PromedioSimilitudResultados = metricas?.PromedioSimilitudResultados ?? 0;
            ViewBag.IntentosFallidosPorUsuario = metricas?.IntentosFallidosPorUsuario ?? new List<IntentoFallidoUsuarioDto>();
            ViewBag.TopTerminosConsultados = metricas?.TopTerminosConsultados ?? new List<TopTerminoDto>();
            ViewBag.RegistrosPorFecha = metricas?.RegistrosPorFecha ?? new List<RegistroPorFechaDto>();
            ViewBag.EstadoServicios = metricas?.EstadoServicios ?? new List<EstadoServicioDto>();

            return View();
        }

        private void CargarMetricasVacias()
        {
            ViewBag.TotalRegistros = 0;
            ViewBag.TotalUsuarios = 0;
            ViewBag.TotalConsultas = 0;
            ViewBag.Errores = 0;
            ViewBag.LatenciaPromedio = 0;
            ViewBag.TasaExito = 0;
            ViewBag.TiempoConsulta = 0;
            ViewBag.Relevancia = 0;
            ViewBag.RegistrosSimilares = 0;
            ViewBag.Vectores = 0;
            ViewBag.DimensionEmbedding = 0;

            ViewBag.TiempoPromedioEmbedding = 0;
            ViewBag.RegistrosPorUsuario = new List<RegistroPorUsuarioDto>();
            ViewBag.ConsultasPorTipoUsuario = new List<ConsultaPorTipoUsuarioDto>();
            ViewBag.PromedioSimilitudResultados = 0;
            ViewBag.IntentosFallidosPorUsuario = new List<IntentoFallidoUsuarioDto>();
            ViewBag.TopTerminosConsultados = new List<TopTerminoDto>();
            ViewBag.RegistrosPorFecha = new List<RegistroPorFechaDto>();
            ViewBag.EstadoServicios = new List<EstadoServicioDto>();
        }

        private class DashboardAdminDto
        {
            public int TotalRegistros { get; set; }
            public int TotalUsuarios { get; set; }
            public int TotalConsultas { get; set; }
            public int Errores { get; set; }
            public double LatenciaPromedio { get; set; }
            public double TasaExito { get; set; }
            public double TiempoConsulta { get; set; }
            public decimal Relevancia { get; set; }
            public int RegistrosSimilares { get; set; }
            public int Vectores { get; set; }
            public int DimensionEmbedding { get; set; }

            public double TiempoPromedioEmbedding { get; set; }
            public List<RegistroPorUsuarioDto> RegistrosPorUsuario { get; set; } = new();
            public List<ConsultaPorTipoUsuarioDto> ConsultasPorTipoUsuario { get; set; } = new();
            public decimal PromedioSimilitudResultados { get; set; }
            public List<IntentoFallidoUsuarioDto> IntentosFallidosPorUsuario { get; set; } = new();
            public List<TopTerminoDto> TopTerminosConsultados { get; set; } = new();
            public List<RegistroPorFechaDto> RegistrosPorFecha { get; set; } = new();
            public List<EstadoServicioDto> EstadoServicios { get; set; } = new();
        }

        private class RegistroPorUsuarioDto
        {
            public string Usuario { get; set; } = string.Empty;
            public string Rol { get; set; } = string.Empty;
            public int TotalRegistros { get; set; }
        }

        private class ConsultaPorTipoUsuarioDto
        {
            public string TipoUsuario { get; set; } = string.Empty;
            public int TotalConsultas { get; set; }
        }

        private class IntentoFallidoUsuarioDto
        {
            public string Usuario { get; set; } = string.Empty;
            public int TotalErrores { get; set; }
        }

        private class TopTerminoDto
        {
            public string Termino { get; set; } = string.Empty;
            public int Cantidad { get; set; }
        }

        private class RegistroPorFechaDto
        {
            public DateTime Fecha { get; set; }
            public int TotalRegistros { get; set; }
        }

        private class EstadoServicioDto
        {
            public string Servicio { get; set; } = string.Empty;
            public string Estado { get; set; } = string.Empty;
            public DateTime UltimoChequeo { get; set; }
            public string? Mensaje { get; set; }
        }
    }
}