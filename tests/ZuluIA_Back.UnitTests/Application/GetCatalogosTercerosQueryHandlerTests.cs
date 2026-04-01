using FluentAssertions;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Application.Features.Terceros.Queries;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.UnitTests.Application;

public class GetCategoriasClientesQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenSoloActivas_ReturnsOnlyActiveNotDeletedOrderedItems()
    {
        var repo = Substitute.For<IRepository<CategoriaCliente>>();
        var activa = CategoriaCliente.Crear("B", "Beta", userId: null);
        var otraActiva = CategoriaCliente.Crear("A", "Alfa", userId: null);
        var inactiva = CategoriaCliente.Crear("C", "Gamma", userId: null);
        inactiva.Desactivar(userId: null);

        repo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IList<CategoriaCliente>>([activa, otraActiva, inactiva]));

        var handler = new GetCategoriasClientesQueryHandler(repo);

        var result = await handler.Handle(new GetCategoriasClientesQuery(SoloActivas: true), CancellationToken.None);

        result.Select(x => x.Codigo).Should().Equal("A", "B");
    }
}

public class GetCategoriasProveedoresQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenSoloActivasIsFalse_ReturnsActiveAndInactiveNotDeletedItems()
    {
        var repo = Substitute.For<IRepository<CategoriaProveedor>>();
        var activa = CategoriaProveedor.Crear("A", "Activa", userId: null);
        var inactiva = CategoriaProveedor.Crear("B", "Inactiva", userId: null);
        inactiva.Desactivar(userId: null);
        var vigente = CategoriaProveedor.Crear("C", "Vigente", userId: null);

        repo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IList<CategoriaProveedor>>([activa, inactiva, vigente]));

        var handler = new GetCategoriasProveedoresQueryHandler(repo);

        var result = await handler.Handle(new GetCategoriasProveedoresQuery(SoloActivas: false), CancellationToken.None);

        result.Select(x => x.Codigo).Should().Equal("A", "C");
    }
}

public class GetEstadosClientesQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenSoloActivos_ReturnsOnlyActiveStates()
    {
        var repo = Substitute.For<IRepository<EstadoCliente>>();
        var habilitado = EstadoCliente.Crear("HAB", "Habilitado", bloquea: false, userId: null);
        var bloqueado = EstadoCliente.Crear("BLO", "Bloqueado", bloquea: true, userId: null);
        bloqueado.Desactivar(userId: null);

        repo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IList<EstadoCliente>>([habilitado, bloqueado]));

        var handler = new GetEstadosClientesQueryHandler(repo);

        var result = await handler.Handle(new GetEstadosClientesQuery(SoloActivos: true), CancellationToken.None);

        result.Select(x => x.Codigo).Should().Equal("HAB");
    }
}

public class GetEstadosProveedoresQueryHandlerTests
{
    [Fact]
    public async Task Handle_OrdersProviderStatesByDescriptionAndCode()
    {
        var repo = Substitute.For<IRepository<EstadoProveedor>>();
        var segundo = EstadoProveedor.Crear("B", "Mismo", bloquea: false, userId: null);
        var primero = EstadoProveedor.Crear("A", "Mismo", bloquea: true, userId: null);
        var ultimo = EstadoProveedor.Crear("C", "Zulu", bloquea: false, userId: null);

        repo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IList<EstadoProveedor>>([segundo, ultimo, primero]));

        var handler = new GetEstadosProveedoresQueryHandler(repo);

        var result = await handler.Handle(new GetEstadosProveedoresQuery(SoloActivos: false), CancellationToken.None);

        result.Select(x => x.Codigo).Should().Equal("A", "B", "C");
    }
}

public class GetCatalogosTercerosQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenSoloActivos_ReturnsGroupedActiveCatalogs()
    {
        var categoriasClientesRepo = Substitute.For<IRepository<CategoriaCliente>>();
        var categoriasProveedoresRepo = Substitute.For<IRepository<CategoriaProveedor>>();
        var estadosClientesRepo = Substitute.For<IRepository<EstadoCliente>>();
        var estadosProveedoresRepo = Substitute.For<IRepository<EstadoProveedor>>();
        var estadosPersonasRepo = Substitute.For<IRepository<EstadoPersonaCatalogo>>();
        var estadosCivilesRepo = Substitute.For<IRepository<EstadoCivilCatalogo>>();
        var tiposDomicilioRepo = Substitute.For<IRepository<TipoDomicilioCatalogo>>();

        var categoriaClienteActiva = CategoriaCliente.Crear("CCA", "Cliente Activo", userId: null);
        var categoriaClienteInactiva = CategoriaCliente.Crear("CCI", "Cliente Inactivo", userId: null);
        categoriaClienteInactiva.Desactivar(userId: null);

        var categoriaProveedorActiva = CategoriaProveedor.Crear("CPA", "Proveedor Activo", userId: null);
        var categoriaProveedorInactiva = CategoriaProveedor.Crear("CPI", "Proveedor Inactivo", userId: null);
        categoriaProveedorInactiva.Desactivar(userId: null);

        var estadoClienteActivo = EstadoCliente.Crear("ECA", "Estado Cliente Activo", bloquea: false, userId: null);
        var estadoClienteInactivo = EstadoCliente.Crear("ECI", "Estado Cliente Inactivo", bloquea: true, userId: null);
        estadoClienteInactivo.Desactivar(userId: null);

        var estadoProveedorActivo = EstadoProveedor.Crear("EPA", "Estado Proveedor Activo", bloquea: false, userId: null);
        var estadoProveedorInactivo = EstadoProveedor.Crear("EPI", "Estado Proveedor Inactivo", bloquea: true, userId: null);
        estadoProveedorInactivo.Desactivar(userId: null);

        var estadoPersonaActivo = EstadoPersonaCatalogo.Crear("Habilitado", userId: null);
        var estadoPersonaInactivo = EstadoPersonaCatalogo.Crear("Suspendido", userId: null);
        estadoPersonaInactivo.Desactivar(userId: null);

        var estadoCivilActivo = EstadoCivilCatalogo.Crear("Soltero/a", userId: null);
        var estadoCivilInactivo = EstadoCivilCatalogo.Crear("Casado/a", userId: null);
        estadoCivilInactivo.Desactivar(userId: null);

        var tipoDomicilio = CreateTipoDomicilio(1, "Legal");

        categoriasClientesRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IList<CategoriaCliente>>([categoriaClienteActiva, categoriaClienteInactiva]));
        categoriasProveedoresRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IList<CategoriaProveedor>>([categoriaProveedorActiva, categoriaProveedorInactiva]));
        estadosClientesRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IList<EstadoCliente>>([estadoClienteActivo, estadoClienteInactivo]));
        estadosProveedoresRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IList<EstadoProveedor>>([estadoProveedorActivo, estadoProveedorInactivo]));
        estadosPersonasRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IList<EstadoPersonaCatalogo>>([estadoPersonaActivo, estadoPersonaInactivo]));
        estadosCivilesRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IList<EstadoCivilCatalogo>>([estadoCivilActivo, estadoCivilInactivo]));
        tiposDomicilioRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IList<TipoDomicilioCatalogo>>([tipoDomicilio]));

        var handler = new GetCatalogosTercerosQueryHandler(
            categoriasClientesRepo,
            categoriasProveedoresRepo,
            estadosClientesRepo,
            estadosProveedoresRepo,
            estadosPersonasRepo,
            estadosCivilesRepo,
            tiposDomicilioRepo);

        var result = await handler.Handle(new GetCatalogosTercerosQuery(SoloActivos: true), CancellationToken.None);

        result.Should().BeEquivalentTo(new CatalogosTercerosDto(
            [new CategoriaTerceroCatalogoDto(categoriaClienteActiva.Id, "CCA", "Cliente Activo", true)],
            [new CategoriaTerceroCatalogoDto(categoriaProveedorActiva.Id, "CPA", "Proveedor Activo", true)],
            [new EstadoTerceroCatalogoDto(estadoClienteActivo.Id, "ECA", "Estado Cliente Activo", false, true)],
            [new EstadoTerceroCatalogoDto(estadoProveedorActivo.Id, "EPA", "Estado Proveedor Activo", false, true)],
            [new EstadoPersonaCatalogoDto(estadoPersonaActivo.Id, "Habilitado", true)],
            [new EstadoCivilCatalogoDto(estadoCivilActivo.Id, "Soltero/a", true)],
            [new TipoDomicilioCatalogoDto(tipoDomicilio.Id, "Legal")]));
    }

    private static TipoDomicilioCatalogo CreateTipoDomicilio(long id, string descripcion)
    {
        var entity = (TipoDomicilioCatalogo)Activator.CreateInstance(typeof(TipoDomicilioCatalogo), nonPublic: true)!;
        typeof(TipoDomicilioCatalogo).GetProperty(nameof(TipoDomicilioCatalogo.Id))!.SetValue(entity, id);
        typeof(TipoDomicilioCatalogo).GetProperty(nameof(TipoDomicilioCatalogo.Descripcion))!.SetValue(entity, descripcion);
        return entity;
    }
}
