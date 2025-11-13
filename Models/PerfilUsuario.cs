using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PredatorsGym.Models
{
    public class PerfilUsuario : IValidatableObject
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Nombre { get; set; }

        [StringLength(100)]
        public string? Apellidos { get; set; }

        // Teléfono: solo dígitos, hasta 15 (opcional)
        [StringLength(15, ErrorMessage = "El teléfono no puede superar 15 dígitos.")]
        [RegularExpression(@"^\d{0,15}$", ErrorMessage = "El teléfono solo puede contener dígitos (máx. 15).")]
        public string? Telefono { get; set; }

        public DateTime? FechaNacimiento { get; set; }

        [StringLength(20)]
        public string? Genero { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        [Range(80, 300, ErrorMessage = "La altura debe estar entre 80 y 300 cm.")]
        public decimal? Altura { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        [Range(20, 500, ErrorMessage = "El peso actual debe estar entre 20 y 500 kg.")]
        public decimal? PesoActual { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        [Range(20, 500, ErrorMessage = "El peso objetivo debe estar entre 20 y 500 kg.")]
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

        [StringLength(30)]
        public string? NivelActividad { get; set; }

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

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FechaNacimiento.HasValue)
            {
                var edad = Edad;
                if (edad.HasValue && (edad < 10 || edad > 120))
                {
                    yield return new ValidationResult(
                        "La edad calculada debe estar entre 10 y 120 años.",
                        new[] { nameof(FechaNacimiento) });
                }
            }
        }
    }
}