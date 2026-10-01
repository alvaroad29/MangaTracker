using System.Net.Http.Json;
using System.Text.Json;
using MangaTracker.Application.DTOs.Catalog;
using MangaTracker.Application.Interfaces;
using MangaTracker.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Tenrai;
using MangaType = MangaTracker.Domain.Enums.MangaType;

namespace MangaTracker.Infrastructure.AiSearch;

public class GeminiAiSearchTranslationService(HttpClient httpClient, IConfiguration config) : IAiSearchTranslationService
{
    private readonly string _model = config["Gemini:Model"] ?? "gemini-2.5-flash-lite";
    
    // Géneros que le ofrecemos a la IA para elegir — excluimos los de contenido adulto,
    // que además ya filtramos en Tenrai con Sfw = true por defecto.
    private static readonly string[] AllowedGenres = Enum.GetNames(typeof(MangaGenreSearch))
        .Except(["Hentai", "Erotica"])
        .ToArray();

    private const string SystemInstruction = """
     Sos un asistente que traduce descripciones de manga en lenguaje natural (en español)
     a un objeto JSON para hacer una búsqueda. Extraé:
     - searchQuery: SOLO si el usuario menciona un título, nombre propio o palabra específica que podría
       aparecer literal en una base de datos en inglés/romaji — copiala tal cual. Si el usuario solo describe
       la trama o el género sin nombrar nada específico, dejá este campo como string vacío "".
     - mangaType: si se menciona el formato, uno de los valores permitidos — si no, null
     - status: si se menciona si busca algo terminado o en curso, uno de los valores permitidos — si no, null
     - genres: los géneros temáticos mencionados, tomados SOLO de la lista permitida, ordenados del más
       al menos central a la descripción — si no aplica ninguno, array vacío
     Respondé ÚNICAMENTE el JSON, sin texto adicional.
     """;

    // El schema: le decimos a Gemini EXACTAMENTE qué forma tiene que tener la respuesta.
    // Esto es lo que garantiza que no nos devuelva texto libre o un formato distinto.
    private static readonly object ResponseSchema = new
    {
        type = "OBJECT",
        properties = new
        {
            searchQuery = new { type = "STRING" },
            mangaType = new { type = "STRING", nullable = true, @enum = new[] { "Manga", "Manhwa", "Manhua", "LightNovel", "OneShot", "Doujinshi" } },
            status = new { type = "STRING", nullable = true, @enum = new[] { "Publishing", "Finished", "Hiatus", "Discontinued" } },
            genres = new { type = "ARRAY", items = new { type = "STRING", @enum = AllowedGenres } }
        },
        required = new[] { "searchQuery" }
    };

    public async Task<MangaSearchCriteria> TranslateAsync(string naturalLanguageDescription)
    {
        var request = new GeminiGenerateRequest
        {
            Contents = [new GeminiContent { Parts = [new GeminiPart { Text = $"{SystemInstruction}\n\nDescripción del usuario: {naturalLanguageDescription}" }] }],
            GenerationConfig = new GeminiGenerationConfig { ResponseSchema = ResponseSchema }
        };

        var endpoint = $"models/{_model}:generateContent?key={config["Gemini:ApiKey"]}";
    
        // Log temporal para ver la URL completa que estás llamando
        Console.WriteLine($"🔍 Llamando a: {httpClient.BaseAddress}{endpoint}");

        var response = await httpClient.PostAsJsonAsync(
            endpoint,
            request,
            new JsonSerializerOptions { PropertyNamingPolicy = null });

        // En lugar de EnsureSuccessStatusCode, leemos el error real
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Error de Gemini API ({(int)response.StatusCode} {response.StatusCode}): {errorBody}");
        }

        var result = await response.Content.ReadFromJsonAsync<GeminiGenerateResponse>();
        var rawJson = result?.Candidates.FirstOrDefault()?.Content.Parts.FirstOrDefault()?.Text
                      ?? throw new InvalidOperationException("La IA no devolvió una respuesta interpretable.");

        return ParseCriteria(rawJson, naturalLanguageDescription);
    }

    private static MangaSearchCriteria ParseCriteria(string rawJson, string fallbackQuery)
    {
        using var doc = JsonDocument.Parse(rawJson);
        var root = doc.RootElement;

        var searchQuery = root.TryGetProperty("searchQuery", out var q) ? q.GetString() ?? fallbackQuery : fallbackQuery;

        MangaType? type = root.TryGetProperty("mangaType", out var t) && t.ValueKind == JsonValueKind.String
            ? Enum.TryParse<MangaType>(t.GetString(), out var parsedType) ? parsedType : null
            : null;

        PublicationStatus? status = root.TryGetProperty("status", out var s) && s.ValueKind == JsonValueKind.String
            ? Enum.TryParse<PublicationStatus>(s.GetString(), out var parsedStatus) ? parsedStatus : null
            : null;

        var genres = root.TryGetProperty("genres", out var g) && g.ValueKind == JsonValueKind.Array
            ? g.EnumerateArray().Select(e => e.GetString() ?? "").Where(x => x != "").ToList()
            : new List<string>();

        return new MangaSearchCriteria(searchQuery, type, status, genres);
    }
}