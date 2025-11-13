using System.Threading.Tasks;
using PredatorsGym.Models;

namespace PredatorsGym.Servicios
{
    public interface INutricionConsejosService
    {
        Task<NutritionAdviceResult> GenerarConsejosAsync(string usuarioId);
    }
}