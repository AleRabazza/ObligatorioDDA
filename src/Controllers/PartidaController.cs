using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ObligatorioDDA.src.Data;
using ObligatorioDDA.src.Models;
using ObligatorioDDA.src.Services;
using ObligatorioDDA.src.Helpers;

namespace ObligatorioDDA.src.Controllers
{
    public class PartidaController : Controller
    {
        private readonly AppDbContext _context;

        public PartidaController(AppDbContext context)
        {
            _context = context;
        }


        public ActionResult Index() 
        {
            Partida  HayPartidaEnJuego = _context.Partidas.FirstOrDefault(p => p.Estado == EstadoPartida.Jugando);
 
            //Partida HayPartidaTerminada = _context.Partidas.FirstOrDefault(p => p.Estado == EstadoPartida.Terminada);
            //if (HayPartidaEnJuego == null) {
            //    //retornar resultados de partida anterior si la hubo

            //}

            //if (HayPartidaEnJuego == null && HayPartidaTerminada == null)
            //{
            //    HayPartidaEnJuego = IniciarPartida();
            //}   

            if (HayPartidaEnJuego == null)
            {
                HayPartidaEnJuego = IniciarPartida();
            }

            // viewbags de meta para mostrar en la vista
            ViewBag.MetaMadera = HayPartidaEnJuego.MetaMadera;
            ViewBag.MetaPiedra = HayPartidaEnJuego.MetaPiedra;
            ViewBag.MetaComida = HayPartidaEnJuego.MetaComida;
            ViewBag.PartidaId = HayPartidaEnJuego.Id;
            return View();
        }

        public Partida IniciarPartida()
        {
            Partida partida = new Partida();
            var ServicioCalcularMeta = new ServicioCalcularMeta();
            partida.Estado = EstadoPartida.Jugando;
            partida.MetaMadera = ServicioCalcularMeta.CalcularMetas();
            partida.MetaPiedra = ServicioCalcularMeta.CalcularMetas();
            partida.MetaComida = ServicioCalcularMeta.CalcularMetas();
            partida.Registros = new List<Registro>();
            _context.Partidas.Add(partida);
            _context.SaveChanges();
            return partida;
        }

        [HttpPost]
        public IActionResult Recolectar(int partidaId, Recurso.TipoRecurso tipo)
        {
            Partida? partida = _context.Partidas.FirstOrDefault(p => p.Id == partidaId && p.Estado == EstadoPartida.Jugando);
            if (partida == null) return BadRequest("No hay partida");

            int? jugadorId = HttpContext.Session.GetInt32(SessionUsuario.JugadorId);
            if (jugadorId == null) return BadRequest("No hay jugador en sesión");

            //nos fijamos antes de sumar el recurso si ya se llego a la meta 
            int madera = _context.Registros
                .Where(r => r.Id_Partida == partidaId && r.TipoRecolectado == Recurso.TipoRecurso.Madera)
                .Sum(r => r.Puntaje);

            int piedra = _context.Registros
                .Where(r => r.Id_Partida == partidaId && r.TipoRecolectado == Recurso.TipoRecurso.Piedra)
                .Sum(r => r.Puntaje);

            int comida = _context.Registros
                .Where(r => r.Id_Partida == partidaId && r.TipoRecolectado == Recurso.TipoRecurso.Comida)
                .Sum(r => r.Puntaje);

            bool maderaLlena = madera >= partida.MetaMadera;
            bool piedraLlena = piedra >= partida.MetaPiedra;
            bool comidaLlena = comida >= partida.MetaComida;

            // si e recurso que se quiere sumar ya llego a la meta, no se suma
            bool puedeSumar =
                (tipo == Recurso.TipoRecurso.Madera && !maderaLlena) ||
                (tipo == Recurso.TipoRecurso.Piedra && !piedraLlena) ||
                (tipo == Recurso.TipoRecurso.Comida && !comidaLlena);

            if (puedeSumar)
            {
                _context.Registros.Add(new Registro
                {
                    Id_Jugador = jugadorId.Value,
                    Id_Partida = partidaId,
                    TipoRecolectado = tipo,
                    Puntaje = 1, // siempre sumamos 1
                    Fecha = DateTime.UtcNow
                });
                _context.SaveChanges();

                // se actualiza el tota del recurso que se sumo
                switch (tipo)
                {
                    case Recurso.TipoRecurso.Madera: madera++; break;
                    case Recurso.TipoRecurso.Piedra: piedra++; break;
                    case Recurso.TipoRecurso.Comida: comida++; break;
                }
            }

            // se vuelve a verificar si se llego a la meta despues de sumar
            maderaLlena = madera >= partida.MetaMadera;
            piedraLlena = piedra >= partida.MetaPiedra;
            comidaLlena = comida >= partida.MetaComida;

            bool completada = maderaLlena && piedraLlena && comidaLlena;
            if (completada && partida.Estado != EstadoPartida.Terminada)
            {
                // Tomamos primer y último registro de la partida desde la base
                DateTime? primerFechaPartida = _context.Registros
                    .Where(registro => registro.Id_Partida == partidaId)
                    .Select(registro => (DateTime?)registro.Fecha)
                    .Min();

                DateTime? ultimaFechaPartida = _context.Registros
                    .Where(registro => registro.Id_Partida == partidaId)
                    .Select(registro => (DateTime?)registro.Fecha)
                    .Max();

                if (primerFechaPartida.HasValue && ultimaFechaPartida.HasValue)
                {
                    partida.TiempoPartida = ultimaFechaPartida.Value - primerFechaPartida.Value; // ⬅️ intervalo exacto
                }

                partida.Estado = EstadoPartida.Terminada;
                _context.SaveChanges();
            }

            string? tiempoMinutos = null;
            if (completada = true && partida.TiempoPartida.HasValue)
            {
                TimeSpan duracionPartida = partida.TiempoPartida.Value;
                                                     
                //sacamos los minutos y segundos y los ponemos en formato mm:ss para mostrarlos en la view  
                tiempoMinutos = $"{(int)duracionPartida.TotalMinutes}:{duracionPartida.Seconds:D2}";
            }
            return Ok(new
            {
                ok = true,
                totales = new { madera, piedra, comida },
                metasAlcanzadas = new { madera = maderaLlena, piedra = piedraLlena, comida = comidaLlena },
                completada,
                tiempoPartida = tiempoMinutos
            });

        }








    }
}
