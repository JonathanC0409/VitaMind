using System.ComponentModel.DataAnnotations;

namespace VitaMind.Models;

public class ServicioEstado
{
    [Key]
    public Guid IdServicio { get; set; }

    public string NombreServicio { get; set; } = string.Empty;
    // Supabase Vector / n8n / OpenAI API / API .NET

    public string Estado { get; set; } = "Activo";
    // Activo / Inactivo / Error

    public DateTime UltimoChequeo { get; set; } = DateTime.Now;

    public string? Mensaje { get; set; }
}