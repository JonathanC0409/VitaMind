using System.ComponentModel.DataAnnotations;

namespace VitaMind.Models;

public class Usuario
{
    [Key]
    public Guid IdUsuario { get; set; }

    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string Estado { get; set; } = "Activo";
    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    // Relaciones
    public ICollection<RegistroSalud>? RegistrosSalud { get; set; }
    public ICollection<ConsultaAgente>? ConsultasAgente { get; set; }
    public ICollection<BusquedaSemantica>? BusquedasSemanticas { get; set; }
    public ICollection<LogOperacion>? LogsOperaciones { get; set; }
    public ICollection<ErrorIngreso>? ErroresIngreso { get; set; }
}