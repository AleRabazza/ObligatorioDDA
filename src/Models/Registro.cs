using System.ComponentModel.DataAnnotations.Schema;
using  ObligatorioDDA.src.Models;

namespace ObligatorioDDA.src.Models
{
    public class Registro
    {
        public int Id { get; set; }

        [ForeignKey("Partida")]
        public int Id_Partida { get; set; }
        public Partida? Partida { get; set; }

        [ForeignKey("Jugador")]
        public int Id_Jugador { get; set; }
        public Jugador? Jugador { get; set; }

        public  Recurso.TipoRecurso TipoRecolectado { get; set; }   
        public int Puntaje { get; set; } = 1;
        
        public DateTime Fecha { get; set; }

    }
}
