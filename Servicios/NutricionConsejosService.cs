using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PredatorsGym.Datos;
using PredatorsGym.Models;

namespace PredatorsGym.Servicios
{
    public class NutricionConsejosService : INutricionConsejosService
    {
        private readonly ApplicationDbContext _ctx;
        private readonly IAzureOpenAIService? _ai;
        private readonly ILogger<NutricionConsejosService>? _logger;

        // Constructor usado en producción (inyección de IA)
        public NutricionConsejosService(ApplicationDbContext ctx,
                                        IAzureOpenAIService ai,
                                        ILogger<NutricionConsejosService> logger)
        {
            _ctx = ctx;
            _ai = ai;
            _logger = logger;
        }

        // Overload para tests existentes (sin IA)
        public NutricionConsejosService(ApplicationDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<NutritionAdviceResult> GenerarConsejosAsync(string usuarioId)
        {
            var r = new NutritionAdviceResult();

            var esElite = await _ctx.Membresias
                .AsNoTracking()
                .AnyAsync(m => m.UsuarioId == usuarioId && m.EsActiva && m.TipoMembresia == "Elite");

            if (!esElite)
            {
                r.EsElegible = false;
                r.MotivoNoElegible = "Solo disponible para membresía Elite.";
                return r;
            }

            var perfil = await _ctx.PerfilesUsuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UsuarioId == usuarioId);

            if (perfil == null)
            {
                r.EsElegible = false;
                r.MotivoNoElegible = "Perfil incompleto.";
                return r;
            }

            r.EsElegible = true;

            // Datos
            var objetivo = (perfil.ObjetivoPrincipal ?? "").Trim();
            var objetivoLower = objetivo.ToLowerInvariant();
            var actividad = (perfil.NivelActividad ?? perfil.NivelExperiencia ?? "").Trim();
            var edad = CalcularEdad(perfil);
            var peso = perfil.PesoActual;
            var altura = perfil.Altura;
            var pesoObj = perfil.PesoObjetivo;

            // Fallback determinista (para tests / si IA falla)
            var reglasBase = GenerarReglasBase(objetivoLower, peso, altura, pesoObj, actividad);

            // Intentar IA si disponible
            if (_ai != null)
            {
                try
                {
                    var prompt = ConstruirPromptNutricion(edad, peso, altura, pesoObj, objetivo, actividad);
                    var texto = await _ai.GenerarTextoAsync(prompt);

                    var iaConsejos = ParsearConsejos(texto);

                    // Si IA devolvió menos de 3, rellenar con reglas base sin duplicar
                    foreach (var c in reglasBase)
                    {
                        if (iaConsejos.Count >= 3) break;
                        if (!iaConsejos.Any(x => Normalizar(x) == Normalizar(c)))
                            iaConsejos.Add(c);
                    }

                    r.Consejos = iaConsejos.Take(3).ToList();
                    return r;
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Fallo al generar consejos IA, usando fallback.");
                }
            }

            // Sin IA o error → usar reglas base (máx 3)
            r.Consejos = reglasBase.Take(3).ToList();
            return r;
        }

        private static string ConstruirPromptNutricion(int? edad, decimal? peso, decimal? altura, decimal? pesoObjetivo,
                                                       string objetivo, string actividad)
        {
            // Instrucciones para salida limpia
            return $@"
Eres un nutricionista deportivo experto.
Genera EXACTAMENTE 3 consejos nutricionales breves (<=120 caracteres cada uno), prácticos y accionables
para complementar una rutina de entrenamiento del usuario descrito:

Edad: {(edad?.ToString() ?? "N/D")}
Peso actual: {(peso?.ToString("F1") ?? "N/D")} kg
Altura: {(altura?.ToString("F1") ?? "N/D")} cm
Peso objetivo: {(pesoObjetivo?.ToString("F1") ?? "N/D")} kg
Objetivo principal: {objetivo}
Nivel de actividad: {actividad}

Reglas:
- No usar enumeraciones (sin 1., 2., -, *, etc.).
- Cada consejo en UNA línea.
- No repetir palabras clave innecesariamente.
- Español neutro.
- Deben ser deportivos (proteína, carbohidratos complejos, hidratación estratégica, micronutrientes, timing).

Formato de salida: solo las 3 líneas de los consejos, nada más.
";
        }

        private static List<string> ParsearConsejos(string texto)
        {
            var lineas = (texto ?? "")
                .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l
                    .Trim()
                    .TrimStart('-', '*', '•', '>', '–')
                    .Trim())
                .Where(l => l.Length > 0 && l.Length <= 180)
                .Distinct()
                .ToList();

            // Eliminar numeraciones al inicio (1. / 2) etc.
            for (int i = 0; i < lineas.Count; i++)
            {
                var l = lineas[i];
                var idx = l.IndexOf(' ');
                if (idx > 0 && idx <= 4 && l.Substring(0, idx).TrimEnd('.', ')').All(c => char.IsDigit(c)))
                    lineas[i] = l[(idx + 1)..].Trim();
            }

            return lineas.Take(5).ToList();
        }

        private static string Normalizar(string v) =>
            new string(v.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());

        private static List<string> GenerarReglasBase(string objetivoLower, decimal? peso, decimal? altura,
                                                      decimal? pesoObjetivo, string actividad)
        {
            var consejos = new List<string>();

            // Hidratación siempre
            consejos.Add("Mantén hidratación suficiente (2–3 L/día).");

            if (objetivoLower.Contains("muscul") || objetivoLower.Contains("masa"))
                consejos.Add("Distribuye proteína magra en 3–5 comidas (1.6–2.2 g/kg).");

            if (objetivoLower.Contains("bajar") || objetivoLower.Contains("peso") || objetivoLower.Contains("defin"))
                consejos.Add("Aplica déficit calórico moderado (10–20%). Prioriza vegetales fibrosos.");

            if (objetivoLower.Contains("rendimiento"))
                consejos.Add("Incluye carbohidratos complejos pre y post entrenamiento para energía sostenida.");

            // IMC aproximado
            if (peso.HasValue && altura.HasValue && peso > 0 && altura > 0)
            {
                var hM = (double)altura.Value / 100.0;
                var imc = (double)peso.Value / (hM * hM);
                if (imc < 18.5)
                    consejos.Add("Incrementa calorías con grasas saludables y carbohidratos complejos.");
                else if (imc >= 25 && imc < 30)
                    consejos.Add("Controla porciones y evita calorías líquidas azucaradas.");
                else if (imc >= 30)
                    consejos.Add("Registra tu ingesta diaria para mejorar adherencia y control.");
            }

            if (peso.HasValue && pesoObjetivo.HasValue)
            {
                var diferencia = Math.Abs(pesoObjetivo.Value - peso.Value);
                var porcentaje = diferencia / peso.Value;
                if (porcentaje >= 0.1m)
                {
                    if (pesoObjetivo > peso)
                        consejos.Add("Añade snacks densos en nutrientes (frutos secos, yogur griego, aguacate).");
                    else
                        consejos.Add("Reduce ultraprocesados y azúcares añadidos para facilitar pérdida de grasa.");
                }
            }

            if (!string.IsNullOrWhiteSpace(actividad))
            {
                var actLower = actividad.ToLowerInvariant();
                if (actLower.Contains("sedent"))
                    consejos.Add("Aumenta NEAT: caminar más, pausas activas, subir escaleras.");
                else if (actLower.Contains("alto") || actLower.Contains("intens"))
                    consejos.Add("Cuida electrolitos (sodio, potasio, magnesio) en días de alta carga.");
            }

            // Calidad base
            consejos.Add("Prioriza alimentos naturales variados (colores) y grasas saludables.");
            return consejos
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .ToList();
        }

        private static int? CalcularEdad(PerfilUsuario perfil)
        {
            if (perfil.FechaNacimiento == null) return null;
            var hoy = DateTime.Today;
            var edad = hoy.Year - perfil.FechaNacimiento.Value.Year;
            if (perfil.FechaNacimiento.Value.Date > hoy.AddYears(-edad)) edad--;
            return edad;
        }
    }
}