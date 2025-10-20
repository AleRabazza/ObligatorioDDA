namespace ObligatorioDDA.src.Services
{
    public class MiniJuegoLogica : IMiniJuego
    {
        private readonly Random _generador;

        public MiniJuegoLogica()
        {
            _generador = new Random();
        }

        public (string enunciado, string? pregunta, string respuestaCorrecta, object? datos) Generar()
        {
            int[] numeros = new int[3];
            for (int i = 0; i < 3; i++)
            {
                numeros[i] = _generador.Next(1, 101);
            }

            string[] proposiciones = new string[]
            {
                "Exactamente 2 números son pares",
                "La suma de los 3 números es par",
                "El número mayor es mayor que la suma de los otros dos",
                "Hay al menos un número mayor que 50",
                "Todos los números son diferentes"
            };

            string proposicion = proposiciones[_generador.Next(proposiciones.Length)];
            bool valor = false;

            if (proposicion == "Exactamente 2 números son pares")
            {
                valor = numeros.Count(x => x % 2 == 0) == 2;
            }
            else if (proposicion == "La suma de los 3 números es par")
            {
                valor = (numeros.Sum() % 2) == 0;
            }
            else if (proposicion == "El número mayor es mayor que la suma de los otros dos")
            {
                int maximo = numeros.Max();
                int sumaResto = numeros.Sum() - maximo;
                valor = maximo > sumaResto;
            }
            else if (proposicion == "Hay al menos un número mayor que 50")
            {
                valor = numeros.Any(x => x > 50);
            }
            else if (proposicion == "Todos los números son diferentes")
            {
                valor = numeros.Distinct().Count() == 3;
            }

            string enunciado = numeros[0].ToString() + ", " + numeros[1].ToString() + ", " + numeros[2].ToString();
            string respuestaCorrecta = valor ? "Verdadero" : "Falso";
            return (enunciado, proposicion, respuestaCorrecta, new { Numeros = numeros });
        }

        public (bool correcto, string mensaje) Validar(string respuestaDelUsuario, object? datos, string respuestaCorrecta)
        {
            string respuesta = (respuestaDelUsuario ?? string.Empty).Trim();
            if (respuesta != "Verdadero" && respuesta != "Falso")
            {
                return (false, "Debes elegir Verdadero o Falso");
            }

            bool ok = respuesta == respuestaCorrecta;
            string mensaje = ok ? "¡Correcto! Has recolectado comida"  : "Respuesta incorrecta. La respuesta correcta era: " + respuestaCorrecta;
            return (ok, mensaje);
        }
    }
}
