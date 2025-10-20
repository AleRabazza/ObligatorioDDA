namespace ObligatorioDDA.src.Services
{
    public class MiniJuegoMemoria : IMiniJuego
    {
        private readonly Random generador;

        public MiniJuegoMemoria()
        {
            generador = new Random();
        }

        public (string enunciado, string? pregunta, string respuestaCorrecta, object? datos) Generar()
        {
            int[] secuencia = new int[5];
            for (int i = 0; i < 5; i++)
            {
                secuencia[i] = generador.Next(1, 21);
            }

            string[] preguntas = new string[]
            {
                "¿Había exactamente 2 números pares?",
                "¿Había exactamente 2 números impares?",
                "¿La suma de todos los números superaba 50?",
                "¿Había 2 números iguales?",
                "¿Había algún número menor a 10?"
            };

            string preguntaElegida = preguntas[generador.Next(preguntas.Length)];
            bool respuestaSiNo = false;

            if (preguntaElegida == "¿Había exactamente 2 números pares?")
            {
                respuestaSiNo = secuencia.Count(x => x % 2 == 0) == 2;
            }
            else if (preguntaElegida == "¿Había exactamente 2 números impares?")
            {
                respuestaSiNo = secuencia.Count(x => x % 2 != 0) == 2;
            }
            else if (preguntaElegida == "¿La suma de todos los números superaba 50?")
            {
                respuestaSiNo = secuencia.Sum() > 50;
            }
            else if (preguntaElegida == "¿Había 2 números iguales?")
            {
                respuestaSiNo = secuencia.GroupBy(x => x).Any(g => g.Count() >= 2);
            }
            else if (preguntaElegida == "¿Había algún número menor a 10?")
            {
                respuestaSiNo = secuencia.Any(x => x < 10);
            }

            string respuestaCorrecta = respuestaSiNo ? "Sí" : "No";
            return ("Memoriza la secuencia y responde:", preguntaElegida, respuestaCorrecta, new { secuencia = secuencia });
        }

        public (bool correcto, string mensaje) Validar(string respuestaDelUsuario, object? datos, string respuestaCorrecta)
        {
            string respuestaNormalizada = (respuestaDelUsuario ?? string.Empty).Trim().ToLowerInvariant();
            if (respuestaNormalizada != "sí" && respuestaNormalizada != "si" && respuestaNormalizada != "no")
            {
                return (false, "Debes responder Sí o No");
            }

            bool ok = respuestaNormalizada == respuestaCorrecta.Trim().ToLowerInvariant();
            string mensaje = ok ? "¡Correcto! Has recolectado piedra"
                                : "Respuesta incorrecta. La respuesta correcta era: " + respuestaCorrecta;
            return (ok, mensaje);
        }
    }
}
