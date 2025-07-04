using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PredatorsGym.Models;

namespace PredatorsGym.Datos
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Rutina> Rutinas { get; set; }
        public DbSet<Ejercicio> Ejercicios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de Rutina
            modelBuilder.Entity<Rutina>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.UsuarioId).IsRequired();
                entity.Property(r => r.RutinaGenerada).HasMaxLength(4000);
                entity.Property(r => r.Estado).HasConversion<string>();

                // Índices para performance
                entity.HasIndex(r => r.UsuarioId);
                entity.HasIndex(r => r.FechaCreacion);
                entity.HasIndex(r => r.Estado);
            });

            // Configuración de Ejercicio
            modelBuilder.Entity<Ejercicio>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Descripcion).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.Instrucciones).HasMaxLength(2000);

                // Relación con Rutina
                entity.HasOne(e => e.Rutina)
                      .WithMany(r => r.Ejercicios)
                      .HasForeignKey(e => e.RutinaId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Índices
                entity.HasIndex(e => e.RutinaId);
                entity.HasIndex(e => e.Orden);
            });
        }
    }
}