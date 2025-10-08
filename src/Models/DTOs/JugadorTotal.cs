namespace ObligatorioDDA.src.Models.DTOs
{
    public class JugadorTotal
    {
        public int JugadorId { get; set; }

        public string? NombreJugador { get; set; }
        
        public int SumaTotalComida { get; set; }

        public int SumaTotalMadera { get; set; }

        public int SumaTotalPiedra { get; set; }

        public int SumaTotalRecursos { get; set; } 

    }
}
