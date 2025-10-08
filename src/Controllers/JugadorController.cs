using Microsoft.AspNetCore.Mvc;
using ObligatorioDDA.src.Data;
using ObligatorioDDA.src.Models;
using ObligatorioDDA.src.Helpers;
            
namespace ObligatorioDDA.src.Controllers
{
    public class JugadorController : Controller
    {
        private readonly AppDbContext _context;

        public JugadorController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult GuardarJugador(string nombre)
        {
            if (string.IsNullOrEmpty(nombre))
            {
                return BadRequest("El nombre del jugador no puede estar vacío.");
            }

            Jugador? jugador = _context.Jugadores.FirstOrDefault(j => j.Nombre == nombre);
            if (jugador == null)
            {
                jugador = new Jugador { Nombre = nombre };
                _context.Jugadores.Add(jugador);
                _context.SaveChanges();
            }
          

            HttpContext.Session.SetInt32(SessionUsuario.JugadorId, jugador.Id);

            return Ok((new
            {
                ok = true,
                jugadorId = jugador.Id,
                nombre = jugador.Nombre
            }));
        }


    }
}
