using Microsoft.AspNetCore.Mvc;

namespace VitaMind.MVC.Controllers
{
    public class ChatbotController : Controller
    {
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
        public IActionResult Preguntar(string pregunta)
        {
            if (string.IsNullOrWhiteSpace(pregunta))
            {
                ViewBag.Respuesta = "Debe ingresar una pregunta.";
                return View("Index");
            }

            // Temporal hasta conectar con n8n
            ViewBag.Pregunta = pregunta;
            ViewBag.Respuesta = "Respuesta temporal del agente conversacional de VitaMind.";

            return View("Index");
        }
    }
}