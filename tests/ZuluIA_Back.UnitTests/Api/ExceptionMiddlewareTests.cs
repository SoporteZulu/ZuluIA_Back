using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Middleware;

namespace ZuluIA_Back.UnitTests.Api;

public class ExceptionMiddlewareTests
{
    private static (ExceptionMiddleware middleware, DefaultHttpContext context, MemoryStream body)
        Build(RequestDelegate next)
    {
        var logger = Substitute.For<ILogger<ExceptionMiddleware>>();
        var middleware = new ExceptionMiddleware(next, logger);
        var context = new DefaultHttpContext();
        var body = new MemoryStream();
        context.Response.Body = body;
        return (middleware, context, body);
    }

    private static async Task<JsonDocument> ReadBodyAsync(MemoryStream body)
    {
        body.Position = 0;
        return await JsonDocument.ParseAsync(body);
    }

    // -----------------------------------------------------------
    // Sin excepción — pasa al siguiente middleware
    // -----------------------------------------------------------

    [Fact]
    public async Task InvokeAsync_SinExcepcion_LlamaSiguienteMiddleware()
    {
        var nextCalled = false;
        var (middleware, context, _) = Build(_ => { nextCalled = true; return Task.CompletedTask; });

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    // -----------------------------------------------------------
    // ArgumentNullException → 400
    // -----------------------------------------------------------

    [Fact]
    public async Task InvokeAsync_ArgumentNullException_Devuelve400()
    {
        var (middleware, context, body) = Build(_ => throw new ArgumentNullException("param"));

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        var json = await ReadBodyAsync(body);
        json.RootElement.GetProperty("message").GetString()
            .Should().Be("Argumento nulo no permitido.");
        json.RootElement.GetProperty("status").GetInt32().Should().Be(400);
    }

    // -----------------------------------------------------------
    // UnauthorizedAccessException → 401
    // -----------------------------------------------------------

    [Fact]
    public async Task InvokeAsync_UnauthorizedAccessException_Devuelve401()
    {
        var (middleware, context, body) = Build(_ => throw new UnauthorizedAccessException());

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
        var json = await ReadBodyAsync(body);
        json.RootElement.GetProperty("message").GetString()
            .Should().Be("No autorizado.");
        json.RootElement.GetProperty("status").GetInt32().Should().Be(401);
    }

    // -----------------------------------------------------------
    // KeyNotFoundException → 404
    // -----------------------------------------------------------

    [Fact]
    public async Task InvokeAsync_KeyNotFoundException_Devuelve404()
    {
        var (middleware, context, body) = Build(_ => throw new KeyNotFoundException());

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        var json = await ReadBodyAsync(body);
        json.RootElement.GetProperty("message").GetString()
            .Should().Be("Recurso no encontrado.");
        json.RootElement.GetProperty("status").GetInt32().Should().Be(404);
    }

    // -----------------------------------------------------------
    // InvalidOperationException → 400 con mensaje de la excepción
    // -----------------------------------------------------------

    [Fact]
    public async Task InvokeAsync_InvalidOperationException_Devuelve400ConMensaje()
    {
        const string msg = "Operación inválida específica";
        var (middleware, context, body) = Build(_ => throw new InvalidOperationException(msg));

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        var json = await ReadBodyAsync(body);
        json.RootElement.GetProperty("message").GetString().Should().Be(msg);
    }

    // -----------------------------------------------------------
    // Excepción genérica → 500
    // -----------------------------------------------------------

    [Fact]
    public async Task InvokeAsync_ExcepcionGenerica_Devuelve500()
    {
        var (middleware, context, body) = Build(_ => throw new Exception("Error inesperado"));

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        var json = await ReadBodyAsync(body);
        json.RootElement.GetProperty("message").GetString()
            .Should().Be("Error interno del servidor.");
        json.RootElement.GetProperty("status").GetInt32().Should().Be(500);
    }

    // -----------------------------------------------------------
    // Content-Type es application/json
    // -----------------------------------------------------------

    [Fact]
    public async Task InvokeAsync_AlManejarExcepcion_ContentTypeEsJson()
    {
        var (middleware, context, _) = Build(_ => throw new Exception());

        await middleware.InvokeAsync(context);

        context.Response.ContentType.Should().Be("application/json");
    }

    // -----------------------------------------------------------
    // detail contiene el mensaje de la excepción original
    // -----------------------------------------------------------

    [Fact]
    public async Task InvokeAsync_RespuestaIncluye_DetailConMensajeOriginal()
    {
        const string originalMsg = "Mensaje original de la excepción";
        var (middleware, context, body) = Build(_ => throw new Exception(originalMsg));

        await middleware.InvokeAsync(context);

        var json = await ReadBodyAsync(body);
        json.RootElement.GetProperty("detail").GetString().Should().Be(originalMsg);
    }
}
