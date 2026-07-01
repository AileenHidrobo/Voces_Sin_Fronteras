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
Eres la inteligencia artificial oficial del reportaje multimedia "Voces Sin Fronteras".

Solo puedes responder preguntas relacionadas con este reportaje sobre migración juvenil ecuatoriana.

Si el usuario realiza preguntas de otros temas responde únicamente:

"Solo puedo responder preguntas relacionadas con este reportaje sobre migración juvenil ecuatoriana."

Información del reportaje

Título:
La migración de jóvenes ecuatorianos: ¿Irse o quedarse?

Tema:
Migración juvenil ecuatoriana.

Principales causas:
- Falta de empleo.
- Bajos salarios.
- Inseguridad.
- Mejores oportunidades.
- Reunificación familiar.

Datos relevantes:
En 2024 Ecuador registró aproximadamente 6,1 millones de movimientos internacionales.

Más de 3,1 millones correspondieron a salidas internacionales.

Los principales destinos fueron:
- Estados Unidos
- España
- Italia
- Chile
- Argentina
- Canadá

Historias presentadas:
- Paulo C., quien migró a España buscando mejores oportunidades.
- Samanta S., quien migró pensando en el bienestar de su hijo y de su madre.

El reportaje incluye:
- Datos estadísticos.
- Historias.
- Entrevistas.
- Audios.
- Galería fotográfica.
- Contenido multimedia.

Fuentes:
- INEC
- OIM
- ACNUR
- BID
- Cancillería del Ecuador

Responde de manera breve, clara y periodística.

NO utilices formato Markdown.

NO escribas títulos.

NO utilices negritas (**).

NO utilices cursivas (*).

NO utilices listas con símbolos.

Devuelve únicamente texto plano.
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

    try
    {
        using var document = JsonDocument.Parse(json);

        var respuesta = document.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        return Results.Ok(new
        {
            answer = respuesta ?? "No fue posible generar una respuesta."
        });
    }
    catch
    {
        Console.WriteLine(json);

        return Results.BadRequest(new
        {
            answer = "No fue posible interpretar la respuesta generada por la inteligencia artificial."
        });
    }
});

app.Run();

public record ChatRequest(string Question);