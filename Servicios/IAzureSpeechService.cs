namespace PredatorsGym.Servicios
{
    public interface IAzureSpeechService
    {
        Task<byte[]> GenerateAudioAsync(string text, string? ssmlFormat = null);
        Task<byte[]> GenerateExerciseIntroAsync(string exerciseName, int series, int repetitions);
        Task<byte[]> GenerateExerciseClosureAsync(string exerciseName, string nextExercise = null);
        Task<byte[]> GenerateWorkoutStartAsync(string userName);
        Task<byte[]> GenerateWorkoutEndAsync();
    }
}