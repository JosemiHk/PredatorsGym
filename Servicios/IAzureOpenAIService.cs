namespace PredatorsGym.Servicios
{
    public interface IAzureOpenAIService
    {
        Task<string> GenerarTextoAsync(string prompt);
        Task<string> GenerarRutinaPersonalizadaAsync(string promptRutina);
        Task<string> GenerarDietaPersonalizadaAsync(string promptDieta);
    }
}