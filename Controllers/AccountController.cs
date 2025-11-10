using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using PredatorsGym.Models;
using PredatorsGym.Datos;

namespace PredatorsGym.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
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
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError(string.Empty, "Intento de inicio de sesión inválido.");
            }
            return View(model);
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
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Perfil()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            // Buscar perfil existente
            var perfilBD = await _context.PerfilesUsuarios
                .FirstOrDefaultAsync(p => p.UsuarioId == user.Id);

            var model = new PerfilViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FechaRegistro = DateTime.Now
            };

            // Si existe perfil, cargar datos
            if (perfilBD != null)
            {
                model.Nombre = perfilBD.Nombre;
                model.Apellidos = perfilBD.Apellidos;
                model.Telefono = perfilBD.Telefono;
                model.FechaNacimiento = perfilBD.FechaNacimiento;
                model.Genero = perfilBD.Genero;
                model.Altura = perfilBD.Altura;
                model.PesoActual = perfilBD.PesoActual;
                model.PesoObjetivo = perfilBD.PesoObjetivo;
                model.NivelExperiencia = perfilBD.NivelExperiencia;
                model.ObjetivoPrincipal = perfilBD.ObjetivoPrincipal;
                model.DiasEntrenamientoSemana = perfilBD.DiasEntrenamientoSemana;
                model.DuracionPreferida = perfilBD.DuracionPreferida;
                model.LesionesLimitaciones = perfilBD.LesionesLimitaciones;
                model.UltimaActividad = perfilBD.UltimaActividad;
                model.TieneImagenPerfil = perfilBD.ImagenPerfil != null;
            }

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Perfil(PerfilViewModel model)
        {
            // Logging para debugging
            var imageFile = Request.Form.Files["ImagenPerfil"];
            if (imageFile != null)
            {
                Console.WriteLine($"Archivo recibido: {imageFile.FileName}, Tamaño: {imageFile.Length}");
            }
            else
            {
                Console.WriteLine("No se recibió archivo de imagen");
            }

            if (!ModelState.IsValid)
            {
                // Log de errores de validación
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Error de validación: {error.ErrorMessage}");
                }
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            try
            {
                // Buscar perfil existente o crear uno nuevo
                var perfilBD = await _context.PerfilesUsuarios
                    .FirstOrDefaultAsync(p => p.UsuarioId == user.Id);

                if (perfilBD == null)
                {
                    perfilBD = new PerfilUsuario
                    {
                        UsuarioId = user.Id,
                        FechaCreacion = DateTime.Now
                    };
                    _context.PerfilesUsuarios.Add(perfilBD);
                }

                // Actualizar datos del perfil
                perfilBD.Nombre = model.Nombre;
                perfilBD.Apellidos = model.Apellidos;
                perfilBD.Telefono = model.Telefono;
                perfilBD.FechaNacimiento = model.FechaNacimiento;
                perfilBD.Genero = model.Genero;
                perfilBD.Altura = model.Altura;
                perfilBD.PesoActual = model.PesoActual;
                perfilBD.PesoObjetivo = model.PesoObjetivo;
                perfilBD.NivelExperiencia = model.NivelExperiencia;
                perfilBD.ObjetivoPrincipal = model.ObjetivoPrincipal;
                perfilBD.DiasEntrenamientoSemana = model.DiasEntrenamientoSemana;
                perfilBD.DuracionPreferida = model.DuracionPreferida;
                perfilBD.LesionesLimitaciones = model.LesionesLimitaciones;
                perfilBD.FechaActualizacion = DateTime.Now;

                // Manejar imagen de perfil
                if (model.ImagenPerfil != null && model.ImagenPerfil.Length > 0)
                {
                    Console.WriteLine($"Procesando imagen: {model.ImagenPerfil.FileName}");
                    
                    // Validaciones
                    var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
                    var extension = Path.GetExtension(model.ImagenPerfil.FileName).ToLowerInvariant();

                    if (!extensionesPermitidas.Contains(extension))
                    {
                        ModelState.AddModelError("ImagenPerfil", "Solo se permiten archivos de imagen (jpg, jpeg, png, gif, bmp, webp)");
                        return View(model);
                    }

                    if (model.ImagenPerfil.Length > 5 * 1024 * 1024) // 5MB
                    {
                        ModelState.AddModelError("ImagenPerfil", "La imagen no puede ser mayor a 5MB");
                        return View(model);
                    }

                    // Convertir imagen a bytes
                    using (var memoryStream = new MemoryStream())
                    {
                        await model.ImagenPerfil.CopyToAsync(memoryStream);
                        perfilBD.ImagenPerfil = memoryStream.ToArray();
                        perfilBD.TipoImagen = extension.TrimStart('.');
                        Console.WriteLine($"Imagen guardada en BD: {perfilBD.ImagenPerfil.Length} bytes");
                    }
                }

                // Actualizar email si cambió
                if (user.Email != model.Email)
                {
                    user.Email = model.Email;
                    user.UserName = model.Email;
                    await _userManager.UpdateAsync(user);
                }

                await _context.SaveChangesAsync();
                Console.WriteLine("Perfil guardado correctamente en la base de datos");
                
                // Actualizar el modelo para la vista
                model.TieneImagenPerfil = perfilBD.ImagenPerfil != null;
                
                TempData["SuccessMessage"] = "Perfil actualizado correctamente";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar perfil: {ex.Message}");
                ModelState.AddModelError("", "Error al guardar el perfil. Inténtalo de nuevo.");
                return View(model);
            }

            return View(model);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ImagenPerfil()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var perfil = await _context.PerfilesUsuarios
                .FirstOrDefaultAsync(p => p.UsuarioId == user.Id);

            if (perfil?.ImagenPerfil == null)
            {
                return NotFound();
            }

            var contentType = perfil.TipoImagen switch
            {
                "jpg" or "jpeg" => "image/jpeg",
                "png" => "image/png",
                "gif" => "image/gif",
                "bmp" => "image/bmp",
                "webp" => "image/webp",
                _ => "image/jpeg"
            };

            return File(perfil.ImagenPerfil, contentType);
        }
    }
}