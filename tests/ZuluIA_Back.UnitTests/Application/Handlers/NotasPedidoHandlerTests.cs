using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.NotasPedido.Commands;
using ZuluIA_Back.Domain.Entities.Ventas;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.UnitTests.Application.Handlers;

public class CrearNotaPedidoCommandHandlerTests
{
    private readonly INotaPedidoRepository _repo = Substitute.For<INotaPedidoRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();

    private CrearNotaPedidoCommandHandler Sut() => new(_repo, _uow, _user);

    [Fact]
    public async Task Handle_SinItems_RetornaFailure()
    {
        var command = new CrearNotaPedidoCommand(
            1,
            10,
            new DateOnly(2026, 3, 20),
            null,
            null,
            null,
            []);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("al menos un ítem");
    }

    [Fact]
    public async Task Handle_ConItems_CreaNotaYPersiste()
    {
        _user.UserId.Returns((long?)1L);

        var command = new CrearNotaPedidoCommand(
            1,
            10,
            new DateOnly(2026, 3, 20),
            new DateOnly(2026, 3, 30),
            "Obs",
            5,
            [new CrearNotaPedidoItemDto(1, 2m, 100m, 0m)]);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).AddAsync(Arg.Any<NotaPedido>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class AnularNotaPedidoCommandHandlerTests
{
    private readonly INotaPedidoRepository _repo = Substitute.For<INotaPedidoRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();

    private AnularNotaPedidoCommandHandler Sut() => new(_repo, _uow, _user);

    [Fact]
    public async Task Handle_NotaNoExiste_RetornaFailure()
    {
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns((NotaPedido?)null);

        var result = await Sut().Handle(new AnularNotaPedidoCommand(99), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("no encontrada");
    }

    [Fact]
    public async Task Handle_NotaAbierta_LaAnulaYPersiste()
    {
        _user.UserId.Returns((long?)1L);
        var nota = NotaPedido.Crear(1, 10, new DateOnly(2026, 3, 20), null, null, null, null);
        nota.AgregarItem(NotaPedidoItem.Crear(0, 1, 2m, 100m, 0m));
        _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(nota);

        var result = await Sut().Handle(new AnularNotaPedidoCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        nota.Estado.Should().Be(EstadoNotaPedido.Anulada);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class NotasPedidoCommandValidatorTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AnularValidator_IdInvalido_RetornaError(long id)
    {
        var validator = new AnularNotaPedidoCommandValidator();

        var result = validator.Validate(new AnularNotaPedidoCommand(id));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == "Id");
    }

    [Fact]
    public void AnularValidator_IdValido_Pasa()
    {
        var validator = new AnularNotaPedidoCommandValidator();

        var result = validator.Validate(new AnularNotaPedidoCommand(1));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CrearValidator_DatosValidos_Pasa()
    {
        var validator = new CrearNotaPedidoCommandValidator();

        var result = validator.Validate(new CrearNotaPedidoCommand(
            1,
            10,
            new DateOnly(2026, 3, 20),
            null,
            null,
            null,
            [new CrearNotaPedidoItemDto(1, 2m, 100m, 0m)]));

        result.IsValid.Should().BeTrue();
    }
}