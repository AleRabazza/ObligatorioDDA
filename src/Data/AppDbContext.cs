using Microsoft.EntityFrameworkCore;
using ObligatorioDDA.src.Models;

namespace ObligatorioDDA.src.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
        public DbSet<Jugador> Jugadores { get; set; }

        public DbSet<Recurso> Recursos { get; set; }

        public DbSet<Registro> Registros { get; set; }

        public DbSet<Partida> Partidas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Jugador>()
                .HasKey(jugador => jugador.Id_Jugador);

            modelBuilder.Entity<Recurso>()
                .HasKey(recurso => recurso.Id_Recurso);

            modelBuilder.Entity<Registro>()
                .HasKey(registro => registro.Id_Registro);

            modelBuilder.Entity<Partida>()
                .HasKey(partida => partida.Id_Partida);


        }
    }
}