using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("Angular", policy =>
        policy.WithOrigins(
            "http://localhost:4200",
            "https://voces-sin-fronteras.onrender.com"
        )
        .AllowAnyHeader()
        .AllowAnyMethod());
});

builder.Services.AddHttpClient();

var app = builder.Build();

app.UseCors("Angular");

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
- Fuentes

Fuentes:
INEC, OIM, ACNUR, BID y Cancillería del Ecuador.

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
                        text = $"{contexto}\n\nPregunta del usuario:\n{request.Question}"
                    }
                }
            }
        }
    };

    var client = httpClientFactory.CreateClient();

    var url =
        $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={apiKey}";

    var response = await client.PostAsync(
        url,
        new StringContent(
            JsonSerializer.Serialize(body),
            Encoding.UTF8,
            "application/json")
    );

    var json = await response.Content.ReadAsStringAsync();

    if (!response.IsSuccessStatusCode)
    {
        return Results.BadRequest(new
        {
            answer = "No pude conectarme correctamente con Gemini. Revisa la API Key o el modelo configurado."
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