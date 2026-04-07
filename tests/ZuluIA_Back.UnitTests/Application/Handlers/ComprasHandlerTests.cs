using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Compras.Commands;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Entities.Compras;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.UnitTests.Application.Handlers;

public class EnviarRequisicionCompraCommandHandlerTests
{
    private readonly IRequisicionCompraRepository _repo = Substitute.For<IRequisicionCompraRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();

    private EnviarRequisicionCompraCommandHandler Sut() => new(_repo, _uow, _user);

    [Fact]
    public async Task Handle_RequisicionNoExiste_RetornaFailure()
    {
        _repo.GetByIdConItemsAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns((RequisicionCompra?)null);

        var result = await Sut().Handle(new EnviarRequisicionCompraCommand(99), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("no encontrada");
    }

    [Fact]
    public async Task Handle_RequisicionBorradorConItems_LaEnviaYPersiste()
    {
        var requisicion = ComprasHandlerTestHelper.CrearRequisicionBorradorConItems();
        _repo.GetByIdConItemsAsync(1, Arg.Any<CancellationToken>()).Returns(requisicion);
        _user.UserId.Returns((long?)1L);

        var result = await Sut().Handle(new EnviarRequisicionCompraCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        requisicion.Estado.Should().Be(EstadoRequisicion.Enviada);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class AprobarRequisicionCompraCommandHandlerTests
{
    private readonly IRequisicionCompraRepository _repo = Substitute.For<IRequisicionCompraRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();

    private AprobarRequisicionCompraCommandHandler Sut() => new(_repo, _uow, _user);

    [Fact]
    public async Task Handle_RequisicionNoExiste_RetornaFailure()
    {
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns((RequisicionCompra?)null);

        var result = await Sut().Handle(new AprobarRequisicionCompraCommand(99), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("no encontrada");
    }

    [Fact]
    public async Task Handle_RequisicionEnviada_LaApruebaYPersiste()
    {
        var requisicion = ComprasHandlerTestHelper.CrearRequisicionEnviada();
        _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(requisicion);
        _user.UserId.Returns((long?)1L);

        var result = await Sut().Handle(new AprobarRequisicionCompraCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        requisicion.Estado.Should().Be(EstadoRequisicion.Aprobada);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class RechazarRequisicionCompraCommandHandlerTests
{
    private readonly IRequisicionCompraRepository _repo = Substitute.For<IRequisicionCompraRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();

    private RechazarRequisicionCompraCommandHandler Sut() => new(_repo, _uow, _user);

    [Fact]
    public async Task Handle_RequisicionNoExiste_RetornaFailure()
    {
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns((RequisicionCompra?)null);

        var result = await Sut().Handle(new RechazarRequisicionCompraCommand(99, "motivo"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("no encontrada");
    }

    [Fact]
    public async Task Handle_RequisicionEnviada_LaRechazaYPersiste()
    {
        var requisicion = ComprasHandlerTestHelper.CrearRequisicionEnviada();
        _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(requisicion);
        _user.UserId.Returns((long?)1L);

        var result = await Sut().Handle(new RechazarRequisicionCompraCommand(1, "motivo"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        requisicion.Estado.Should().Be(EstadoRequisicion.Rechazada);
        requisicion.Observacion.Should().Be("motivo");
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class CancelarRequisicionCompraCommandHandlerTests
{
    private readonly IRequisicionCompraRepository _repo = Substitute.For<IRequisicionCompraRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();

    private CancelarRequisicionCompraCommandHandler Sut() => new(_repo, _uow, _user);

    [Fact]
    public async Task Handle_RequisicionNoExiste_RetornaFailure()
    {
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns((RequisicionCompra?)null);

        var result = await Sut().Handle(new CancelarRequisicionCompraCommand(99), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("no encontrada");
    }

    [Fact]
    public async Task Handle_RequisicionBorrador_LaCancelaYPersiste()
    {
        var requisicion = ComprasHandlerTestHelper.CrearRequisicionBorradorConItems();
        _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(requisicion);
        _user.UserId.Returns((long?)1L);

        var result = await Sut().Handle(new CancelarRequisicionCompraCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        requisicion.Estado.Should().Be(EstadoRequisicion.Cancelada);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class RequisicionCompraStateCommandValidatorTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void EnviarValidator_IdInvalido_RetornaError(long id)
    {
        var validator = new EnviarRequisicionCompraCommandValidator();
        var result = validator.Validate(new EnviarRequisicionCompraCommand(id));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == "Id");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AprobarValidator_IdInvalido_RetornaError(long id)
    {
        var validator = new AprobarRequisicionCompraCommandValidator();
        var result = validator.Validate(new AprobarRequisicionCompraCommand(id));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == "Id");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RechazarValidator_IdInvalido_RetornaError(long id)
    {
        var validator = new RechazarRequisicionCompraCommandValidator();
        var result = validator.Validate(new RechazarRequisicionCompraCommand(id, null));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == "Id");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CancelarValidator_IdInvalido_RetornaError(long id)
    {
        var validator = new CancelarRequisicionCompraCommandValidator();
        var result = validator.Validate(new CancelarRequisicionCompraCommand(id));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == "Id");
    }

    [Fact]
    public void Validators_IdsValidos_Pasan()
    {
        new EnviarRequisicionCompraCommandValidator().Validate(new EnviarRequisicionCompraCommand(1)).IsValid.Should().BeTrue();
        new AprobarRequisicionCompraCommandValidator().Validate(new AprobarRequisicionCompraCommand(1)).IsValid.Should().BeTrue();
        new RechazarRequisicionCompraCommandValidator().Validate(new RechazarRequisicionCompraCommand(1, null)).IsValid.Should().BeTrue();
        new CancelarRequisicionCompraCommandValidator().Validate(new CancelarRequisicionCompraCommand(1)).IsValid.Should().BeTrue();
    }
}

public class CrearCotizacionCompraCommandHandlerTests
{
    private readonly ICotizacionCompraRepository _repo = Substitute.For<ICotizacionCompraRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();

    private CrearCotizacionCompraCommandHandler Sut() => new(_repo, _uow, _user);

    [Fact]
    public async Task Handle_SinItems_RetornaFailure()
    {
        var command = new CrearCotizacionCompraCommand(
            1,
            null,
            10,
            new DateOnly(2026, 3, 20),
            null,
            null,
            []);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("al menos un ítem");
    }

    [Fact]
    public async Task Handle_ConItems_CreaCotizacionYPersiste()
    {
        _user.UserId.Returns((long?)1L);

        var command = new CrearCotizacionCompraCommand(
            1,
            null,
            10,
            new DateOnly(2026, 3, 20),
            new DateOnly(2026, 3, 30),
            "Observacion",
            [new CrearCotizacionItemDto(1, "Item", 2, 100m)]);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).AddAsync(Arg.Any<CotizacionCompra>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class AceptarCotizacionCompraCommandHandlerTests
{
    private readonly ICotizacionCompraRepository _repo = Substitute.For<ICotizacionCompraRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();

    private AceptarCotizacionCompraCommandHandler Sut() => new(_repo, _uow, _user);

    [Fact]
    public async Task Handle_CotizacionNoExiste_RetornaFailure()
    {
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns((CotizacionCompra?)null);

        var result = await Sut().Handle(new AceptarCotizacionCompraCommand(99), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("no encontrada");
    }

    [Fact]
    public async Task Handle_CotizacionPendiente_LaAceptaYPersiste()
    {
        var cotizacion = CrearCotizacionPendiente();
        _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(cotizacion);
        _user.UserId.Returns((long?)1L);

        var result = await Sut().Handle(new AceptarCotizacionCompraCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        cotizacion.Estado.Should().Be(EstadoCotizacionCompra.Aceptada);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static CotizacionCompra CrearCotizacionPendiente()
    {
        var cotizacion = CotizacionCompra.Crear(1, null, 10, new DateOnly(2026, 3, 20), null, null, null);
        cotizacion.AgregarItem(CotizacionCompraItem.Crear(0, 1, "Item", 2, 100));
        return cotizacion;
    }
}

public class RechazarCotizacionCompraCommandHandlerTests
{
    private readonly ICotizacionCompraRepository _repo = Substitute.For<ICotizacionCompraRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();

    private RechazarCotizacionCompraCommandHandler Sut() => new(_repo, _uow, _user);

    [Fact]
    public async Task Handle_CotizacionNoExiste_RetornaFailure()
    {
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns((CotizacionCompra?)null);

        var result = await Sut().Handle(new RechazarCotizacionCompraCommand(99), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("no encontrada");
    }

    [Fact]
    public async Task Handle_CotizacionPendiente_LaRechazaYPersiste()
    {
        var cotizacion = AceptarCotizacionCompraCommandHandlerTests_CrearCotizacionPendiente();
        _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(cotizacion);
        _user.UserId.Returns((long?)1L);

        var result = await Sut().Handle(new RechazarCotizacionCompraCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        cotizacion.Estado.Should().Be(EstadoCotizacionCompra.Rechazada);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static CotizacionCompra AceptarCotizacionCompraCommandHandlerTests_CrearCotizacionPendiente()
    {
        var cotizacion = CotizacionCompra.Crear(1, null, 10, new DateOnly(2026, 3, 20), null, null, null);
        cotizacion.AgregarItem(CotizacionCompraItem.Crear(0, 1, "Item", 2, 100));
        return cotizacion;
    }
}

public class CotizacionCompraStateCommandValidatorTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AceptarValidator_IdInvalido_RetornaError(long id)
    {
        var validator = new AceptarCotizacionCompraCommandValidator();

        var result = validator.Validate(new AceptarCotizacionCompraCommand(id));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == "Id");
    }

    [Fact]
    public void AceptarValidator_IdValido_Pasa()
    {
        var validator = new AceptarCotizacionCompraCommandValidator();

        var result = validator.Validate(new AceptarCotizacionCompraCommand(1));

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RechazarValidator_IdInvalido_RetornaError(long id)
    {
        var validator = new RechazarCotizacionCompraCommandValidator();

        var result = validator.Validate(new RechazarCotizacionCompraCommand(id));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == "Id");
    }

    [Fact]
    public void RechazarValidator_IdValido_Pasa()
    {
        var validator = new RechazarCotizacionCompraCommandValidator();

        var result = validator.Validate(new RechazarCotizacionCompraCommand(1));

        result.IsValid.Should().BeTrue();
    }
}

public class RecibirOrdenCompraCommandHandlerTests
{
    private readonly IRepository<OrdenCompraMeta> _repo = Substitute.For<IRepository<OrdenCompraMeta>>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    private RecibirOrdenCompraCommandHandler Sut() => new(_repo, _uow);

    [Fact]
    public async Task Handle_OrdenNoExiste_RetornaFailure()
    {
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns((OrdenCompraMeta?)null);

        var result = await Sut().Handle(new RecibirOrdenCompraCommand(99), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("No se encontro");
    }

    [Fact]
    public async Task Handle_OrdenPendiente_LaRecibeYPersiste()
    {
        var orden = OrdenCompraMeta.Crear(
            comprobanteId: 1,
            proveedorId: 10,
            fechaEntregaReq: null,
            condicionesEntrega: null,
            cantidadTotal: 1m);
        _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(orden);

        var result = await Sut().Handle(new RecibirOrdenCompraCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        orden.EstadoOc.Should().Be(EstadoOrdenCompra.Recibida);
        _repo.Received(1).Update(orden);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class CancelarOrdenCompraCommandHandlerTests
{
    private readonly IRepository<OrdenCompraMeta> _repo = Substitute.For<IRepository<OrdenCompraMeta>>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    private CancelarOrdenCompraCommandHandler Sut() => new(_repo, _uow);

    [Fact]
    public async Task Handle_OrdenNoExiste_RetornaFailure()
    {
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns((OrdenCompraMeta?)null);

        var result = await Sut().Handle(new CancelarOrdenCompraCommand(99), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("No se encontro");
    }

    [Fact]
    public async Task Handle_OrdenPendiente_LaCancelaYPersiste()
    {
        var orden = OrdenCompraMeta.Crear(
            comprobanteId: 1,
            proveedorId: 10,
            fechaEntregaReq: null,
            condicionesEntrega: null,
            cantidadTotal: 1m);
        _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(orden);

        var result = await Sut().Handle(new CancelarOrdenCompraCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        orden.EstadoOc.Should().Be(EstadoOrdenCompra.Cancelada);
        orden.Habilitada.Should().BeFalse();
        _repo.Received(1).Update(orden);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class OrdenCompraMetaStateCommandValidatorTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RecibirValidator_IdInvalido_RetornaError(long id)
    {
        var validator = new RecibirOrdenCompraCommandValidator();
        var result = validator.Validate(new RecibirOrdenCompraCommand(id));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == "Id");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CancelarValidator_IdInvalido_RetornaError(long id)
    {
        var validator = new CancelarOrdenCompraCommandValidator();
        var result = validator.Validate(new CancelarOrdenCompraCommand(id));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == "Id");
    }

    [Fact]
    public void Validators_IdsValidos_Pasan()
    {
        new RecibirOrdenCompraCommandValidator().Validate(new RecibirOrdenCompraCommand(1)).IsValid.Should().BeTrue();
        new CancelarOrdenCompraCommandValidator().Validate(new CancelarOrdenCompraCommand(1)).IsValid.Should().BeTrue();
    }
}

internal static class ComprasHandlerTestHelper
{
    public static RequisicionCompra CrearRequisicionBorradorConItems()
    {
        var requisicion = RequisicionCompra.Crear(1, 10, new DateOnly(2026, 3, 20), "Req", null, null);
        requisicion.AgregarItem(RequisicionCompraItem.Crear(0, 1, "Item", 2, "UN", null));
        return requisicion;
    }

    public static RequisicionCompra CrearRequisicionEnviada()
    {
        var requisicion = CrearRequisicionBorradorConItems();
        requisicion.Enviar(null);
        return requisicion;
    }
}