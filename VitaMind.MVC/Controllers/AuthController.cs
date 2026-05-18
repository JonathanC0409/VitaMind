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

            try
            {
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

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var usuario = JsonConvert.DeserializeObject<Usuario>(json);

                    if (usuario != null)
                    {
                        GuardarSesion(usuario);

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
                }
            }
            catch
            {
                /*
                    Si la API no está levantada o hay error de conexión,
                    se continúa con usuarios temporales locales.
                */
            }

            var usuarioTemporal = ObtenerUsuarioTemporal(correo, password);

            if (usuarioTemporal == null)
            {
                ViewBag.Error = "Credenciales incorrectas o API no disponible.";
                return View();
            }

            GuardarSesion(usuarioTemporal);

            if (usuarioTemporal.Rol == "Administrador")
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            if (usuarioTemporal.Rol == "Cliente")
            {
                return RedirectToAction("Index", "Cliente");
            }

            ViewBag.Error = "Rol no válido.";
            return View();
        }

        private Usuario? ObtenerUsuarioTemporal(string correo, string password)
        {
            if (correo == "admin@vitamind.com" && password == "123456")
            {
                return new Usuario
                {
                    IdUsuario = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Nombre = "Administrador VitaMind",
                    Correo = "admin@vitamind.com",
                    PasswordHash = "123456",
                    Rol = "Administrador",
                    Estado = "Activo",
                    FechaRegistro = DateTime.UtcNow
                };
            }

            if (correo == "juan@vitamind.com" && password == "123456")
            {
                return new Usuario
                {
                    IdUsuario = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Nombre = "Juan Pérez",
                    Correo = "juan@vitamind.com",
                    PasswordHash = "123456",
                    Rol = "Cliente",
                    Estado = "Activo",
                    FechaRegistro = DateTime.UtcNow
                };
            }

            return null;
        }

        private void GuardarSesion(Usuario usuario)
        {
            HttpContext.Session.SetString("IdUsuario", usuario.IdUsuario.ToString());
            HttpContext.Session.SetString("Rol", usuario.Rol);
            HttpContext.Session.SetString("Correo", usuario.Correo);
            HttpContext.Session.SetString("Nombre", usuario.Nombre);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Auth");
        }
    }
}