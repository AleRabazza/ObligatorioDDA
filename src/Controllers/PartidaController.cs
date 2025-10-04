using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ObligatorioDDA.src.Data;
using ObligatorioDDA.src.Models;
using ObligatorioDDA.src.Services;

namespace ObligatorioDDA.src.Controllers
{
    public class PartidaController : Controller
    {
        private readonly AppDbContext _context;
        private const string SessionJugadorId = "JugadorId";

        public PartidaController(AppDbContext context)
        {
            _context = context;
        }

        // GET: JuegoController
        public ActionResult Index()
        {
            Partida?  HayPartidaEnJuego = _context.Partidas.FirstOrDefault(p => p.Estado == EstadoPartida.Jugando);
            if (HayPartidaEnJuego == null) {                
 
                Partida? HayPartidaTerminada = _context.Partidas.FirstOrDefault(p => p.Estado == EstadoPartida.Terminada);
                if (HayPartidaTerminada == null) {
                    IniciarPartida();
                    HayPartidaEnJuego = _context.Partidas.FirstOrDefault(p => p.Estado == EstadoPartida.Jugando);
                } 



            }
 

            // viewbags de meta para mostrar en la vista
            ViewBag.MetaMadera = HayPartidaEnJuego.MetaMadera;
            ViewBag.MetaPiedra = HayPartidaEnJuego.MetaPiedra;
            ViewBag.MetaComida = HayPartidaEnJuego.MetaComida;
            ViewBag.PartidaId = HayPartidaEnJuego.Id;
            return View();
        }

        public void IniciarPartida()



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

        }

        [HttpPost]
        public IActionResult GuardarJugador(string nombre)
        {
            if (string.IsNullOrEmpty(nombre)) {
                return BadRequest("El nombre del jugador no puede estar vacío.");
            }

            Jugador? jugador = _context.Jugadores.FirstOrDefault(j => j.Nombre == nombre);
            if (jugador == null) {
                jugador = new Jugador { Nombre = nombre };
                _context.Jugadores.Add(jugador);
                _context.SaveChanges();
            }
            HttpContext.Session.SetInt32(SessionJugadorId, jugador.Id);
                

            return Ok(
                new { ok = true, jugadorId = jugador.Id, nombre = jugador.Nombre }    // manda el statatus 200 con la info del jugador
            );
        }

        [HttpPost]
        public IActionResult Recolectar(int partidaId, Recurso.TipoRecurso tipo)
        {
            Partida partida = _context.Partidas.FirstOrDefault(p => p.Id == partidaId && p.Estado == EstadoPartida.Jugando);
            if (partida == null) {
                return BadRequest("No hay partida");
            }


            int? jugadorId = HttpContext.Session.GetInt32(SessionJugadorId);
            if (jugadorId == null) {
                return BadRequest("No hay jugador en sesión");
            }

            _context.Registros.Add(new Registro
            {
                Id_Jugador = jugadorId.Value,
                Id_Partida = partidaId,
                TipoRecolectado = tipo,
                Puntaje = 1, // cada vez que se recolecta, se suma 1
                Fecha = DateTime.Now
            });
            _context.SaveChanges();

            // Calcular totales actuales
            int maderaActual = _context.Registros.Count(registro => registro.Id_Partida == partidaId && registro.TipoRecolectado == Recurso.TipoRecurso.Madera);
            int piedraActual= _context.Registros.Count(registro => registro.Id_Partida == partidaId && registro.TipoRecolectado == Recurso.TipoRecurso.Piedra);
            int comidaActual = _context.Registros.Count(registro => registro.Id_Partida == partidaId && registro.TipoRecolectado == Recurso.TipoRecurso.Comida);

            // Verificar si se alcanzaron las metas
            bool done = false;
            if (maderaActual >= partida.MetaMadera && piedraActual >= partida.MetaPiedra && comidaActual >= partida.MetaComida) {
                partida.Estado = EstadoPartida.Terminada;
                _context.SaveChanges();
                done = true;
            }
            return Ok(       //aca vamos a devolver datos para actualizart desp en la vista los recursos recolectados actuales y si completa la partioda deshabilitar los vbotones de recoleccion
               (new { ok = true, totales = new { madera = maderaActual, piedra = piedraActual, comida = comidaActual }, completada = done })
            );

        }

        // Se crean los registros bien, falta implemenmtar que cuando llegue a la meta no deje hacer ,mas y se deshabiuite el boton
        // ma;ana vemos eso y lo resolvemos para que quede bien, ademas hay quie hacer que se fije que cuando llegue a todas las metas, ponga e calculo de tiempo de partida en la partida \



        




    }
}
