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
    }    
}
