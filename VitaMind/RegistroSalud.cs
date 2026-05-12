using System.ComponentModel.DataAnnotations;
using Pgvector;

namespace VitaMind.Models;

public class RegistroSalud
{
    [Key]
    public Guid IdRegistro { get; set; }

    public Guid IdUsuario { get; set; }

    public string Categoria { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string? Intensidad { get; set; }
    public string? Observacion { get; set; }
    public string TipoEntrada { get; set; } = "Formulario";
    public DateTime FechaRegistro { get; set; } = DateTime.Now.Date;
    public TimeSpan HoraRegistro { get; set; } = DateTime.Now.TimeOfDay;
    public string EstadoInsercion { get; set; } = "Insertado";
    public int LatenciaMs { get; set; }
    public int TiempoEmbeddingMs { get; set; }

    public Vector? Embedding { get; set; }

    public int DimensionEmbedding { get; set; } = 1536;
    public bool EsSimilar { get; set; }
    public decimal SimilitudDetectada { get; set; }

    public Guid? IdRegistroSimilar { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    // Navegación
    public Usuario? Usuario { get; set; }
    public RegistroSalud? RegistroSimilar { get; set; }
    public ICollection<ResultadoBusqueda>? ResultadosBusqueda { get; set; }
}