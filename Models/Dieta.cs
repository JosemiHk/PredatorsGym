namespace PredatorsGym.Models
{
    public class Dieta
    {
        public int Id { get; set; }
        public string UsuarioId { get; set; }
        public int Edad { get; set; }
        public double Altura { get; set; }
        public double Peso { get; set; }
        public double IMC { get; set; }
        public string EstadoIMC { get; set; }
        public double PesoObjetivo { get; set; }
        public string Objetivo { get; set; }

        public string PreferenciasAlimenticias { get; set; }
        public string Alergias { get; set; }
        public int ComidasPorDia { get; set; }
        public int CaloriasDeseadas { get; set; }
        public string AlimentosFavoritos { get; set; }

        public string DietaGenerada { get; set; }
        public DateTime FechaCreacion { get; set; }
    }

}
