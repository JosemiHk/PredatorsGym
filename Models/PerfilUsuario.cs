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

        // Columna para almacenar la imagen como bytes (BLOB)
        public byte[]? ImagenPerfil { get; set; }

        [StringLength(10)]
        public string? TipoImagen { get; set; } // jpg, png, etc.

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public DateTime FechaActualizacion { get; set; } = DateTime.Now;

        public DateTime? UltimaActividad { get; set; }

        // Propiedades calculadas (no mapeadas)
        [NotMapped]
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

        [NotMapped]
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

        [NotMapped]
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

        [NotMapped]
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