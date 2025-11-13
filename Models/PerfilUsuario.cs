using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PredatorsGym.Models
{
    public class PerfilUsuario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Nombre { get; set; }

        [StringLength(100)]
        public string? Apellidos { get; set; }

        [Phone]
        public string? Telefono { get; set; }

        public DateTime? FechaNacimiento { get; set; }

        [StringLength(20)]
        public string? Genero { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? Altura { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? PesoActual { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? PesoObjetivo { get; set; }

        [StringLength(20)]
        public string? NivelExperiencia { get; set; }

        [StringLength(50)]
        public string? ObjetivoPrincipal { get; set; }

        public int? DiasEntrenamientoSemana { get; set; }

        public int? DuracionPreferida { get; set; }

        [StringLength(500)]
        public string? LesionesLimitaciones { get; set; }

        public byte[]? ImagenPerfil { get; set; }

        [StringLength(10)]
        public string? TipoImagen { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

        public DateTime? UltimaActividad { get; set; }

        // NUEVA propiedad (nivel de actividad general: Sedentario, Moderado, Alto, Intensivo, etc.)
        [StringLength(30)]
        public string? NivelActividad { get; set; }

        // Propiedades calculadas (no mapeadas)
        [NotMapped]
        public int? Edad
        {
            get
            {
                if (!FechaNacimiento.HasValue) return null;
                var hoy = DateTime.Today;
                var edad = hoy.Year - FechaNacimiento.Value.Year;
                if (FechaNacimiento.Value.Date > hoy.AddYears(-edad)) edad--;
                return edad;
            }
        }

        [NotMapped]
        public decimal? IMC
        {
            get
            {
                if (!PesoActual.HasValue || !Altura.HasValue || PesoActual <= 0 || Altura <= 0) return null;
                var hM = Altura.Value / 100m;
                var valor = PesoActual.Value / (hM * hM);
                return Math.Round(valor, 2);
            }
        }
    }
}