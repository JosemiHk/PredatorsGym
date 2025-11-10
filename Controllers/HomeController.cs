using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PredatorsGym.Models;

namespace PredatorsGym.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult PreguntasFrecuentes()
        {
            var faqs = ObtenerPreguntasFrecuentes();
            return View(faqs);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private List<CategoriaFAQ> ObtenerPreguntasFrecuentes()
        {
            return new List<CategoriaFAQ>
            {
                new CategoriaFAQ
                {
                    Nombre = "Cuenta y Perfil",
                    Icono = "bi-person-circle",
                    Color = "primary",
                    Preguntas = new List<PreguntaFAQ>
                    {
                        new PreguntaFAQ
                        {
                            Pregunta = "¿Cómo creo una cuenta en Predator's?",
                            Respuesta = "Para crear una cuenta, haz clic en 'Comenzar' en la página principal o en 'Registrarse' en el menú. Solo necesitas tu email y una contraseña segura. El proceso es completamente gratuito y toma menos de 2 minutos."
                        },
                        new PreguntaFAQ
                        {
                            Pregunta = "¿Cómo actualizo mi información de perfil?",
                            Respuesta = "Ve a tu perfil haciendo clic en tu avatar en el menú superior, luego selecciona 'Perfil'. Allí podrás actualizar toda tu información personal, objetivos fitness, y preferencias de entrenamiento."
                        },
                        new PreguntaFAQ
                        {
                            Pregunta = "¿Puedo cambiar mi foto de perfil?",
                            Respuesta = "Sí, en la sección de perfil puedes subir una foto personalizada. Acepta formatos JPG, PNG, GIF, BMP y WEBP con un tamaño máximo de 5MB."
                        },
                        new PreguntaFAQ
                        {
                            Pregunta = "¿Cómo elimino mi cuenta?",
                            Respuesta = "Para eliminar tu cuenta, contáctanos a través del formulario de contacto o envía un email a soporte@predatorsgym.com. Procesaremos tu solicitud en un plazo de 48 horas."
                        }
                    }
                },
                new CategoriaFAQ
                {
                    Nombre = "Rutinas y Entrenamientos",
                    Icono = "bi-lightning-charge",
                    Color = "success",
                    Preguntas = new List<PreguntaFAQ>
                    {
                        new PreguntaFAQ
                        {
                            Pregunta = "¿Cómo funciona la IA para crear rutinas?",
                            Respuesta = "Nuestra IA analiza tu perfil, objetivos, nivel de experiencia y preferencias para crear rutinas personalizadas. Utiliza algoritmos avanzados y bases de datos de ejercicios para optimizar cada entrenamiento según tus necesidades específicas."
                        },
                        new PreguntaFAQ
                        {
                            Pregunta = "¿Puedo modificar las rutinas generadas?",
                            Respuesta = "Absolutamente. Las rutinas generadas son sugerencias basadas en tu perfil. Puedes adaptarlas según tus preferencias, lesiones o limitaciones específicas."
                        },
                        new PreguntaFAQ
                        {
                            Pregunta = "¿Qué pasa si no tengo experiencia en el gimnasio?",
                            Respuesta = "¡Perfecto! Predator's está diseñado para todos los niveles. La IA creará rutinas de principiante con ejercicios básicos, instrucciones detalladas y progresión gradual para que aprendas de forma segura."
                        },
                        new PreguntaFAQ
                        {
                            Pregunta = "¿Con qué frecuencia debo entrenar?",
                            Respuesta = "Depende de tus objetivos y nivel. Recomendamos 3-4 días por semana para principiantes, 4-5 días para intermedios y 5-6 días para avanzados. La IA ajustará la frecuencia según tu perfil."
                        },
                        new PreguntaFAQ
                        {
                            Pregunta = "¿Necesito equipamiento especial?",
                            Respuesta = "No necesariamente. Puedes especificar en tu perfil qué equipamiento tienes disponible (gimnasio completo, casa, solo peso corporal) y la IA adaptará las rutinas en consecuencia."
                        }
                    }
                },
                new CategoriaFAQ
                {
                    Nombre = "Membresías y Pagos",
                    Icono = "bi-gem",
                    Color = "warning",
                    Preguntas = new List<PreguntaFAQ>
                    {
                        new PreguntaFAQ
                        {
                            Pregunta = "¿Cuál es la diferencia entre los planes?",
                            Respuesta = "Plan Básico: Rutinas básicas con IA y seguimiento simple. Plan Premium: Rutinas avanzadas, planes de nutrición, análisis detallado y soporte prioritario. Plan Elite: Todo lo anterior más entrenador personal, nutricionista y soporte VIP."
                        },
                        new PreguntaFAQ
                        {
                            Pregunta = "¿Puedo cambiar de plan en cualquier momento?",
                            Respuesta = "Sí, puedes actualizar o degradar tu plan en cualquier momento. Los cambios se aplican inmediatamente y se ajusta la facturación proporcionalmente."
                        },
                        new PreguntaFAQ
                        {
                            Pregunta = "¿Hay descuentos por pago anual?",
                            Respuesta = "Sí, ofrecemos un 20% de descuento en todos los planes cuando pagas anualmente. Es una excelente forma de ahorrar mientras te comprometes con tu transformación."
                        },
                        new PreguntaFAQ
                        {
                            Pregunta = "¿Puedo cancelar mi suscripción?",
                            Respuesta = "Por supuesto. Puedes cancelar en cualquier momento desde tu perfil. Tu acceso continuará hasta el final del período pagado, sin cargos adicionales."
                        },
                        new PreguntaFAQ
                        {
                            Pregunta = "¿Qué métodos de pago aceptan?",
                            Respuesta = "Aceptamos todas las tarjetas de crédito principales (Visa, MasterCard, American Express), PayPal y transferencias bancarias. Todos los pagos son seguros y encriptados."
                        }
                    }
                },
                new CategoriaFAQ
                {
                    Nombre = "Nutrición y Dietas",
                    Icono = "bi-heart-pulse",
                    Color = "info",
                    Preguntas = new List<PreguntaFAQ>
                    {
                        new PreguntaFAQ
                        {
                            Pregunta = "¿Incluye planes de alimentación?",
                            Respuesta = "Sí, los planes Premium y Elite incluyen planes de nutrición personalizados. La IA considera tus objetivos, restricciones alimentarias, preferencias y metabolismo para crear planes efectivos."
                        },
                        new PreguntaFAQ
                        {
                            Pregunta = "¿Puedo especificar alergias o restricciones?",
                            Respuesta = "Absolutamente. En tu perfil puedes indicar alergias, intolerancias, preferencias dietéticas (vegetariano, vegano, keto, etc.) y la IA adaptará todas las recomendaciones nutricionales."
                        },
                        new PreguntaFAQ
                        {
                            Pregunta = "¿Los planes incluyen recetas?",
                            Respuesta = "Sí, los planes Premium y Elite incluyen recetas detalladas, listas de compras y guías de preparación para hacer más fácil seguir tu plan nutricional."
                        },
                        new PreguntaFAQ
                        {
                            Pregunta = "¿Cómo calculo mis macronutrientes?",
                            Respuesta = "La IA calcula automáticamente tus macros (proteínas, carbohidratos, grasas) según tu peso, altura, edad, nivel de actividad y objetivos. También incluye una calculadora manual si prefieres ajustarlos."
                        }
                    }
                },
                new CategoriaFAQ
                {
                    Nombre = "Soporte Técnico",
                    Icono = "bi-gear",
                    Color = "secondary",
                    Preguntas = new List<PreguntaFAQ>
                    {
                        new PreguntaFAQ
                        {
                            Pregunta = "¿Qué navegadores son compatibles?",
                            Respuesta = "Predator's funciona en todos los navegadores modernos: Chrome, Firefox, Safari, Edge. Recomendamos mantener tu navegador actualizado para la mejor experiencia."
                        },
                        new PreguntaFAQ
                        {
                            Pregunta = "¿Hay una aplicación móvil?",
                            Respuesta = "Actualmente Predator's es una aplicación web optimizada para móviles. Funciona perfectamente en cualquier dispositivo. Una app nativa está en desarrollo y llegará pronto."
                        },
                        new PreguntaFAQ
                        {
                            Pregunta = "¿Puedo usar Predator's sin internet?",
                            Respuesta = "Necesitas conexión a internet para generar nuevas rutinas y sincronizar datos. Sin embargo, puedes consultar rutinas previamente cargadas en modo offline limitado."
                        },
                        new PreguntaFAQ
                        {
                            Pregunta = "¿Cómo reporto un error o problema?",
                            Respuesta = "Puedes reportar problemas a través del formulario de contacto, enviando un email a soporte@predatorsgym.com, o usando el chat de soporte disponible para usuarios Premium y Elite."
                        }
                    }
                },
                new CategoriaFAQ
                {
                    Nombre = "Seguridad y Privacidad",
                    Icono = "bi-shield-check",
                    Color = "danger",
                    Preguntas = new List<PreguntaFAQ>
                    {
                        new PreguntaFAQ
                        {
                            Pregunta = "¿Mis datos están seguros?",
                            Respuesta = "Sí, utilizamos encriptación de grado bancario (SSL/TLS) para proteger todos los datos. Nunca compartimos información personal con terceros sin tu consentimiento explícito."
                        },
                        new PreguntaFAQ
                        {
                            Pregunta = "¿Qué información recopilan?",
                            Respuesta = "Recopilamos solo la información necesaria para crear rutinas personalizadas: datos físicos, objetivos, preferencias de entrenamiento. Puedes ver todos los datos almacenados en tu perfil."
                        },
                        new PreguntaFAQ
                        {
                            Pregunta = "¿Puedo descargar mis datos?",
                            Respuesta = "Sí, cumplimos con GDPR. Puedes solicitar una copia completa de todos tus datos almacenados contactando nuestro equipo de soporte."
                        },
                        new PreguntaFAQ
                        {
                            Pregunta = "¿Cómo cambio mi contraseña?",
                            Respuesta = "Ve a tu perfil > Configuración > Seguridad. Allí podrás cambiar tu contraseña. Recomendamos usar contraseñas fuertes con al menos 8 caracteres, números y símbolos."
                        }
                    }
                }
            };
        }
    }

    public class CategoriaFAQ
    {
        public string Nombre { get; set; } = string.Empty;
        public string Icono { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public List<PreguntaFAQ> Preguntas { get; set; } = new();
    }

    public class PreguntaFAQ
    {
        public string Pregunta { get; set; } = string.Empty;
        public string Respuesta { get; set; } = string.Empty;
    }
}
