using Microsoft.AspNetCore.Http;
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
                IniciarPartida();
            }

            // viewbags de meta para mostrar en la vista
            ViewBag.MetaMadera = HayPartidaEnJuego.MetaMadera;
            ViewBag.MetaPiedra = HayPartidaEnJuego.MetaPiedra;
            ViewBag.MetaComida = HayPartidaEnJuego.MetaComida;



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

        public void MostrarMetas(){

            Partida? partida = _context.Partidas.SingleOrDefault(p => p.Estado == EstadoPartida.Jugando); //ver despues 

        }

        
        public void Recolectar (){

        }
        




    }
}
