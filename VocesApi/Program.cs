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
    context.Response.Headers.Append(
        "Access-Control-Allow-Origin",
        "*"
    );

    context.Response.Headers.Append(
        "Access-Control-Allow-Methods",
        "GET, POST, OPTIONS"
    );

    context.Response.Headers.Append(
        "Access-Control-Allow-Headers",
        "Content-Type, Authorization"
    );

    if (context.Request.Method == "OPTIONS")
    {
        context.Response.StatusCode = StatusCodes.Status200OK;
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
    if (string.IsNullOrWhiteSpace(request.Question))
    {
        return Results.BadRequest(new
        {
            answer = "Escribe una pregunta relacionada con el reportaje."
        });
    }

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

Información del reportaje:

Título:
La migración de jóvenes ecuatorianos: ¿Irse o quedarse?

Tema:
Migración juvenil ecuatoriana.

Principales causas:
Falta de empleo, bajos salarios, inseguridad, búsqueda de mejores oportunidades y reunificación familiar.

Datos relevantes:

En 2025 Ecuador registró 6.265.521 movimientos internacionales entre entradas y salidas.

Durante 2025 se registraron 3.139.329 salidas internacionales y 3.126.192 entradas internacionales.

El saldo migratorio general de 2025 fue de -13.137 movimientos, debido a que se registraron ligeramente más salidas que entradas.

En 2024 se registraron 3.145.428 salidas internacionales.

En 2025 se registraron 3.139.329 salidas internacionales, una cifra ligeramente menor que la registrada en 2024.

El grupo de 18 a 29 años representa un grupo juvenil importante dentro del análisis de la movilidad migratoria ecuatoriana.

Principales destinos:
Estados Unidos, España, Italia, Chile, Argentina y Canadá.

Historias presentadas:

Paulo C. migró a España buscando mejores oportunidades y un futuro con mayor estabilidad laboral.

Samanta S. tomó la decisión de migrar pensando en el bienestar de su hijo y de su madre.

Análisis sociológico:
El reportaje también presenta una mirada sociológica sobre las causas económicas, sociales y culturales de la migración juvenil, así como sus consecuencias para las familias, las comunidades y el país.

El reportaje incluye:
Datos estadísticos, historias, entrevistas, audios, galería fotográfica, videos y contenido multimedia.

Fuentes:
Instituto Nacional de Estadística y Censos, mediante el Registro Estadístico de Entradas y Salidas Internacionales 2025.
Organización Internacional para las Migraciones.
Alto Comisionado de las Naciones Unidas para los Refugiados.
Banco Interamericano de Desarrollo.
Cancillería del Ecuador.

Instrucciones para responder:

Responde de manera breve, clara y periodística.

Utiliza únicamente la información proporcionada en este contexto.

No inventes datos, porcentajes, nombres, fechas ni testimonios.

Cuando una cifra no aparezca en el contexto, indica que el reportaje no proporciona ese dato específico.

No utilices formato Markdown.

No uses negritas, cursivas, encabezados ni listas con símbolos.

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
                        text = $"{contexto}\n\nPregunta del usuario:\n{request.Question}"
                    }
                }
            }
        }
    };

    var client = httpClientFactory.CreateClient();

    var url =
        $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

    try
    {
        var response = await client.PostAsync(
            url,
            new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json"
            )
        );

        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("===== ERROR GEMINI =====");
            Console.WriteLine($"Código HTTP: {(int)response.StatusCode}");
            Console.WriteLine(json);
            Console.WriteLine("========================");

            if ((int)response.StatusCode == 429)
            {
                return Results.Json(
                    new
                    {
                        answer = "La inteligencia artificial alcanzó el límite de consultas disponible. Inténtalo nuevamente más tarde."
                    },
                    statusCode: StatusCodes.Status429TooManyRequests
                );
            }

            return Results.BadRequest(new
            {
                answer = "La inteligencia artificial no está disponible en este momento. Inténtalo nuevamente más tarde."
            });
        }

        using var document = JsonDocument.Parse(json);

        if (!document.RootElement.TryGetProperty(
                "candidates",
                out var candidates) ||
            candidates.GetArrayLength() == 0)
        {
            Console.WriteLine("===== RESPUESTA SIN CANDIDATOS =====");
            Console.WriteLine(json);
            Console.WriteLine("===================================");

            return Results.BadRequest(new
            {
                answer = "No fue posible generar una respuesta en este momento."
            });
        }

        var candidate = candidates[0];

        if (!candidate.TryGetProperty(
                "content",
                out var content) ||
            !content.TryGetProperty(
                "parts",
                out var parts) ||
            parts.GetArrayLength() == 0 ||
            !parts[0].TryGetProperty(
                "text",
                out var textElement))
        {
            Console.WriteLine("===== FORMATO DE RESPUESTA INESPERADO =====");
            Console.WriteLine(json);
            Console.WriteLine("===========================================");

            return Results.BadRequest(new
            {
                answer = "No fue posible interpretar la respuesta generada por la inteligencia artificial."
            });
        }

        var respuesta = textElement.GetString();

        return Results.Ok(new
        {
            answer = string.IsNullOrWhiteSpace(respuesta)
                ? "No fue posible generar una respuesta."
                : respuesta.Trim()
        });
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine("===== ERROR DE CONEXIÓN =====");
        Console.WriteLine(ex);
        Console.WriteLine("=============================");

        return Results.BadRequest(new
        {
            answer = "No fue posible conectarse con la inteligencia artificial. Inténtalo nuevamente más tarde."
        });
    }
    catch (JsonException ex)
    {
        Console.WriteLine("===== ERROR AL LEER JSON =====");
        Console.WriteLine(ex);
        Console.WriteLine("==============================");

        return Results.BadRequest(new
        {
            answer = "No fue posible interpretar la respuesta generada por la inteligencia artificial."
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine("===== ERROR GENERAL =====");
        Console.WriteLine(ex);
        Console.WriteLine("=========================");

        return Results.BadRequest(new
        {
            answer = "Ocurrió un problema al procesar la consulta. Inténtalo nuevamente más tarde."
        });
    }
});

app.Run();

public record ChatRequest(string Question);