using System.ComponentModel.DataAnnotations;

namespace VitaMind.Models;

public class MetricaSistema
{
    [Key]
    public Guid IdMetrica { get; set; }

    public string NombreMetrica { get; set; } = string.Empty;

    public decimal ValorNumerico { get; set; }

    public string? Unidad { get; set; }

    public string? Descripcion { get; set; }

    public DateTime FechaCalculo { get; set; } = DateTime.Now;
}