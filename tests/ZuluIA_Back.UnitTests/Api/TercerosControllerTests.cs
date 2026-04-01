using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Features.Terceros.Commands;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Application.Features.Terceros.Queries;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.UnitTests.Api;

public class TercerosControllerTests
{
    [Fact]
    public async Task GetAll_CuandoHayDatos_DevuelveOkConPaginado()
    {
        var mediator = Substitute.For<IMediator>();
        var paged = new PagedResult<TerceroListDto>(
        [
            new TerceroListDto { Id = 1, Legajo = "CLI001", RazonSocial = "Cliente Uno", Activo = true, RolDisplay = "Cliente" },
            new TerceroListDto { Id = 2, Legajo = "PRO001", RazonSocial = "Proveedor Uno", Activo = true, RolDisplay = "Proveedor" }
        ],
        2,
        25,
        70);
        mediator.Send(Arg.Any<GetTercerosPagedQuery>(), Arg.Any<CancellationToken>())
            .Returns(paged);
        var controller = CreateController(mediator);

        var result = await controller.GetAll(2, 25, "garcia", true, false, null, true, 3, 4, 5, null, null, null, null, 6, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var payload = ok.Value.Should().BeOfType<PagedResult<TerceroListDto>>().Subject;
        payload.Page.Should().Be(2);
        payload.PageSize.Should().Be(25);
        payload.TotalCount.Should().Be(70);
        payload.Items.Should().HaveCount(2);
        await mediator.Received(1).Send(
            Arg.Is<GetTercerosPagedQuery>(q =>
                q.Page == 2 &&
                q.PageSize == 25 &&
                q.Search == "garcia" &&
                q.SoloClientes == true &&
                q.SoloProveedores == false &&
                q.SoloActivos == true &&
                q.CondicionIvaId == 3 &&
                q.CategoriaId == 4 &&
                q.EstadoPersonaId == 5 &&
                q.SucursalId == 6),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetTerceroByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<TerceroDto>("Tercero no encontrado."));
        var controller = CreateController(mediator);

        var result = await controller.GetById(7, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("Tercero no encontrado");
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOkConDetalle()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetTerceroByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(new TerceroDto { Id = 7, Legajo = "CLI001", RazonSocial = "Cliente Uno", Activo = true }));
        var controller = CreateController(mediator);

        var result = await controller.GetById(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<TerceroDto>().Which.Legajo.Should().Be("CLI001");
    }

    [Fact]
    public async Task GetByLegajo_CuandoExiste_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetTerceroByLegajoQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(new TerceroDto { Id = 7, Legajo = "CLI001", RazonSocial = "Cliente Uno" }));
        var controller = CreateController(mediator);

        var result = await controller.GetByLegajo("cli001", CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<TerceroDto>().Which.Id.Should().Be(7);
    }

    [Fact]
    public async Task GetClientesActivos_CuandoHayDatos_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetClientesActivosQuery>(), Arg.Any<CancellationToken>())
            .Returns([new TerceroSelectorDto { Id = 1, Legajo = "CLI001", RazonSocial = "Cliente Uno", NroDocumento = "20-1" }]);
        var controller = CreateController(mediator);

        var result = await controller.GetClientesActivos(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeAssignableTo<IReadOnlyList<TerceroSelectorDto>>().Subject.Should().ContainSingle();
    }

    [Fact]
    public async Task GetProveedoresActivos_CuandoHayDatos_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetProveedoresActivosQuery>(), Arg.Any<CancellationToken>())
            .Returns([new TerceroSelectorDto { Id = 2, Legajo = "PRO001", RazonSocial = "Proveedor Uno", NroDocumento = "30-2" }]);
        var controller = CreateController(mediator);

        var result = await controller.GetProveedoresActivos(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeAssignableTo<IReadOnlyList<TerceroSelectorDto>>().Subject.Should().ContainSingle();
    }

    [Fact]
    public async Task GetDomicilios_CuandoHayDatos_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetTerceroDomiciliosQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<TerceroDomicilioDto>>([
                new TerceroDomicilioDto { Id = 1, TerceroId = 7, Calle = "Calle A", Orden = 0, EsDefecto = true }
            ]));
        var controller = CreateController(mediator);

        var result = await controller.GetDomicilios(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeAssignableTo<IReadOnlyList<TerceroDomicilioDto>>().Subject.Should().ContainSingle();
    }

    [Fact]
    public async Task GetContactos_CuandoHayDatos_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetTerceroContactosQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<TerceroContactoDto>>([
                new TerceroContactoDto { Id = 1, TerceroId = 7, Nombre = "Juan Perez", Orden = 0, Principal = true }
            ]));
        var controller = CreateController(mediator);

        var result = await controller.GetContactos(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeAssignableTo<IReadOnlyList<TerceroContactoDto>>().Subject.Should().ContainSingle();
    }

    [Fact]
    public async Task GetMediosContacto_CuandoHayDatos_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetTerceroMediosContactoQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<TerceroMedioContactoDto>>([
                new TerceroMedioContactoDto { Id = 1, TerceroId = 7, Valor = "contacto@cliente.com", Orden = 0, EsDefecto = true }
            ]));
        var controller = CreateController(mediator);

        var result = await controller.GetMediosContacto(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeAssignableTo<IReadOnlyList<TerceroMedioContactoDto>>().Subject.Should().ContainSingle();
    }

    [Fact]
    public async Task Create_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateTerceroCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("El legajo es requerido."));
        var controller = CreateController(mediator);

        var result = await controller.Create(BuildCreateCommand(), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("legajo es requerido");
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtRoute()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateTerceroCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(41L));
        var controller = CreateController(mediator);

        var result = await controller.Create(BuildCreateCommand(), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetTerceroById");
        AssertAnonymousProperty(created.Value!, "id", 41L);
    }

    [Fact]
    public async Task Update_CuandoIdNoCoincide_DevuelveBadRequestSinInvocarMediator()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);

        var result = await controller.Update(7, BuildUpdateCommand(8), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("Id de la URL no coincide");
        await mediator.DidNotReceive().Send(Arg.Any<UpdateTerceroCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateTerceroCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.Update(7, BuildUpdateCommand(7), CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Delete_CuandoTieneExito_DevuelveOkYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteTerceroCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.Delete(7, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<DeleteTerceroCommand>(command => command.Id == 7),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Activar_CuandoTieneExito_DevuelveOkYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<ActivarTerceroCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.Activar(7, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<ActivarTerceroCommand>(command => command.Id == 7),
            Arg.Any<CancellationToken>());
    }

    private static TercerosController CreateController(IMediator mediator)
    {
        return new TercerosController(mediator)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    private static CreateTerceroCommand BuildCreateCommand()
        => new(
            Legajo: "CLI001",
            RazonSocial: "Cliente Uno",
            NombreFantasia: null,
            TipoPersoneria: null,
            Nombre: null,
            Apellido: null,
            Tratamiento: null,
            Profesion: null,
            EstadoPersonaId: null,
            EstadoCivilId: null,
            EstadoCivil: null,
            Nacionalidad: null,
            Sexo: null,
            FechaNacimiento: null,
            FechaRegistro: null,
            EsEntidadGubernamental: false,
            ClaveFiscal: null,
            ValorClaveFiscal: null,
            TipoDocumentoId: 1,
            NroDocumento: "20123456789",
            CondicionIvaId: 1,
            EsCliente: true,
            EsProveedor: false,
            EsEmpleado: false,
            Calle: "Calle",
            Nro: "123",
            Piso: null,
            Dpto: null,
            CodigoPostal: "5000",
            PaisId: 1,
            ProvinciaId: 2,
            LocalidadId: 3,
            BarrioId: null,
            NroIngresosBrutos: null,
            NroMunicipal: null,
            Telefono: "123",
            Celular: null,
            Email: "a@demo.com",
            Web: null,
            MonedaId: 1,
            CategoriaId: null,
            CategoriaClienteId: 2,
            EstadoClienteId: 3,
            CategoriaProveedorId: null,
            EstadoProveedorId: null,
            LimiteCredito: 1000m,
            PorcentajeMaximoDescuento: null,
            VigenciaCreditoDesde: null,
            VigenciaCreditoHasta: null,
            Facturable: true,
            CobradorId: null,
            AplicaComisionCobrador: false,
            PctComisionCobrador: 0m,
            VendedorId: null,
            AplicaComisionVendedor: false,
            PctComisionVendedor: 0m,
            Observacion: null,
            SucursalId: 1);

    private static UpdateTerceroCommand BuildUpdateCommand(long id)
        => new(
            Id: id,
            RazonSocial: "Cliente Uno",
            NombreFantasia: null,
            TipoPersoneria: null,
            Nombre: null,
            Apellido: null,
            Tratamiento: null,
            Profesion: null,
            EstadoPersonaId: null,
            EstadoCivilId: null,
            EstadoCivil: null,
            Nacionalidad: null,
            Sexo: null,
            FechaNacimiento: null,
            FechaRegistro: null,
            EsEntidadGubernamental: false,
            ClaveFiscal: null,
            ValorClaveFiscal: null,
            NroDocumento: "20123456789",
            CondicionIvaId: 1,
            EsCliente: true,
            EsProveedor: false,
            EsEmpleado: false,
            Calle: "Calle",
            Nro: "123",
            Piso: null,
            Dpto: null,
            CodigoPostal: "5000",
            PaisId: 1,
            ProvinciaId: 2,
            LocalidadId: 3,
            BarrioId: null,
            NroIngresosBrutos: null,
            NroMunicipal: null,
            Telefono: "123",
            Celular: null,
            Email: "a@demo.com",
            Web: null,
            MonedaId: 1,
            CategoriaId: null,
            CategoriaClienteId: 2,
            EstadoClienteId: 3,
            CategoriaProveedorId: null,
            EstadoProveedorId: null,
            LimiteCredito: 1000m,
            PorcentajeMaximoDescuento: null,
            VigenciaCreditoDesde: null,
            VigenciaCreditoHasta: null,
            Facturable: true,
            CobradorId: null,
            AplicaComisionCobrador: false,
            PctComisionCobrador: 0m,
            VendedorId: null,
            AplicaComisionVendedor: false,
            PctComisionVendedor: 0m,
            Observacion: null);

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}
