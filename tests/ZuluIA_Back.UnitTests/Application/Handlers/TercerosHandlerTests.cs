using AutoMapper;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Terceros.Commands;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Application.Features.Terceros.Queries;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.Entities.Geografia;
using ZuluIA_Back.Domain.Entities.Sucursales;
using ZuluIA_Back.Domain.Entities.Usuarios;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Application.Handlers;

// ── CreateTerceroCommandHandler ───────────────────────────────────────────────

public class CreateTerceroCommandHandlerTests
{
    private readonly ITerceroRepository _repo = Substitute.For<ITerceroRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private CreateTerceroCommandHandler Sut() => new(_repo, _uow, _user, _db);

    private static CreateTerceroCommand ValidCommand() => new(
        Legajo: "CLI001", RazonSocial: "Empresa SA", NombreFantasia: null,
        TipoPersoneria: null, Nombre: null, Apellido: null, Tratamiento: null, Profesion: null,
        EstadoPersonaId: null, EstadoCivilId: null, EstadoCivil: null, Nacionalidad: null, Sexo: null,
        FechaNacimiento: null, FechaRegistro: null, EsEntidadGubernamental: false,
        ClaveFiscal: null, ValorClaveFiscal: null,
        TipoDocumentoId: 1, NroDocumento: "30-11111111-1", CondicionIvaId: 1,
        EsCliente: true, EsProveedor: false, EsEmpleado: false,
        Calle: null, Nro: null, Piso: null, Dpto: null, CodigoPostal: null,
        PaisId: null, ProvinciaId: null, LocalidadId: null, BarrioId: null, NroIngresosBrutos: null, NroMunicipal: null,
        Telefono: null, Celular: null, Email: null, Web: null,
        MonedaId: null, CategoriaId: null, CategoriaClienteId: null, EstadoClienteId: null,
        CategoriaProveedorId: null, EstadoProveedorId: null, LimiteCredito: null,
        PorcentajeMaximoDescuento: null, VigenciaCreditoDesde: null, VigenciaCreditoHasta: null,
        Facturable: true, CobradorId: null, AplicaComisionCobrador: false, PctComisionCobrador: 0,
        VendedorId: null, AplicaComisionVendedor: false, PctComisionVendedor: 0,
        Observacion: null, SucursalId: null, PerfilComercial: null, Domicilios: null, Contactos: null,
        SucursalesEntrega: null, Transportes: null, VentanasCobranza: null);

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
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private UpdateTerceroCommandHandler Sut() => new(_repo, _uow, _user, _db);

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
        TipoPersoneria: null, Nombre: null, Apellido: null, Tratamiento: null, Profesion: null,
        EstadoPersonaId: null, EstadoCivilId: null, EstadoCivil: null, Nacionalidad: null, Sexo: null,
        FechaNacimiento: null, FechaRegistro: null, EsEntidadGubernamental: false,
        ClaveFiscal: null, ValorClaveFiscal: null,
        NroDocumento: null, CondicionIvaId: 1,
        EsCliente: true, EsProveedor: false, EsEmpleado: false,
        Calle: null, Nro: null, Piso: null, Dpto: null, CodigoPostal: null,
        PaisId: null, ProvinciaId: null, LocalidadId: null, BarrioId: null, NroIngresosBrutos: null, NroMunicipal: null,
        Telefono: null, Celular: null, Email: null, Web: null,
        MonedaId: null, CategoriaId: null, CategoriaClienteId: null, EstadoClienteId: null,
        CategoriaProveedorId: null, EstadoProveedorId: null, LimiteCredito: null,
        PorcentajeMaximoDescuento: null, VigenciaCreditoDesde: null, VigenciaCreditoHasta: null,
        Facturable: true, CobradorId: null, AplicaComisionCobrador: false, PctComisionCobrador: 0,
        VendedorId: null, AplicaComisionVendedor: false, PctComisionVendedor: 0,
        Observacion: null, PerfilComercial: null, Domicilios: null, Contactos: null, SucursalesEntrega: null,
        Transportes: null, VentanasCobranza: null);
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

    private void ConfigureEmptyLookupSets()
    {
        _db.CondicionesIva.Returns(MockDbSetHelper.CreateMockDbSet<CondicionIva>());
        _db.Paises.Returns(MockDbSetHelper.CreateMockDbSet<Pais>());
        _db.CategoriasTerceros.Returns(MockDbSetHelper.CreateMockDbSet<CategoriaTercero>());
        _db.Monedas.Returns(MockDbSetHelper.CreateMockDbSet<Moneda>());
        _db.Sucursales.Returns(MockDbSetHelper.CreateMockDbSet<Sucursal>());
        _db.TercerosPerfilesComerciales.Returns(MockDbSetHelper.CreateMockDbSet<TerceroPerfilComercial>());
        _db.EstadosCiviles.Returns(MockDbSetHelper.CreateMockDbSet<EstadoCivilCatalogo>());
        _db.EstadosPersonas.Returns(MockDbSetHelper.CreateMockDbSet<EstadoPersonaCatalogo>());
        _db.Localidades.Returns(MockDbSetHelper.CreateMockDbSet<Localidad>());
        _db.Barrios.Returns(MockDbSetHelper.CreateMockDbSet<Barrio>());
        _db.Provincias.Returns(MockDbSetHelper.CreateMockDbSet<Provincia>());
        _db.CategoriasClientes.Returns(MockDbSetHelper.CreateMockDbSet<CategoriaCliente>());
        _db.EstadosClientes.Returns(MockDbSetHelper.CreateMockDbSet<EstadoCliente>());
        _db.CategoriasProveedores.Returns(MockDbSetHelper.CreateMockDbSet<CategoriaProveedor>());
        _db.EstadosProveedores.Returns(MockDbSetHelper.CreateMockDbSet<EstadoProveedor>());
    }

    [Fact]
    public async Task Handle_SinResultados_RetornaPaginaVacia()
    {
        ConfigureEmptyLookupSets();

        var empty = new PagedResult<Tercero>([], 1, 10, 0);
        _repo.GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(),
            Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<bool?>(), Arg.Any<bool?>(),
            Arg.Any<bool?>(), Arg.Any<long?>(), Arg.Any<long?>(), Arg.Any<long?>(),
            Arg.Any<long?>(), Arg.Any<long?>(), Arg.Any<long?>(), Arg.Any<long?>(), Arg.Any<long?>(),
            Arg.Any<CancellationToken>()).Returns(empty);

        var result = await Sut().Handle(
            new GetTercerosPagedQuery(1, 10, null, null, null, null, null, null, null, null, null, null),
            CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ConUsuarioCliente_RetornaResumenDirectoDeAccesoUsuarioYGrupo()
    {
        ConfigureEmptyLookupSets();

        var usuarioCliente = Usuario.Crear("cliente-web", "Cliente Web", "cliente@demo.com", null, null);
        var usuarioGrupo = Usuario.Crear("grupo-clientes", "Grupo Clientes", "grupo@demo.com", null, null);
        var cobrador = Usuario.Crear("cobra-uno", "Cobrador Uno", "cobrador@demo.com", null, null);
        var vendedor = Usuario.Crear("vende-uno", "Vendedor Uno", "vendedor@demo.com", null, null);
        var categoria = CategoriaTercero.Crear("Distribuidor");
        var moneda = CreateMoneda(31, "ARS", "Pesos", "$", false);
        var sucursal = Sucursal.Crear("Casa Central", "30709999888", 1, moneda.Id, 1, true, null);

        typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id))!.SetValue(usuarioCliente, 11L);
        typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id))!.SetValue(usuarioGrupo, 21L);
        typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id))!.SetValue(cobrador, 31L);
        typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id))!.SetValue(vendedor, 41L);
        typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id))!.SetValue(categoria, 51L);
        typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id))!.SetValue(sucursal, 61L);

        _db.Usuarios.Returns(MockDbSetHelper.CreateMockDbSet<Usuario>([usuarioCliente, usuarioGrupo, cobrador, vendedor]));
        _db.UsuariosXUsuario.Returns(MockDbSetHelper.CreateMockDbSet<UsuarioXUsuario>([
            UsuarioXUsuario.Crear(usuarioCliente.Id, usuarioGrupo.Id)
        ]));
        _db.CategoriasTerceros.Returns(MockDbSetHelper.CreateMockDbSet<CategoriaTercero>([categoria]));
        _db.Monedas.Returns(MockDbSetHelper.CreateMockDbSet<Moneda>([moneda]));
        _db.Sucursales.Returns(MockDbSetHelper.CreateMockDbSet<Sucursal>([sucursal]));

        var tercero = Tercero.Crear("CLI001", "Cliente Uno", 1, "30712345678", 1, true, false, false, null, null);
        typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id))!.SetValue(tercero, 71L);
        var perfilComercial = TerceroPerfilComercial.Crear(tercero.Id, null);
        perfilComercial.ActualizarCuentaCorriente(50000m, new DateOnly(2025, 2, 1), new DateOnly(2025, 11, 30), null);

        _db.TercerosPerfilesComerciales.Returns(MockDbSetHelper.CreateMockDbSet<TerceroPerfilComercial>([perfilComercial]));

        tercero.SetUsuario(usuarioCliente.Id, null);
        tercero.SetCategoria(categoria.Id);
        tercero.SetMoneda(moneda.Id);
        tercero.SetSucursal(sucursal.Id);
        tercero.Actualizar(
            razonSocial: tercero.RazonSocial,
            nombreFantasia: null,
            condicionIvaId: tercero.CondicionIvaId,
            telefono: null,
            celular: null,
            email: null,
            web: null,
            domicilio: tercero.Domicilio,
            nroIngresosBrutos: null,
            nroMunicipal: null,
            limiteCredito: 150000m,
            porcentajeMaximoDescuento: null,
            vigenciaCreditoDesde: new DateOnly(2025, 1, 1),
            vigenciaCreditoHasta: new DateOnly(2025, 12, 31),
            facturable: true,
            cobradorId: cobrador.Id,
            aplicaComisionCobrador: false,
            pctComisionCobrador: 0m,
            vendedorId: vendedor.Id,
            aplicaComisionVendedor: false,
            pctComisionVendedor: 0m,
            observacion: null,
            userId: null);

        var paged = new PagedResult<Tercero>([tercero], 1, 10, 1);
        _repo.GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(),
            Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<bool?>(), Arg.Any<bool?>(),
            Arg.Any<bool?>(), Arg.Any<long?>(), Arg.Any<long?>(), Arg.Any<long?>(),
            Arg.Any<long?>(), Arg.Any<long?>(), Arg.Any<long?>(), Arg.Any<long?>(), Arg.Any<long?>(),
            Arg.Any<CancellationToken>()).Returns(paged);

        _mapper.Map<IReadOnlyList<TerceroListDto>>(paged.Items).Returns([
            new TerceroListDto { Id = tercero.Id, Legajo = tercero.Legajo, RazonSocial = tercero.RazonSocial }
        ]);

        var result = await Sut().Handle(new GetTercerosPagedQuery(Page: 1, PageSize: 10), CancellationToken.None);

        result.Items.Should().ContainSingle();
        result.Items[0].AccesoUsuarioCliente.Should().BeTrue();
        result.Items[0].TieneUsuarioCliente.Should().BeTrue();
        result.Items[0].UsuarioClienteActivo.Should().BeTrue();
        result.Items[0].UsuarioClienteUserName.Should().Be("cliente-web");
        result.Items[0].UsuarioClienteGrupoUserName.Should().Be("grupo-clientes");
        result.Items[0].CategoriaDescripcion.Should().Be("Distribuidor");
        result.Items[0].MonedaDescripcion.Should().Be("Pesos");
        result.Items[0].LimiteCreditoResumen.Should().Be("150000");
        result.Items[0].LimiteSaldoResumen.Should().Be("50000");
        result.Items[0].VigenciaCreditoResumen.Should().Be("2025-01-01 a 2025-12-31");
        result.Items[0].VigenciaLimiteSaldoResumen.Should().Be("2025-02-01 a 2025-11-30");
        result.Items[0].SucursalDescripcion.Should().Be("Casa Central");
        result.Items[0].CobradorUserName.Should().Be("cobra-uno");
        result.Items[0].CobradorNombre.Should().Be("Cobrador Uno");
        result.Items[0].VendedorUserName.Should().Be("vende-uno");
        result.Items[0].VendedorNombre.Should().Be("Vendedor Uno");
        result.Items[0].EstadoVisibleDescripcion.Should().Be(result.Items[0].EstadoOperativoDescripcion);
        result.Items[0].EstadoVisibleBloquea.Should().Be(result.Items[0].EstadoOperativoBloquea);
    }

    private static Moneda CreateMoneda(long id, string codigo, string descripcion, string simbolo, bool sinDecimales)
    {
        var entity = (Moneda)Activator.CreateInstance(typeof(Moneda), nonPublic: true)!;
        typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id))!.SetValue(entity, id);
        typeof(Moneda).GetProperty(nameof(Moneda.Codigo))!.SetValue(entity, codigo);
        typeof(Moneda).GetProperty(nameof(Moneda.Descripcion))!.SetValue(entity, descripcion);
        typeof(Moneda).GetProperty(nameof(Moneda.Simbolo))!.SetValue(entity, simbolo);
        typeof(Moneda).GetProperty(nameof(Moneda.SinDecimales))!.SetValue(entity, sinDecimales);
        typeof(Moneda).GetProperty(nameof(Moneda.Activa))!.SetValue(entity, true);
        return entity;
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
    private readonly ILogger<GetTerceroByIdQueryHandler> _logger = Substitute.For<ILogger<GetTerceroByIdQueryHandler>>();
    private GetTerceroByIdQueryHandler Sut() => new(_repo, _db, _mapper, _logger);

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
    private readonly ISender _sender = Substitute.For<ISender>();
    private GetTerceroByLegajoQueryHandler Sut() => new(_repo, _sender);

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
        var dto = new TerceroDto();
        _sender.Send(Arg.Is<GetTerceroByIdQuery>(q => q.Id == tercero.Id), Arg.Any<CancellationToken>())
            .Returns(Result.Success(dto));

        var result = await Sut().Handle(new GetTerceroByLegajoQuery("CLI001"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}

// ── GetClientesActivosQueryHandler ────────────────────────────────────────────

public class GetClientesActivosQueryHandlerTests
{
    private readonly ITerceroRepository _repo = Substitute.For<ITerceroRepository>();
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetClientesActivosQueryHandler Sut() => new(_repo, _db, _mapper);

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
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetProveedoresActivosQueryHandler Sut() => new(_repo, _db, _mapper);

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
