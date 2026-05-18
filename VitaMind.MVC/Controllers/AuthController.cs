using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using VitaMind.Models;
using ConsumeCRUD;
using System.Diagnostics;

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

                        RegistrarLog(
    usuario.IdUsuario,
    "Inicio de sesión",
    $"El usuario {usuario.Correo} inició sesión como {usuario.Rol}.",
    "Exitosa",
    0,
    "MVC/Auth"
);

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
    
            ViewBag.Error = "Rol no válido.";
            return View();
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
                // Evita que un error de log rompa el login.
            }
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