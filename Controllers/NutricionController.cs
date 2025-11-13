using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PredatorsGym.Models;
using PredatorsGym.Servicios;

namespace PredatorsGym.Controllers
{
    [Authorize]
    public class NutricionController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly INutricionConsejosService _consejosService;
        private readonly ILogger<NutricionController> _logger;

        public NutricionController(UserManager<IdentityUser> userManager,
                                   INutricionConsejosService consejosService,
                                   ILogger<NutricionController> logger)
        {
            _userManager = userManager;
            _consejosService = consejosService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Consejos()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("Nutricion/Consejos: usuario no autenticado");
                return RedirectToAction("Login", "Account");
            }

            var resultado = await _consejosService.GenerarConsejosAsync(user.Id);
            return View(resultado);
        }
    }
}