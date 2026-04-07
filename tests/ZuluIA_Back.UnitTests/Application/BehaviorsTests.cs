using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using ZuluIA_Back.Application.Common.Behaviors;

namespace ZuluIA_Back.UnitTests.Application;

// ─────────────────────────────────────────────────────────────────────────────
// Shared helpers (public — required so Castle DynamicProxy can generate proxies)
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Minimal MediatR request used only by pipeline behavior tests.</summary>
public record BehaviorsTestRequest(string Value) : IRequest<string>;

/// <summary>Validator that always passes.</summary>
public class AlwaysValidBehaviorValidator : AbstractValidator<BehaviorsTestRequest> { }

/// <summary>Validator that always fails with a fixed message.</summary>
public class AlwaysInvalidBehaviorValidator : AbstractValidator<BehaviorsTestRequest>
{
    public AlwaysInvalidBehaviorValidator()
        => RuleFor(x => x.Value).Must(_ => false).WithMessage("Error A");
}

/// <summary>Validator that fails only when Value is shorter than 3 chars.</summary>
public class MinLengthBehaviorValidator : AbstractValidator<BehaviorsTestRequest>
{
    public MinLengthBehaviorValidator()
        => RuleFor(x => x.Value).MinimumLength(3).WithMessage("Error B");
}

// ─────────────────────────────────────────────────────────────────────────────
// ValidationBehavior
// ─────────────────────────────────────────────────────────────────────────────

public class ValidationBehaviorTests
{

    [Fact]
    public async Task Handle_SinValidadores_LlamaANextYRetornaResultado()
    {
        var behavior = new ValidationBehavior<BehaviorsTestRequest, string>([]);
        RequestHandlerDelegate<string> next = () => Task.FromResult("resultado");

        var result = await behavior.Handle(new BehaviorsTestRequest("x"), next, CancellationToken.None);

        result.Should().Be("resultado");
    }

    [Fact]
    public async Task Handle_ValidadoresQueApueban_LlamaANextYRetornaResultado()
    {
        var behavior = new ValidationBehavior<BehaviorsTestRequest, string>(
            [new AlwaysValidBehaviorValidator()]);
        RequestHandlerDelegate<string> next = () => Task.FromResult("resultado");

        var result = await behavior.Handle(new BehaviorsTestRequest("valido"), next, CancellationToken.None);

        result.Should().Be("resultado");
    }

    [Fact]
    public async Task Handle_ValidadoresConFallos_LanzaValidationException()
    {
        var behavior = new ValidationBehavior<BehaviorsTestRequest, string>(
            [new AlwaysInvalidBehaviorValidator()]);
        RequestHandlerDelegate<string> next = () => Task.FromResult("resultado");

        var act = async () =>
            await behavior.Handle(new BehaviorsTestRequest(""), next, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_MultiplesValidadores_UnFallo_LanzaValidationException()
    {
        // AlwaysValid + MinLength(3) — request with short value should fail
        var behavior = new ValidationBehavior<BehaviorsTestRequest, string>(
            [new AlwaysValidBehaviorValidator(), new MinLengthBehaviorValidator()]);
        RequestHandlerDelegate<string> next = () => Task.FromResult("resultado");

        var act = async () =>
            await behavior.Handle(new BehaviorsTestRequest("x"), next, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_MultiplesValidadoresConFallos_IncluieTodosLosErrores()
    {
        // Both validators always fail → 2 errors total
        var behavior = new ValidationBehavior<BehaviorsTestRequest, string>(
            [new AlwaysInvalidBehaviorValidator(), new MinLengthBehaviorValidator()]);
        RequestHandlerDelegate<string> next = () => Task.FromResult("resultado");

        var act = async () =>
            await behavior.Handle(new BehaviorsTestRequest(""), next, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task Handle_SinValidadores_NoLlamaAlValidador()
    {
        var llamado = false;
        var behavior = new ValidationBehavior<BehaviorsTestRequest, string>([]);
        RequestHandlerDelegate<string> next = () =>
        {
            llamado = true;
            return Task.FromResult("ok");
        };

        await behavior.Handle(new BehaviorsTestRequest("x"), next, CancellationToken.None);

        llamado.Should().BeTrue("next() debe ser llamado cuando no hay validadores");
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// LoggingBehavior
// ─────────────────────────────────────────────────────────────────────────────

public class LoggingBehaviorTests
{
    // NullLogger.Instance discards all log output — no mocking needed
    private static LoggingBehavior<BehaviorsTestRequest, string> BuildBehavior() =>
        new(NullLogger<LoggingBehavior<BehaviorsTestRequest, string>>.Instance);

    [Fact]
    public async Task Handle_RequestExitoso_RetornaResultado()
    {
        var behavior = BuildBehavior();
        RequestHandlerDelegate<string> next = () => Task.FromResult("ok");

        var result = await behavior.Handle(new BehaviorsTestRequest("x"), next, CancellationToken.None);

        result.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_RequestFalla_RelanzaLaExcepcion()
    {
        var behavior = BuildBehavior();
        RequestHandlerDelegate<string> next =
            () => throw new InvalidOperationException("Error simulado");

        var act = async () =>
            await behavior.Handle(new BehaviorsTestRequest("x"), next, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("Error simulado");
    }

    [Fact]
    public async Task Handle_RequestExitoso_LlamaANext()
    {
        var behavior = BuildBehavior();
        var llamado = false;
        RequestHandlerDelegate<string> next = () =>
        {
            llamado = true;
            return Task.FromResult("ok");
        };

        await behavior.Handle(new BehaviorsTestRequest("x"), next, CancellationToken.None);

        llamado.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_RequestFalla_PropagaElMismoTipoDeExcepcion()
    {
        var behavior = BuildBehavior();
        RequestHandlerDelegate<string> next =
            () => throw new ArgumentException("arg inválido");

        var act = async () =>
            await behavior.Handle(new BehaviorsTestRequest("x"), next, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
                 .WithMessage("arg inválido");
    }
}
