using System.Text;
using System.Text.Json;

namespace PredatorsGym.Servicios
{
    public class AzureOpenAIService : IAzureOpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AzureOpenAIService> _logger;

        public AzureOpenAIService(HttpClient httpClient, IConfiguration configuration, ILogger<AzureOpenAIService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> GenerarTextoAsync(string prompt)
        {
            return await GenerarRutinaPersonalizadaAsync(prompt);
        }

        public async Task<string> GenerarRutinaPersonalizadaAsync(string promptRutina)
        {
            try
            {
                //  Configuración correcta
                var endpoint = _configuration["AzureOpenAI:Endpoint"];
                var apiKey = _configuration["AzureOpenAI:ApiKey"];
                var deploymentName = _configuration["AzureOpenAI:DeploymentName"];
                var apiVersion = _configuration["AzureOpenAI:ApiVersion"];

                //  Validaciones
                if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(deploymentName))
                {
                    _logger.LogError(" Configuración de Azure OpenAI incompleta");
                    return "Error: Configuración de Azure OpenAI incompleta";
                }

                //  Construir URL correcta para Azure Resource
                var url = $"{endpoint.TrimEnd('/')}/openai/deployments/{deploymentName}/chat/completions?api-version={apiVersion}";

                var requestBody = new
                {
                    messages = new[]
                    {
                        new
                        {
                            role = "system",
                            content = @"
Eres un entrenador fitness profesional certificado especializado en crear rutinas de entrenamiento personalizadas. 

CARACTERÍSTICAS DE TUS RESPUESTAS:
- Profesional pero accesible
- Basadas en ciencia del ejercicio
- Adaptadas al nivel del usuario
- Seguras y progresivas
- Incluyen calentamiento y enfriamiento
- Formato estructurado y fácil de seguir

ESTRUCTURA REQUERIDA:
1. Día de la semana
2. Ejercicio principal (nombre, series, repeticiones/tiempo)
3. Calentamiento específico (1 línea)
4. Ejercicios complementarios (2-3)
5. Estiramiento/enfriamiento
6. Recomendación específica del día
7. Emojis apropiados para mejor visualización

LÍMITES:
- Máximo 7 días de rutina
- Respuesta concisa pero completa
- Considera limitaciones y equipamiento disponible
- Adapta intensidad según experiencia"
                        },
                        new
                        {
                            role = "user",
                            content = promptRutina
                        }
                    },
                    max_tokens = 2048,
                    temperature = 0.7,
                    top_p = 0.9,
                    frequency_penalty = 0.0,
                    presence_penalty = 0.0
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                //  Headers correctos para Azure OpenAI Resource
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("api-key", apiKey);

                _logger.LogInformation(" Enviando solicitud a Azure OpenAI Resource: {Url}", url);

                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(" Error de Azure OpenAI: {StatusCode} - {Content}", response.StatusCode, responseContent);
                    return $"Error al generar la rutina. Status: {response.StatusCode}";
                }

                using var document = JsonDocument.Parse(responseContent);
                var resultado = document.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                _logger.LogInformation(" Rutina generada exitosamente. Longitud: {Length} caracteres", resultado?.Length ?? 0);

                return resultado ?? "Error: No se pudo generar la rutina";
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, " Error de conexión HTTP con Azure OpenAI");
                return "Error de conexión. Verifica tu configuración de Azure OpenAI.";
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, " Error al procesar respuesta JSON de Azure OpenAI");
                return "Error al procesar la respuesta. Intenta nuevamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Error inesperado al generar rutina");
                return "Lo siento, hubo un error al generar tu rutina personalizada. Por favor intenta nuevamente.";
            }
        }

        public async Task<string> GenerarDietaPersonalizadaAsync(string promptDieta)
        {
            try
            {
                var endpoint = _configuration["AzureOpenAI:Endpoint"];
                var apiKey = _configuration["AzureOpenAI:ApiKey"];
                var deploymentName = _configuration["AzureOpenAI:DeploymentName"];
                var apiVersion = _configuration["AzureOpenAI:ApiVersion"];

                var url = $"{endpoint.TrimEnd('/')}/openai/deployments/{deploymentName}/chat/completions?api-version={apiVersion}";

                var requestBody = new
                {
                    messages = new[]
                    {
                        new
                        {
                            role = "system",
                            content = @"
Eres un nutricionista deportivo certificado especializado en planes alimenticios para deportistas y personas activas.

CARACTERÍSTICAS DE TUS RESPUESTAS:
- Científicamente fundamentadas
- Adaptadas a objetivos específicos (pérdida de peso, ganancia muscular, mantenimiento)
- Consideran alergias y restricciones alimentarias
- Incluyen timing de nutrientes
- Prácticas y realizables
- Balanceadas en macronutrientes

ESTRUCTURA REQUERIDA:
1. Plan por comidas (desayuno, almuerzo, cena, snacks)
2. Porciones específicas
3. Timing respecto al entrenamiento
4. Hidratación
5. Suplementación básica (si aplica)
6. Tips de preparación
7. Emojis para mejor visualización

LÍMITES:
- Plan semanal completo
- Respuesta estructurada y clara
- Considera presupuesto y disponibilidad de alimentos
- Adapta calorías según objetivos"
                        },
                        new
                        {
                            role = "user",
                            content = promptDieta
                        }
                    },
                    max_tokens = 2048,
                    temperature = 0.6,
                    top_p = 0.85,
                    frequency_penalty = 0.1,
                    presence_penalty = 0.1
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("api-key", apiKey);

                _logger.LogInformation(" Generando dieta personalizada con Azure OpenAI");

                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(" Error de Azure OpenAI: {StatusCode} - {Content}", response.StatusCode, responseContent);
                    return "Error al generar la dieta. Por favor intenta nuevamente.";
                }

                using var document = JsonDocument.Parse(responseContent);
                var resultado = document.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                _logger.LogInformation(" Dieta generada exitosamente. Longitud: {Length} caracteres", resultado?.Length ?? 0);

                return resultado ?? "Error: No se pudo generar la dieta";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Error al generar dieta con Azure OpenAI");
                return "Lo siento, hubo un error al generar tu plan nutricional personalizado. Por favor intenta nuevamente.";
            }
        }

        /// <summary>
        /// Método de prueba para validar la conexión con Azure OpenAI
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var endpoint = _configuration["AzureOpenAI:Endpoint"];
                var apiKey = _configuration["AzureOpenAI:ApiKey"];
                var deploymentName = _configuration["AzureOpenAI:DeploymentName"];
                var apiVersion = _configuration["AzureOpenAI:ApiVersion"];

                var url = $"{endpoint.TrimEnd('/')}/openai/deployments/{deploymentName}/chat/completions?api-version={apiVersion}";

                var testRequest = new
                {
                    messages = new[]
                    {
                        new { role = "system", content = "Responde solo 'OK' si funciona" },
                        new { role = "user", content = "Test" }
                    },
                    max_tokens = 10
                };

                var json = JsonSerializer.Serialize(testRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("api-key", apiKey);

                var response = await _httpClient.PostAsync(url, content);

                _logger.LogInformation(" Test connection - Status: {StatusCode}", response.StatusCode);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Test connection failed");
                return false;
            }
        }
    }
}