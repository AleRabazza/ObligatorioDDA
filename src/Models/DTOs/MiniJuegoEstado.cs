namespace ObligatorioDDA.src.Models.DTOs
{
    public class MiniJuegoEstado
    {
        public int PartidaId { get; set; }
        public int TipoRecurso { get; set; } 
        public string Enunciado { get; set; } = string.Empty;
        public string? Pregunta { get; set; }
        public string RespuestaCorrecta { get; set; } = string.Empty;
        public DateTime Expira{ get; set; }
        public object? Datos { get; set; }
    }
}
