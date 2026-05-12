using System.ComponentModel.DataAnnotations;
using Pgvector;

namespace VitaMind.Models;

public class BusquedaSemantica
{
    [Key]
    public Guid IdBusqueda { get; set; }

    public Guid IdUsuario { get; set; }

    public string Consulta { get; set; } = string.Empty;

    public Vector? EmbeddingConsulta { get; set; }

    public int TiempoBusquedaMs { get; set; }

    public int TotalResultados { get; set; }

    public decimal SimilitudMaxima { get; set; }

    public decimal SimilitudPromedio { get; set; }

    public string Estado { get; set; } = "Exitosa";

    public DateTime FechaHora { get; set; } = DateTime.Now;

    // Navegación
    public Usuario? Usuario { get; set; }
    public ICollection<ResultadoBusqueda>? ResultadosBusqueda { get; set; }
}