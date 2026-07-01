using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddHttpClient();

var app = builder.Build();

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
    context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
    context.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Authorization");

    if (context.Request.Method == "OPTIONS")
    {
        context.Response.StatusCode = 200;
        await context.Response.CompleteAsync();
        return;
    }

    await next();
});

app.UseCors();

app.MapPost("/api/chat", async (
    ChatRequest request,
    IConfiguration config,
    IHttpClientFactory httpClientFactory) =>
{
    var apiKey = config["Gemini:ApiKey"];

    if (string.IsNullOrWhiteSpace(apiKey))
    {
        return Results.BadRequest(new
        {
            answer = "No se encontró la API Key de Gemini."
        });
    }

    var contexto = """
Eres la inteligencia artificial del reportaje multimedia "Voces Sin Fronteras".

Solo puedes responder preguntas relacionadas con este reportaje sobre migración juvenil ecuatoriana.

Si el usuario pregunta cualquier otra cosa responde únicamente:
"Solo puedo responder preguntas relacionadas con este reportaje sobre migración juvenil ecuatoriana."

Información del reportaje:

Título:
La migración de jóvenes ecuatorianos: ¿Irse o quedarse?

Tema:
Migración juvenil ecuatoriana.

Causas:
- Falta de empleo
- Bajos salarios
- Inseguridad
- Mejores oportunidades
- Reunificación familiar

Datos:
En 2024 Ecuador registró aproximadamente 6,1 millones de movimientos internacionales.
Más de 3,1 millones fueron salidas internacionales.

Destinos principales:
- Estados Unidos
- España
- Italia
- Chile
- Argentina
- Canadá

Historias:
Paulo C. migró a España buscando mejores oportunidades.
Samanta S. migró pensando en el bienestar de su hijo y su madre.

El reportaje contiene:
- Datos estadísticos
- Historias
- Entrevistas
- Audios
- Galería fotográfica
- Multimedia

Fuentes:
INEC
OIM
ACNUR
BID
Cancillería del Ecuador

Responde siempre de forma breve, clara y periodística.
""";

    var body = new
    {
        contents = new[]
        {
            new
            {
                parts = new[]
                {
                    new
                    {
                        text = $"{contexto}\n\nPregunta:\n{request.Question}"
                    }
                }
            }
        }
    };

    var client = httpClientFactory.CreateClient();

    var url =
    $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key={apiKey}";
   
    var response = await client.PostAsync(
        url,
        new StringContent(
            JsonSerializer.Serialize(body),
            Encoding.UTF8,
            "application/json"));

    var json = await response.Content.ReadAsStringAsync();

    if (!response.IsSuccessStatusCode)
    {
    Console.WriteLine("===== ERROR GEMINI =====");
    Console.WriteLine(json);
    Console.WriteLine("========================");

    return Results.BadRequest(new
    {
        answer = json
    });
    }

    using var document = JsonDocument.Parse(json);

    var respuesta = document.RootElement
        .GetProperty("candidates")[0]
        .GetProperty("content")
        .GetProperty("parts")[0]
        .GetProperty("text")
        .GetString();

    return Results.Ok(new
    {
        answer = respuesta ?? "No pude generar una respuesta."
    });
});

app.Run();

public record ChatRequest(string Question);