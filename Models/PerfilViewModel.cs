using System.ComponentModel.DataAnnotations;

namespace PredatorsGym.Models
{
    public class PerfilViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Formato de teléfono inválido")]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [Display(Name = "Nombre")]
        [StringLength(50, ErrorMessage = "El nombre no puede tener más de 50 caracteres")]
        public string? Nombre { get; set; }

        [Display(Name = "Apellidos")]
        [StringLength(100, ErrorMessage = "Los apellidos no pueden tener más de 100 caracteres")]
        public string? Apellidos { get; set; }

        [Display(Name = "Fecha de Nacimiento")]
        [DataType(DataType.Date)]
        public DateTime? FechaNacimiento { get; set; }

        [Display(Name = "Género")]
        public string? Genero { get; set; }

        [Display(Name = "Altura (cm)")]
        [Range(50, 300, ErrorMessage = "La altura debe estar entre 50 y 300 cm")]
        public decimal? Altura { get; set; }

        [Display(Name = "Peso Actual (kg)")]
        [Range(20, 500, ErrorMessage = "El peso debe estar entre 20 y 500 kg")]
        public decimal? PesoActual { get; set; }

        [Display(Name = "Peso Objetivo (kg)")]
        [Range(20, 500, ErrorMessage = "El peso objetivo debe estar entre 20 y 500 kg")]
        public decimal? PesoObjetivo { get; set; }

        [Display(Name = "Nivel de Experiencia")]
        public string? NivelExperiencia { get; set; }

        [Display(Name = "Objetivo Principal")]
        public string? ObjetivoPrincipal { get; set; }

        [Display(Name = "Días de entrenamiento por semana")]
        [Range(1, 7, ErrorMessage = "Los días de entrenamiento deben estar entre 1 y 7")]
        public int? DiasEntrenamientoSemana { get; set; }

        [Display(Name = "Duración preferida de entrenamiento (minutos)")]
        [Range(15, 180, ErrorMessage = "La duración debe estar entre 15 y 180 minutos")]
        public int? DuracionPreferida { get; set; }

        [Display(Name = "Lesiones o Limitaciones")]
        [StringLength(500, ErrorMessage = "Las lesiones/limitaciones no pueden tener más de 500 caracteres")]
        public string? LesionesLimitaciones { get; set; }

        // Propiedades para la imagen de perfil
        [Display(Name = "Foto de perfil")]
        public IFormFile? ImagenPerfil { get; set; }

        public bool TieneImagenPerfil { get; set; }

        // Propiedades calculadas
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        public DateTime? UltimaActividad { get; set; }

        public int? Edad
        {
            get
            {
                if (FechaNacimiento.HasValue)
                {
                    var hoy = DateTime.Today;
                    var edad = hoy.Year - FechaNacimiento.Value.Year;
                    if (FechaNacimiento.Value.Date > hoy.AddYears(-edad)) edad--;
                    return edad;
                }
                return null;
            }
        }

        public decimal? IMC
        {
            get
            {
                if (PesoActual.HasValue && Altura.HasValue && Altura > 0)
                {
                    var alturaMetros = Altura.Value / 100;
                    return Math.Round(PesoActual.Value / (alturaMetros * alturaMetros), 2);
                }
                return null;
            }
        }

        public string CategoriaIMC
        {
            get
            {
                if (IMC.HasValue)
                {
                    var imc = IMC.Value;
                    if (imc < 18.5m) return "Bajo peso";
                    if (imc < 25m) return "Peso normal";
                    if (imc < 30m) return "Sobrepeso";
                    return "Obesidad";
                }
                return "";
            }
        }

        public string ColorIMC
        {
            get
            {
                if (IMC.HasValue)
                {
                    var imc = IMC.Value;
                    if (imc < 18.5m) return "warning";
                    if (imc < 25m) return "success";
                    if (imc < 30m) return "warning";
                    return "danger";
                }
                return "secondary";
            }
        }
    }
}