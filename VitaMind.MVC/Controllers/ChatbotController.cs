using ConsumeCRUD;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
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
            var reloj = Stopwatch.StartNew();

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
                reloj.Stop();

                RegistrarError(
                    idUsuario,
                    "Pregunta",
                    "Campo obligatorio",
                    "La pregunta del agente no puede estar vacía.",
                    "Pregunta vacía"
                );

                GuardarConsultaAgente(
                    idUsuario.Value,
                    pregunta ?? "",
                    "Debe ingresar una pregunta.",
                    "Validación",
                    "Fallida",
                    (int)reloj.ElapsedMilliseconds,
                    0,
                    0
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
                    reloj.Stop();

                    var error = await response.Content.ReadAsStringAsync();

                    RegistrarError(
                        idUsuario,
                        "Chatbot",
                        "Error del agente",
                        "n8n devolvió un error al procesar la pregunta.",
                        error
                    );

                    GuardarConsultaAgente(
                        idUsuario.Value,
                        pregunta,
                        "Error al consultar el agente: " + error,
                        "Consulta al agente",
                        "Fallida",
                        (int)reloj.ElapsedMilliseconds,
                        0,
                        0
                    );

                    ViewBag.Pregunta = pregunta;
                    ViewBag.Respuesta = "Error al consultar el agente: " + error;

                    return View("Index");
                }

                var respuestaAgente = await response.Content.ReadFromJsonAsync<RespuestaAgenteViewModel>();

                reloj.Stop();

                var respuestaTexto = respuestaAgente?.Respuesta ?? "No se recibió respuesta del agente.";
                var tiempoConsulta = respuestaAgente?.TiempoConsultaMs ?? (int)reloj.ElapsedMilliseconds;
                var registrosRecuperados = respuestaAgente?.RegistrosRecuperados ?? 0;
                var similitudPromedio = respuestaAgente?.SimilitudPromedio ?? 0;

                GuardarConsultaAgente(
                    idUsuario.Value,
                    pregunta,
                    respuestaTexto,
                    "Consulta al agente",
                    "Exitosa",
                    tiempoConsulta,
                    registrosRecuperados,
                    similitudPromedio
                );

                ViewBag.Pregunta = pregunta;
                ViewBag.Respuesta = respuestaTexto;
                ViewBag.TiempoConsultaMs = tiempoConsulta;
                ViewBag.RegistrosRecuperados = registrosRecuperados;
                ViewBag.SimilitudPromedio = similitudPromedio;

                return View("Index");
            }
            catch (Exception ex)
            {
                reloj.Stop();

                RegistrarError(
                    idUsuario,
                    "Chatbot",
                    "Excepción",
                    ex.Message,
                    "Pregunta: " + pregunta
                );

                if (idUsuario.HasValue)
                {
                    GuardarConsultaAgente(
                        idUsuario.Value,
                        pregunta,
                        "Error al conectar con n8n: " + ex.Message,
                        "Consulta al agente",
                        "Fallida",
                        (int)reloj.ElapsedMilliseconds,
                        0,
                        0
                    );
                }

                ViewBag.Pregunta = pregunta;
                ViewBag.Respuesta = "Error al conectar con n8n: " + ex.Message;

                return View("Index");
            }
        }

        private void GuardarConsultaAgente(
            Guid idUsuario,
            string pregunta,
            string respuesta,
            string accionDetectada,
            string estado,
            int tiempoConsultaMs,
            int registrosRecuperados,
            decimal similitudPromedio)
        {
            try
            {
                var consulta = new ConsultaAgente
                {
                    IdConsulta = Guid.NewGuid(),
                    IdUsuario = idUsuario,
                    Pregunta = pregunta,
                    Respuesta = respuesta,
                    AccionDetectada = accionDetectada,
                    Estado = estado,
                    TiempoConsultaMs = tiempoConsultaMs,
                    RegistrosRecuperados = registrosRecuperados,
                    SimilitudPromedio = similitudPromedio,
                    FechaHora = DateTime.UtcNow
                };

                CRUD<ConsultaAgente>.Create(consulta);
            }
            catch
            {
                // Evita que un error al guardar la consulta rompa la pantalla del chatbot.
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