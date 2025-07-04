using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PredatorsGym.Servicios;

namespace PredatorsGym.Controllers.Api
{
    [Authorize]
    [ApiController]
    [Route("api/workout")]
    public class WorkoutApiController : ControllerBase
    {
        private readonly IWorkoutService _workoutService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<WorkoutApiController> _logger;

        public WorkoutApiController(
            IWorkoutService workoutService,
            UserManager<IdentityUser> userManager,
            ILogger<WorkoutApiController> logger)
        {
            _workoutService = workoutService;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpPost("start/{routineId}")]
        public async Task<IActionResult> StartWorkout(int routineId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }

                var result = await _workoutService.StartWorkoutAsync(routineId, user.Id);

                if (result)
                {
                    return Ok(new { message = "Entrenamiento iniciado exitosamente", routineId });
                }
                else
                {
                    return BadRequest(new { message = "No se pudo iniciar el entrenamiento" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al iniciar entrenamiento");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost("pause/{routineId}")]
        public async Task<IActionResult> PauseWorkout(int routineId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var result = await _workoutService.PauseWorkoutAsync(routineId, user.Id);

                return result ? Ok() : BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al pausar entrenamiento");
                return StatusCode(500);
            }
        }

        [HttpPost("resume/{routineId}")]
        public async Task<IActionResult> ResumeWorkout(int routineId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var result = await _workoutService.ResumeWorkoutAsync(routineId, user.Id);

                return result ? Ok() : BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reanudar entrenamiento");
                return StatusCode(500);
            }
        }

        [HttpPost("complete/{routineId}")]
        public async Task<IActionResult> CompleteWorkout(int routineId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var result = await _workoutService.CompleteWorkoutAsync(routineId, user.Id);

                return result ? Ok() : BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al completar entrenamiento");
                return StatusCode(500);
            }
        }

        [HttpGet("status/{routineId}")]
        public async Task<IActionResult> GetWorkoutStatus(int routineId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var rutina = await _workoutService.GetWorkoutStatusAsync(routineId, user.Id);

                if (rutina == null)
                {
                    return NotFound();
                }

                return Ok(new
                {
                    id = rutina.Id,
                    estado = rutina.Estado.ToString(),
                    fechaInicio = rutina.FechaInicioEntrenamiento,
                    ejercicios = rutina.Ejercicios.Select(e => new
                    {
                        id = e.Id,
                        nombre = e.Nombre,
                        series = e.Series,
                        repeticiones = e.Repeticiones,
                        orden = e.Orden
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estado del entrenamiento");
                return StatusCode(500);
            }
        }
    }
}