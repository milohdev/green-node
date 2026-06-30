using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using GreenNode.Application.Common.Interfaces;
using GreenNode.Application.Common.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GreenNode.Infrastructure.Services;

public class AiService(HttpClient httpClient, IConfiguration configuration, ILogger<AiService> logger)
    : IClaudeAiService
{
    private const string ApiUrl = "https://api.anthropic.com/v1/messages";
    private const string Model = "claude-3-5-haiku-20241022";
    private const int MaxTokens = 1024;

    private HttpRequestMessage BuildRequest(object payload)
    {
        var apiKey = configuration["Anthropic:ApiKey"]
            ?? throw new InvalidOperationException("Anthropic:ApiKey no configurada.");

        var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
        request.Headers.Add("x-api-key", apiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");
        request.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );
        return request;
    }

    private static string ExtractText(JsonDocument doc)
    {
        return doc.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString() ?? string.Empty;
    }

    public async Task<string> GenerateQuoteProposalAsync(
        string sector, int numberOfTrees, string territory,
        CancellationToken ct = default)
    {
        try
        {
            var prompt =
                $"Eres un experto en reforestación en Colombia. Genera un marco lógico breve para una cotización de " +
                $"{numberOfTrees} árboles en el sector {sector}, territorio {territory}. " +
                $"Incluye: objetivo general, actividades principales, indicadores de éxito y plazo estimado. " +
                $"Responde en español, máximo 400 palabras.";

            var payload = new
            {
                model = Model,
                max_tokens = MaxTokens,
                messages = new[] { new { role = "user", content = prompt } }
            };

            using var request = BuildRequest(payload);
            using var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            return ExtractText(doc);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generando propuesta de cotización con IA.");
            throw new InvalidOperationException("No se pudo generar la propuesta de IA.", ex);
        }
    }

    public async Task<EvidenceValidationResult> ValidateEvidencePhotoAsync(string base64Image, string mediaType, CancellationToken ct = default)

    {
        try
        {
            var payload = new
            {
                model = Model,
                max_tokens = MaxTokens,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new
                            {
                                type = "image",
                                source = new
                                {
                                    type = "base64",
                                    media_type = mediaType,
                                    data = base64Image
                                }
                            },
                            new
                            {
                                type = "text",
                                text = "Analiza esta imagen de evidencia de siembra de árboles. " +
                                       "Determina: 1) ¿Es una imagen real de siembra o plantación de árboles? (true/false) " +
                                       "2) Tu nivel de confianza del 0 al 1 (decimal) " +
                                       "3) Cuántos árboles puedes identificar o estimar en la imagen (número entero) " +
                                       "4) Observaciones breves sobre la imagen. " +
                                       "Responde ÚNICAMENTE en JSON con este formato exacto: " +
                                       "{\"isValid\": true, \"confidence\": 0.95, \"estimatedTrees\": 42, \"observations\": \"...\"} " +
                                       "Sin texto adicional, sin markdown, solo el JSON."
                            }
                        }
                    }
                }
            };

            using var request = BuildRequest(payload);
            using var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            var text = ExtractText(doc);

            try
            {
                using var resultDoc = JsonDocument.Parse(text);
                var root = resultDoc.RootElement;
                return new EvidenceValidationResult(
                    IsValid: root.GetProperty("isValid").GetBoolean(),
                    Confidence: root.GetProperty("confidence").GetDecimal(),
                    EstimatedTrees: root.GetProperty("estimatedTrees").GetInt32(),
                    Observations: root.GetProperty("observations").GetString() ?? string.Empty
                );
            }
            catch
            {
                return new EvidenceValidationResult(false, 0, 0, "Error al procesar respuesta de IA.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validando evidencia fotográfica con IA.");
            return new EvidenceValidationResult(false, 0, 0, "Error al procesar respuesta de IA.");
        }
    }

    public async Task<string> GetProjectAdvisorResponseAsync(
        string companyName, string sector, int numberOfTrees,
        int validatedTrees, decimal progress, string userQuestion,
        CancellationToken ct = default)
    {
        try
        {
            var prompt =
                $"Eres el asesor ambiental de GreenNode. La empresa '{companyName}' tiene un proyecto de reforestación " +
                $"en el sector {sector} con {numberOfTrees} árboles planificados, {validatedTrees} validados hasta ahora " +
                $"({progress:F1}% de avance). El responsable pregunta: '{userQuestion}' " +
                $"Responde de forma profesional, útil y en español. Máximo 300 palabras.";

            var payload = new
            {
                model = Model,
                max_tokens = MaxTokens,
                messages = new[] { new { role = "user", content = prompt } }
            };

            using var request = BuildRequest(payload);
            using var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            return ExtractText(doc);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error obteniendo respuesta del asesor IA.");
            throw new InvalidOperationException("No se pudo obtener respuesta del asesor IA.", ex);
        }
    }
}
