using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Terceros.Commands;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Application.Features.Terceros.Queries;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.UnitTests.Helpers;

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

        var result = await controller.GetAll(2, 25, "garcia", true, false, null, true, 3, 4, 5, CancellationToken.None);

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
                q.SucursalId == 5),
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
    public async Task GetByLegajo_CuandoNoExiste_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetTerceroByLegajoQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<TerceroDto>("No se encontró ningún tercero con el legajo 'CLI404'."));
        var controller = CreateController(mediator);

        var result = await controller.GetByLegajo("CLI404", CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("CLI404");
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
    public async Task Update_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateTerceroCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Tercero no encontrado."));
        var controller = CreateController(mediator);

        var result = await controller.Update(7, BuildUpdateCommand(7), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("Tercero no encontrado");
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
    public async Task Delete_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteTerceroCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se puede eliminar porque tiene comprobantes."));
        var controller = CreateController(mediator);

        var result = await controller.Delete(7, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("tiene comprobantes");
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
    public async Task Activar_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<ActivarTerceroCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Tercero no encontrado."));
        var controller = CreateController(mediator);

        var result = await controller.Activar(7, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("Tercero no encontrado");
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

    [Fact]
    public async Task GetDomicilios_CuandoHayDatos_DevuelveFiltradosYOrdenados()
    {
        var controller = CreateController(
            Substitute.For<IMediator>(),
            BuildDomiciliosDbSet([
                BuildDomicilio(2, 7, 1, 2, 3, "Calle B", "Barrio B", "5000", null, 2, false),
                BuildDomicilio(1, 7, 1, 2, 3, "Calle A", "Barrio A", "4000", null, 1, true),
                BuildDomicilio(3, 8, 1, 2, 3, "Calle X", "Barrio X", "3000", null, 1, false)
            ]),
            BuildMediosDbSet(Array.Empty<MedioContacto>()));

        var result = await controller.GetDomicilios(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((System.Collections.IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Id", 1L);
        AssertAnonymousProperty(items[1], "Id", 2L);
    }

    [Fact]
    public async Task AddDomicilio_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AddTerceroDomicilioCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("PersonaId es requerido."));
        var controller = CreateController(mediator);

        var result = await controller.AddDomicilio(0, new DomicilioRequest(1, 2, 3, "Calle", "Barrio", "5000", null, 1, true), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("PersonaId es requerido");
    }

    [Fact]
    public async Task AddDomicilio_CuandoTieneExito_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AddTerceroDomicilioCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(51L));
        var controller = CreateController(mediator);

        var result = await controller.AddDomicilio(7, new DomicilioRequest(1, 2, 3, "Calle", "Barrio", "5000", null, 1, true), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(TercerosController.GetDomicilios));
        AssertAnonymousProperty(created.Value!, "Id", 51L);
    }

    [Fact]
    public async Task UpdateDomicilio_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateTerceroDomicilioCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Domicilio no encontrado."));
        var controller = CreateController(mediator);

        var result = await controller.UpdateDomicilio(7, 9, new DomicilioRequest(1, 2, 3, "Calle", "Barrio", "5000", null, 1, true), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Domicilio no encontrado");
    }

    [Fact]
    public async Task UpdateDomicilio_CuandoTieneExito_DevuelveOkConId()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateTerceroDomicilioCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.UpdateDomicilio(7, 9, new DomicilioRequest(1, 2, 3, "Calle", "Barrio", "5000", null, 1, true), CancellationToken.None);

        AssertAnonymousProperty(result.Should().BeOfType<OkObjectResult>().Subject.Value!, "Id", 9L);
    }

    [Fact]
    public async Task DeleteDomicilio_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteTerceroDomicilioCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Domicilio no encontrado."));
        var controller = CreateController(mediator);

        var result = await controller.DeleteDomicilio(7, 9, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Domicilio no encontrado");
    }

    [Fact]
    public async Task DeleteDomicilio_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteTerceroDomicilioCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.DeleteDomicilio(7, 9, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetMediosContacto_CuandoHayDatos_DevuelveFiltradosYOrdenados()
    {
        var controller = CreateController(
            Substitute.For<IMediator>(),
            BuildDomiciliosDbSet(Array.Empty<PersonaDomicilio>()),
            BuildMediosDbSet([
                BuildMedio(2, 7, 1, "b@demo.com", 2, false, null),
                BuildMedio(1, 7, 1, "a@demo.com", 1, true, null),
                BuildMedio(3, 8, 1, "x@demo.com", 1, false, null)
            ]));

        var result = await controller.GetMediosContacto(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((System.Collections.IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Id", 1L);
        AssertAnonymousProperty(items[1], "Id", 2L);
    }

    [Fact]
    public async Task AddMedioContacto_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AddTerceroMedioContactoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("El valor es requerido."));
        var controller = CreateController(mediator);

        var result = await controller.AddMedioContacto(7, new MedioContactoRequest("", 1, 1, true, null), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("valor es requerido");
    }

    [Fact]
    public async Task AddMedioContacto_CuandoTieneExito_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AddTerceroMedioContactoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(61L));
        var controller = CreateController(mediator);

        var result = await controller.AddMedioContacto(7, new MedioContactoRequest("a@demo.com", 1, 1, true, null), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(TercerosController.GetMediosContacto));
        AssertAnonymousProperty(created.Value!, "Id", 61L);
    }

    [Fact]
    public async Task UpdateMedioContacto_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateTerceroMedioContactoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Medio de contacto no encontrado."));
        var controller = CreateController(mediator);

        var result = await controller.UpdateMedioContacto(7, 9, new MedioContactoRequest("a@demo.com", 1, 1, true, null), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Medio de contacto no encontrado");
    }

    [Fact]
    public async Task UpdateMedioContacto_CuandoTieneExito_DevuelveOkConId()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateTerceroMedioContactoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.UpdateMedioContacto(7, 9, new MedioContactoRequest("a@demo.com", 1, 1, true, null), CancellationToken.None);

        AssertAnonymousProperty(result.Should().BeOfType<OkObjectResult>().Subject.Value!, "Id", 9L);
    }

    [Fact]
    public async Task DeleteMedioContacto_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteTerceroMedioContactoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Medio de contacto no encontrado."));
        var controller = CreateController(mediator);

        var result = await controller.DeleteMedioContacto(7, 9, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Medio de contacto no encontrado");
    }

    [Fact]
    public async Task DeleteMedioContacto_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteTerceroMedioContactoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.DeleteMedioContacto(7, 9, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetTiposPersona_CuandoHayDatos_DevuelveFiltrados()
    {
        var controller = CreateController(
            Substitute.For<IMediator>(),
            BuildDomiciliosDbSet(Array.Empty<PersonaDomicilio>()),
            BuildMediosDbSet(Array.Empty<MedioContacto>()),
            BuildTiposPersonaDbSet([
                BuildPersonaTipo(1, 7, 11, "CLI001", 1),
                BuildPersonaTipo(2, 7, 12, "PRO001", 2),
                BuildPersonaTipo(3, 8, 11, "CLI999", 3)
            ]),
            BuildVinculacionesDbSet(Array.Empty<VinculacionPersona>()),
            BuildContactosDbSet(Array.Empty<Contacto>()));

        var result = await controller.GetTiposPersona(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((System.Collections.IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Id", 1L);
        AssertAnonymousProperty(items[1], "TipoPersonaId", 12L);
    }

    [Fact]
    public async Task AddTipoPersona_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AddTerceroTipoPersonaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Este tipo de persona ya está asignado al tercero."));
        var controller = CreateController(mediator);

        var result = await controller.AddTipoPersona(7, new PersonaXTipoPersonaRequest(11, "CLI001", 1), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("ya está asignado");
    }

    [Fact]
    public async Task AddTipoPersona_CuandoTieneExito_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AddTerceroTipoPersonaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(71L));
        var controller = CreateController(mediator);

        var result = await controller.AddTipoPersona(7, new PersonaXTipoPersonaRequest(11, "CLI001", 1), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(TercerosController.GetTiposPersona));
        AssertAnonymousProperty(created.Value!, "Id", 71L);
    }

    [Fact]
    public async Task DeleteTipoPersona_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteTerceroTipoPersonaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Tipo de persona no encontrado."));
        var controller = CreateController(mediator);

        var result = await controller.DeleteTipoPersona(7, 9, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Tipo de persona no encontrado");
    }

    [Fact]
    public async Task DeleteTipoPersona_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteTerceroTipoPersonaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.DeleteTipoPersona(7, 9, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetVinculaciones_CuandoHayDatos_DevuelveFiltradas()
    {
        var controller = CreateController(
            Substitute.For<IMediator>(),
            BuildDomiciliosDbSet(Array.Empty<PersonaDomicilio>()),
            BuildMediosDbSet(Array.Empty<MedioContacto>()),
            BuildTiposPersonaDbSet(Array.Empty<PersonaXTipoPersona>()),
            BuildVinculacionesDbSet([
                BuildVinculacion(1, 7, 20, true, 1),
                BuildVinculacion(2, 30, 7, false, 2),
                BuildVinculacion(3, 8, 9, false, 3)
            ]),
            BuildContactosDbSet(Array.Empty<Contacto>()));

        var result = await controller.GetVinculaciones(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((System.Collections.IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Id", 1L);
        AssertAnonymousProperty(items[1], "Id", 2L);
    }

    [Fact]
    public async Task AddVinculacion_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AddTerceroVinculacionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("ProveedorId es requerido."));
        var controller = CreateController(mediator);

        var result = await controller.AddVinculacion(7, new VinculacionPersonaRequest(7, 0, true, 1), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("ProveedorId es requerido");
    }

    [Fact]
    public async Task AddVinculacion_CuandoTieneExito_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AddTerceroVinculacionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(81L));
        var controller = CreateController(mediator);

        var result = await controller.AddVinculacion(7, new VinculacionPersonaRequest(7, 20, true, 1), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(TercerosController.GetVinculaciones));
        AssertAnonymousProperty(created.Value!, "Id", 81L);
    }

    [Fact]
    public async Task DeleteVinculacion_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteTerceroVinculacionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Vinculación no encontrada."));
        var controller = CreateController(mediator);

        var result = await controller.DeleteVinculacion(7, 9, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Vinculación no encontrada");
    }

    [Fact]
    public async Task DeleteVinculacion_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteTerceroVinculacionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.DeleteVinculacion(7, 9, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetContactos_CuandoHayDatos_DevuelveFiltrados()
    {
        var controller = CreateController(
            Substitute.For<IMediator>(),
            BuildDomiciliosDbSet(Array.Empty<PersonaDomicilio>()),
            BuildMediosDbSet(Array.Empty<MedioContacto>()),
            BuildTiposPersonaDbSet(Array.Empty<PersonaXTipoPersona>()),
            BuildVinculacionesDbSet(Array.Empty<VinculacionPersona>()),
            BuildContactosDbSet([
                BuildContacto(1, 7, 20, 1),
                BuildContacto(2, 7, 21, 2),
                BuildContacto(3, 8, 22, 3)
            ]));

        var result = await controller.GetContactos(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((System.Collections.IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Id", 1L);
        AssertAnonymousProperty(items[1], "PersonaContactoId", 21L);
    }

    [Fact]
    public async Task AddContacto_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AddTerceroContactoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("PersonaContactoId es requerido."));
        var controller = CreateController(mediator);

        var result = await controller.AddContacto(7, new ContactoRequest(0, 1), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("PersonaContactoId es requerido");
    }

    [Fact]
    public async Task AddContacto_CuandoTieneExito_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AddTerceroContactoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(91L));
        var controller = CreateController(mediator);

        var result = await controller.AddContacto(7, new ContactoRequest(20, 1), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(TercerosController.GetContactos));
        AssertAnonymousProperty(created.Value!, "Id", 91L);
    }

    [Fact]
    public async Task DeleteContacto_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteTerceroContactoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Contacto no encontrado."));
        var controller = CreateController(mediator);

        var result = await controller.DeleteContacto(7, 9, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Contacto no encontrado");
    }

    [Fact]
    public async Task DeleteContacto_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteTerceroContactoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.DeleteContacto(7, 9, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    private static TercerosController CreateController(IMediator mediator)
        => CreateController(
            mediator,
            BuildDomiciliosDbSet(Array.Empty<PersonaDomicilio>()),
            BuildMediosDbSet(Array.Empty<MedioContacto>()),
            BuildTiposPersonaDbSet(Array.Empty<PersonaXTipoPersona>()),
            BuildVinculacionesDbSet(Array.Empty<VinculacionPersona>()),
            BuildContactosDbSet(Array.Empty<Contacto>()));

    private static TercerosController CreateController(IMediator mediator, DbSet<PersonaDomicilio> domicilios, DbSet<MedioContacto> medios)
        => CreateController(mediator, domicilios, medios, BuildTiposPersonaDbSet(Array.Empty<PersonaXTipoPersona>()), BuildVinculacionesDbSet(Array.Empty<VinculacionPersona>()), BuildContactosDbSet(Array.Empty<Contacto>()));

    private static TercerosController CreateController(IMediator mediator, DbSet<PersonaDomicilio> domicilios, DbSet<MedioContacto> medios, DbSet<PersonaXTipoPersona> tiposPersona, DbSet<VinculacionPersona> vinculaciones, DbSet<Contacto> contactos)
    {
        var db = Substitute.For<IApplicationDbContext>();
        db.Domicilios.Returns(domicilios);
        db.MediosContacto.Returns(medios);
        db.PersonasXTipoPersona.Returns(tiposPersona);
        db.VinculacionesPersona.Returns(vinculaciones);
        db.Contactos.Returns(contactos);

        return new TercerosController(mediator, db)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    private static DbSet<PersonaDomicilio> BuildDomiciliosDbSet(IEnumerable<PersonaDomicilio> items)
        => MockDbSetHelper.CreateMockDbSet(items.ToArray());

    private static DbSet<MedioContacto> BuildMediosDbSet(IEnumerable<MedioContacto> items)
        => MockDbSetHelper.CreateMockDbSet(items.ToArray());

    private static DbSet<PersonaXTipoPersona> BuildTiposPersonaDbSet(IEnumerable<PersonaXTipoPersona> items)
        => MockDbSetHelper.CreateMockDbSet(items.ToArray());

    private static DbSet<VinculacionPersona> BuildVinculacionesDbSet(IEnumerable<VinculacionPersona> items)
        => MockDbSetHelper.CreateMockDbSet(items.ToArray());

    private static DbSet<Contacto> BuildContactosDbSet(IEnumerable<Contacto> items)
        => MockDbSetHelper.CreateMockDbSet(items.ToArray());

    private static CreateTerceroCommand BuildCreateCommand()
        => new(
            "CLI001", "Cliente Uno", null,
            1, "20123456789", 1,
            true, false, false,
            "Calle", "123", null, null, "5000", 1, 2,
            null, null,
            "123", null, "a@demo.com", null,
            1, 2, 1000m, true, null, 0m, null, 0m, null,
            1);

    private static UpdateTerceroCommand BuildUpdateCommand(long id)
        => new(
            id,
            "Cliente Uno", null,
            "20123456789", 1,
            true, false, false,
            "Calle", "123", null, null, "5000", 1, 2,
            null, null,
            "123", null, "a@demo.com", null,
            1, 2, 1000m, true, null, 0m, null, 0m, null);

    private static PersonaDomicilio BuildDomicilio(long id, long personaId, long? tipoDomicilioId, long? provinciaId, long? localidadId, string? calle, string? barrio, string? codigoPostal, string? observacion, int orden, bool esDefecto)
    {
        var entity = PersonaDomicilio.Crear(personaId, tipoDomicilioId, provinciaId, localidadId, calle, barrio, codigoPostal, observacion, orden, esDefecto);
        SetEntityId(entity, id);
        return entity;
    }

    private static MedioContacto BuildMedio(long id, long personaId, long? tipoMedioContactoId, string valor, int orden, bool esDefecto, string? observacion)
    {
        var entity = MedioContacto.Crear(personaId, valor, tipoMedioContactoId, orden, esDefecto, observacion);
        SetEntityId(entity, id);
        return entity;
    }

    private static PersonaXTipoPersona BuildPersonaTipo(long id, long personaId, long tipoPersonaId, string? legajo, int? legajoOrden)
    {
        var entity = PersonaXTipoPersona.Crear(personaId, tipoPersonaId, legajo, legajoOrden);
        SetEntityId(entity, id);
        return entity;
    }

    private static VinculacionPersona BuildVinculacion(long id, long clienteId, long proveedorId, bool esPredeterminado, long? tipoRelacionId)
    {
        var entity = VinculacionPersona.Crear(clienteId, proveedorId, esPredeterminado, tipoRelacionId);
        SetEntityId(entity, id);
        return entity;
    }

    private static Contacto BuildContacto(long id, long personaId, long personaContactoId, long? tipoRelacionId)
    {
        var entity = Contacto.Crear(personaId, personaContactoId, tipoRelacionId);
        SetEntityId(entity, id);
        return entity;
    }

    private static void SetEntityId(object entity, long id)
    {
        var type = entity.GetType();
        while (type is not null)
        {
            var property = type.GetProperty("Id");
            if (property is not null)
            {
                property.SetValue(entity, id);
                return;
            }

            type = type.BaseType;
        }

        throw new InvalidOperationException("No se pudo localizar la propiedad Id.");
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}