using Microsoft.AspNetCore.Mvc;

namespace VitaMind.MVC.Controllers
{
    public class ClienteController : Controller
    {
        public IActionResult Index()
        {
            var rol = HttpContext.Session.GetString("Rol");

            if (rol != "Cliente")
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.Nombre = HttpContext.Session.GetString("Nombre");

            return View();
        }
    }
}