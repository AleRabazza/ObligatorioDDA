namespace ObligatorioDDA.src.Services
{
    public interface IMiniJuego
    {
        void GenerarPregunta( int tipoJuego);
        bool VerificarRespuesta(string respuesta);
    }
}
