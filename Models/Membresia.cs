using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PredatorsGym.Models
{
    public class Membresia
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string TipoMembresia { get; set; } = string.Empty; // Básica, Premium, Elite

        [Column(TypeName = "decimal(10,2)")]
        public decimal Precio { get; set; }

        public DateTime FechaInicio { get; set; }

        public DateTime FechaFin { get; set; }

        public bool EsActiva { get; set; } = true;

        [StringLength(100)]
        public string? MetodoPago { get; set; }

        [StringLength(50)]
        public string? TransaccionId { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public DateTime? FechaCancelacion { get; set; }

        [StringLength(200)]
        public string? NotasCancelacion { get; set; }

        // Propiedades calculadas
        [NotMapped]
        public bool EstaPorVencer
        {
            get
            {
                if (!EsActiva) return false;
                return (FechaFin - DateTime.Now).TotalDays <= 7;
            }
        }

        [NotMapped]
        public int DiasRestantes
        {
            get
            {
                if (!EsActiva) return 0;
                var dias = (FechaFin - DateTime.Now).TotalDays;
                return dias > 0 ? (int)Math.Ceiling(dias) : 0;
            }
        }

        [NotMapped]
        public string EstadoTexto
        {
            get
            {
                if (!EsActiva) return "Inactiva";
                if (DateTime.Now > FechaFin) return "Vencida";
                if (EstaPorVencer) return "Por vencer";
                return "Activa";
            }
        }
    }

    public class PlanMembresia
    {
        public string Nombre { get; set; } = string.Empty;
        public decimal PrecioMensual { get; set; }
        public decimal PrecioAnual { get; set; }
        public List<string> Caracteristicas { get; set; } = new();
        public string Icono { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public bool EsPopular { get; set; } = false;
        public string Descripcion { get; set; } = string.Empty;
    }
}