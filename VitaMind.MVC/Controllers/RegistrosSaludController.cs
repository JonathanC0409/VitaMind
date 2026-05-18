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
            var idUsuarioSesion = HttpContext.Session.GetString("IdUsuario");

            if (string.IsNullOrEmpty(rol))
            {
                return RedirectToAction("Login", "Auth");
            }

            var registros = CRUD<RegistroSalud>.GetAll();

            if (rol == "Cliente")
            {
                if (string.IsNullOrEmpty(idUsuarioSesion))
                {
                    return RedirectToAction("Login", "Auth");
                }

                registros = registros
                    .Where(r => r.IdUsuario.ToString() == idUsuarioSesion)
                    .ToList();
            }

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

        /*  CREATE CEPEDA
         * [HttpPost]
         public async Task<IActionResult> Create(RegistroSalud registro)
         {
             Guid? idUsuario = null;

             try
             {
                 var rol = HttpContext.Session.GetString("Rol");

                 if (rol != "Cliente")
                 {
                     RegistrarError(
                         null,
                         "Rol",
                         "Acceso no autorizado",
                         "El usuario intentó registrar información sin rol de Cliente.",
                         $"Rol recibido: {rol}"
                     );

                     return RedirectToAction("Login", "Auth");
                 }

                 var idUsuarioSesion = HttpContext.Session.GetString("IdUsuario");

                 if (string.IsNullOrEmpty(idUsuarioSesion))
                 {
                     RegistrarError(
                         null,
                         "IdUsuario",
                         "Sesión inválida",
                         "No se encontró el IdUsuario en la sesión.",
                         "IdUsuario vacío o nulo"
                     );

                     return RedirectToAction("Login", "Auth");
                 }

                 idUsuario = Guid.Parse(idUsuarioSesion);
                 registro.IdUsuario = idUsuario.Value;

                 if (string.IsNullOrWhiteSpace(registro.Categoria))
                 {
                     RegistrarError(
                         idUsuario,
                         "Categoria",
                         "Campo obligatorio",
                         "La categoría del registro no puede estar vacía.",
                         $"Descripcion: {registro.Descripcion}, Intensidad: {registro.Intensidad}, Observacion: {registro.Observacion}"
                     );

                     ViewBag.Error = "Debe seleccionar una categoría.";
                     return View(registro);
                 }

                 if (string.IsNullOrWhiteSpace(registro.Descripcion))
                 {
                     RegistrarError(
                         idUsuario,
                         "Descripcion",
                         "Campo obligatorio",
                         "La descripción del registro no puede estar vacía.",
                         $"Categoria: {registro.Categoria}, Intensidad: {registro.Intensidad}, Observacion: {registro.Observacion}"
                     );

                     ViewBag.Error = "Debe ingresar una descripción.";
                     return View(registro);
                 }

                 if (registro.Descripcion.Length < 5)
                 {
                     RegistrarError(
                         idUsuario,
                         "Descripcion",
                         "Dato inválido",
                         "La descripción ingresada es demasiado corta.",
                         $"Descripcion: {registro.Descripcion}"
                     );

                     ViewBag.Error = "La descripción debe tener al menos 5 caracteres.";
                     return View(registro);
                 }

                 var client = _httpClientFactory.CreateClient();

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

                 RegistrarError(
                     idUsuario,
                     "n8n",
                     "Error de inserción",
                     "n8n devolvió un error al intentar insertar el registro.",
                     errorN8n
                 );

                 ViewBag.Error = "Error al guardar en n8n: " + errorN8n;
                 return View(registro);
             }
             catch (Exception ex)
             {
                 RegistrarError(
                     idUsuario,
                     "Sistema",
                     "Excepción",
                     ex.Message,
                     $"Categoria: {registro.Categoria}, Descripcion: {registro.Descripcion}, Intensidad: {registro.Intensidad}, Observacion: {registro.Observacion}"
                 );

                 ViewBag.Error = "Ocurrió un error al registrar la información: " + ex.Message;
                 return View(registro);
             }
         }*/

        //CREATE ANDERSON
        [HttpPost]
        public async Task<IActionResult> Create(RegistroSalud registro)
        {
            Guid? idUsuario = null;

            try
            {
                var rol = HttpContext.Session.GetString("Rol");

                if (rol != "Cliente")
                {
                    RegistrarError(
                        null,
                        "Rol",
                        "Acceso no autorizado",
                        "El usuario intentó registrar información sin rol de Cliente.",
                        $"Rol recibido: {rol}"
                    );

                    return RedirectToAction("Login", "Auth");
                }

                var idUsuarioSesion = HttpContext.Session.GetString("IdUsuario");

                if (string.IsNullOrEmpty(idUsuarioSesion))
                {
                    RegistrarError(
                        null,
                        "IdUsuario",
                        "Sesión inválida",
                        "No se encontró el IdUsuario en la sesión.",
                        "IdUsuario vacío o nulo"
                    );

                    return RedirectToAction("Login", "Auth");
                }

                idUsuario = Guid.Parse(idUsuarioSesion);
                registro.IdUsuario = idUsuario.Value;

                if (string.IsNullOrWhiteSpace(registro.Categoria))
                {
                    RegistrarError(
                        idUsuario,
                        "Categoria",
                        "Campo obligatorio",
                        "La categoría del registro no puede estar vacía.",
                        $"Descripcion: {registro.Descripcion}, Intensidad: {registro.Intensidad}, Observacion: {registro.Observacion}"
                    );

                    ViewBag.Error = "Debe seleccionar una categoría.";
                    return View(registro);
                }

                if (string.IsNullOrWhiteSpace(registro.Descripcion))
                {
                    RegistrarError(
                        idUsuario,
                        "Descripcion",
                        "Campo obligatorio",
                        "La descripción del registro no puede estar vacía.",
                        $"Categoria: {registro.Categoria}, Intensidad: {registro.Intensidad}, Observacion: {registro.Observacion}"
                    );

                    ViewBag.Error = "Debe ingresar una descripción.";
                    return View(registro);
                }

                if (registro.Descripcion.Length < 5)
                {
                    RegistrarError(
                        idUsuario,
                        "Descripcion",
                        "Dato inválido",
                        "La descripción ingresada es demasiado corta.",
                        $"Descripcion: {registro.Descripcion}"
                    );

                    ViewBag.Error = "La descripción debe tener al menos 5 caracteres.";
                    return View(registro);
                }

                var client = _httpClientFactory.CreateClient();

                var datos = new
                {
                    IdUsuario = registro.IdUsuario,
                    Categoria = registro.Categoria,
                    Descripcion = registro.Descripcion,
                    Intensidad = registro.Intensidad,
                    Observacion = registro.Observacion,
                    TipoEntrada = "Formulario"
                };

                // ============================================================
                // 1. INTENTAR GUARDAR MEDIANTE N8N
                // ============================================================

                try
                {
                    var urlN8n = "http://localhost:5678/webhook-test/insertar-registro-salud";

                    var responseN8n = await client.PostAsJsonAsync(urlN8n, datos);

                    if (responseN8n.IsSuccessStatusCode)
                    {
                        TempData["Mensaje"] = "Registro guardado correctamente con n8n y embedding.";
                        return RedirectToAction("Index");
                    }

                    var errorN8n = await responseN8n.Content.ReadAsStringAsync();

                    RegistrarError(
                        idUsuario,
                        "n8n",
                        "Error de inserción",
                        "n8n devolvió un error al intentar insertar el registro.",
                        errorN8n
                    );
                }
                catch (Exception exN8n)
                {
                    RegistrarError(
                        idUsuario,
                        "n8n",
                        "Servicio no disponible",
                        "No se pudo conectar con n8n. Se intentará guardar directamente en la API.",
                        exN8n.Message
                    );
                }

                // ============================================================
                // 2. RESPALDO: GUARDAR DIRECTAMENTE EN LA API
                // ============================================================

                var registroApi = new RegistroSalud
                {
                    IdRegistro = Guid.NewGuid(),
                    IdUsuario = registro.IdUsuario,
                    Categoria = registro.Categoria,
                    Descripcion = registro.Descripcion,
                    Intensidad = registro.Intensidad,
                    Observacion = registro.Observacion,
                    TipoEntrada = "Formulario"
                };

                var urlApi = "https://localhost:7042/api/RegistrosSalud";

                var responseApi = await client.PostAsJsonAsync(urlApi, registroApi);

                if (responseApi.IsSuccessStatusCode)
                {
                    TempData["Mensaje"] = "Registro guardado directamente en la API porque n8n no estaba disponible.";
                    return RedirectToAction("Index");
                }

                var errorApi = await responseApi.Content.ReadAsStringAsync();

                RegistrarError(
                    idUsuario,
                    "API",
                    "Error de inserción",
                    "La API no pudo guardar el registro después de fallar n8n.",
                    errorApi
                );

                ViewBag.Error = "No se pudo guardar el registro ni por n8n ni por la API.";
                return View(registro);
            }
            catch (Exception ex)
            {
                RegistrarError(
                    idUsuario,
                    "Sistema",
                    "Excepción",
                    ex.Message,
                    $"Categoria: {registro.Categoria}, Descripcion: {registro.Descripcion}, Intensidad: {registro.Intensidad}, Observacion: {registro.Observacion}"
                );

                ViewBag.Error = "Ocurrió un error al registrar la información: " + ex.Message;
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

        private void RegistrarError(Guid? idUsuario, string? campo, string tipoError, string mensaje, string? datosIntentados)
        {
            try
            {
                var error = new ErrorIngreso
                {
                    IdError = Guid.NewGuid(),
                    IdUsuario = idUsuario,
                    CampoAfectado = campo,
                    TipoError = tipoError,
                    MensajeError = mensaje,
                    DatosIntentados = datosIntentados,
                    FechaHora = DateTime.UtcNow
                };

                CRUD<ErrorIngreso>.Create(error);
            }
            catch
            {
                // Evita que un error al registrar el error rompa la aplicación.
            }
        }
    }
}