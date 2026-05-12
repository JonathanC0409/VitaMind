using System.ComponentModel.DataAnnotations;

namespace VitaMind.Models;

public class ErrorIngreso
{
    [Key]
    public Guid IdError { get; set; }

    public Guid? IdUsuario { get; set; }

    public string? CampoAfectado { get; set; }

    public string TipoError { get; set; } = string.Empty;

    public string MensajeError { get; set; } = string.Empty;

    public string? DatosIntentados { get; set; }

    public DateTime FechaHora { get; set; } = DateTime.Now;

    // Navegación
    public Usuario? Usuario { get; set; }
}