using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PredatorsGym.Models;

namespace PredatorsGym.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<IdentityUser> userManager, 
                                SignInManager<IdentityUser> signInManager,
                                ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegistroViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = model.Telefono
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("Usuario creado exitosamente: {Email}", model.Email);
                
                try
                {
                    // Asignar rol por defecto: UsuarioSinMembresia
                    await _userManager.AddToRoleAsync(user, "UsuarioSinMembresia");
                    _logger.LogInformation("Rol asignado exitosamente a: {Email}", model.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al asignar rol al usuario: {Email}", model.Email);
                    // Continuar con el proceso aunque falle la asignación de rol
                }

                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
                _logger.LogWarning("Error en registro: {Error}", error.Description);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Usuario cerró sesión");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("Usuario inició sesión exitosamente: {Email}", model.Email);
                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("Cuenta bloqueada: {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "Cuenta bloqueada temporalmente.");
                return View(model);
            }

            _logger.LogWarning("Intento de inicio de sesión fallido: {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
            return View(model);
        }
    }
}
