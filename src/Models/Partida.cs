namespace ObligatorioDDA.src.Models
{
    public enum EstadoPartida { Jugando = 0, Terminada = 1 }

    public class Partida
    {
        public int Id { get; set; }
        public EstadoPartida Estado { get; set; } = EstadoPartida.Jugando;
        public TimeSpan? TiempoPartida { get; set; }  //cambio a timespan para que sea la diferencia entre dos fechas
        public ICollection<Registro> Registros { get; set; } = new List<Registro>();
        public int MetaMadera { get; set; }
        public int MetaPiedra { get; set; }
        public int MetaComida { get; set; }

        public Partida() { }
    }
}
