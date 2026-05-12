using Microsoft.AspNetCore.Mvc;

namespace VitaMind.MVC.Controllers
{
    public class AuthController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string correo, string password)
        {
            if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Ingrese correo y contraseña.";
                return View();
            }

            if (correo == "admin@vitamind.com" && password == "123456")
            {
                HttpContext.Session.SetString("Rol", "Administrador");
                HttpContext.Session.SetString("Correo", correo);
                HttpContext.Session.SetString("Nombre", "Administrador VitaMind");

                return RedirectToAction("Dashboard", "Admin");
            }

            if (correo == "juan@vitamind.com" && password == "123456")
            {
                HttpContext.Session.SetString("IdUsuario", "22222222-2222-2222-2222-222222222222");
                HttpContext.Session.SetString("Rol", "Cliente");
                HttpContext.Session.SetString("Correo", correo);
                HttpContext.Session.SetString("Nombre", "Juan Pérez");

                return RedirectToAction("Index", "Cliente");
            }

            ViewBag.Error = "Credenciales incorrectas.";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Auth");
        }
    }
}