using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EvalSystem.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EvalSystem.Infrastructure.Services;

public class OpenAiAnalisisService : IAnalisisIAService
{
    private readonly HttpClient _httpClient;
    private readonly string _model;
    private readonly ILogger<OpenAiAnalisisService> _logger;

    public OpenAiAnalisisService(HttpClient httpClient, IConfiguration configuration, ILogger<OpenAiAnalisisService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        var apiKey = configuration["OpenAI:ApiKey"];
        _model = configuration["OpenAI:Model"] ?? "gpt-4o-mini";

        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }

        var baseUrl = configuration["OpenAI:BaseUrl"] ?? "https://api.openai.com/v1/";
        if (!baseUrl.EndsWith('/')) baseUrl += "/";
        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    public async Task<string> GenerarAnalisisAsync(AnalisisContexto contexto)
    {
        var apiKey = _httpClient.DefaultRequestHeaders.Authorization?.Parameter;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("OpenAI API Key no configurada. Usando análisis por reglas.");
            return GenerarAnalisisPorReglas(contexto);
        }

        try
        {
            var prompt = ConstruirPrompt(contexto);

            var request = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "system", content = "Eres un analista experto en evaluación de talento tecnológico. Genera análisis profesionales, concisos y accionables para reclutadores." },
                    new { role = "user", content = prompt }
                },
                max_tokens = 1500,
                temperature = 0.7
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("chat/completions", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("OpenAI API error {StatusCode}: {Body}", response.StatusCode, errorBody);
                return GenerarAnalisisPorReglas(contexto);
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<OpenAiResponse>(responseBody);

            var analisis = result?.Choices?.FirstOrDefault()?.Message?.Content;
            if (string.IsNullOrWhiteSpace(analisis))
            {
                _logger.LogWarning("OpenAI devolvió respuesta vacía. Usando análisis por reglas.");
                return GenerarAnalisisPorReglas(contexto);
            }

            return analisis.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al llamar a OpenAI. Usando análisis por reglas.");
            return GenerarAnalisisPorReglas(contexto);
        }
    }

    private static string ConstruirPrompt(AnalisisContexto ctx)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Analiza los resultados de la evaluación técnica de un candidato y genera una recomendación profesional para el reclutador.");
        sb.AppendLine();
        sb.AppendLine($"## Datos de la evaluación");
        sb.AppendLine($"- **Candidato:** {ctx.NombreCandidato}");
        sb.AppendLine($"- **Evaluación:** {ctx.NombreEvaluacion}");
        sb.AppendLine($"- **Tecnología:** {ctx.TecnologiaNombre}");
        sb.AppendLine($"- **Nivel de dificultad:** {ctx.NivelDificultad}");
        sb.AppendLine($"- **Score total:** {ctx.ScoreTotal}%");
        sb.AppendLine($"- **Preguntas respondidas:** {ctx.PreguntasRespondidas}/{ctx.TotalPreguntas}");
        sb.AppendLine($"- **Tiempo total:** {ctx.TiempoTotalSegundos / 60} minutos");
        sb.AppendLine();

        sb.AppendLine("## Resultados por sección");
        foreach (var seccion in ctx.ScoresPorSeccion)
        {
            sb.AppendLine($"- **{seccion.Nombre}:** {seccion.Obtenido}/{seccion.Maximo} ({seccion.Porcentaje}%)");
        }
        sb.AppendLine();

        if (ctx.Fortalezas.Count > 0)
            sb.AppendLine($"**Fortalezas:** {string.Join(", ", ctx.Fortalezas)}");
        if (ctx.Brechas.Count > 0)
            sb.AppendLine($"**Brechas:** {string.Join(", ", ctx.Brechas)}");
        sb.AppendLine();

        if (ctx.RespuestasAbiertas.Count > 0)
        {
            sb.AppendLine("## Respuestas abiertas/código del candidato");
            foreach (var r in ctx.RespuestasAbiertas)
            {
                sb.AppendLine($"### Pregunta: {r.PreguntaTexto}");
                sb.AppendLine($"**Respuesta del candidato:** {r.RespuestaCandidato}");
                sb.AppendLine($"**Puntuación:** {r.PuntajeObtenido}/{r.PuntajeMaximo}");
                sb.AppendLine();
            }
        }

        sb.AppendLine("## Instrucciones");
        sb.AppendLine("Genera un análisis en español con las siguientes secciones:");
        sb.AppendLine("1. **Resumen general** (2-3 oraciones)");
        sb.AppendLine("2. **Fortalezas técnicas** (lista breve)");
        sb.AppendLine("3. **Áreas de mejora** (lista breve)");
        sb.AppendLine("4. **Recomendación** (apto / apto con reservas / no apto, con justificación)");
        sb.AppendLine("5. **Plan de desarrollo sugerido** (si aplica, 2-3 puntos)");

        return sb.ToString();
    }

    private static string GenerarAnalisisPorReglas(AnalisisContexto ctx)
    {
        var sb = new StringBuilder();

        // Resumen general
        var calificacion = ctx.ScoreTotal switch
        {
            >= 90 => "excelente",
            >= 70 => "bueno",
            >= 50 => "intermedio",
            _ => "bajo"
        };
        sb.AppendLine($"**Resumen general:** El candidato {ctx.NombreCandidato} obtuvo un score de {ctx.ScoreTotal}% en la evaluación \"{ctx.NombreEvaluacion}\" ({ctx.TecnologiaNombre}, nivel {ctx.NivelDificultad}), demostrando un nivel {calificacion}. Respondió {ctx.PreguntasRespondidas} de {ctx.TotalPreguntas} preguntas.");
        sb.AppendLine();

        // Fortalezas
        if (ctx.Fortalezas.Count > 0)
        {
            sb.AppendLine("**Fortalezas técnicas:**");
            foreach (var f in ctx.Fortalezas)
            {
                var seccion = ctx.ScoresPorSeccion.FirstOrDefault(s => s.Nombre == f);
                sb.AppendLine($"- {f}: {seccion?.Porcentaje ?? 0}% de acierto");
            }
            sb.AppendLine();
        }

        // Áreas de mejora
        if (ctx.Brechas.Count > 0)
        {
            sb.AppendLine("**Áreas de mejora:**");
            foreach (var b in ctx.Brechas)
            {
                var seccion = ctx.ScoresPorSeccion.FirstOrDefault(s => s.Nombre == b);
                sb.AppendLine($"- {b}: {seccion?.Porcentaje ?? 0}% de acierto — requiere refuerzo");
            }
            sb.AppendLine();
        }

        // Recomendación
        var recomendacion = ctx.ScoreTotal switch
        {
            >= 90 => "**Recomendación: APTO.** Candidato con excelente dominio técnico. Altamente recomendado para el puesto.",
            >= 70 => "**Recomendación: APTO CON RESERVAS.** Buen nivel general. Considerar para el puesto con plan de desarrollo en las áreas de mejora identificadas.",
            >= 50 => "**Recomendación: NO APTO (requiere desarrollo).** Nivel intermedio insuficiente para las exigencias del puesto. Se sugiere capacitación previa.",
            _ => "**Recomendación: NO APTO.** El candidato necesita formación adicional significativa antes de ser considerado."
        };
        sb.AppendLine(recomendacion);
        sb.AppendLine();

        // Plan de desarrollo
        if (ctx.ScoreTotal < 90 && ctx.Brechas.Count > 0)
        {
            sb.AppendLine("**Plan de desarrollo sugerido:**");
            foreach (var brecha in ctx.Brechas.Take(3))
            {
                sb.AppendLine($"- Capacitación en {brecha} ({ctx.TecnologiaNombre})");
            }
        }

        return sb.ToString();
    }
}

// Internal DTOs for OpenAI API response deserialization
file class OpenAiResponse
{
    [JsonPropertyName("choices")]
    public List<OpenAiChoice>? Choices { get; set; }
}

file class OpenAiChoice
{
    [JsonPropertyName("message")]
    public OpenAiMessage? Message { get; set; }
}

file class OpenAiMessage
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }
}
