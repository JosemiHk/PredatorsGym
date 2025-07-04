using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PredatorsGym.Models
{
    public class Ejercicio
    {
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; }

        [Required]
        public string Descripcion { get; set; }

        [Required]
        public int Series { get; set; }

        [Required]
        public int Repeticiones { get; set; }

        /// <summary>
        /// Tiempo de descanso entre series en segundos
        /// </summary>
        public int TiempoDescanso { get; set; } = 60;

        /// <summary>
        /// Tiempo estimado total del ejercicio en segundos
        /// </summary>
        public int DuracionEstimada { get; set; }

        /// <summary>
        /// Instrucciones específicas para el ejercicio
        /// </summary>
        public string? Instrucciones { get; set; }

        /// <summary>
        /// Orden del ejercicio dentro de la rutina
        /// </summary>
        public int Orden { get; set; }

        // Navegación
        public int RutinaId { get; set; }

        [ForeignKey("RutinaId")]
        public virtual Rutina Rutina { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}