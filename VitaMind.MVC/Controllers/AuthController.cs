using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using VitaMind.Models;

namespace VitaMind.MVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly string endpointLogin = "https://localhost:7042/api/Usuarios/login";

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string correo, string password)
        {
            if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Ingrese correo y contraseña.";
                return View();
            }

            var login = new Usuario
            {
                Correo = correo,
                PasswordHash = password,
                Nombre = "",
                Rol = "",
                Estado = "Activo",
                FechaRegistro = DateTime.UtcNow
            };

            using var client = new HttpClient();

            var content = new StringContent(
                JsonConvert.SerializeObject(login),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync(endpointLogin, content);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Credenciales incorrectas.";
                return View();
            }

            var json = await response.Content.ReadAsStringAsync();
            var usuario = JsonConvert.DeserializeObject<Usuario>(json);

            if (usuario == null)
            {
                ViewBag.Error = "No se pudo obtener la información del usuario.";
                return View();
            }

            HttpContext.Session.SetString("IdUsuario", usuario.IdUsuario.ToString());
            HttpContext.Session.SetString("Rol", usuario.Rol);
            HttpContext.Session.SetString("Correo", usuario.Correo);
            HttpContext.Session.SetString("Nombre", usuario.Nombre);

            if (usuario.Rol == "Administrador")
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            if (usuario.Rol == "Cliente")
            {
                return RedirectToAction("Index", "Cliente");
            }

            ViewBag.Error = "Rol no válido.";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Auth");
        }
    }
}