using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Terceros.Commands;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Application.Features.Terceros.Queries;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.Domain.Entities.Geografia;
using ZuluIA_Back.Domain.Entities.Usuarios;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Application.Handlers;

// ── CreateTerceroCommandHandler ───────────────────────────────────────────────

public class CreateTerceroCommandHandlerTests
{
    private readonly ITerceroRepository _repo = Substitute.For<ITerceroRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private CreateTerceroCommandHandler Sut() => new(_repo, _uow, _user);

    private static CreateTerceroCommand ValidCommand() => new(
        Legajo: "CLI001", RazonSocial: "Empresa SA", NombreFantasia: null,
        TipoDocumentoId: 1, NroDocumento: "30-11111111-1", CondicionIvaId: 1,
        EsCliente: true, EsProveedor: false, EsEmpleado: false,
        Calle: null, Nro: null, Piso: null, Dpto: null, CodigoPostal: null,
        LocalidadId: null, BarrioId: null, NroIngresosBrutos: null, NroMunicipal: null,
        Telefono: null, Celular: null, Email: null, Web: null,
        MonedaId: null, CategoriaId: null, LimiteCredito: null, Facturable: true,
        CobradorId: null, PctComisionCobrador: 0, VendedorId: null, PctComisionVendedor: 0,
        Observacion: null, SucursalId: null);

    [Fact]
    public async Task Handle_LegajoDuplicado_RetornaFailure()
    {
        _repo.ExisteLegajoAsync(Arg.Any<string>(), Arg.Any<long?>(), Arg.Any<CancellationToken>())
             .Returns(true);

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("legajo");
    }

    [Fact]
    public async Task Handle_NroDocumentoDuplicado_RetornaFailure()
    {
        _repo.ExisteLegajoAsync(Arg.Any<string>(), Arg.Any<long?>(), Arg.Any<CancellationToken>())
             .Returns(false);
        _repo.ExisteNroDocumentoAsync(Arg.Any<string>(), Arg.Any<long?>(), Arg.Any<CancellationToken>())
             .Returns(true);

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_DatosValidos_CreaTerceroYRetornaId()
    {
        _repo.ExisteLegajoAsync(Arg.Any<string>(), Arg.Any<long?>(), Arg.Any<CancellationToken>())
             .Returns(false);
        _repo.ExisteNroDocumentoAsync(Arg.Any<string>(), Arg.Any<long?>(), Arg.Any<CancellationToken>())
             .Returns(false);
        _user.UserId.Returns((long?)1L);

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).AddAsync(Arg.Any<Tercero>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── UpdateTerceroCommandHandler ───────────────────────────────────────────────

public class UpdateTerceroCommandHandlerTests
{
    private readonly ITerceroRepository _repo = Substitute.For<ITerceroRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private UpdateTerceroCommandHandler Sut() => new(_repo, _uow, _user);

    [Fact]
    public async Task Handle_TerceroNoExiste_RetornaFailure()
    {
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
             .Returns((Tercero?)null);

        var result = await Sut().Handle(BuildCommand(1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_TerceroExisteYDatosValidos_ActualizaYRetornaSuccess()
    {
        var tercero = Tercero.Crear("CLI001", "Empresa SA", 1, "30-11111111-1", 1,
                                    true, false, false, null, null);
        _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(tercero);
        _repo.ExisteNroDocumentoAsync(Arg.Any<string>(), Arg.Any<long?>(), Arg.Any<CancellationToken>())
             .Returns(false);
        _repo.TieneEmpleadoActivoAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
             .Returns(false);
        _user.UserId.Returns((long?)1L);

        var result = await Sut().Handle(BuildCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repo.Received(1).Update(Arg.Any<Tercero>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static UpdateTerceroCommand BuildCommand(long id) => new(
        Id: id, RazonSocial: "Empresa SA Updated", NombreFantasia: null,
        NroDocumento: null, CondicionIvaId: 1,
        EsCliente: true, EsProveedor: false, EsEmpleado: false,
        Calle: null, Nro: null, Piso: null, Dpto: null, CodigoPostal: null,
        LocalidadId: null, BarrioId: null, NroIngresosBrutos: null, NroMunicipal: null,
        Telefono: null, Celular: null, Email: null, Web: null,
        MonedaId: null, CategoriaId: null, LimiteCredito: null, Facturable: true,
        CobradorId: null, PctComisionCobrador: 0, VendedorId: null, PctComisionVendedor: 0,
        Observacion: null);
}

// ── DeleteTerceroCommandHandler ───────────────────────────────────────────────

public class DeleteTerceroCommandHandlerTests
{
    private readonly ITerceroRepository _repo = Substitute.For<ITerceroRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private DeleteTerceroCommandHandler Sut() => new(_repo, _uow, _user);

    [Fact]
    public async Task Handle_TerceroNoExiste_RetornaFailure()
    {
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
             .Returns((Tercero?)null);

        var result = await Sut().Handle(new DeleteTerceroCommand(99), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_TerceroYaDadoDeBaja_RetornaFailure()
    {
        var tercero = Tercero.Crear("CLI001", "Empresa SA", 1, "30-11111111-1", 1,
                                    true, false, false, null, null);
        tercero.Desactivar(null);
        _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(tercero);

        var result = await Sut().Handle(new DeleteTerceroCommand(1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_TerceroConComprobantes_RetornaFailure()
    {
        var tercero = Tercero.Crear("CLI001", "Empresa SA", 1, "30-11111111-1", 1,
                                    true, false, false, null, null);
        _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(tercero);
        _repo.TieneComprobantesAsync(1, Arg.Any<CancellationToken>()).Returns(true);

        var result = await Sut().Handle(new DeleteTerceroCommand(1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("comprobantes");
    }

    [Fact]
    public async Task Handle_TerceroSinDependencias_DesactivaYPersiste()
    {
        var tercero = Tercero.Crear("CLI001", "Empresa SA", 1, "30-11111111-1", 1,
                                    true, false, false, null, null);
        _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(tercero);
        _repo.TieneComprobantesAsync(1, Arg.Any<CancellationToken>()).Returns(false);
        _repo.TieneMovimientosCuentaCorrienteAsync(1, Arg.Any<CancellationToken>()).Returns(false);
        _user.UserId.Returns((long?)1L);

        var result = await Sut().Handle(new DeleteTerceroCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repo.Received(1).Update(Arg.Any<Tercero>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── GetTercerosPagedQueryHandler ──────────────────────────────────────────────

public class GetTercerosPagedQueryHandlerTests
{
    private readonly ITerceroRepository _repo = Substitute.For<ITerceroRepository>();
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetTercerosPagedQueryHandler Sut() => new(_repo, _db, _mapper);

    [Fact]
    public async Task Handle_SinResultados_RetornaPaginaVacia()
    {
        var condIvaSet = MockDbSetHelper.CreateMockDbSet<CondicionIva>();
        var locSet = MockDbSetHelper.CreateMockDbSet<Localidad>();
        _db.CondicionesIva.Returns(condIvaSet);
        _db.Localidades.Returns(locSet);

        var empty = new PagedResult<Tercero>([], 1, 10, 0);
        _repo.GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(),
            Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<bool?>(), Arg.Any<bool?>(),
            Arg.Any<bool?>(), Arg.Any<long?>(), Arg.Any<long?>(), Arg.Any<long?>(),
            Arg.Any<CancellationToken>()).Returns(empty);

        var result = await Sut().Handle(
            new GetTercerosPagedQuery(1, 10, null, null, null, null, null, null, null, null),
            CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}

// ── ActivarTerceroCommandHandler ──────────────────────────────────────────────

public class ActivarTerceroCommandHandlerTests
{
    private readonly ITerceroRepository _repo = Substitute.For<ITerceroRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private ActivarTerceroCommandHandler Sut() => new(_repo, _uow, _user);

    private static Tercero CrearTercero() =>
        Tercero.Crear("CLI001", "Empresa Test", 1, "20-11111111-1", 1,
                      true, false, false, null, null);

    [Fact]
    public async Task Handle_TerceroNoExiste_RetornaFailure()
    {
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
             .Returns((Tercero?)null);

        var result = await Sut().Handle(new ActivarTerceroCommand(99), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_TerceroYaActivo_RetornaFailure()
    {
        var tercero = CrearTercero(); // Activo = true, IsDeleted = false
        _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(tercero);

        var result = await Sut().Handle(new ActivarTerceroCommand(1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("activo");
    }

    [Fact]
    public async Task Handle_TerceroDesactivado_ReactivaYRetornaSuccess()
    {
        var tercero = CrearTercero();
        tercero.Desactivar(null); // Marca IsDeleted = true, Activo = false
        _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(tercero);
        _user.UserId.Returns((long?)1L);

        var result = await Sut().Handle(new ActivarTerceroCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        tercero.Activo.Should().BeTrue();
        _repo.Received(1).Update(tercero);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

// ── GetTerceroByIdQueryHandler ────────────────────────────────────────────────

public class GetTerceroByIdQueryHandlerTests
{
    private readonly ITerceroRepository _repo = Substitute.For<ITerceroRepository>();
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetTerceroByIdQueryHandler Sut() => new(_repo, _db, _mapper);

    [Fact]
    public async Task Handle_TerceroNoExiste_RetornaFailure()
    {
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
             .Returns((Tercero?)null);

        var result = await Sut().Handle(new GetTerceroByIdQuery(99), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_TerceroExiste_RetornaSuccess()
    {
        var tercero = Tercero.Crear("CLI001", "Empresa Test", 1, "20-11111111-1", 1,
                                   true, false, false, null, null);
        _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(tercero);
        var dto = new TerceroDto();
        _mapper.Map<TerceroDto>(tercero).Returns(dto);
        var mockTiposDocumento71 = MockDbSetHelper.CreateMockDbSet<TipoDocumento>();
        _db.TiposDocumento.Returns(mockTiposDocumento71);
        var mockCondicionesIva72 = MockDbSetHelper.CreateMockDbSet<CondicionIva>();
        _db.CondicionesIva.Returns(mockCondicionesIva72);
        var mockUsuarios73 = MockDbSetHelper.CreateMockDbSet<Usuario>();
        _db.Usuarios.Returns(mockUsuarios73);

        var result = await Sut().Handle(new GetTerceroByIdQuery(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}

// ── GetTerceroByLegajoQueryHandler ────────────────────────────────────────────

public class GetTerceroByLegajoQueryHandlerTests
{
    private readonly ITerceroRepository _repo = Substitute.For<ITerceroRepository>();
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetTerceroByLegajoQueryHandler Sut() => new(_repo, _db, _mapper);

    [Fact]
    public async Task Handle_LegajoVacio_RetornaFailure()
    {
        var result = await Sut().Handle(new GetTerceroByLegajoQuery("  "), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_LegajoNoEncontrado_RetornaFailure()
    {
        _repo.GetByLegajoAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
             .Returns((Tercero?)null);

        var result = await Sut().Handle(new GetTerceroByLegajoQuery("NODEX"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("NODEX");
    }

    [Fact]
    public async Task Handle_LegajoEncontrado_RetornaSuccess()
    {
        var tercero = Tercero.Crear("CLI001", "Empresa Test", 1, "20-11111111-1", 1,
                                   true, false, false, null, null);
        _repo.GetByLegajoAsync("CLI001", Arg.Any<CancellationToken>()).Returns(tercero);
        _repo.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>()).Returns(tercero);
        var dto = new TerceroDto();
        _mapper.Map<TerceroDto>(tercero).Returns(dto);
        var mockTiposDocumento74 = MockDbSetHelper.CreateMockDbSet<TipoDocumento>();
        _db.TiposDocumento.Returns(mockTiposDocumento74);
        var mockCondicionesIva75 = MockDbSetHelper.CreateMockDbSet<CondicionIva>();
        _db.CondicionesIva.Returns(mockCondicionesIva75);
        var mockUsuarios76 = MockDbSetHelper.CreateMockDbSet<Usuario>();
        _db.Usuarios.Returns(mockUsuarios76);

        var result = await Sut().Handle(new GetTerceroByLegajoQuery("CLI001"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}

// ── GetClientesActivosQueryHandler ────────────────────────────────────────────

public class GetClientesActivosQueryHandlerTests
{
    private readonly ITerceroRepository _repo = Substitute.For<ITerceroRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetClientesActivosQueryHandler Sut() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_RetornaListaMapeada()
    {
        var clientes = (IReadOnlyList<Tercero>)Array.Empty<Tercero>();
        var dtos = (IReadOnlyList<TerceroSelectorDto>)Array.Empty<TerceroSelectorDto>();
        _repo.GetClientesActivosAsync(Arg.Any<long?>(), Arg.Any<CancellationToken>()).Returns(clientes);
        _mapper.Map<IReadOnlyList<TerceroSelectorDto>>(clientes).Returns(dtos);

        var result = await Sut().Handle(new GetClientesActivosQuery(null), CancellationToken.None);

        result.Should().BeEmpty();
        await _repo.Received(1).GetClientesActivosAsync(null, Arg.Any<CancellationToken>());
    }
}

// ── GetProveedoresActivosQueryHandler ─────────────────────────────────────────

public class GetProveedoresActivosQueryHandlerTests
{
    private readonly ITerceroRepository _repo = Substitute.For<ITerceroRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetProveedoresActivosQueryHandler Sut() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_RetornaListaMapeada()
    {
        var proveedores = (IReadOnlyList<Tercero>)Array.Empty<Tercero>();
        var dtos = (IReadOnlyList<TerceroSelectorDto>)Array.Empty<TerceroSelectorDto>();
        _repo.GetProveedoresActivosAsync(Arg.Any<long?>(), Arg.Any<CancellationToken>()).Returns(proveedores);
        _mapper.Map<IReadOnlyList<TerceroSelectorDto>>(proveedores).Returns(dtos);

        var result = await Sut().Handle(new GetProveedoresActivosQuery(null), CancellationToken.None);

        result.Should().BeEmpty();
        await _repo.Received(1).GetProveedoresActivosAsync(null, Arg.Any<CancellationToken>());
    }
}
