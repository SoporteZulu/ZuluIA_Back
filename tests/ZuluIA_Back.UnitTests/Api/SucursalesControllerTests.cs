using System.Collections;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Sucursales.Commands;
using ZuluIA_Back.Application.Features.Sucursales.DTOs;
using ZuluIA_Back.Application.Features.Sucursales.Queries;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Entities.Sucursales;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class SucursalesControllerTests
{
    [Fact]
    public async Task GetAll_CuandoHayDatos_DevuelveOkConLista()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetSucursalesQuery>(), Arg.Any<CancellationToken>())
            .Returns(new List<SucursalListDto>
            {
                new() { Id = 1, RazonSocial = "Casa Central", NombreFantasia = "Central", Cuit = "20123456789", CasaMatriz = true, Activa = true },
                new() { Id = 2, RazonSocial = "Sucursal Norte", NombreFantasia = "Norte", Cuit = "20987654321", CasaMatriz = false, Activa = false }
            });
        var controller = CreateController(mediator);

        var result = await controller.GetAll(false, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ok.Value.Should().BeAssignableTo<IReadOnlyList<SucursalListDto>>().Subject;
        items.Should().HaveCount(2);
        items[0].CasaMatriz.Should().BeTrue();
        await mediator.Received(1).Send(Arg.Is<GetSucursalesQuery>(q => q.SoloActivas == false), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetSucursalByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns((SucursalDto?)null);
        var controller = CreateController(mediator);

        var result = await controller.GetById(7, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOkConDto()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetSucursalByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(new SucursalDto
            {
                Id = 7,
                RazonSocial = "Sucursal Norte",
                NombreFantasia = "Norte",
                Cuit = "20987654321",
                MonedaId = 1,
                CondicionIvaId = 2,
                PaisId = 3,
                CasaMatriz = false,
                Activa = true
            });
        var controller = CreateController(mediator);

        var result = await controller.GetById(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = ok.Value.Should().BeOfType<SucursalDto>().Subject;
        dto.Id.Should().Be(7);
        dto.RazonSocial.Should().Be("Sucursal Norte");
        dto.Activa.Should().BeTrue();
    }

    [Fact]
    public async Task Create_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateSucursalCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("El CUIT ya existe."));
        var controller = CreateController(mediator);

        var result = await controller.Create(BuildCreateCommand(), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("CUIT ya existe");
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtRoute()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateSucursalCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(15L));
        var controller = CreateController(mediator);

        var result = await controller.Create(BuildCreateCommand(), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetSucursalById");
        AssertAnonymousProperty(created.Value!, "id", 15L);
    }

    [Fact]
    public async Task Update_CuandoIdNoCoincide_DevuelveBadRequestSinInvocarMediator()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);

        var result = await controller.Update(7, BuildUpdateCommand(8), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("ID de la URL no coincide");
        await mediator.DidNotReceive().Send(Arg.Any<UpdateSucursalCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateSucursalCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontró la sucursal con ID 7."));
        var controller = CreateController(mediator);

        var result = await controller.Update(7, BuildUpdateCommand(7), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("No se encontró la sucursal con ID 7");
    }

    [Fact]
    public async Task Update_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateSucursalCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.Update(7, BuildUpdateCommand(7), CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Update_CuandoTieneExito_DevuelveOkYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var command = BuildUpdateCommand(7);
        mediator.Send(Arg.Any<UpdateSucursalCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.Update(7, command, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<UpdateSucursalCommand>(c =>
                c.Id == 7 &&
                c.RazonSocial == command.RazonSocial &&
                c.NombreFantasia == command.NombreFantasia &&
                c.Cuit == command.Cuit &&
                c.MonedaId == command.MonedaId &&
                c.CondicionIvaId == command.CondicionIvaId &&
                c.PaisId == command.PaisId &&
                c.Telefono == command.Telefono &&
                c.Email == command.Email &&
                c.PuertoAfip == command.PuertoAfip &&
                c.CasaMatriz == command.CasaMatriz),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteSucursalCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontró la sucursal con ID 7."));
        var controller = CreateController(mediator);

        var result = await controller.Delete(7, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("No se encontró la sucursal con ID 7");
    }

    [Fact]
    public async Task Delete_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteSucursalCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.Delete(7, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Activar_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<ActivateSucursalCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontró la sucursal con ID 7."));
        var controller = CreateController(mediator);

        var result = await controller.Activar(7, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("No se encontró la sucursal con ID 7");
    }

    [Fact]
    public async Task Activar_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<ActivateSucursalCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.Activar(7, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetAreas_CuandoHayDatos_FiltraPorSucursalYOrdenaPorDescripcion()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var areas = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildArea(1, "Ventas", "VEN", 7),
            BuildArea(2, "Administracion", "ADM", 7),
            BuildArea(3, "Deposito", "DEP", 8)
        });
        db.Areas.Returns(areas);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAreas(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Id", 2L);
        AssertAnonymousProperty(items[0], "Descripcion", "Administracion");
        AssertAnonymousProperty(items[1], "Id", 1L);
    }

    [Fact]
    public async Task GetAreaById_CuandoNoExiste_DevuelveNotFoundConMensaje()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var areas = MockDbSetHelper.CreateMockDbSet<Area>(Array.Empty<Area>());
        db.Areas.Returns(areas);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAreaById(7, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Área 7 no encontrada");
    }

    [Fact]
    public async Task GetAreaById_CuandoExiste_DevuelveOkConEntidad()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var area = BuildArea(7, "Ventas", "VEN", 2);
        var areas = MockDbSetHelper.CreateMockDbSet(new[] { area });
        db.Areas.Returns(areas);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAreaById(7, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeSameAs(area);
    }

    [Fact]
    public async Task CreateArea_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateAreaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("La descripcion es requerida."));
        var controller = CreateController(mediator);

        var result = await controller.CreateArea(new CreateAreaRequest("", "ADM", 7), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("descripcion es requerida");
    }

    [Fact]
    public async Task CreateArea_CuandoTieneExito_DevuelveCreatedAtRoute()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateAreaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(11L));
        var controller = CreateController(mediator);

        var result = await controller.CreateArea(new CreateAreaRequest("Administracion", "ADM", 7), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetAreaById");
        AssertAnonymousProperty(created.Value!, "Id", 11L);
    }

    [Fact]
    public async Task UpdateArea_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateAreaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Area 7 no encontrada."));
        var controller = CreateController(mediator);

        var result = await controller.UpdateArea(7, new CreateAreaRequest("Administracion", "ADM", 7), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Area 7 no encontrada");
    }

    [Fact]
    public async Task UpdateArea_CuandoTieneExito_DevuelveOkConId()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateAreaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.UpdateArea(7, new CreateAreaRequest("Administracion", "ADM", 7), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 7L);
    }

    [Fact]
    public async Task DeleteArea_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteAreaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Area 7 no encontrada."));
        var controller = CreateController(mediator);

        var result = await controller.DeleteArea(7, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Area 7 no encontrada");
    }

    [Fact]
    public async Task DeleteArea_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteAreaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.DeleteArea(7, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetTiposComprobante_CuandoHayDatos_FiltraPorSucursal()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var tipos = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildTipoComprobanteSucursal(1, 10, 7, 1001, 3, 80, 2, true, true, false, 1, true, false, true, 1, 9999),
            BuildTipoComprobanteSucursal(2, 11, 8, 2001, 2, 60, 1, false, false, false, null, true, true, false, null, null)
        });
        db.TiposComprobantesSucursal.Returns(tipos);
        var controller = CreateController(mediator, db);

        var result = await controller.GetTiposComprobante(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().ContainSingle();
        AssertAnonymousProperty(items[0], "Id", 1L);
        AssertAnonymousProperty(items[0], "TipoComprobanteId", 10L);
        AssertAnonymousProperty(items[0], "SucursalId", 7L);
        AssertAnonymousProperty(items[0], "NumeroComprobanteProximo", 1001L);
    }

    [Fact]
    public async Task CreateTipoComprobanteSucursal_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateTipoComprobanteSucursalCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Ya existe configuracion para ese tipo de comprobante en esta sucursal."));
        var controller = CreateController(mediator);

        var result = await controller.CreateTipoComprobanteSucursal(7, BuildTipoComprobanteRequest(), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("Ya existe configuracion");
    }

    [Fact]
    public async Task CreateTipoComprobanteSucursal_CuandoTieneExito_DevuelveCreated()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateTipoComprobanteSucursalCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(21L));
        var controller = CreateController(mediator);

        var result = await controller.CreateTipoComprobanteSucursal(7, BuildTipoComprobanteRequest(), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedResult>().Subject;
        created.Location.Should().BeEmpty();
        AssertAnonymousProperty(created.Value!, "Id", 21L);
    }

    [Fact]
    public async Task UpdateTipoComprobanteSucursal_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateTipoComprobanteSucursalCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Configuracion 21 no encontrada."));
        var controller = CreateController(mediator);

        var result = await controller.UpdateTipoComprobanteSucursal(7, 21, BuildTipoComprobanteRequest(), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Configuracion 21 no encontrada");
    }

    [Fact]
    public async Task UpdateTipoComprobanteSucursal_CuandoTieneExito_DevuelveOkConId()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateTipoComprobanteSucursalCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.UpdateTipoComprobanteSucursal(7, 21, BuildTipoComprobanteRequest(), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 21L);
    }

    [Fact]
    public async Task GetDomicilios_CuandoHayDatos_FiltraPorSucursalYOrdena()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var domicilios = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildSucursalDomicilio(1, 7, 2, 10, 20, "Calle 2", "Centro", "5001", "Obs 2", 2, false),
            BuildSucursalDomicilio(2, 7, 1, 10, 20, "Calle 1", "Norte", "5000", "Obs 1", 1, true),
            BuildSucursalDomicilio(3, 8, 1, 11, 21, "Calle X", "Sur", "6000", null, 0, false)
        });
        db.SucursalesDomicilio.Returns(domicilios);
        var controller = CreateController(mediator, db);

        var result = await controller.GetDomicilios(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Id", 2L);
        AssertAnonymousProperty(items[0], "Orden", 1);
        AssertAnonymousProperty(items[1], "Id", 1L);
    }

    [Fact]
    public async Task AddDomicilio_CuandoSucursalNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AddSucursalDomicilioCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Sucursal no encontrada."));
        var controller = CreateController(mediator);

        var result = await controller.AddDomicilio(7, BuildDomicilioRequest(), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Sucursal no encontrada");
    }

    [Fact]
    public async Task AddDomicilio_CuandoFallaPorValidacion_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AddSucursalDomicilioCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("SucursalId es requerido."));
        var controller = CreateController(mediator);

        var result = await controller.AddDomicilio(7, BuildDomicilioRequest(), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("SucursalId es requerido");
    }

    [Fact]
    public async Task AddDomicilio_CuandoTieneExito_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AddSucursalDomicilioCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(31L));
        var controller = CreateController(mediator);

        var result = await controller.AddDomicilio(7, BuildDomicilioRequest(), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(SucursalesController.GetDomicilios));
        AssertAnonymousProperty(created.Value!, "Id", 31L);
    }

    [Fact]
    public async Task UpdateDomicilio_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateSucursalDomicilioCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Domicilio no encontrado."));
        var controller = CreateController(mediator);

        var result = await controller.UpdateDomicilio(7, 31, BuildDomicilioRequest(), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Domicilio no encontrado");
    }

    [Fact]
    public async Task UpdateDomicilio_CuandoTieneExito_DevuelveOkConId()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateSucursalDomicilioCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.UpdateDomicilio(7, 31, BuildDomicilioRequest(), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 31L);
    }

    [Fact]
    public async Task DeleteDomicilio_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteSucursalDomicilioCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Domicilio no encontrado."));
        var controller = CreateController(mediator);

        var result = await controller.DeleteDomicilio(7, 31, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Domicilio no encontrado");
    }

    [Fact]
    public async Task DeleteDomicilio_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteSucursalDomicilioCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.DeleteDomicilio(7, 31, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetMediosContacto_CuandoHayDatos_FiltraPorSucursalYOrdena()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var medios = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildSucursalMedioContacto(1, 7, "ventas@example.com", 2, 10, false, "mail ventas"),
            BuildSucursalMedioContacto(2, 7, "3511234567", 1, 11, true, "telefono principal"),
            BuildSucursalMedioContacto(3, 8, "otra@example.com", 0, 10, false, null)
        });
        db.SucursalesMedioContacto.Returns(medios);
        var controller = CreateController(mediator, db);

        var result = await controller.GetMediosContacto(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Id", 2L);
        AssertAnonymousProperty(items[0], "Orden", 1);
        AssertAnonymousProperty(items[0], "Valor", "3511234567");
    }

    [Fact]
    public async Task AddMedioContacto_CuandoSucursalNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AddSucursalMedioContactoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Sucursal no encontrada."));
        var controller = CreateController(mediator);

        var result = await controller.AddMedioContacto(7, BuildMedioContactoRequest(), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Sucursal no encontrada");
    }

    [Fact]
    public async Task AddMedioContacto_CuandoFallaPorValidacion_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AddSucursalMedioContactoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("El valor es requerido."));
        var controller = CreateController(mediator);

        var result = await controller.AddMedioContacto(7, BuildMedioContactoRequest(), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("valor es requerido");
    }

    [Fact]
    public async Task AddMedioContacto_CuandoTieneExito_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AddSucursalMedioContactoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(41L));
        var controller = CreateController(mediator);

        var result = await controller.AddMedioContacto(7, BuildMedioContactoRequest(), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(SucursalesController.GetMediosContacto));
        AssertAnonymousProperty(created.Value!, "Id", 41L);
    }

    [Fact]
    public async Task AddMedioContacto_CuandoTieneExito_DevuelveCreatedAtActionYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var request = BuildMedioContactoRequest();
        mediator.Send(Arg.Any<AddSucursalMedioContactoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(41L));
        var controller = CreateController(mediator);

        var result = await controller.AddMedioContacto(7, request, CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(SucursalesController.GetMediosContacto));
        created.RouteValues.Should().NotBeNull();
        created.RouteValues!["id"].Should().Be(7L);
        AssertAnonymousProperty(created.Value!, "Id", 41L);
        await mediator.Received(1).Send(
            Arg.Is<AddSucursalMedioContactoCommand>(c =>
                c.SucursalId == 7 &&
                c.Valor == request.Valor &&
                c.TipoMedioContactoId == request.TipoMedioContactoId &&
                c.Orden == request.Orden &&
                c.EsDefecto == request.EsDefecto &&
                c.Observacion == request.Observacion),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateMedioContacto_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateSucursalMedioContactoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Medio de contacto no encontrado."));
        var controller = CreateController(mediator);

        var result = await controller.UpdateMedioContacto(7, 41, BuildMedioContactoRequest(), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Medio de contacto no encontrado");
    }

    [Fact]
    public async Task UpdateMedioContacto_CuandoTieneExito_DevuelveOkConId()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateSucursalMedioContactoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.UpdateMedioContacto(7, 41, BuildMedioContactoRequest(), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 41L);
    }

    [Fact]
    public async Task DeleteMedioContacto_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteSucursalMedioContactoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Medio de contacto no encontrado."));
        var controller = CreateController(mediator);

        var result = await controller.DeleteMedioContacto(7, 41, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Medio de contacto no encontrado");
    }

    [Fact]
    public async Task DeleteMedioContacto_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteSucursalMedioContactoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.DeleteMedioContacto(7, 41, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    private static SucursalesController CreateController(IMediator mediator, IApplicationDbContext? db = null)
    {
        var controller = new SucursalesController(mediator, db ?? Substitute.For<IApplicationDbContext>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }

    private static CreateSucursalCommand BuildCreateCommand()
        => new(
            "Sucursal Norte",
            "Norte",
            "20987654321",
            "123-456",
            2,
            1,
            54,
            "Calle 123",
            "100",
            null,
            null,
            "5000",
            10,
            20,
            "3511234567",
            "sucursal@example.com",
            "https://example.com",
            "2850590940090418135201",
            "SUCURSAL.NORTE",
            "CAI-123",
            3,
            false);

    private static UpdateSucursalCommand BuildUpdateCommand(long id)
        => new(
            id,
            "Sucursal Norte",
            "Norte",
            "20987654321",
            "123-456",
            2,
            1,
            54,
            "Calle 123",
            "100",
            null,
            null,
            "5000",
            10,
            20,
            "3511234567",
            "sucursal@example.com",
            "https://example.com",
            "2850590940090418135201",
            "SUCURSAL.NORTE",
            "CAI-123",
            3,
            false);

    private static CreateTipoComprobanteSucursalRequest BuildTipoComprobanteRequest()
        => new(
            10,
            1001,
            3,
            80,
            2,
            true,
            true,
            false,
            1,
            true,
            false,
            true,
            1,
            9999);

    private static SucDomicilioRequest BuildDomicilioRequest()
        => new(1, 10, 20, "Calle 1", "Centro", "5000", "Obs 1", 1, true);

    private static SucMedioContactoRequest BuildMedioContactoRequest()
        => new("ventas@example.com", 10, 1, true, "mail principal");

    private static Area BuildArea(long id, string descripcion, string? codigo, long? sucursalId)
    {
        var entity = Area.Crear(descripcion, codigo, sucursalId);
        SetEntityId(entity, id);
        return entity;
    }

    private static TipoComprobanteSucursal BuildTipoComprobanteSucursal(
        long id,
        long tipoComprobanteId,
        long? sucursalId,
        long numeroProximo,
        int filasCantidad,
        int filasAnchoMaximo,
        int cantidadCopias,
        bool imprimirControladorFiscal,
        bool varianteNroUnico,
        bool permitirSeleccionMoneda,
        long? monedaId,
        bool editable,
        bool vistaPrevia,
        bool controlIntervalo,
        long? numeroDesde,
        long? numeroHasta)
    {
        var entity = TipoComprobanteSucursal.Crear(
            tipoComprobanteId,
            sucursalId,
            numeroProximo,
            filasCantidad,
            filasAnchoMaximo,
            cantidadCopias,
            imprimirControladorFiscal,
            varianteNroUnico,
            permitirSeleccionMoneda,
            monedaId,
            editable,
            vistaPrevia,
            controlIntervalo,
            numeroDesde,
            numeroHasta);
        SetEntityId(entity, id);
        return entity;
    }

    private static SucursalDomicilio BuildSucursalDomicilio(
        long id,
        long sucursalId,
        long? tipoDomicilioId,
        long? provinciaId,
        long? localidadId,
        string? calle,
        string? barrio,
        string? codigoPostal,
        string? observacion,
        int orden,
        bool esDefecto)
    {
        var entity = SucursalDomicilio.Crear(
            sucursalId,
            tipoDomicilioId,
            provinciaId,
            localidadId,
            calle,
            barrio,
            codigoPostal,
            observacion,
            orden,
            esDefecto);
        SetEntityId(entity, id);
        return entity;
    }

    private static SucursalMedioContacto BuildSucursalMedioContacto(
        long id,
        long sucursalId,
        string valor,
        int orden,
        long? tipoMedioContactoId,
        bool esDefecto,
        string? observacion)
    {
        var entity = SucursalMedioContacto.Crear(
            sucursalId,
            valor,
            tipoMedioContactoId,
            orden,
            esDefecto,
            observacion);
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