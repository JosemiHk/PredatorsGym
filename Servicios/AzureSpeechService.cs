using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace PredatorsGym.Servicios
{
    public class AzureSpeechService : IAzureSpeechService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AzureSpeechService> _logger;
        private readonly SpeechConfig _speechConfig;

        public AzureSpeechService(IConfiguration configuration, ILogger<AzureSpeechService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            var subscriptionKey = _configuration["AzureSpeech:SubscriptionKey"];
            var region = _configuration["AzureSpeech:Region"];

            _speechConfig = SpeechConfig.FromSubscription(subscriptionKey, region);
            _speechConfig.SpeechSynthesisVoiceName = _configuration["AzureSpeech:VoiceName"];
            _speechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Audio16Khz32KBitRateMonoMp3);
        }

        public async Task<byte[]> GenerateAudioAsync(string text, string? ssmlFormat = null)
        {
            try
            {
                using var synthesizer = new SpeechSynthesizer(_speechConfig, null);

                SpeechSynthesisResult result;

                if (!string.IsNullOrEmpty(ssmlFormat))
                {
                    result = await synthesizer.SpeakSsmlAsync(ssmlFormat);
                }
                else
                {
                    result = await synthesizer.SpeakTextAsync(text);
                }

                if (result.Reason == ResultReason.SynthesizingAudioCompleted)
                {
                    _logger.LogInformation("Audio generado exitosamente. Tamaño: {Size} bytes", result.AudioData.Length);
                    return result.AudioData;
                }
                else
                {
                    _logger.LogError("Error en síntesis de voz: {Reason}", result.Reason);
                    throw new Exception($"Error en síntesis de voz: {result.Reason}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar audio");
                throw;
            }
        }

        public async Task<byte[]> GenerateExerciseIntroAsync(string exerciseName, int series, int repetitions)
        {
            var ssml = $@"
<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='es-ES'>
    <voice name='{_configuration["AzureSpeech:VoiceName"]}'>
        <prosody rate='medium' pitch='medium'>
            <emphasis level='moderate'>¡Comenzamos con {exerciseName}!</emphasis>
            <break time='500ms'/>
            Vamos a realizar <emphasis level='strong'>{series} series</emphasis> 
            de <emphasis level='strong'>{repetitions} repeticiones</emphasis> cada una.
            <break time='300ms'/>
            <prosody rate='slow'>¡Prepárate y comencemos!</prosody>
        </prosody>
    </voice>
</speak>";

            return await GenerateAudioAsync(exerciseName, ssml);
        }

        public async Task<byte[]> GenerateExerciseClosureAsync(string exerciseName, string nextExercise = null)
        {
            var nextExerciseText = !string.IsNullOrEmpty(nextExercise)
                ? $"<break time='500ms'/>El siguiente ejercicio será <emphasis level='moderate'>{nextExercise}</emphasis>."
                : "<break time='500ms'/>¡Has completado tu rutina de entrenamiento!";

            var ssml = $@"
<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='es-ES'>
    <voice name='{_configuration["AzureSpeech:VoiceName"]}'>
        <prosody rate='medium' pitch='medium'>
            <emphasis level='moderate'>¡Excelente trabajo!</emphasis>
            <break time='300ms'/>
            Has completado <emphasis level='strong'>{exerciseName}</emphasis>.
            {nextExerciseText}
            <break time='500ms'/>
            <prosody rate='slow'>Toma un descanso y prepárate para continuar.</prosody>
        </prosody>
    </voice>
</speak>";

            return await GenerateAudioAsync(exerciseName, ssml);
        }

        public async Task<byte[]> GenerateWorkoutStartAsync(string userName)
        {
            var ssml = $@"
<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='es-ES'>
    <voice name='{_configuration["AzureSpeech:VoiceName"]}'>
        <prosody rate='medium' pitch='high'>
            <emphasis level='strong'>¡Hola {userName}!</emphasis>
            <break time='500ms'/>
            Bienvenido a tu sesión de entrenamiento personalizada.
            <break time='300ms'/>
            <prosody rate='slow'>¡Vamos a darlo todo hoy!</prosody>
        </prosody>
    </voice>
</speak>";

            return await GenerateAudioAsync(userName, ssml);
        }

        public async Task<byte[]> GenerateWorkoutEndAsync()
        {
            var ssml = $@"
<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='es-ES'>
    <voice name='{_configuration["AzureSpeech:VoiceName"]}'>
        <prosody rate='medium' pitch='high'>
            <emphasis level='strong'>¡Felicitaciones!</emphasis>
            <break time='500ms'/>
            Has completado tu rutina de entrenamiento.
            <break time='300ms'/>
            <prosody rate='slow'>¡Excelente trabajo! Nos vemos en la próxima sesión.</prosody>
        </prosody>
    </voice>
</speak>";

            return await GenerateAudioAsync("workout_end", ssml);
        }
    }
}