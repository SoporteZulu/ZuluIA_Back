using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Recibos.Commands;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.UnitTests.Application.Handlers;

public class EmitirReciboCommandHandlerTests
{
    private readonly IReciboRepository _repo = Substitute.For<IReciboRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();

    private EmitirReciboCommandHandler Sut() => new(_repo, _uow, _user);

    [Fact]
    public async Task Handle_SinItems_RetornaFailure()
    {
        var command = new EmitirReciboCommand(
            1,
            10,
            new DateOnly(2026, 3, 20),
            "A",
            null,
            null,
            []);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("al menos un concepto");
    }

    [Fact]
    public async Task Handle_ConItems_EmiteReciboYPersiste()
    {
        _user.UserId.Returns((long?)1L);
        _repo.GetUltimoNumeroAsync(1, "A", Arg.Any<CancellationToken>()).Returns(7);

        var command = new EmitirReciboCommand(
            1,
            10,
            new DateOnly(2026, 3, 20),
            "A",
            "Obs",
            null,
            [new EmitirReciboItemDto("Concepto", 100m)]);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).AddAsync(Arg.Any<Recibo>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class AnularReciboCommandHandlerTests
{
    private readonly IReciboRepository _repo = Substitute.For<IReciboRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();

    private AnularReciboCommandHandler Sut() => new(_repo, _uow, _user);

    [Fact]
    public async Task Handle_ReciboNoExiste_RetornaFailure()
    {
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns((Recibo?)null);

        var result = await Sut().Handle(new AnularReciboCommand(99), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("no encontrado");
    }

    [Fact]
    public async Task Handle_ReciboEmitido_LoAnulaYPersiste()
    {
        _user.UserId.Returns((long?)1L);
        var recibo = Recibo.Crear(1, 10, new DateOnly(2026, 3, 20), "A", 8, null, null, null);
        recibo.AgregarItem(ReciboItem.Crear(0, "Concepto", 100m));
        _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(recibo);

        var result = await Sut().Handle(new AnularReciboCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        recibo.Estado.Should().Be(EstadoRecibo.Anulado);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class RecibosCommandValidatorTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AnularValidator_IdInvalido_RetornaError(long id)
    {
        var validator = new AnularReciboCommandValidator();

        var result = validator.Validate(new AnularReciboCommand(id));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == "Id");
    }

    [Fact]
    public void AnularValidator_IdValido_Pasa()
    {
        var validator = new AnularReciboCommandValidator();

        var result = validator.Validate(new AnularReciboCommand(1));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void EmitirValidator_DatosValidos_Pasa()
    {
        var validator = new EmitirReciboCommandValidator();

        var result = validator.Validate(new EmitirReciboCommand(
            1,
            10,
            new DateOnly(2026, 3, 20),
            "A",
            null,
            null,
            [new EmitirReciboItemDto("Concepto", 100m)]));

        result.IsValid.Should().BeTrue();
    }
}