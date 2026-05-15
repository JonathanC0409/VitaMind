namespace VitaMind.MVC.Models
{
    public class RespuestaAgenteViewModel
    {
        public string Pregunta { get; set; } = string.Empty;
        public string Respuesta { get; set; } = string.Empty;
        public int TiempoConsultaMs { get; set; }
        public int RegistrosRecuperados { get; set; }
        public decimal SimilitudPromedio { get; set; }
    }
}