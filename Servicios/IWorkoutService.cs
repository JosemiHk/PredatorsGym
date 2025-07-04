using PredatorsGym.Models;

namespace PredatorsGym.Servicios
{
    public interface IWorkoutService
    {
        Task<bool> StartWorkoutAsync(int routineId, string userId);
        Task<bool> PauseWorkoutAsync(int routineId, string userId);
        Task<bool> ResumeWorkoutAsync(int routineId, string userId);
        Task<bool> CompleteWorkoutAsync(int routineId, string userId);
        Task<Rutina?> GetWorkoutStatusAsync(int routineId, string userId);
        Task<List<Ejercicio>> ParseAndCreateExercisesAsync(int routineId, string rutinaGenerada);
    }
}