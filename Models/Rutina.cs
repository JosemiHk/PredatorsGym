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

        // 🆕 NUEVAS PROPIEDADES PARA WORKOUT
        /// <summary>
        /// Duración total estimada de la rutina en minutos
        /// </summary>
        public int DuracionTotalMinutos { get; set; }

        /// <summary>
        /// Estado de la rutina (Creada, EnProgreso, Completada, Pausada)
        /// </summary>
        public EstadoRutina Estado { get; set; } = EstadoRutina.Creada;

        /// <summary>
        /// Fecha y hora de inicio del entrenamiento
        /// </summary>
        public DateTime? FechaInicioEntrenamiento { get; set; }

        /// <summary>
        /// Fecha y hora de finalización del entrenamiento
        /// </summary>
        public DateTime? FechaFinEntrenamiento { get; set; }

        // 🆕 NAVEGACIÓN A EJERCICIOS
        public virtual ICollection<Ejercicio> Ejercicios { get; set; } = new List<Ejercicio>();
    }

    public enum EstadoRutina
    {
        Creada,
        EnProgreso,
        Pausada,
        Completada,
        Cancelada
    }
}