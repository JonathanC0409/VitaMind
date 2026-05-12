using System.ComponentModel.DataAnnotations;

namespace VitaMind.Models;

public class ConsultaAgente
{
    [Key]
    public Guid IdConsulta { get; set; }

    public Guid IdUsuario { get; set; }

    public string Pregunta { get; set; } = string.Empty;
    public string? Respuesta { get; set; }
    public string AccionDetectada { get; set; } = "Consulta";
    public string Estado { get; set; } = "Exitosa";
    public int TiempoConsultaMs { get; set; }
    public int RegistrosRecuperados { get; set; }
    public decimal SimilitudPromedio { get; set; }
    public DateTime FechaHora { get; set; } = DateTime.Now;

    // Navegación
    public Usuario? Usuario { get; set; }
}