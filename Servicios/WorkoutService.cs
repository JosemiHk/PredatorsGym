using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PredatorsGym.Datos;
using PredatorsGym.Hubs;
using PredatorsGym.Models;
using System.Text.RegularExpressions;

namespace PredatorsGym.Servicios
{
    public class WorkoutService : IWorkoutService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<WorkoutHub> _hubContext;
        private readonly IAzureSpeechService _speechService;
        private readonly ILogger<WorkoutService> _logger;

        public WorkoutService(
            ApplicationDbContext context,
            IHubContext<WorkoutHub> hubContext,
            IAzureSpeechService speechService,
            ILogger<WorkoutService> logger)
        {
            _context = context;
            _hubContext = hubContext;
            _speechService = speechService;
            _logger = logger;
        }

        public async Task<bool> StartWorkoutAsync(int routineId, string userId)
        {
            try
            {
                var rutina = await _context.Rutinas
                    .Include(r => r.Ejercicios.OrderBy(e => e.Orden))
                    .FirstOrDefaultAsync(r => r.Id == routineId && r.UsuarioId == userId);

                if (rutina == null)
                {
                    _logger.LogWarning("Rutina no encontrada: {RutinaId} para usuario {UserId}", routineId, userId);
                    return false;
                }

                // Si no tiene ejercicios, parsear la rutina generada
                if (!rutina.Ejercicios.Any())
                {
                    await ParseAndCreateExercisesAsync(routineId, rutina.RutinaGenerada);

                    // Recargar con ejercicios
                    rutina = await _context.Rutinas
                        .Include(r => r.Ejercicios.OrderBy(e => e.Orden))
                        .FirstOrDefaultAsync(r => r.Id == routineId);
                }

                // Actualizar estado
                rutina.Estado = EstadoRutina.EnProgreso;
                rutina.FechaInicioEntrenamiento = DateTime.Now;
                await _context.SaveChangesAsync();

                // Iniciar sesión de entrenamiento
                _ = Task.Run(() => ExecuteWorkoutSessionAsync(rutina, userId));

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al iniciar entrenamiento");
                return false;
            }
        }

        private async Task ExecuteWorkoutSessionAsync(Rutina rutina, string userId)
        {
            var groupName = $"workout_{rutina.Id}";

            try
            {
                // Audio de inicio
                var startAudio = await _speechService.GenerateWorkoutStartAsync("atleta");
                await _hubContext.Clients.Group(groupName).SendAsync("ReceiveAudio", Convert.ToBase64String(startAudio));

                await Task.Delay(3000); // Pausa inicial

                // Ejecutar cada ejercicio
                for (int i = 0; i < rutina.Ejercicios.Count; i++)
                {
                    var ejercicio = rutina.Ejercicios.ElementAt(i);
                    var nextEjercicio = i + 1 < rutina.Ejercicios.Count ? rutina.Ejercicios.ElementAt(i + 1).Nombre : null;

                    // Audio de introducción del ejercicio
                    var introAudio = await _speechService.GenerateExerciseIntroAsync(
                        ejercicio.Nombre, ejercicio.Series, ejercicio.Repeticiones);

                    await _hubContext.Clients.Group(groupName).SendAsync("ReceiveAudio", Convert.ToBase64String(introAudio));
                    await _hubContext.Clients.Group(groupName).SendAsync("ReceiveExerciseStart", new
                    {
                        ExerciseId = ejercicio.Id,
                        Name = ejercicio.Nombre,
                        Series = ejercicio.Series,
                        Repetitions = ejercicio.Repeticiones,
                        Duration = ejercicio.DuracionEstimada
                    });

                    // Esperar duración del ejercicio
                    await Task.Delay(ejercicio.DuracionEstimada * 1000);

                    // Audio de cierre del ejercicio
                    var closureAudio = await _speechService.GenerateExerciseClosureAsync(ejercicio.Nombre, nextEjercicio);
                    await _hubContext.Clients.Group(groupName).SendAsync("ReceiveAudio", Convert.ToBase64String(closureAudio));

                    await _hubContext.Clients.Group(groupName).SendAsync("ReceiveExerciseEnd", new
                    {
                        ExerciseId = ejercicio.Id,
                        Completed = true
                    });

                    // Tiempo de descanso
                    if (i < rutina.Ejercicios.Count - 1) // No descanso después del último ejercicio
                    {
                        await _hubContext.Clients.Group(groupName).SendAsync("ReceiveRestTime", new
                        {
                            Duration = ejercicio.TiempoDescanso
                        });

                        await Task.Delay(ejercicio.TiempoDescanso * 1000);
                    }
                }

                // Audio de finalización
                var endAudio = await _speechService.GenerateWorkoutEndAsync();
                await _hubContext.Clients.Group(groupName).SendAsync("ReceiveAudio", Convert.ToBase64String(endAudio));

                // Completar rutina
                await CompleteWorkoutAsync(rutina.Id, userId);

                await _hubContext.Clients.Group(groupName).SendAsync("ReceiveWorkoutCompleted", new
                {
                    Message = "¡Entrenamiento completado exitosamente!",
                    Duration = (DateTime.Now - rutina.FechaInicioEntrenamiento.Value).TotalMinutes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la sesión de entrenamiento");
                await _hubContext.Clients.Group(groupName).SendAsync("ReceiveError", "Error durante el entrenamiento");
            }
        }

        public async Task<List<Ejercicio>> ParseAndCreateExercisesAsync(int routineId, string rutinaGenerada)
        {
            var ejercicios = new List<Ejercicio>();

            try
            {
                // Parsing simple - esto se puede mejorar con IA o regex más complejos
                var lines = rutinaGenerada.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                int orden = 1;

                foreach (var line in lines)
                {
                    // Buscar patrones como "Ejercicio:", "Series:", "Repeticiones:"
                    if (line.ToLower().Contains("ejercicio") && !line.ToLower().Contains("calentamiento"))
                    {
                        var ejercicio = new Ejercicio
                        {
                            RutinaId = routineId,
                            Nombre = ExtractExerciseName(line),
                            Descripcion = line.Trim(),
                            Series = ExtractSeries(rutinaGenerada, line),
                            Repeticiones = ExtractRepetitions(rutinaGenerada, line),
                            TiempoDescanso = 60, // Valor por defecto
                            DuracionEstimada = 180, // 3 minutos por defecto
                            Orden = orden++,
                            FechaCreacion = DateTime.Now
                        };

                        ejercicios.Add(ejercicio);
                    }
                }

                if (ejercicios.Any())
                {
                    _context.Ejercicios.AddRange(ejercicios);
                    await _context.SaveChangesAsync();
                }

                return ejercicios;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al parsear ejercicios");
                return ejercicios;
            }
        }

        private string ExtractExerciseName(string line)
        {
            // Extraer nombre del ejercicio de la línea
            var match = Regex.Match(line, @"ejercicio[:\s]*([^,\n\r]+)", RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value.Trim() : "Ejercicio sin nombre";
        }

        private int ExtractSeries(string fullText, string exerciseLine)
        {
            var match = Regex.Match(fullText, @"(\d+)\s*series?", RegexOptions.IgnoreCase);
            return match.Success ? int.Parse(match.Groups[1].Value) : 3;
        }

        private int ExtractRepetitions(string fullText, string exerciseLine)
        {
            var match = Regex.Match(fullText, @"(\d+)\s*repeticion", RegexOptions.IgnoreCase);
            return match.Success ? int.Parse(match.Groups[1].Value) : 12;
        }

        public async Task<bool> PauseWorkoutAsync(int routineId, string userId)
        {
            var rutina = await _context.Rutinas.FirstOrDefaultAsync(r => r.Id == routineId && r.UsuarioId == userId);
            if (rutina == null) return false;

            rutina.Estado = EstadoRutina.Pausada;
            await _context.SaveChangesAsync();

            var groupName = $"workout_{routineId}";
            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveWorkoutPaused");

            return true;
        }

        public async Task<bool> ResumeWorkoutAsync(int routineId, string userId)
        {
            var rutina = await _context.Rutinas.FirstOrDefaultAsync(r => r.Id == routineId && r.UsuarioId == userId);
            if (rutina == null) return false;

            rutina.Estado = EstadoRutina.EnProgreso;
            await _context.SaveChangesAsync();

            var groupName = $"workout_{routineId}";
            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveWorkoutResumed");

            return true;
        }

        public async Task<bool> CompleteWorkoutAsync(int routineId, string userId)
        {
            var rutina = await _context.Rutinas.FirstOrDefaultAsync(r => r.Id == routineId && r.UsuarioId == userId);
            if (rutina == null) return false;

            rutina.Estado = EstadoRutina.Completada;
            rutina.FechaFinEntrenamiento = DateTime.Now;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<Rutina?> GetWorkoutStatusAsync(int routineId, string userId)
        {
            return await _context.Rutinas
                .Include(r => r.Ejercicios)
                .FirstOrDefaultAsync(r => r.Id == routineId && r.UsuarioId == userId);
        }
    }
}