namespace ObligatorioDDA.src.Models
{
    public enum EstadoPartida { Jugando = 0, Terminada = 1 }

    public class Partida
    {
        public int Id { get; set; }
        public EstadoPartida estado { get; set; } = EstadoPartida.Jugando;
        public DateTime TiempoPartida { get; set; }
        public List<Registro>? Registros { get; set; }
        public int MetaMadera { get; set; }
        public int MetaPiedra { get; set; }
        public int MetaComida { get; set; }

    }
}
