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
        public DbSet<PerfilUsuario> PerfilesUsuarios { get; set; }
        public DbSet<Membresia> Membresias { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ CONFIGURACIÓN: Rutina
            modelBuilder.Entity<Rutina>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.UsuarioId).IsRequired();

                //  Campo largo para IA
                entity.Property(r => r.RutinaGenerada)
                      .HasColumnType("nvarchar(MAX)")
                      .IsRequired();

                entity.Property(r => r.Estado).HasConversion<string>();

                //  NUEVA: Relación FK explícita
                entity.HasOne<IdentityUser>()
                      .WithMany()
                      .HasForeignKey(r => r.UsuarioId)
                      .OnDelete(DeleteBehavior.Cascade);

                //  Índices para performance
                entity.HasIndex(r => r.UsuarioId)
                      .HasDatabaseName("IX_Rutinas_UsuarioId");

                entity.HasIndex(r => r.FechaCreacion)
                      .HasDatabaseName("IX_Rutinas_FechaCreacion");

                entity.HasIndex(r => r.Estado)
                      .HasDatabaseName("IX_Rutinas_Estado");

                //  NUEVO: Índice compuesto para consultas frecuentes
                entity.HasIndex(r => new { r.UsuarioId, r.FechaCreacion, r.Estado })
                      .HasDatabaseName("IX_Rutinas_Usuario_Fecha_Estado");

                //  NUEVAS: Validaciones a nivel de BD
                entity.HasCheckConstraint("CK_Rutinas_EdadValida",
                    "[Edad] >= 12 AND [Edad] <= 120");

                entity.HasCheckConstraint("CK_Rutinas_PesoValido",
                    "[Peso] > 0 AND [Peso] <= 500");

                entity.HasCheckConstraint("CK_Rutinas_AlturaValida",
                    "[Altura] >= 50 AND [Altura] <= 300");
            });

            //  CONFIGURACIÓN: Ejercicio
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
                entity.HasIndex(e => e.RutinaId)
                      .HasDatabaseName("IX_Ejercicios_RutinaId");

                entity.HasIndex(e => e.Orden)
                      .HasDatabaseName("IX_Ejercicios_Orden");

                //  NUEVO: Índice compuesto para ordenamiento
                entity.HasIndex(e => new { e.RutinaId, e.Orden })
                      .HasDatabaseName("IX_Ejercicios_RutinaId_Orden");

                //  NUEVAS: Validaciones
                entity.HasCheckConstraint("CK_Ejercicios_SeriesValidas",
                    "[Series] >= 1 AND [Series] <= 10");

                entity.HasCheckConstraint("CK_Ejercicios_RepeticionesValidas",
                    "[Repeticiones] >= 1 AND [Repeticiones] <= 100");

                entity.HasCheckConstraint("CK_Ejercicios_OrdenValido",
                    "[Orden] >= 1");
            });

            //  CONFIGURACIÓN: PerfilUsuario
            modelBuilder.Entity<PerfilUsuario>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.UsuarioId).IsRequired();

                //  NUEVA: Relación FK explícita
                entity.HasOne<IdentityUser>()
                      .WithMany()
                      .HasForeignKey(p => p.UsuarioId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Índice único
                entity.HasIndex(p => p.UsuarioId)
                      .IsUnique()
                      .HasDatabaseName("IX_PerfilesUsuarios_UsuarioId_Unique");

                // Índices para consultas
                entity.HasIndex(p => p.FechaActualizacion)
                      .HasDatabaseName("IX_PerfilesUsuarios_FechaActualizacion");

                entity.HasIndex(p => p.UltimaActividad)
                      .HasDatabaseName("IX_PerfilesUsuarios_UltimaActividad");

                //  NUEVAS: Validaciones
                entity.HasCheckConstraint("CK_PerfilesUsuarios_AlturaValida",
                    "[Altura] IS NULL OR ([Altura] >= 50 AND [Altura] <= 300)");

                entity.HasCheckConstraint("CK_PerfilesUsuarios_PesoValido",
                    "[PesoActual] IS NULL OR ([PesoActual] > 0 AND [PesoActual] <= 500)");

                entity.HasCheckConstraint("CK_PerfilesUsuarios_DiasValidos",
                    "[DiasEntrenamientoSemana] IS NULL OR ([DiasEntrenamientoSemana] >= 1 AND [DiasEntrenamientoSemana] <= 7)");
            });

            //  CONFIGURACIÓN: Membresia
            modelBuilder.Entity<Membresia>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Property(m => m.UsuarioId).IsRequired();
                entity.Property(m => m.TipoMembresia).IsRequired();
                entity.Property(m => m.Precio).HasPrecision(10, 2);

                //  NUEVA: Relación FK explícita
                entity.HasOne<IdentityUser>()
                      .WithMany()
                      .HasForeignKey(m => m.UsuarioId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Índices individuales
                entity.HasIndex(m => m.UsuarioId)
                      .HasDatabaseName("IX_Membresias_UsuarioId");

                entity.HasIndex(m => m.FechaInicio)
                      .HasDatabaseName("IX_Membresias_FechaInicio");

                entity.HasIndex(m => m.FechaFin)
                      .HasDatabaseName("IX_Membresias_FechaFin");

                entity.HasIndex(m => m.EsActiva)
                      .HasDatabaseName("IX_Membresias_EsActiva");

                //  NUEVO: Índice compuesto para consultas frecuentes
                entity.HasIndex(m => new { m.UsuarioId, m.EsActiva, m.FechaFin })
                      .HasDatabaseName("IX_Membresias_Usuario_Activa_Fin");

                //  NUEVAS: Validaciones a nivel de BD
                entity.HasCheckConstraint("CK_Membresias_FechaFinMayor",
                    "[FechaFin] > [FechaInicio]");

                entity.HasCheckConstraint("CK_Membresias_PrecioPositivo",
                    "[Precio] > 0");

                entity.HasCheckConstraint("CK_Membresias_TipoValido",
                    "[TipoMembresia] IN ('Básico', 'Premium', 'Elite')");
            });
        }

        //  NUEVO: Auditoría automática
        public override int SaveChanges()
        {
            AgregarAuditoriaAutomatica();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            AgregarAuditoriaAutomatica();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void AgregarAuditoriaAutomatica()
        {
            var entradas = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entrada in entradas)
            {
                // Auditoría en PerfilUsuario
                if (entrada.Entity is PerfilUsuario perfil)
                {
                    if (entrada.State == EntityState.Modified)
                    {
                        perfil.FechaActualizacion = DateTime.UtcNow;
                    }
                }

                // Auditoría en Rutina
                if (entrada.Entity is Rutina rutina)
                {
                    if (entrada.State == EntityState.Added)
                    {
                        rutina.FechaCreacion = DateTime.UtcNow;
                    }
                    else if (entrada.State == EntityState.Modified)
                    {
                        rutina.FechaModificacion = DateTime.UtcNow;
                    }
                }
            }
        }
    }
}