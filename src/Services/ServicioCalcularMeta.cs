using ObligatorioDDA.src.Models;

namespace ObligatorioDDA.src.Services
{
    public class ServicioCalcularMeta
    {
        private readonly Random _random;

        public ServicioCalcularMeta()
        {
            _random = new Random();
        }
        
        // Método para calcular las metas (solo V1)
        public void CalcularMetas(Partida partida)
        {

            partida.MetaMadera = GenerarMeta();
            partida.MetaPiedra = GenerarMeta();
            partida.MetaComida = GenerarMeta();
        }

         public int GenerarMeta()
        {
            double numero = _random.NextDouble(); // valor entre 0 y 1
            int meta = (int)Math.Round(numero * 100);

            if (meta < 10) meta = 10;
            if (meta > 100) meta = 100;

            return meta;
        }
    }
}



