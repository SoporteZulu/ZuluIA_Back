using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Features.Terceros.Commands;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Application.Features.Terceros.Queries;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.UnitTests.Api;

public class TercerosCatalogosAliasControllerTests
{
    [Fact]
    public async Task GetCatalogos_CuandoTieneExito_DevuelveOkEInvocaQueryAgregada()
    {
        var mediator = Substitute.For<IMediator>();
        var payload = new CatalogosTercerosDto(
            [new CategoriaTerceroCatalogoDto(1, "CC", "Cliente", true)],
            [new CategoriaTerceroCatalogoDto(2, "CP", "Proveedor", true)],
            [new EstadoTerceroCatalogoDto(3, "EC", "Estado cliente", false, true)],
            [new EstadoTerceroCatalogoDto(4, "EP", "Estado proveedor", true, true)],
            [],
            [new EstadoCivilCatalogoDto(5, "Soltero/a", true)],
            []);
        mediator.Send(Arg.Any<GetCatalogosTercerosQuery>(), Arg.Any<CancellationToken>())
            .Returns(payload);
        var controller = new TercerosController(mediator);

        var result = await controller.GetCatalogos(soloActivos: true, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(payload);
        await mediator.Received(1).Send(
            Arg.Is<GetCatalogosTercerosQuery>(x => x.SoloActivos),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetConfiguracionClientes_CuandoTieneExito_DevuelveOkEInvocaQueryAgregada()
    {
        var mediator = Substitute.For<IMediator>();
        var payload = new ConfiguracionClientesTercerosDto(
            new CatalogosTercerosDto([], [], [], [], [], [], []),
            [new ConfiguracionClienteReferenciaDto(1, "Mayorista")],
            [new ConfiguracionClienteCodigoDescripcionDto(2, "1", "Responsable Inscripto")],
            [new ConfiguracionClienteCodigoDescripcionDto(3, "80", "CUIT")],
            [new ConfiguracionClienteMonedaDto(4, "ARS", "Peso", "$", false)],
            [new ConfiguracionClienteReferenciaDto(5, "Argentina")],
            [new ConfiguracionClienteCodigoDescripcionDto(6, "NORTE", "Zona Norte")],
            [new ConfiguracionClienteSucursalDto(7, "Casa Central", null, true)],
            [new ConfiguracionClienteUsuarioComercialDto(8, "vendedor.norte", "Vendedor Norte", "vendedor@zuluia.local")],
            [new ConfiguracionClienteUsuarioComercialDto(9, "cobrador.norte", "Cobrador Norte", "cobrador@zuluia.local")]);
        mediator.Send(Arg.Any<GetConfiguracionClientesTercerosQuery>(), Arg.Any<CancellationToken>())
            .Returns(payload);
        var controller = new TercerosController(mediator);

        var result = await controller.GetConfiguracionClientes(soloActivos: true, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(payload);
        await mediator.Received(1).Send(
            Arg.Is<GetConfiguracionClientesTercerosQuery>(x => x.SoloActivos),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCuentaCorriente_CuandoTieneExito_DevuelveResultadoDelQuery()
    {
        var mediator = Substitute.For<IMediator>();
        var payload = Result.Success(new TerceroCuentaCorrienteDto
        {
            LimiteSaldo = 100m,
            LimiteCreditoTotal = 250m
        });
        mediator.Send(Arg.Any<GetTerceroCuentaCorrienteQuery>(), Arg.Any<CancellationToken>())
            .Returns(payload);
        var controller = new TercerosController(mediator);

        var result = await controller.GetCuentaCorriente(42, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(payload.Value);
        await mediator.Received(1).Send(
            Arg.Is<GetTerceroCuentaCorrienteQuery>(x => x.TerceroId == 42),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpsertCuentaCorriente_CuandoIdsNoCoinciden_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = new TercerosController(mediator);

        var result = await controller.UpsertCuentaCorriente(
            42,
            new UpsertTerceroCuentaCorrienteCommand(99, 100m, null, null, 250m, null, null),
            CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
        await mediator.DidNotReceive().Send(Arg.Any<UpsertTerceroCuentaCorrienteCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateCategoriaCliente_CuandoTieneExito_DevuelveCreatedEInvocaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateCategoriaClienteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(11L));
        var controller = new TercerosController(mediator);

        var result = await controller.CreateCategoriaCliente(new CategoriaClienteRequest("CCAPI", "Categoria cliente smoke"), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(TercerosController.GetCategoriasClientes));
        created.Value.Should().BeEquivalentTo(new { id = 11L });
        await mediator.Received(1).Send(
            Arg.Is<CreateCategoriaClienteCommand>(x => x.Codigo == "CCAPI" && x.Descripcion == "Categoria cliente smoke"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateEstadoCliente_CuandoTieneExito_DevuelveCreatedEInvocaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateEstadoClienteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(12L));
        var controller = new TercerosController(mediator);

        var result = await controller.CreateEstadoCliente(new EstadoClienteRequest("ECAPI", "Estado cliente smoke", false), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(TercerosController.GetEstadosClientes));
        created.Value.Should().BeEquivalentTo(new { id = 12L });
        await mediator.Received(1).Send(
            Arg.Is<CreateEstadoClienteCommand>(x => x.Codigo == "ECAPI" && x.Descripcion == "Estado cliente smoke" && x.Bloquea == false),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateCategoriaProveedor_CuandoTieneExito_DevuelveCreatedEInvocaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateCategoriaProveedorCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(13L));
        var controller = new TercerosController(mediator);

        var result = await controller.CreateCategoriaProveedor(new CategoriaProveedorRequest("CPAPI", "Categoria proveedor smoke"), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(TercerosController.GetCategoriasProveedores));
        created.Value.Should().BeEquivalentTo(new { id = 13L });
        await mediator.Received(1).Send(
            Arg.Is<CreateCategoriaProveedorCommand>(x => x.Codigo == "CPAPI" && x.Descripcion == "Categoria proveedor smoke"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateEstadoProveedor_CuandoTieneExito_DevuelveCreatedEInvocaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateEstadoProveedorCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(14L));
        var controller = new TercerosController(mediator);

        var result = await controller.CreateEstadoProveedor(new EstadoProveedorRequest("EPAPI", "Estado proveedor smoke", true), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(TercerosController.GetEstadosProveedores));
        created.Value.Should().BeEquivalentTo(new { id = 14L });
        await mediator.Received(1).Send(
            Arg.Is<CreateEstadoProveedorCommand>(x => x.Codigo == "EPAPI" && x.Descripcion == "Estado proveedor smoke" && x.Bloquea),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetEstadosCiviles_CuandoTieneExito_DevuelveOkEInvocaQuery()
    {
        var mediator = Substitute.For<IMediator>();
        var payload = new[] { new EstadoCivilCatalogoDto(8, "Viudo/a", true) };
        mediator.Send(Arg.Any<GetEstadosCivilesQuery>(), Arg.Any<CancellationToken>())
            .Returns(payload);
        var controller = new TercerosController(mediator);

        var result = await controller.GetEstadosCiviles(soloActivos: true, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(payload);
        await mediator.Received(1).Send(
            Arg.Is<GetEstadosCivilesQuery>(x => x.SoloActivos),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateEstadoCivil_CuandoTieneExito_DevuelveCreatedEInvocaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateEstadoCivilCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(15L));
        var controller = new TercerosController(mediator);

        var result = await controller.CreateEstadoCivil(new EstadoCivilCatalogoRequest("Divorciado/a"), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(TercerosController.GetEstadosCiviles));
        created.Value.Should().BeEquivalentTo(new { id = 15L });
        await mediator.Received(1).Send(
            Arg.Is<CreateEstadoCivilCommand>(x => x.Descripcion == "Divorciado/a"),
            Arg.Any<CancellationToken>());
    }
}
