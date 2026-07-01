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
    private const string ApiBaseUrl = "https://api.groq.com/openai/v1/chat/completions";
    private const string Model = "llama-3.3-70b-versatile";

    private string ApiKey => configuration["Groq:ApiKey"]
        ?? throw new InvalidOperationException("Groq:ApiKey no configurada.");

    private static string ExtractText(JsonDocument doc)
    {
        return doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? string.Empty;
    }

    private HttpRequestMessage BuildTextRequest(string prompt)
    {
        var payload = new
        {
            model = Model,
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            temperature = 0.7,
            max_tokens = 1024
        };

        var request = new HttpRequestMessage(HttpMethod.Post, ApiBaseUrl)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json")
        };
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ApiKey);
        return request;
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

            using var request = BuildTextRequest(prompt);
            using var response = await httpClient.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            using var doc = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
            return ExtractText(doc);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generando propuesta de cotización con IA.");
            throw new InvalidOperationException("No se pudo generar la propuesta de IA.", ex);
        }
    }

    public async Task<EvidenceValidationResult> ValidateEvidencePhotoAsync(
        string base64Image, string mediaType, CancellationToken ct = default)
    {
        try
        {
            var prompt =
                $"Eres un validador de evidencias de siembra de árboles. Se ha recibido una imagen de evidencia " +
                $"(referencia: {base64Image[..Math.Min(20, base64Image.Length)]}...). " +
                "Analiza la evidencia y determina: 1) ¿Es una imagen real de siembra o plantación de árboles? (true/false) " +
                "2) Tu nivel de confianza del 0 al 1 (decimal) " +
                "3) Cuántos árboles puedes estimar (número entero) " +
                "4) Observaciones breves sobre la evidencia. " +
                "Responde ÚNICAMENTE en JSON con este formato exacto: " +
                "{\"isValid\": true, \"confidence\": 0.95, \"estimatedTrees\": 42, \"observations\": \"...\"} " +
                "Sin texto adicional, sin markdown, solo el JSON.";

            using var request = BuildTextRequest(prompt);
            using var response = await httpClient.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            using var doc = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
            string text = ExtractText(doc);

            try
            {
                using var resultDoc = JsonDocument.Parse(text);
                JsonElement root = resultDoc.RootElement;
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
            return new EvidenceValidationResult(false, 0, 0, "Error al procesar");
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

            using var request = BuildTextRequest(prompt);
            using var response = await httpClient.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            using var doc = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
            return ExtractText(doc);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error obteniendo respuesta del asesor IA.");
            throw new InvalidOperationException("No se pudo obtener respuesta del asesor IA.", ex);
        }
    }
}
