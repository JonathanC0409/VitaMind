using VitaMind.Models;
using VitaMind.MVC.Models;
using ConsumeCRUD;

var builder = WebApplication.CreateBuilder(args);

// ENDPOINTS API
CRUD<Usuario>.EndPoint = "https://localhost:7042/api/Usuarios";

CRUD<RegistroSalud>.EndPoint = "https://localhost:7042/api/RegistrosSalud";


CRUD<BusquedaSemantica>.EndPoint = "https://localhost:7042/api/BusquedaSemanticas";
CRUD<ConsultaAgente>.EndPoint = "https://localhost:7042/api/ConsultaAgentes";
CRUD<ErrorIngreso>.EndPoint = "https://localhost:7042/api/ErrorIngresos";
CRUD<LogOperacion>.EndPoint = "https://localhost:7042/api/LogOperaciones";
CRUD<MetricaSistema>.EndPoint = "https://localhost:7042/api/MetricaSistemas";
CRUD<ResultadoBusqueda>.EndPoint = "https://localhost:7042/api/ResultadoBusquedas";
CRUD<ServicioEstado>.EndPoint = "https://localhost:7042/api/ServicioEstados";

// Servicios MVC
builder.Services.AddControllersWithViews();

// Permite usar IHttpClientFactory en los controladores
builder.Services.AddHttpClient();

// Habilitar sesiones
builder.Services.AddSession();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();