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
        private readonly ICohereService _cohereService;

        public RutinasController(ApplicationDbContext context,
                                 UserManager<IdentityUser> userManager,
                                 ICohereService cohereService)
        {
            _context = context;
            _userManager = userManager;
            _cohereService = cohereService;
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

            var user = await _userManager.GetUserAsync(User);
            model.UsuarioId = user.Id;
            model.FechaCreacion = DateTime.Now;
            model.IMC = model.Peso / (model.Altura * model.Altura);
            model.EstadoIMC = CalcularEstadoIMC(model.IMC);

            var prompt = $@"
Eres un entrenador personal profesional. Crea una rutina de entrenamiento en español para una persona con las siguientes características:

- Edad: {model.Edad}, Género: {model.Genero}, Experiencia: {model.Experiencia}, Objetivo: {model.Objetivo}, Peso actual: {model.Peso}kg, Peso objetivo: {model.PesoObjetivo}kg, Altura: {model.Altura}m, Lugar: {model.LugarEntrenamiento}, Tiene implementos básicos: {(model.TieneImplementosBasicos ? "Sí" : "No")}

Crea una rutina semanal (7 días) pero resume cada día con:

1. Nombre del día
2. 1 ejercicio principal (nombre, series, repeticiones)
3. Breve calentamiento (máx 1 línea)
4. Estiramiento
5. Una sola recomendación
6. Usa emojis si caben

No expliques ni introduzcas demasiado. Solo rutina. Hazlo breve para ahorrar espacio. No te pases de 2048 tokens.
";

            model.RutinaGenerada = await _cohereService.GenerarTextoAsync(prompt);

            _context.Rutinas.Add(model);
            await _context.SaveChangesAsync();

            return View("RutinaGenerada", model);
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

        private string CalcularEstadoIMC(double imc)
        {
            return imc < 18.5 ? "Bajo peso" :
                   imc < 25 ? "Normal" :
                   imc < 30 ? "Sobrepeso" : "Obesidad";
        }
    }
}
