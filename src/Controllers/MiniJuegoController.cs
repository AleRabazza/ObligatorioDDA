using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ObligatorioDDA.src.Data;
using ObligatorioDDA.src.Models;
using ObligatorioDDA.src.Models.DTOs;
using ObligatorioDDA.src.Services;


namespace ObligatorioDDA.src.Controllers
{
    public class MinijuegoController : Controller
    {
        private readonly AppDbContext _context;
        private const string SesionKey = "MiniJuegoActual";
        private const int TimeoutSegundos = 60;

        public MinijuegoController(AppDbContext context)
        {
            _context = context;
        }



        [HttpGet]
        public IActionResult Generar(int partidaId, Recurso.TipoRecurso tipo)
        {
            int? jugadorId = HttpContext.Session.GetInt32("JugadorId");
            if (jugadorId == null)
            {
                return BadRequest(new { ok = false, msg = "Debes ingresar tu nombre antes de jugar." });
            }

            Partida? partida = _context.Partidas.FirstOrDefault(p => p.Id == partidaId && p.Estado == EstadoPartida.Jugando);
            if (partida == null)
            {
                return BadRequest(new { ok = false, msg = "No hay partida activa." });
            }

            IMiniJuego juego = CrearMinijuego(tipo);

            (string enunciado, string? pregunta, string respuestaCorrecta, object? datos) generado = juego.Generar();

            MiniJuegoEstado state = new MiniJuegoEstado
            {
                PartidaId = partidaId,
                TipoRecurso = (int)tipo,
                Enunciado = generado.enunciado,
                Pregunta = generado.pregunta,
                RespuestaCorrecta = generado.respuestaCorrecta,
                Datos = generado.datos,
                Expira = DateTime.UtcNow.AddSeconds(TimeoutSegundos)
            };

            string json = JsonSerializer.Serialize(state);
            HttpContext.Session.SetString(SesionKey, json);

            if (tipo == Recurso.TipoRecurso.Piedra)
            {
                return Ok(new
                {
                    ok = true,
                    enunciado = "Memoriza la secuencia:",
                    pregunta = (string?)null,
                    preguntaDiferida = generado.pregunta,
                    countdown = TimeoutSegundos,
                    datos = generado.datos  
                });
            }

            return Ok(new
            {
                ok = true,
                enunciado = generado.enunciado,
                pregunta = generado.pregunta,
                countdown = TimeoutSegundos
            });
        }

        [HttpPost]
        public IActionResult Responder(int partidaId, Recurso.TipoRecurso tipo, string respuesta)
        {
            int? jugadorId = HttpContext.Session.GetInt32("JugadorId");
            if (jugadorId == null)
            {
                return BadRequest(new { ok = false, msg = "No hay un jugador en sesión." });
            }

            string? raw = HttpContext.Session.GetString(SesionKey);
            if (string.IsNullOrEmpty(raw))
            {
                return BadRequest(new { ok = false, msg = "No hay minijuego activo." });
            }

            MiniJuegoEstado? state = JsonSerializer.Deserialize<MiniJuegoEstado>(raw);
            if (state == null || state.PartidaId != partidaId || state.TipoRecurso != (int)tipo)
            {
                return BadRequest(new { ok = false, msg = "El minijuego no coincide con la solicitud." });
            }

            if (DateTime.UtcNow > state.Expira)
            {
                HttpContext.Session.Remove(SesionKey);
                return Ok(new { ok = true, correcto = false, timeout = true, msg = "Tiempo agotado" });
            }

            IMiniJuego juego = CrearMinijuego(tipo);
            (bool correcto, string mensaje) validacion = juego.Validar(respuesta, state.Datos, state.RespuestaCorrecta);

            // Siempre cerrar el minijuego después de responder (correcto o incorrecto)
            HttpContext.Session.Remove(SesionKey);

            if (!validacion.correcto)
            {
                return Ok(new
                {
                    ok = true,
                    correcto = false,
                    msg = validacion.mensaje
                });
            }

            // ========= LÓGICA DE GUARDAR REGISTRO AQUÍ (sin servicio) =========

            Partida? partida = _context.Partidas.FirstOrDefault(p => p.Id == partidaId && p.Estado == EstadoPartida.Jugando);
            if (partida == null)
            {
                // Si ya no está en juego, devolvemos finalizada para que el front pase a resultados
                return Ok(new { ok = true, correcto = true, msg = validacion.mensaje, partidaCompletada = true });
            }

            // Totales actuales
            Totales totales = CalcularTotales(partidaId);
            (bool maderaLlena, bool piedraLlena, bool comidaLlena) metas = VerificarMetas(partida, totales);

            // Solo sumamos si el recurso todavía no llegó a su meta
            if (PuedeSumar(tipo, metas))
            {
                Registro nuevoRegistro = new Registro
                {
                    Id_Partida = partidaId,
                    Id_Jugador = jugadorId.Value,
                    TipoRecolectado = tipo,
                    Puntaje = 1,
                    Fecha = DateTime.UtcNow
                };

                _context.Registros.Add(nuevoRegistro);
                _context.SaveChanges();

                if (tipo == Recurso.TipoRecurso.Madera)
                {
                    totales.Madera++;
                }
                else if (tipo == Recurso.TipoRecurso.Piedra)
                {
                    totales.Piedra++;
                }
                else if (tipo == Recurso.TipoRecurso.Comida)
                {
                    totales.Comida++;
                }
            }

            // Recalcular metas alcanzadas
            metas = VerificarMetas(partida, totales);

            bool partidaCompletada = metas.maderaLlena && metas.piedraLlena && metas.comidaLlena;
            List<JugadorTotal>? registros = null;

            if (partidaCompletada)
            {
                MarcarPartidaCompletada(partida);
                registros = ObtenerTotalesPorJugador(partidaId);
            }

            string? tiempoPartida = partidaCompletada ? MostrarTiempoPartidaEnMin(partida.TiempoPartida) : null;

            return Ok(new
            {
                ok = true,
                correcto = true,
                msg = validacion.mensaje,
                totales = new { madera = totales.Madera, piedra = totales.Piedra, comida = totales.Comida },
                metasAlcanzadas = new { madera = metas.maderaLlena, piedra = metas.piedraLlena, comida = metas.comidaLlena },
                partidaCompletada = partidaCompletada,
                tiempoPartida = tiempoPartida,
                registros = partidaCompletada ? registros : null
            });
        }




        private IMiniJuego CrearMinijuego(Recurso.TipoRecurso tipo)
        {
            if (tipo == Recurso.TipoRecurso.Madera)
            {
                return new MiniJuegoMatematicas();
            }

            if (tipo == Recurso.TipoRecurso.Piedra)
            {
                return new MiniJuegoMemoria();
            }

            return new MiniJuegoLogica();
        }

        private class Totales
        {
            public int Madera { get; set; }
            public int Piedra { get; set; }
            public int Comida { get; set; }
        }

        private class RecursoTipoTotal
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

            foreach (RecursoTipoTotal recurso in recursosTotales)
            {
                if (recurso.Tipo == Recurso.TipoRecurso.Madera)
                {
                    totales.Madera = recurso.SumaTotal;
                }
                else if (recurso.Tipo == Recurso.TipoRecurso.Piedra)
                {
                    totales.Piedra = recurso.SumaTotal;
                }
                else if (recurso.Tipo == Recurso.TipoRecurso.Comida)
                {
                    totales.Comida = recurso.SumaTotal;
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
            bool puedeMadera = tipo == Recurso.TipoRecurso.Madera && !metas.madera;
            bool puedePiedra = tipo == Recurso.TipoRecurso.Piedra && !metas.piedra;
            bool puedeComida = tipo == Recurso.TipoRecurso.Comida && !metas.comida;
            return puedeMadera || puedePiedra || puedeComida;
        }

        private void MarcarPartidaCompletada(Partida partida)
        {
            if (partida.Estado == EstadoPartida.Terminada)
            {
                return;
            }

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
        }
        private string? MostrarTiempoPartidaEnMin(TimeSpan? tiempo)
        {
            if (!tiempo.HasValue)
                return null;

            return $"{(int)tiempo.Value.TotalMinutes}:{tiempo.Value.Seconds:D2}";
        }

        private List<JugadorTotal> ObtenerTotalesPorJugador(int partidaId)
        {
            List<JugadorTotal> totalesPorJugador = _context.Registros
                .Where(r => r.Id_Partida == partidaId)
                .GroupBy(r => r.Id_Jugador)
                .Select(g => new JugadorTotal
                {
                    JugadorId = g.Key,
                    SumaTotalComida = g.Where(r => r.TipoRecolectado == Recurso.TipoRecurso.Comida).Sum(r => r.Puntaje),
                    SumaTotalMadera = g.Where(r => r.TipoRecolectado == Recurso.TipoRecurso.Madera).Sum(r => r.Puntaje),
                    SumaTotalPiedra = g.Where(r => r.TipoRecolectado == Recurso.TipoRecurso.Piedra).Sum(r => r.Puntaje),
                    SumaTotalRecursos = g.Sum(r => r.Puntaje)
                }).ToList();

            List<JugadorTotal> totalesConNombre = totalesPorJugador
                .Join(_context.Jugadores,
                      s => s.JugadorId,
                      j => j.Id,
                      (s, j) => new JugadorTotal
                      {
                          JugadorId = s.JugadorId,
                          NombreJugador = j.Nombre,
                          SumaTotalComida = s.SumaTotalComida,
                          SumaTotalMadera = s.SumaTotalMadera,
                          SumaTotalPiedra = s.SumaTotalPiedra,
                          SumaTotalRecursos = s.SumaTotalRecursos
                      })
                .OrderByDescending(x => x.SumaTotalRecursos)
                .ThenBy(x => x.NombreJugador)
                .ToList();

            return totalesConNombre;
        }
    }
}