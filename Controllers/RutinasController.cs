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
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var perfil = await _context.PerfilesUsuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UsuarioId == user.Id);

            var model = new Rutina
            {
                UsuarioId = user.Id
            };

            if (perfil != null)
            {
                // Prefill desde perfil
                if (!string.IsNullOrWhiteSpace(perfil.Genero)) model.Genero = perfil.Genero;
                var edad = CalcularEdad(perfil.FechaNacimiento);
                if (edad.HasValue) model.Edad = edad.Value;

                if (perfil.Altura.HasValue) model.Altura = (double)perfil.Altura.Value;
                if (perfil.PesoActual.HasValue) model.Peso = (double)perfil.PesoActual.Value;
                if (perfil.PesoObjetivo.HasValue) model.PesoObjetivo = (double)perfil.PesoObjetivo.Value;

                if (!string.IsNullOrWhiteSpace(perfil.NivelExperiencia)) model.Experiencia = perfil.NivelExperiencia;
                if (!string.IsNullOrWhiteSpace(perfil.ObjetivoPrincipal)) model.Objetivo = perfil.ObjetivoPrincipal;

                if (perfil.DiasEntrenamientoSemana.HasValue)
                    model.DiasEntrenamiento = perfil.DiasEntrenamientoSemana.Value.ToString();

                // Opcionales/por defecto (si no existe en perfil)
                if (string.IsNullOrWhiteSpace(model.LugarEntrenamiento))
                    model.LugarEntrenamiento = "Casa";
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerarRutina(Rutina model)
        {
            // Quitar validación innecesaria (se calculan/llenan en backend)
            ModelState.Remove("UsuarioId");
            ModelState.Remove("IMC");
            ModelState.Remove("EstadoIMC");
            ModelState.Remove("RutinaGenerada");
            ModelState.Remove("FechaCreacion");

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogError("Usuario no encontrado en GenerarRutina");
                    return RedirectToAction("Login", "Account");
                }

                // Completar datos faltantes desde el perfil ANTES de validar
                await CompletarDesdePerfilAsync(model, user.Id);

                // Revalidar el modelo tras completar datos
                ModelState.Clear();
                TryValidateModel(model);

                if (!ModelState.IsValid)
                {
                    return View("Create", model);
                }

                model.UsuarioId = user.Id;
                model.FechaCreacion = DateTime.Now;

                // Cálculo del IMC y estado
                model.IMC = CalcularIMC(model.Peso, model.Altura);
                model.EstadoIMC = CalcularEstadoIMC(model.IMC);

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

                // Llamada a Azure OpenAI
                model.RutinaGenerada = await _azureOpenAIService.GenerarRutinaPersonalizadaAsync(prompt);

                if (string.IsNullOrEmpty(model.RutinaGenerada))
                {
                    _logger.LogError("RutinaGenerada está vacía después de llamada a Azure OpenAI");
                    ModelState.AddModelError("", "No se pudo generar la rutina. Intenta nuevamente.");
                    return View("Create", model);
                }

                _context.Rutinas.Add(model);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Rutina generada y guardada exitosamente para usuario {UserId}. ID: {RutinaId}", user.Id, model.Id);

                return RedirectToAction("RutinaGenerada", new { id = model.Id });
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
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogError("Usuario no encontrado en RutinaGenerada");
                    return RedirectToAction("Login", "Account");
                }

                var rutina = await _context.Rutinas
                    .Include(r => r.Ejercicios)
                    .FirstOrDefaultAsync(r => r.Id == id && r.UsuarioId == user.Id);

                if (rutina == null)
                {
                    _logger.LogWarning("Rutina no encontrada. ID: {RutinaId}, Usuario: {UserId}", id, user.Id);
                    return NotFound();
                }

                _logger.LogInformation("Mostrando rutina {RutinaId} para usuario {UserId}. Contenido: {Length} caracteres",
                    rutina.Id, user.Id, rutina.RutinaGenerada?.Length ?? 0);

                return View(rutina);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al mostrar rutina {RutinaId}", id);
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var rutina = await _context.Rutinas
                    .Where(r => r.UsuarioId == user.Id)
                    .OrderByDescending(r => r.FechaCreacion)
                    .FirstOrDefaultAsync();

                _logger.LogInformation("Index: Usuario {UserId}, Rutina encontrada: {HasRutina}",
                    user.Id, rutina != null);

                return View(rutina);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Index de rutinas");
                return View((Rutina?)null);
            }
        }

        // Completa campos faltantes del modelo con los valores del perfil del usuario
        private async Task CompletarDesdePerfilAsync(Rutina model, string userId)
        {
            var perfil = await _context.PerfilesUsuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UsuarioId == userId);

            if (perfil == null) return;

            if (string.IsNullOrWhiteSpace(model.Genero) && !string.IsNullOrWhiteSpace(perfil.Genero))
                model.Genero = perfil.Genero;

            if (model.Edad <= 0)
            {
                var edad = CalcularEdad(perfil.FechaNacimiento);
                if (edad.HasValue) model.Edad = edad.Value;
            }

            if (model.Altura <= 0 && perfil.Altura.HasValue)
                model.Altura = (double)perfil.Altura.Value;

            if (model.Peso <= 0 && perfil.PesoActual.HasValue)
                model.Peso = (double)perfil.PesoActual.Value;

            if (model.PesoObjetivo <= 0 && perfil.PesoObjetivo.HasValue)
                model.PesoObjetivo = (double)perfil.PesoObjetivo.Value;

            if (string.IsNullOrWhiteSpace(model.Experiencia) && !string.IsNullOrWhiteSpace(perfil.NivelExperiencia))
                model.Experiencia = perfil.NivelExperiencia;

            if (string.IsNullOrWhiteSpace(model.Objetivo) && !string.IsNullOrWhiteSpace(perfil.ObjetivoPrincipal))
                model.Objetivo = perfil.ObjetivoPrincipal;

            if (string.IsNullOrWhiteSpace(model.DiasEntrenamiento) && perfil.DiasEntrenamientoSemana.HasValue)
                model.DiasEntrenamiento = perfil.DiasEntrenamientoSemana.Value.ToString();

            if (model.DuracionTotalMinutos <= 0 && perfil.DuracionPreferida.HasValue)
                model.DuracionTotalMinutos = perfil.DuracionPreferida.Value;

            if (string.IsNullOrWhiteSpace(model.LugarEntrenamiento))
                model.LugarEntrenamiento = "Casa"; // por defecto si no existe en perfil

            // TieneImplementosBasicos: si lo agregas al perfil, mapea aquí.
        }

        private static int? CalcularEdad(DateTime? fechaNacimiento)
        {
            if (!fechaNacimiento.HasValue) return null;
            var hoy = DateTime.Today;
            var edad = hoy.Year - fechaNacimiento.Value.Year;
            if (fechaNacimiento.Value.Date > hoy.AddYears(-edad)) edad--;
            return edad;
        }

        private double CalcularIMC(double pesoKg, double alturaCm)
        {
            if (pesoKg <= 0 || alturaCm <= 0)
            {
                throw new ArgumentException("El peso y la altura deben ser valores positivos");
            }

            double alturaMetros = alturaCm / 100.0;
            double imc = pesoKg / (alturaMetros * alturaMetros);
            return Math.Round(imc, 2);
        }

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