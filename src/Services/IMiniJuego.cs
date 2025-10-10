namespace ObligatorioDDA.src.Services
{
    public interface IMiniJuego
    {
        (string enunciado, string? pregunta, string respuestaCorrecta, object? datos) Generar();

        (bool correcto, string mensaje) Validar(string respuestaDelUsuario, object? datos, string respuestaCorrecta);
    }
}
