using ConsumeCRUD;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using VitaMind.Models;

namespace VitaMind.MVC.Controllers
{
    public class RegistrosSaludController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public RegistrosSaludController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Index()
        {
            var rol = HttpContext.Session.GetString("Rol");

            if (string.IsNullOrEmpty(rol))
            {
                return RedirectToAction("Login", "Auth");
            }

            var registros = CRUD<RegistroSalud>.GetAll();
            return View(registros);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var rol = HttpContext.Session.GetString("Rol");

            if (rol != "Cliente")
            {
                return RedirectToAction("Login", "Auth");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(RegistroSalud registro)
        {
            try
            {
                var rol = HttpContext.Session.GetString("Rol");

                if (rol != "Cliente")
                {
                    return RedirectToAction("Login", "Auth");
                }

                var idUsuarioSesion = HttpContext.Session.GetString("IdUsuario");

                if (string.IsNullOrEmpty(idUsuarioSesion))
                {
                    return RedirectToAction("Login", "Auth");
                }

                registro.IdUsuario = Guid.Parse(idUsuarioSesion);

                var client = _httpClientFactory.CreateClient();

                // Mientras pruebas en n8n usa webhook-test
                var url = "http://localhost:5678/webhook-test/insertar-registro-salud";

                var datos = new
                {
                    IdUsuario = registro.IdUsuario,
                    Categoria = registro.Categoria,
                    Descripcion = registro.Descripcion,
                    Intensidad = registro.Intensidad,
                    Observacion = registro.Observacion,
                    TipoEntrada = "Formulario"
                };

                var response = await client.PostAsJsonAsync(url, datos);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Mensaje"] = "Registro guardado correctamente con n8n y embedding.";
                    return RedirectToAction("Index");
                }

                var errorN8n = await response.Content.ReadAsStringAsync();
                ViewBag.Error = "Error al guardar en n8n: " + errorN8n;

                return View(registro);
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(registro);
            }
        }

        public IActionResult Details(Guid id)
        {
            var registro = CRUD<RegistroSalud>.GetById(id);

            if (registro == null)
            {
                return NotFound();
            }

            return View(registro);
        }

        [HttpGet]
        public IActionResult Delete(Guid id)
        {
            var registro = CRUD<RegistroSalud>.GetById(id);

            if (registro == null)
            {
                return NotFound();
            }

            return View(registro);
        }

        [HttpPost]
        public IActionResult DeleteConfirmed(Guid id)
        {
            CRUD<RegistroSalud>.Delete(id);

            return RedirectToAction("Index");
        }
    }
}