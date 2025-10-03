using Microsoft.EntityFrameworkCore;
using ObligatorioDDA.src.Models;

namespace ObligatorioDDA.src.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Tablas
        public DbSet<Jugador> Jugadores { get; set; }
        public DbSet<Recurso> Recursos { get; set; }
        public DbSet<Registro> Registros { get; set; }
        public DbSet<Partida> Partidas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // PKs (explícitas)
            modelBuilder.Entity<Jugador>().HasKey(jugador => jugador.Id);
            modelBuilder.Entity<Recurso>().HasKey(recurso => recurso.Id);
            modelBuilder.Entity<Registro>().HasKey(registro => registro.Id);
            modelBuilder.Entity<Partida>().HasKey(partida => partida.Id);

            // Relaciones mínimas (sin restricciones extra)
            modelBuilder.Entity<Registro>()
                .HasOne(registro => registro.Partida)
                .WithMany(p => p.Registros)
                .HasForeignKey(registro => registro.Id_Partida);

            modelBuilder.Entity<Registro>()
                .HasOne(registro => registro.Jugador)
                .WithMany() // si añadís ICollection<Registro> en Jugador, cambiá por .WithMany(j => j.Registros)
                .HasForeignKey(registro => registro.Id_Jugador);

            // Nota: los enums se guardarán como int por defecto (sin conversiones ni seeds).
        }
    }
}
