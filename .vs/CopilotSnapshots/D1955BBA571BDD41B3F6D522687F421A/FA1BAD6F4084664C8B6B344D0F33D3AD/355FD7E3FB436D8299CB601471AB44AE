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
                    await _userManager.AddToRoleAsync(user, "UsuarioSinMembresia");
                    _logger.LogInformation("Rol asignado exitosamente a: {Email}", model.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al asignar rol al usuario: {Email}", model.Email);
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

        // ✅ MÉTODO LOGIN GET
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // ✅ MÉTODO LOGIN POST COMPLETO
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Modelo de login inválido para: {Email}", model.Email);
                return View(model);
            }

            try
            {
                _logger.LogInformation("Intento de inicio de sesión para: {Email}", model.Email);

                // ✅ BUSCAR USUARIO POR EMAIL
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    _logger.LogWarning("Usuario no encontrado: {Email}", model.Email);
                    ModelState.AddModelError("", "Email o contraseña incorrectos.");
                    return View(model);
                }

                // ✅ VERIFICAR CONTRASEÑA E INICIAR SESIÓN
                var result = await _signInManager.PasswordSignInAsync(
                    user.UserName,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Inicio de sesión exitoso para: {Email}", model.Email);
                    return RedirectToAction("Index", "Home");
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("Cuenta bloqueada para: {Email}", model.Email);
                    ModelState.AddModelError("", "Cuenta bloqueada. Intenta más tarde.");
                    return View(model);
                }

                if (result.RequiresTwoFactor)
                {
                    _logger.LogInformation("Se requiere 2FA para: {Email}", model.Email);
                    // Implementar 2FA si es necesario
                    ModelState.AddModelError("", "Se requiere autenticación de dos factores.");
                    return View(model);
                }

                // ✅ FALLO DE AUTENTICACIÓN
                _logger.LogWarning("Intento de inicio de sesión fallido: {Email}", model.Email);
                ModelState.AddModelError("", "Email o contraseña incorrectos.");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el inicio de sesión para: {Email}", model.Email);
                ModelState.AddModelError("", "Error interno. Intenta nuevamente.");
                return View(model);
            }
        }

        // ✅ LOGOUT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Usuario cerró sesión");
            return RedirectToAction("Index", "Home");
        }

        // ✅ ACCESS DENIED
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}