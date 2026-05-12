using System.ComponentModel.DataAnnotations;

namespace VitaMind.Models;

public class LogOperacion
{
    [Key]
    public Guid IdLog { get; set; }

    public Guid? IdUsuario { get; set; }

    public string TipoOperacion { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public string Estado { get; set; } = string.Empty;

    public int TiempoRespuestaMs { get; set; }

    public string? Origen { get; set; }

    public DateTime FechaHora { get; set; } = DateTime.Now;

    // Navegación
    public Usuario? Usuario { get; set; }
}