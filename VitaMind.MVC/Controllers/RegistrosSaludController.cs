using ConsumeCRUD;
using Microsoft.AspNetCore.Mvc;
using VitaMind.Models;

namespace VitaMind.MVC.Controllers
{
    public class RegistrosSaludController : Controller
    {

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
        public IActionResult Create(RegistroSalud registro)
        {
            try
            {

                registro.IdRegistro = Guid.NewGuid();

                // Temporal hasta manejar el usuario real desde login/API
                var idUsuarioSesion = HttpContext.Session.GetString("IdUsuario");

                if (string.IsNullOrEmpty(idUsuarioSesion))
                {
                    return RedirectToAction("Login", "Auth");
                }

                registro.IdUsuario = Guid.Parse(idUsuarioSesion);

                registro.FechaRegistro = DateTime.UtcNow.Date;
                registro.HoraRegistro = DateTime.UtcNow.TimeOfDay;
                registro.FechaCreacion = DateTime.UtcNow;
                registro.EstadoInsercion = "Insertado";
                registro.FechaCreacion = DateTime.Now;

                CRUD<RegistroSalud>.Create(registro);

                return RedirectToAction("Index");
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