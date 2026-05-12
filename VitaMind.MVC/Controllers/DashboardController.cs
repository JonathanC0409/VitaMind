using Microsoft.AspNetCore.Mvc;

namespace VitaMind.MVC.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            var rol = HttpContext.Session.GetString("Rol");

            if (rol != "Administrador")
            {
                return RedirectToAction("Login", "Auth");
            }

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

            return View();
        }
    }
}