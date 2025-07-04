using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PredatorsGym.Models;
using PredatorsGym.Datos;
using PredatorsGym.Servicios;

namespace PredatorsGym.Controllers
{
    [Authorize]
    public class RutinasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IAzureOpenAIService _azureOpenAIService;
        private readonly ILogger<RutinasController> _logger;

        public RutinasController(ApplicationDbContext context,
                                 UserManager<IdentityUser> userManager,
                                 IAzureOpenAIService azureOpenAIService,
                                 ILogger<RutinasController> logger)
        {
            _context = context;
            _userManager = userManager;
            _azureOpenAIService = azureOpenAIService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerarRutina(Rutina model)
        {
            // Quitar validación innecesaria
            ModelState.Remove("UsuarioId");
            ModelState.Remove("IMC");
            ModelState.Remove("EstadoIMC");
            ModelState.Remove("RutinaGenerada");
            ModelState.Remove("FechaCreacion");

            if (!ModelState.IsValid)
            {
                return View("Create", model);
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                model.UsuarioId = user.Id;
                model.FechaCreacion = DateTime.Now;

                // ✅ CÁLCULO CORREGIDO DEL IMC
                model.IMC = CalcularIMC(model.Peso, model.Altura);
                model.EstadoIMC = CalcularEstadoIMC(model.IMC);

                // 🔄 NUEVO PROMPT OPTIMIZADO PARA AZURE OPENAI
                var prompt = $@"
Necesito una rutina de entrenamiento personalizada para las siguientes características:

📊 DATOS PERSONALES:
- Edad: {model.Edad} años
- Género: {model.Genero}
- Peso actual: {model.Peso} kg
- Altura: {model.Altura} cm
- IMC: {model.IMC:F1} ({model.EstadoIMC})
- Peso objetivo: {model.PesoObjetivo} kg

🎯 OBJETIVOS Y EXPERIENCIA:
- Objetivo principal: {model.Objetivo}
- Nivel de experiencia: {model.Experiencia}
- Días de entrenamiento semanales: {model.DiasEntrenamiento}

🏋️ CONDICIONES DE ENTRENAMIENTO:
- Lugar de entrenamiento: {model.LugarEntrenamiento}
- Implementos básicos disponibles: {(model.TieneImplementosBasicos ? "Sí (mancuernas, bandas, etc.)" : "No, solo peso corporal")}

Por favor crea una rutina completa y personalizada considerando estos factores.
";

                _logger.LogInformation("Generando rutina para usuario {UserId} con Azure OpenAI", user.Id);

                // 🚀 LLAMADA A AZURE OPENAI
                model.RutinaGenerada = await _azureOpenAIService.GenerarRutinaPersonalizadaAsync(prompt);

                _context.Rutinas.Add(model);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Rutina generada y guardada exitosamente para usuario {UserId}", user.Id);

                return View("RutinaGenerada", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar rutina para usuario");
                ModelState.AddModelError("", "Hubo un error al generar tu rutina. Por favor intenta nuevamente.");
                return View("Create", model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> RutinaGenerada(int id)
        {
            var rutina = await _context.Rutinas.FindAsync(id);
            if (rutina == null) return NotFound();

            return View(rutina);
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var rutina = await _context.Rutinas
                .Where(r => r.UsuarioId == user.Id)
                .OrderByDescending(r => r.FechaCreacion)
                .FirstOrDefaultAsync();

            return View(rutina);
        }

        /// <summary>
        /// Calcula el Índice de Masa Corporal (IMC) según la fórmula estándar de la OMS
        /// </summary>
        /// <param name="pesoKg">Peso en kilogramos</param>
        /// <param name="alturaCm">Altura en centímetros</param>
        /// <returns>IMC calculado con precisión de 2 decimales</returns>
        private double CalcularIMC(double pesoKg, double alturaCm)
        {
            // Validación de entrada
            if (pesoKg <= 0 || alturaCm <= 0)
            {
                throw new ArgumentException("El peso y la altura deben ser valores positivos");
            }

            // Convertir altura de centímetros a metros
            double alturaMetros = alturaCm / 100.0;

            // Fórmula estándar del IMC: peso (kg) / altura² (m²)
            double imc = pesoKg / (alturaMetros * alturaMetros);

            // Redondear a 2 decimales para precisión clínica
            return Math.Round(imc, 2);
        }

        /// <summary>
        /// Clasifica el IMC según los rangos establecidos por la Organización Mundial de la Salud (OMS)
        /// </summary>
        /// <param name="imc">Índice de Masa Corporal calculado</param>
        /// <returns>Clasificación del estado nutricional</returns>
        private string CalcularEstadoIMC(double imc)
        {
            return imc switch
            {
                < 18.5 => "Bajo peso",
                >= 18.5 and < 25.0 => "Normal",
                >= 25.0 and < 30.0 => "Sobrepeso",
                >= 30.0 and < 35.0 => "Obesidad grado I",
                >= 35.0 and < 40.0 => "Obesidad grado II",
                >= 40.0 => "Obesidad grado III",
                _ => "Valor inválido"
            };
        }
    }
}