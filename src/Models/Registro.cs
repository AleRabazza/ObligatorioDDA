namespace ObligatorioDDA.src.Models
{
    public class Registro
    {
        public int Id_Registro { get; set; }

        public int id_Partida { get; set; }

        public int id_Jugador { get; set; }

        public int id_Recurso { get; set; }

        public DateTime Fecha { get; set; }

        public int puntaje { get; set; } = 1;


    }
}
