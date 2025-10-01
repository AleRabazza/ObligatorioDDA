namespace ObligatorioDDA.src.Models
{

    public class Recurso
    {

        public int Id   { get; set; }

        public TipoRecurso Nombre { get; set; }

        public enum TipoRecurso
        {
            Madera,Piedra,Comida
        }
    }
}
