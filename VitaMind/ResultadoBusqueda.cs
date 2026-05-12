using System.ComponentModel.DataAnnotations;

namespace VitaMind.Models;

public class ResultadoBusqueda
{
    [Key]
    public Guid IdResultado { get; set; }

    public Guid IdBusqueda { get; set; }

    public Guid IdRegistro { get; set; }

    public decimal NivelSimilitud { get; set; }

    public int PosicionResultado { get; set; }

    public DateTime FechaHora { get; set; } = DateTime.Now;

    // Navegación
    public BusquedaSemantica? BusquedaSemantica { get; set; }
    public RegistroSalud? RegistroSalud { get; set; }
}