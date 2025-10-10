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
            Partida? partida = _context.Partidas.FirstOrDefault(p => p.Estado == EstadoPartida.Jugando);
            if (partida == null)
            {
                partida = IniciarPartida();
            }

            // metas de partida
            ViewBag.MetaMadera = partida.MetaMadera;
            ViewBag.MetaPiedra = partida.MetaPiedra;
            ViewBag.MetaComida = partida.MetaComida;
            ViewBag.PartidaId = partida.Id;

            // totakes de partida
            int totalMadera = _context.Registros
                .Where(r => r.Id_Partida == partida.Id && r.TipoRecolectado == Recurso.TipoRecurso.Madera)
                .Sum(r => (int?)r.Puntaje) ?? 0;

            int totalPiedra = _context.Registros
                .Where(r => r.Id_Partida == partida.Id && r.TipoRecolectado == Recurso.TipoRecurso.Piedra)
                .Sum(r => (int?)r.Puntaje) ?? 0;

            int totalComida = _context.Registros
                .Where(r => r.Id_Partida == partida.Id && r.TipoRecolectado == Recurso.TipoRecurso.Comida)
                .Sum(r => (int?)r.Puntaje) ?? 0;

            ViewBag.TotalMadera = totalMadera;
            ViewBag.TotalPiedra = totalPiedra;
            ViewBag.TotalComida = totalComida;

            // desabhilita si se cumple la meta
            ViewBag.MaderaLlena = totalMadera >= partida.MetaMadera;
            ViewBag.PiedraLlena = totalPiedra >= partida.MetaPiedra;
            ViewBag.ComidaLlena = totalComida >= partida.MetaComida;

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

        [HttpGet]
        public IActionResult EstadoActual()
        {
            Partida? partida = _context.Partidas
                .OrderByDescending(p => p.Id)
                .FirstOrDefault(p => p.Estado == EstadoPartida.Jugando || p.Estado == EstadoPartida.Terminada);

            if (partida == null)
                return Ok(new { ok = true, sinPartida = true });

            int totMadera = _context.Registros
                .Where(r => r.Id_Partida == partida.Id && r.TipoRecolectado == Recurso.TipoRecurso.Madera)
                .Sum(r => (int?)r.Puntaje) ?? 0;

            int totPiedra = _context.Registros
                .Where(r => r.Id_Partida == partida.Id && r.TipoRecolectado == Recurso.TipoRecurso.Piedra)
                .Sum(r => (int?)r.Puntaje) ?? 0;

            int totComida = _context.Registros
                .Where(r => r.Id_Partida == partida.Id && r.TipoRecolectado == Recurso.TipoRecurso.Comida)
                .Sum(r => (int?)r.Puntaje) ?? 0;

            bool completa =
                totMadera >= partida.MetaMadera &&
                totPiedra >= partida.MetaPiedra &&
                totComida >= partida.MetaComida;

            return Ok(new
            {
                ok = true,
                partidaId = partida.Id,
                metas = new { madera = partida.MetaMadera, piedra = partida.MetaPiedra, comida = partida.MetaComida },
                totales = new { madera = totMadera, piedra = totPiedra, comida = totComida },
                partidaCompletada = completa
            });
        }
    }
}
   
