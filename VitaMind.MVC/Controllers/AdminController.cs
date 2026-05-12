using Microsoft.AspNetCore.Mvc;

namespace VitaMind.MVC.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Dashboard()
        {
            var rol = HttpContext.Session.GetString("Rol");

            if (rol != "Administrador")
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.Nombre = HttpContext.Session.GetString("Nombre");

            return View();
        }
    }
}
