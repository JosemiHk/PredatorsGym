using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PredatorsGym.Models
{
    public class Rutina
    {
        public int Id { get; set; }

        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Genero { get; set; } = string.Empty;

        [Required]
        [Range(12, 120)]
        public int Edad { get; set; }

        [Required]
        [Range(50, 300)]
        public double Altura { get; set; }

        [Required]
        [Range(1, 500)]
        public double Peso { get; set; }

        public double IMC { get; set; }

        [StringLength(50)]
        public string EstadoIMC { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Objetivo { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Experiencia { get; set; } = string.Empty;

        [Required]
        [Range(1, 500)]
        public double PesoObjetivo { get; set; }

        [Required]
        [StringLength(100)]
        public string LugarEntrenamiento { get; set; } = string.Empty;

        public bool TieneImplementosBasicos { get; set; }

        [Required]
        [StringLength(50)]
        public string DiasEntrenamiento { get; set; } = string.Empty;

        [Required]
        public string RutinaGenerada { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        //  NUEVOS: Campos de auditoría
        public DateTime? FechaModificacion { get; set; }

        [StringLength(450)]
        public string? CreadoPor { get; set; }

        [StringLength(450)]
        public string? ModificadoPor { get; set; }

        [Range(0, 300)]
        public int DuracionTotalMinutos { get; set; }

        public EstadoRutina Estado { get; set; } = EstadoRutina.Creada;

        public DateTime? FechaInicioEntrenamiento { get; set; }

        public DateTime? FechaFinEntrenamiento { get; set; }

        // Navegación a ejercicios
        public virtual ICollection<Ejercicio> Ejercicios { get; set; } = new List<Ejercicio>();

        // Propiedades calculadas
        [NotMapped]
        public TimeSpan? DuracionReal
        {
            get
            {
                if (FechaInicioEntrenamiento.HasValue && FechaFinEntrenamiento.HasValue)
                {
                    return FechaFinEntrenamiento.Value - FechaInicioEntrenamiento.Value;
                }
                return null;
            }
        }

        [NotMapped]
        public bool PuedeIniciar => Estado == EstadoRutina.Creada || Estado == EstadoRutina.Pausada;

        [NotMapped]
        public bool PuedeCompletar => Estado == EstadoRutina.EnProgreso;
    }

    public enum EstadoRutina
    {
        Creada = 0,
        EnProgreso = 1,
        Pausada = 2,
        Completada = 3,
        Cancelada = 4
    }
}