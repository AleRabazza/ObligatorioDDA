namespace ObligatorioDDA.src.Services
{
    public class MiniJuegoMatematicas : IMiniJuego
    {
        private readonly Random generador;

        public MiniJuegoMatematicas()
        {
            this.generador = new Random();
        }

        public (string enunciado, string? pregunta, string respuestaCorrecta, object? datos) Generar()
        {
            int a = this.generador.Next(1, 101);
            int b = this.generador.Next(1, 101);
            int c = this.generador.Next(1, 101);
            int suma = a + b + c;

            string enunciado = a.ToString() + " + " + b.ToString() + " + " + c.ToString() + " = ?";
            return (enunciado, null, suma.ToString(), new { A = a, B = b, C = c, Suma = suma });
        }

        public (bool correcto, string mensaje) Validar(string respuestaDelUsuario, object? datos, string respuestaCorrecta)
        {
            int valor;
            bool esNumero = int.TryParse((respuestaDelUsuario ?? string.Empty).Trim(), out valor);
            if (!esNumero || valor < 0 || valor > 999)
            {
                return (false, "Por favor ingresa un número válido");
            }

            bool ok = valor.ToString() == respuestaCorrecta;
            string mensaje = ok ? "¡Correcto! Has recolectado madera" : "Respuesta incorrecta. La suma correcta era " + respuestaCorrecta;
            return (ok, mensaje);
        }
    }
}
