using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PredatorsGym.Models
{
    public class PerfilViewModel : IValidatableObject
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; } = string.Empty;

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        [StringLength(50, ErrorMessage = "El nombre no puede superar 50 caracteres.")]
        public string? Nombre { get; set; }

        [StringLength(100, ErrorMessage = "Los apellidos no pueden superar 100 caracteres.")]
        public string? Apellidos { get; set; }

        [StringLength(15, ErrorMessage = "El teléfono no puede superar 15 dígitos.")]
        [RegularExpression(@"^\d{0,15}$", ErrorMessage = "El teléfono solo puede contener dígitos (máx. 15).")]
        public string? Telefono { get; set; }

        [DataType(DataType.Date)]
        public DateTime? FechaNacimiento { get; set; }

        [StringLength(20, ErrorMessage = "El género no puede superar 20 caracteres.")]
        public string? Genero { get; set; }

        [Range(80, 300, ErrorMessage = "La altura debe estar entre 80 y 300 cm.")]
        public decimal? Altura { get; set; }

        [Range(20, 500, ErrorMessage = "El peso actual debe estar entre 20 y 500 kg.")]
        public decimal? PesoActual { get; set; }

        [Range(20, 500, ErrorMessage = "El peso objetivo debe estar entre 20 y 500 kg.")]
        public decimal? PesoObjetivo { get; set; }

        [StringLength(20, ErrorMessage = "El nivel de experiencia no puede superar 20 caracteres.")]
        public string? NivelExperiencia { get; set; }

        [StringLength(50, ErrorMessage = "El objetivo principal no puede superar 50 caracteres.")]
        public string? ObjetivoPrincipal { get; set; }

        [Range(1, 7, ErrorMessage = "Los días de entrenamiento deben estar entre 1 y 7.")]
        public int? DiasEntrenamientoSemana { get; set; }

        [Range(15, 180, ErrorMessage = "La duración preferida debe estar entre 15 y 180 minutos.")]
        public int? DuracionPreferida { get; set; }

        [StringLength(500, ErrorMessage = "El campo de lesiones/limitaciones no puede superar 500 caracteres.")]
        public string? LesionesLimitaciones { get; set; }

        public DateTime? UltimaActividad { get; set; }

        public bool TieneImagenPerfil { get; set; }

        public IFormFile? ImagenPerfil { get; set; }

        // Edad calculada (solo lectura)
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