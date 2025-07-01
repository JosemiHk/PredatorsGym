using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace PredatorsGym.Models
{
    public class Rutina
    {
        public int Id { get; set; }

        // ❌ No poner Required, se llena en backend
        public string UsuarioId { get; set; }

        [Required]
        public string Genero { get; set; }

        [Required]
        public int Edad { get; set; }

        [Required]
        public double Altura { get; set; }

        [Required]
        public double Peso { get; set; }

        public double IMC { get; set; }
        public string EstadoIMC { get; set; }

        [Required]
        public string Objetivo { get; set; }

        [Required]
        public string Experiencia { get; set; }

        [Required]
        public double PesoObjetivo { get; set; }

        [Required]
        public string LugarEntrenamiento { get; set; }

        public bool TieneImplementosBasicos { get; set; }

        [Required]
        public string DiasEntrenamiento { get; set; }

        public string RutinaGenerada { get; set; }

        public DateTime FechaCreacion { get; set; }
    }
}
