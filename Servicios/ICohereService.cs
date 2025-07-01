namespace PredatorsGym.Servicios
{
    public interface ICohereService
    {
        Task<string> GenerarTextoAsync(string prompt);
    }
}
