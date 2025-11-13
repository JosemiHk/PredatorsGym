using System.Collections.Generic;

namespace PredatorsGym.Models
{
    public class NutritionAdviceResult
    {
        public bool EsElegible { get; set; }
        public string? MotivoNoElegible { get; set; }
        public List<string> Consejos { get; set; } = new();
    }
}