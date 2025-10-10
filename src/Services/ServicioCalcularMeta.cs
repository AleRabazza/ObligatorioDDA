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
        public int CalcularMetas()
        {

            int metaFinal = GenerarMeta();

            return metaFinal;
        }

         private  int GenerarMeta()
        {
            double numero = _random.NextDouble(); // valor entre 0 y 1
            int meta = (int)Math.Round(numero * 10);

            if (meta < 1) meta = 1;
            if (meta > 10) meta = 10;

            return meta;
        }
    }
}



