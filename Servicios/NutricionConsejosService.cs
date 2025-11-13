using System.Linq;
using System.Threading.Tasks;
using PredatorsGym.Datos;
using PredatorsGym.Models;
using Microsoft.EntityFrameworkCore;

namespace PredatorsGym.Servicios
{
    public class NutricionConsejosService : INutricionConsejosService
    {
        private readonly ApplicationDbContext _ctx;
        public NutricionConsejosService(ApplicationDbContext ctx) => _ctx = ctx;

        public async Task<NutritionAdviceResult> GenerarConsejosAsync(string usuarioId)
        {
            var r = new NutritionAdviceResult();

            var esElite = await _ctx.Membresias
                .AnyAsync(m => m.UsuarioId == usuarioId && m.EsActiva && m.TipoMembresia == "Elite");

            if (!esElite)
            {
                r.EsElegible = false;
                r.MotivoNoElegible = "Solo Elite";
                return r;
            }

            var perfil = await _ctx.PerfilesUsuarios
                .FirstOrDefaultAsync(p => p.UsuarioId == usuarioId);

            if (perfil == null)
            {
                r.EsElegible = false;
                r.MotivoNoElegible = "Perfil incompleto";
                return r;
            }

            r.EsElegible = true;

            var objetivo = perfil.ObjetivoPrincipal?.ToLowerInvariant() ?? "";
            if (objetivo.Contains("masa") || objetivo.Contains("muscul"))
                r.Consejos.Add("Aumenta tu ingesta de proteína magra.");

            // Consejo base
            r.Consejos.Add("Mantén hidratación suficiente.");

            return r;
        }
    }
}