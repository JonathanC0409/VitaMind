using ConsumeCRUD;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using VitaMind.Models;
using VitaMind.MVC.Models;

namespace VitaMind.MVC.Controllers
{
    public class ChatbotController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ChatbotController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var rol = HttpContext.Session.GetString("Rol");

            if (string.IsNullOrEmpty(rol))
            {
                return RedirectToAction("Login", "Auth");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Preguntar(string pregunta)
        {
            Guid? idUsuario = null;

            var rol = HttpContext.Session.GetString("Rol");

            if (string.IsNullOrEmpty(rol))
            {
                return RedirectToAction("Login", "Auth");
            }

            var idUsuarioSesion = HttpContext.Session.GetString("IdUsuario");

            if (string.IsNullOrEmpty(idUsuarioSesion))
            {
                RegistrarError(
                    null,
                    "IdUsuario",
                    "Sesión inválida",
                    "No se encontró el IdUsuario en la sesión al usar el agente.",
                    "Pregunta: " + pregunta
                );

                return RedirectToAction("Login", "Auth");
            }

            idUsuario = Guid.Parse(idUsuarioSesion);

            if (string.IsNullOrWhiteSpace(pregunta))
            {
                RegistrarError(
                    idUsuario,
                    "Pregunta",
                    "Campo obligatorio",
                    "La pregunta del agente no puede estar vacía.",
                    "Pregunta vacía"
                );

                ViewBag.Respuesta = "Debe ingresar una pregunta.";
                return View("Index");
            }

            try
            {
                var client = _httpClientFactory.CreateClient();

                var url = "http://localhost:5678/webhook-test/vitamind-chatbot";

                var datos = new
                {
                    IdUsuario = idUsuarioSesion,
                    Pregunta = pregunta
                };

                var response = await client.PostAsJsonAsync(url, datos);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();

                    RegistrarError(
                        idUsuario,
                        "Chatbot",
                        "Error del agente",
                        "n8n devolvió un error al procesar la pregunta.",
                        error
                    );

                    ViewBag.Pregunta = pregunta;
                    ViewBag.Respuesta = "Error al consultar el agente: " + error;

                    return View("Index");
                }

                var respuestaAgente = await response.Content.ReadFromJsonAsync<RespuestaAgenteViewModel>();

                ViewBag.Pregunta = pregunta;
                ViewBag.Respuesta = respuestaAgente?.Respuesta ?? "No se recibió respuesta del agente.";
                ViewBag.TiempoConsultaMs = respuestaAgente?.TiempoConsultaMs ?? 0;
                ViewBag.RegistrosRecuperados = respuestaAgente?.RegistrosRecuperados ?? 0;
                ViewBag.SimilitudPromedio = respuestaAgente?.SimilitudPromedio ?? 0;

                return View("Index");
            }
            catch (Exception ex)
            {
                RegistrarError(
                    idUsuario,
                    "Chatbot",
                    "Excepción",
                    ex.Message,
                    "Pregunta: " + pregunta
                );

                ViewBag.Pregunta = pregunta;
                ViewBag.Respuesta = "Error al conectar con n8n: " + ex.Message;

                return View("Index");
            }
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
                // Evita que falle la aplicación si no se puede registrar el error.
            }
        }
    }
}