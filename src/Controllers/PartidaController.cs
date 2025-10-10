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
            Partida HayPartidaEnJuego = _context.Partidas.FirstOrDefault(p => p.Estado == EstadoPartida.Jugando);


            if (HayPartidaEnJuego == null)
            {
                HayPartidaEnJuego = IniciarPartida();
            }

            // viewbags de meta para mostrar en la vista
            ViewBag.MetaMadera = HayPartidaEnJuego.MetaMadera;
            ViewBag.MetaPiedra = HayPartidaEnJuego.MetaPiedra;
            ViewBag.MetaComida = HayPartidaEnJuego.MetaComida;
            ViewBag.PartidaId = HayPartidaEnJuego.Id;

            int? jugadorId = HttpContext.Session.GetInt32(SessionUsuario.JugadorId);
            string? nombreJugador = null;
            if (jugadorId.HasValue)
            {
                nombreJugador = _context.Jugadores
                    .Where(j => j.Id == jugadorId.Value)
                    .Select(j => j.Nombre)
                    .FirstOrDefault();
            }

            ViewBag.NombreJugador = nombreJugador;                 
            ViewBag.RequiereNombre = !jugadorId.HasValue;
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

        public IActionResult Reiniciar()
        {
            try
            {
                // Verificamos si ya existe una partida activa (puede haberla iniciado otro jugador)
                Partida? partidaEnCurso = _context.Partidas
                    .FirstOrDefault(p => p.Estado == EstadoPartida.Jugando);

                if (partidaEnCurso != null)
                {
                    // Si ya existe, no creamos una nueva: devolvemos esa
                    return Ok(new
                    {
                        ok = true,
                        existente = true,
                        partidaId = partidaEnCurso.Id,
                        metaMadera = partidaEnCurso.MetaMadera,
                        metaPiedra = partidaEnCurso.MetaPiedra,
                        metaComida = partidaEnCurso.MetaComida
                    });
                }

                // Si no hay partida activa, terminamos la anterior (si existía)
                Partida? ultimaPartida = _context.Partidas
                    .OrderByDescending(p => p.Id)
                    .FirstOrDefault();

                if (ultimaPartida != null && ultimaPartida.Estado != EstadoPartida.Terminada)
                {
                    ultimaPartida.Estado = EstadoPartida.Terminada;
                    _context.SaveChanges();
                }

                // Creamos una nueva usando tu propia función IniciarPartida()
                Partida nuevaPartida = IniciarPartida();

                // Devolvemos los datos al frontend
                return Ok(new
                {
                    ok = true,
                    existente = false,
                    partidaId = nuevaPartida.Id,
                    metaMadera = nuevaPartida.MetaMadera,
                    metaPiedra = nuevaPartida.MetaPiedra,
                    metaComida = nuevaPartida.MetaComida
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ok = false, mensaje = ex.Message });
            }
        }
    }
}    
