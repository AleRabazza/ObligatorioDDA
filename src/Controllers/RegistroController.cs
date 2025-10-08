using Microsoft.AspNetCore.Mvc;
using ObligatorioDDA.src.Data;
using ObligatorioDDA.src.Helpers;
using ObligatorioDDA.src.Models;
using ObligatorioDDA.src.Models.DTOs;

namespace ObligatorioDDA.src.Controllers
{
    public class RegistroController : Controller
    {
        private readonly AppDbContext _context;

        public RegistroController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult GuardarRegistro(int partidaId, Recurso.TipoRecurso tipo)
        {
            Partida? partida = _context.Partidas.FirstOrDefault(partida => partida.Id == partidaId && partida.Estado == EstadoPartida.Jugando);

            if (partida == null)
            {
                return BadRequest("No hay una partida en juego con el ID proporcionado.");
            }

            int? jugadorId = HttpContext.Session.GetInt32("JugadorId");
            if (jugadorId == null)
            {
                return BadRequest("No hay un jugador en sesión.");
            }
           

            //traemos los totales actuales
            Totales totales = CalcularTotales(partidaId);
            (bool maderaLlena, bool piedraLlena, bool comidaLlena) metas = VerificarMetas(partida, totales);
            // si todavia no llego a la meta del recurso que intenta sumar, lo sumo
            if (PuedeSumar(tipo, metas))
            {
                Registro nuevoRegistro = new Registro
                {
                    Id_Partida = partidaId,
                    Id_Jugador = jugadorId.Value,
                    TipoRecolectado = tipo,
                    Puntaje = 1,
                    Fecha = DateTime.Now
                };
                _context.Registros.Add(nuevoRegistro);
                _context.SaveChanges();

                // actualizar los totales desp de sumar un registro 

                switch (tipo)
                {
                    case Recurso.TipoRecurso.Madera:
                        totales.Madera++;
                        break;
                    case Recurso.TipoRecurso.Piedra:
                        totales.Piedra++;
                        break;
                    case Recurso.TipoRecurso.Comida:
                        totales.Comida++;
                        break;
                }

            }
    
            // se vuelec a calcular si se cumplieron las metas 
            metas = VerificarMetas(partida, totales);

            // ver si se termina la partida 
            bool partidaCompletada = metas.maderaLlena && metas.piedraLlena && metas.comidaLlena;
            if (partidaCompletada)
            {
                MarcarPartidaCompletada(partida);
               List<JugadorTotal> registros = ObtenerTotalesPorJugador(partidaId);
                ViewBag.Registros = registros;
            }

            string? tiempoPartida = partidaCompletada ? MostrarTiempoPartidaEnMin(partida.TiempoPartida) : null;

            return Ok(new
            {
                ok = true,
                totales = new { madera = totales.Madera, piedra = totales.Piedra, comida = totales.Comida },
                metasAlcanzadas = new { madera = metas.maderaLlena, piedra = metas.piedraLlena, comida = metas.comidaLlena },
                partidaCompletada,
                tiempoPartida,
                
            });
        }



        private class Totales
        {
            public int Madera { get; set; }
            public int Piedra { get; set; }
            public int Comida { get; set; }
        }

        private class  RecursoTipoTotal
        {
            public Recurso.TipoRecurso Tipo { get; set; }
            public int SumaTotal { get; set; }
        }

        private Totales CalcularTotales(int partidaId)
        {
            Totales totales = new Totales();

            List<RecursoTipoTotal> recursosTotales = _context.Registros
                .Where(r => r.Id_Partida == partidaId)
                .GroupBy(r => r.TipoRecolectado)
                .Select(g => new RecursoTipoTotal
                {
                    Tipo = g.Key,
                    SumaTotal = g.Sum(r => r.Puntaje)
                })
                .ToList();

            foreach(RecursoTipoTotal recurso in recursosTotales)
            {
                switch (recurso.Tipo)
                {
                    case Recurso.TipoRecurso.Madera:
                        totales.Madera = recurso.SumaTotal;
                        break;
                    case Recurso.TipoRecurso.Piedra:
                        totales.Piedra = recurso.SumaTotal;
                        break;
                    case Recurso.TipoRecurso.Comida:
                        totales.Comida = recurso.SumaTotal;
                        break;
                }
            }
            return totales;
        }

        private (bool madera, bool piedra, bool comida) VerificarMetas(Partida partida, Totales totales)
        {
            bool maderaLlena = totales.Madera >= partida.MetaMadera;
            bool piedraLlena = totales.Piedra >= partida.MetaPiedra;
            bool comidaLlena = totales.Comida >= partida.MetaComida;

            return (maderaLlena, piedraLlena, comidaLlena);
        }

        private bool PuedeSumar(Recurso.TipoRecurso tipo, (bool madera, bool piedra, bool comida) metas)
        {
            return (tipo == Recurso.TipoRecurso.Madera && !metas.madera) ||
                   (tipo == Recurso.TipoRecurso.Piedra && !metas.piedra) ||
                   (tipo == Recurso.TipoRecurso.Comida && !metas.comida);
        }

        private void MarcarPartidaCompletada(Partida partida)
        {
            if (partida.Estado == EstadoPartida.Terminada)
                return;

            DateTime? primerFecha = _context.Registros
                .Where(registro => registro.Id_Partida == partida.Id)
                .Select(registro => (DateTime?)registro.Fecha)
                .Min();

            DateTime? ultimaFecha = _context.Registros
                .Where(registro => registro.Id_Partida == partida.Id)
                .Select(registro => (DateTime?)registro.Fecha)
                .Max();

            if (primerFecha.HasValue && ultimaFecha.HasValue)
            {
                partida.TiempoPartida = ultimaFecha.Value - primerFecha.Value;
            }

            partida.Estado = EstadoPartida.Terminada;
            _context.SaveChanges();

            ViewBag.EstadoPartida = partida.Estado;

            int partidaId = partida.Id;
            ObtenerTotalesPorJugador(partidaId);
        }

        private string? MostrarTiempoPartidaEnMin(TimeSpan? tiempo)
        {
            if (!tiempo.HasValue)
                return null;

            return $"{(int)tiempo.Value.TotalMinutes}:{tiempo.Value.Seconds:D2}";
        }

        List<JugadorTotal> ObtenerTotalesPorJugador(int partidaId)
        {
            List<JugadorTotal> totalesPorJugador = _context.Registros
                .Where(r => r.Id_Partida == partidaId)
                .GroupBy(r => r.Id_Jugador)
                .Select(g => new JugadorTotal
                {
                    JugadorId = g.Key,
                    SumaTotalComida = g.Where(r => r.TipoRecolectado == Recurso.TipoRecurso.Comida).Sum(r => r.Puntaje),
                    SumaTotalMadera = g.Where(r => r.TipoRecolectado == Recurso.TipoRecurso.Madera).Sum(r => r.Puntaje),
                    SumaTotalPiedra = g.Where(r => r.TipoRecolectado == Recurso.TipoRecurso.Piedra).Sum(r => r.Puntaje)
                })
                .ToList();
            return totalesPorJugador;
        }
    }
}
