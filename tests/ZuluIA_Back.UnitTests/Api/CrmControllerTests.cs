using System.Collections;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.CRM.Commands;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.CRM;
using ZuluIA_Back.Domain.Entities.Sucursales;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Entities.Usuarios;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class CrmControllerTests
{
    [Fact]
    public async Task GetAll_FiltraContactosPorPersona()
    {
        Contacto[] contactos =
        [
            BuildContacto(1, 10, 20, 30),
            BuildContacto(2, 10, 21, 31),
            BuildContacto(3, 11, 22, 32)
        ];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(contactos: contactos));

        var result = await controller.GetAll(10, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "PersonaId", 10L);
        AssertAnonymousProperty(items[1], "PersonaId", 10L);
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb());

        var result = await controller.GetById(9, CancellationToken.None);

        var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        AssertAnonymousProperty(notFound.Value!, "error", "Contacto 9 no encontrado.");
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOk()
    {
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(contactos: [BuildContacto(9, 10, 20, 30)]));

        var result = await controller.GetById(9, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 9L);
        AssertAnonymousProperty(ok.Value!, "PersonaContactoId", 20L);
    }

    [Fact]
    public async Task Create_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateContactoCrmCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("PersonaId es requerido."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.Create(new ContactoCrmRequest(0, 20, 30), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateContactoCrmCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(12L));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.Create(new ContactoCrmRequest(10, 20, 30), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(CrmController.GetById));
        created.RouteValues!["id"].Should().Be(12L);
        AssertAnonymousProperty(created.Value!, "Id", 12L);
    }

    [Fact]
    public async Task Update_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateContactoCrmCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Contacto 9 no encontrado."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.Update(9, new ContactoCrmUpdateRequest(40), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteContactoCrmCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.Delete(9, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetTiposComunicado_FiltraActivosYOrdenaPorCodigo()
    {
        CrmTipoComunicado[] tipos =
        [
            BuildTipoComunicado(2, "B", "Beta", true),
            BuildTipoComunicado(1, "A", "Alfa", true),
            BuildTipoComunicado(3, "C", "Gamma", false)
        ];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(tiposComunicado: tipos));

        var result = await controller.GetTiposComunicado(true, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Codigo", "A");
        AssertAnonymousProperty(items[1], "Codigo", "B");
    }

    [Fact]
    public async Task CreateTipoComunicado_CuandoDuplicado_DevuelveConflict()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateCrmTipoComunicadoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Ya existe un tipo con ese codigo."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.CreateTipoComunicado(new CrmCatalogoCreateRequest("A", "Alfa"), CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task UpdateTipoComunicado_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateCrmTipoComunicadoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.UpdateTipoComunicado(8, new CrmCatalogoUpdateRequest("Nuevo"), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 8L);
    }

    [Fact]
    public async Task DesactivarTipoComunicado_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeactivateCrmTipoComunicadoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Tipo 8 no encontrado."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.DesactivarTipoComunicado(8, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DesactivarTipoComunicado_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeactivateCrmTipoComunicadoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.DesactivarTipoComunicado(8, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task ActivarTipoComunicado_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<ActivateCrmTipoComunicadoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.ActivarTipoComunicado(8, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetMotivos_FiltraActivosYOrdenaPorCodigo()
    {
        CrmMotivo[] motivos =
        [
            BuildMotivo(2, "B", "Beta", true),
            BuildMotivo(1, "A", "Alfa", true),
            BuildMotivo(3, "C", "Gamma", false)
        ];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(motivos: motivos));

        var result = await controller.GetMotivos(true, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Codigo", "A");
        AssertAnonymousProperty(items[1], "Codigo", "B");
    }

    [Fact]
    public async Task CreateMotivo_CuandoDuplicado_DevuelveConflict()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateCrmMotivoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Ya existe un motivo con ese codigo."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.CreateMotivo(new CrmCatalogoCreateRequest("A", "Alfa"), CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task ActivarMotivo_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<ActivateCrmMotivoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Motivo 3 no encontrado."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.ActivarMotivo(3, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task ActivarMotivo_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<ActivateCrmMotivoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.ActivarMotivo(3, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetIntereses_FiltraActivosYOrdenaPorCodigo()
    {
        CrmInteres[] intereses =
        [
            BuildInteres(2, "B", "Beta", true),
            BuildInteres(1, "A", "Alfa", true),
            BuildInteres(3, "C", "Gamma", false)
        ];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(intereses: intereses));

        var result = await controller.GetIntereses(true, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Codigo", "A");
        AssertAnonymousProperty(items[1], "Codigo", "B");
    }

    [Fact]
    public async Task CreateInteres_CuandoDuplicado_DevuelveConflict()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateCrmInteresCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Ya existe un interes con ese codigo."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.CreateInteres(new CrmCatalogoCreateRequest("A", "Alfa"), CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
        public async Task DesactivarInteres_CuandoNoExiste_DevuelveNotFound()
        {
            var mediator = Substitute.For<IMediator>();
            mediator.Send(Arg.Any<DeactivateCrmInteresCommand>(), Arg.Any<CancellationToken>())
                .Returns(Result.Failure("Interes 7 no encontrado."));
            var controller = CreateController(mediator, BuildDb());

            var result = await controller.DesactivarInteres(7, CancellationToken.None);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
    public async Task DesactivarInteres_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeactivateCrmInteresCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.DesactivarInteres(7, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetCampanas_AplicaFiltrosYOrdenaPorFechaInicioDescYNombre()
    {
        CrmCampana[] campanas =
        [
            BuildCampana(1, 10, "Beta", new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), 100, true),
            BuildCampana(2, 10, "Alfa", new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), 100, true),
            BuildCampana(3, 10, "Vieja", new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 28), 100, true),
            BuildCampana(4, 11, "Otra", new DateOnly(2026, 4, 1), new DateOnly(2026, 4, 30), 100, true),
            BuildCampana(5, 10, "Cerrada", new DateOnly(2026, 5, 1), new DateOnly(2026, 5, 31), 100, false)
        ];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(campanas: campanas));

        var result = await controller.GetCampanas(10, true, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(3);
        AssertAnonymousProperty(items[0], "Nombre", "Alfa");
        AssertAnonymousProperty(items[1], "Nombre", "Beta");
        AssertAnonymousProperty(items[2], "Nombre", "Vieja");
    }

    [Fact]
    public async Task GetCampanaById_CuandoExiste_DevuelvePayload()
    {
        CrmCampana[] campanas = [BuildCampana(14, 10, "Camp", new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), 100, true)];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(campanas: campanas));

        var result = await controller.GetCampanaById(14, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", "14");
        AssertAnonymousProperty(ok.Value!, "Nombre", "Camp");
    }

    [Fact]
    public async Task CreateCampana_CuandoTieneExito_DevuelveCreatedConPayload()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateCrmCampanaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(14L));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.CreateCampana(
            new CrmCampanaRequest(
                10,
                "Camp",
                "email",
                "generacion_leads",
                null,
                new DateTimeOffset(new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc)),
                new DateTimeOffset(new DateTime(2026, 3, 31, 0, 0, 0, DateTimeKind.Utc)),
                100,
                0,
                null,
                "Notas",
                1,
                2,
                3),
            CancellationToken.None);

        var created = result.Should().BeOfType<ObjectResult>().Subject;
        created.StatusCode.Should().Be(StatusCodes.Status201Created);
        AssertAnonymousProperty(created.Value!, "Id", "14");
    }

    [Fact]
    public async Task CreateCampana_SinSucursalExplicita_UsaSucursalActivaPorDefecto()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateCrmCampanaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(14L));
        Sucursal[] sucursales = [BuildSucursal(10, true)];
        CrmCampana[] campanas = [BuildCampana(14, 10, "Camp", new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), 100, true)];
        var controller = CreateController(mediator, BuildDb(campanas: campanas, sucursales: sucursales));

        var result = await controller.CreateCampana(
            new CrmCampanaRequest(
                null,
                "Camp",
                "email",
                "generacion_leads",
                null,
                new DateTimeOffset(new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc)),
                new DateTimeOffset(new DateTime(2026, 3, 31, 0, 0, 0, DateTimeKind.Utc)),
                100,
                0,
                null,
                "Notas",
                1,
                2,
                3),
            CancellationToken.None);

        result.Should().BeOfType<ObjectResult>();
        await mediator.Received(1).Send(
            Arg.Is<CreateCrmCampanaCommand>(x => x.SucursalId == 10),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CerrarCampana_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CloseCrmCampanaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Campana 14 no encontrada."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.CerrarCampana(14, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetComunicados_AplicaFiltrosYOrdenaPorFechaDescEId()
    {
        CrmComunicado[] comunicados =
        [
            BuildComunicado(1, 10, 20, 30, 40, new DateOnly(2026, 3, 1), "Uno", "A", 50),
            BuildComunicado(2, 10, 20, 30, 40, new DateOnly(2026, 3, 2), "Dos", "B", 50),
            BuildComunicado(3, 10, 20, 31, 40, new DateOnly(2026, 3, 2), "Tres", "C", 50),
            BuildComunicado(4, 11, 20, 30, 40, new DateOnly(2026, 3, 3), "Cuatro", "D", 50)
        ];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(comunicados: comunicados));

        var result = await controller.GetComunicados(10, 20, 30, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Id", 2L);
        AssertAnonymousProperty(items[1], "Id", 1L);
    }

    [Fact]
    public async Task CreateComunicado_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateCrmComunicadoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("El asunto es requerido."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.CreateComunicado(new CrmComunicadoRequest(10, 20, 30, 40, new DateOnly(2026, 3, 1), "", null, 50), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task DeleteComunicado_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteCrmComunicadoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.DeleteComunicado(9, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetClientes_FiltraPorBusquedaYDevuelveIdComoTexto()
    {
        Tercero[] terceros =
        [
            BuildTercero(10, "Acme SA", "80012345-6", "ventas@acme.com"),
            BuildTercero(11, "Beta SRL", "80099999-1", "contacto@beta.com")
        ];
        CrmCliente[] clientes =
        [
            BuildCrmCliente(10, "prospecto", "pyme", "Retail", "Paraguay", "Asuncion", "Centro", "Palma 123", "web", "nuevo"),
            BuildCrmCliente(11, "activo", "corporativo", "Agro", "Paraguay", "Central", "Luque", "Ruta 1", "referido", "fidelizado")
        ];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(terceros: terceros, clientesCrm: clientes));

        var result = await controller.GetClientes("acme", CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(1);
        AssertAnonymousProperty(items[0], "Id", "10");
        AssertAnonymousProperty(items[0], "Nombre", "Acme SA");
    }

    [Fact]
    public async Task GetOportunidades_AplicaFiltrosAvanzados()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        CrmOportunidad[] oportunidades =
        [
            BuildCrmOportunidad(8, 10, "Pipeline Acme", "lead", today.AddDays(3), 2500m, 7),
            BuildCrmOportunidad(9, 10, "Pipeline Beta", "propuesta", today.AddDays(10), 5000m, 8)
        ];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(oportunidadesCrm: oportunidades));

        var result = await controller.GetOportunidades(
            clienteId: "10",
            responsableId: "7",
            etapa: "lead",
            busqueda: "Acme",
            fechaCierreDesde: today,
            fechaCierreHasta: today.AddDays(5),
            ct: CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(1);
        AssertAnonymousProperty(items[0], "Id", "8");
    }

    [Fact]
    public async Task GetSegmentos_CalculaCantidadClientesSegunCriterios()
    {
        Tercero[] terceros =
        [
            BuildTercero(10, "Acme SA", "80012345-6", "ventas@acme.com"),
            BuildTercero(11, "Beta SRL", "80099999-1", "contacto@beta.com")
        ];
        CrmCliente[] clientes =
        [
            BuildCrmCliente(10, "prospecto", "pyme", "Retail", "Paraguay", "Asuncion", "Centro", "Palma 123", "web", "nuevo"),
            BuildCrmCliente(11, "activo", "corporativo", "Agro", "Paraguay", "Central", "Luque", "Ruta 1", "referido", "fidelizado")
        ];
        CrmSegmento[] segmentos =
        [
            BuildCrmSegmento(1, "PYME", "Segmento PyME", "[{\"Campo\":\"segmento\",\"Operador\":\"igual\",\"Valor\":\"pyme\"}]", "dinamico")
        ];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(terceros: terceros, clientesCrm: clientes, segmentosCrm: segmentos));

        var result = await controller.GetSegmentos(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(1);
        AssertAnonymousProperty(items[0], "CantidadClientes", 1);
    }

    [Fact]
    public async Task GetUsuariosCrm_UsaPerfilYDivideNombreCompleto()
    {
        Usuario[] usuarios = [BuildUsuario(7, "juan.perez", "Juan Perez", "juan@crm.com", true)];
        CrmUsuarioPerfil[] perfiles = [BuildCrmUsuarioPerfil(1, 7, "supervisor", true)];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(usuarios: usuarios, usuariosCrm: perfiles));

        var result = await controller.GetUsuariosCrm(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(1);
        AssertAnonymousProperty(items[0], "Nombre", "Juan");
        AssertAnonymousProperty(items[0], "Apellido", "Perez");
        AssertAnonymousProperty(items[0], "Rol", "supervisor");
    }

    [Fact]
    public async Task GetCrmContactoById_CuandoExiste_DevuelvePayload()
    {
        CrmContacto[] contactos = [BuildCrmContacto(5, 10, "Ana", "Lopez", "Compras")];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(crmContactos: contactos));

        var result = await controller.GetCrmContactoById(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", "5");
        AssertAnonymousProperty(ok.Value!, "Nombre", "Ana");
    }

    [Fact]
    public async Task GetOportunidadById_CuandoExiste_DevuelvePayload()
    {
        CrmOportunidad[] oportunidades = [BuildCrmOportunidad(8, 10, "Pipeline", "lead")];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(oportunidadesCrm: oportunidades));

        var result = await controller.GetOportunidadById(8, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", "8");
        AssertAnonymousProperty(ok.Value!, "Titulo", "Pipeline");
    }

    [Fact]
    public async Task GetInteraccionById_CuandoExiste_DevuelvePayload()
    {
        CrmInteraccion[] interacciones = [BuildCrmInteraccion(9, 10, "llamada", "telefono")];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(interaccionesCrm: interacciones));

        var result = await controller.GetInteraccionById(9, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", "9");
        AssertAnonymousProperty(ok.Value!, "TipoInteraccion", "llamada");
    }

    [Fact]
    public async Task GetTareaById_CuandoExiste_DevuelvePayload()
    {
        CrmTarea[] tareas = [BuildCrmTarea(11, 10, "Seguimiento")];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(tareasCrm: tareas));

        var result = await controller.GetTareaById(11, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", "11");
        AssertAnonymousProperty(ok.Value!, "Titulo", "Seguimiento");
    }

    [Fact]
    public async Task GetSegmentoById_CuandoExiste_DevuelvePayload()
    {
        Tercero[] terceros = [BuildTercero(10, "Acme SA", "80012345-6", "ventas@acme.com")];
        CrmCliente[] clientes = [BuildCrmCliente(10, "prospecto", "pyme", "Retail", "Paraguay", "Asuncion", "Centro", "Palma 123", "web", "nuevo")];
        CrmSegmento[] segmentos = [BuildCrmSegmento(4, "PYME", "Segmento PyME", "[]", "dinamico")];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(terceros: terceros, clientesCrm: clientes, segmentosCrm: segmentos));

        var result = await controller.GetSegmentoById(4, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", "4");
        AssertAnonymousProperty(ok.Value!, "Nombre", "PYME");
    }

    [Fact]
    public async Task GetSegmentoMiembros_CuandoSegmentoEsEstatico_DevuelveMembresiaManual()
    {
        Tercero[] terceros = [BuildTercero(10, "Acme SA", "80012345-6", "ventas@acme.com")];
        CrmCliente[] clientes = [BuildCrmCliente(10, "prospecto", "pyme", "Retail", "Paraguay", "Asuncion", "Centro", "Palma 123", "web", "nuevo")];
        CrmSegmento[] segmentos = [BuildCrmSegmento(4, "VIP", "Manual", "[]", "estatico")];
        CrmSegmentoMiembro[] miembros = [BuildCrmSegmentoMiembro(1, 4, 10)];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(terceros: terceros, clientesCrm: clientes, segmentosCrm: segmentos, segmentosMiembrosCrm: miembros));

        var result = await controller.GetSegmentoMiembros(4, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(1);
        AssertAnonymousProperty(items[0], "Id", "10");
    }

    [Fact]
    public async Task PreviewSegmento_CuandoEsDinamico_DevuelveClientesCoincidentes()
    {
        Tercero[] terceros =
        [
            BuildTercero(10, "Acme SA", "80012345-6", "ventas@acme.com"),
            BuildTercero(11, "Beta SRL", "80099999-1", "contacto@beta.com")
        ];
        CrmCliente[] clientes =
        [
            BuildCrmCliente(10, "prospecto", "pyme", "Retail", "Paraguay", "Asuncion", "Centro", "Palma 123", "web", "nuevo"),
            BuildCrmCliente(11, "activo", "corporativo", "Agro", "Paraguay", "Central", "Luque", "Ruta 1", "referido", "fidelizado")
        ];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(terceros: terceros, clientesCrm: clientes));

        var result = await controller.PreviewSegmento(
            new CrmSegmentoPreviewRequest([new CrmSegmentCriterionRequest("segmento", "igual", "pyme")], "dinamico"),
            CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "CantidadClientes", 1);
    }

    [Fact]
    public async Task GetReportes_CuandoHayDatos_DevuelveResumenComercialYMarketing()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        Tercero[] terceros = [BuildTercero(10, "Acme SA", "80012345-6", "ventas@acme.com")];
        CrmCliente[] clientes = [BuildCrmCliente(10, "activo", "pyme", "Retail", "Paraguay", "Asuncion", "Centro", "Palma 123", "web", "en_riesgo", 7)];
        CrmOportunidad[] oportunidades = [BuildCrmOportunidad(8, 10, "Pipeline", "lead", today.AddDays(-1), 2500m, 7)];
        CrmInteraccion[] interacciones = [BuildCrmInteraccion(9, 10, "llamada", "telefono", DateTimeOffset.UtcNow.AddDays(-35), 7)];
        CrmCampana[] campanas = [BuildCampana(14, 10, "Camp", today.AddDays(-5), today.AddDays(5), 100m, 120m, 7, 10, 4, 2, true)];
        Usuario[] usuarios = [BuildUsuario(7, "juan.perez", "Juan Perez", "juan@crm.com", true)];
        CrmUsuarioPerfil[] perfiles = [BuildCrmUsuarioPerfil(1, 7, "comercial", true)];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(terceros: terceros, clientesCrm: clientes, oportunidadesCrm: oportunidades, interaccionesCrm: interacciones, campanas: campanas, usuarios: usuarios, usuariosCrm: perfiles));

        var result = await controller.GetReportes(ct: CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var resumenComercial = ok.Value!.GetType().GetProperty("ResumenComercial")!.GetValue(ok.Value!);
        var resumenMarketing = ok.Value!.GetType().GetProperty("ResumenMarketing")!.GetValue(ok.Value!);
        var pipelinePorEtapa = ((IEnumerable)ok.Value!.GetType().GetProperty("PipelinePorEtapa")!.GetValue(ok.Value!)!).Cast<object>().ToList();

        AssertAnonymousProperty(resumenComercial!, "ClientesActivos", 1);
        AssertAnonymousProperty(resumenComercial!, "PipelineAbierto", 2500m);
        AssertAnonymousProperty(resumenMarketing!, "CampanasActivas", 1);
        AssertAnonymousProperty(resumenMarketing!, "Leads", 10);
        var lead = pipelinePorEtapa.Single(x => Equals(x.GetType().GetProperty("Etapa")!.GetValue(x), "lead"));
        AssertAnonymousProperty(lead, "Cantidad", 1);
    }

    [Fact]
    public async Task GetReportes_AplicaFiltrosAvanzados()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        Tercero[] terceros =
        [
            BuildTercero(10, "Acme SA", "80012345-6", "ventas@acme.com"),
            BuildTercero(11, "Beta SRL", "80099999-1", "contacto@beta.com")
        ];
        CrmCliente[] clientes =
        [
            BuildCrmCliente(10, "activo", "pyme", "Retail", "Paraguay", "Asuncion", "Centro", "Palma 123", "web", "nuevo", 7),
            BuildCrmCliente(11, "activo", "corporativo", "Agro", "Paraguay", "Central", "Luque", "Ruta 1", "referido", "fidelizado", 8)
        ];
        CrmOportunidad[] oportunidades =
        [
            BuildCrmOportunidad(8, 10, "Pipeline Acme", "lead", today.AddDays(3), 2500m, 7),
            BuildCrmOportunidad(9, 11, "Pipeline Beta", "lead", today.AddDays(3), 5000m, 8)
        ];
        CrmInteraccion[] interacciones =
        [
            BuildCrmInteraccion(9, 10, "llamada", "telefono", DateTimeOffset.UtcNow.AddDays(-2), 7),
            BuildCrmInteraccion(10, 11, "email", "email", DateTimeOffset.UtcNow.AddDays(-40), 8)
        ];
        CrmCampana[] campanas =
        [
            BuildCampana(14, 10, "Camp Acme", today.AddDays(-2), today.AddDays(2), 100m, 120m, 7, 10, 4, 2, true),
            BuildCampana(15, 10, "Camp Beta", today.AddDays(-40), today.AddDays(-20), 200m, 180m, 8, 20, 6, 3, false)
        ];
        Usuario[] usuarios =
        [
            BuildUsuario(7, "juan.perez", "Juan Perez", "juan@crm.com", true),
            BuildUsuario(8, "ana.lopez", "Ana Lopez", "ana@crm.com", true)
        ];
        CrmUsuarioPerfil[] perfiles =
        [
            BuildCrmUsuarioPerfil(1, 7, "comercial", true),
            BuildCrmUsuarioPerfil(2, 8, "comercial", true)
        ];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(terceros: terceros, clientesCrm: clientes, oportunidadesCrm: oportunidades, interaccionesCrm: interacciones, campanas: campanas, usuarios: usuarios, usuariosCrm: perfiles));

        var result = await controller.GetReportes(
            responsableId: "7",
            segmento: "pyme",
            campanaId: "14",
            desde: today.AddDays(-5),
            hasta: today.AddDays(5),
            ct: CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var resumenComercial = ok.Value!.GetType().GetProperty("ResumenComercial")!.GetValue(ok.Value!);
        var resumenMarketing = ok.Value!.GetType().GetProperty("ResumenMarketing")!.GetValue(ok.Value!);
        var actividad = ((IEnumerable)ok.Value!.GetType().GetProperty("ActividadPorUsuario")!.GetValue(ok.Value!)!).Cast<object>().ToList();

        AssertAnonymousProperty(resumenComercial!, "ClientesActivos", 1);
        AssertAnonymousProperty(resumenComercial!, "PipelineAbierto", 2500m);
        AssertAnonymousProperty(resumenMarketing!, "CampanasActivas", 1);
        actividad.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetCatalogos_CuandoHayDatos_DevuelveOpcionesYSelectoresActivos()
    {
        Tercero[] terceros = [BuildTercero(10, "Acme SA", "80012345-6", "ventas@acme.com")];
        CrmCliente[] clientes = [BuildCrmCliente(10, "activo", "pyme", "Retail", "Paraguay", "Asuncion", "Centro", "Palma 123", "web", "nuevo", 7)];
        CrmContacto[] crmContactos = [BuildCrmContacto(5, 10, "Ana", "Lopez", "Compras")];
        CrmSegmento[] segmentos = [BuildCrmSegmento(4, "VIP", "Manual", "[]", "estatico")];
        TipoRelacionContactoCatalogo[] tiposRelacion = [BuildTipoRelacionContacto(3, "CLIENTE_CONTACTO", "Cliente / contacto", true)];
        Usuario[] usuarios = [BuildUsuario(7, "juan.perez", "Juan Perez", "juan@crm.com", true)];
        CrmUsuarioPerfil[] perfiles = [BuildCrmUsuarioPerfil(1, 7, "comercial", true)];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(terceros: terceros, clientesCrm: clientes, crmContactos: crmContactos, segmentosCrm: segmentos, tiposRelacionContacto: tiposRelacion, usuarios: usuarios, usuariosCrm: perfiles));

        var result = await controller.GetCatalogos(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var tiposCliente = ((IEnumerable)ok.Value!.GetType().GetProperty("TiposCliente")!.GetValue(ok.Value!)!).Cast<object>().ToList();
        var tiposRelacionContacto = ((IEnumerable)ok.Value!.GetType().GetProperty("TiposRelacionContacto")!.GetValue(ok.Value!)!).Cast<object>().ToList();
        var clientesSelector = ((IEnumerable)ok.Value!.GetType().GetProperty("Clientes")!.GetValue(ok.Value!)!).Cast<object>().ToList();
        var usuariosSelector = ((IEnumerable)ok.Value!.GetType().GetProperty("Usuarios")!.GetValue(ok.Value!)!).Cast<object>().ToList();

        tiposCliente.Should().NotBeEmpty();
        tiposRelacionContacto.Should().HaveCount(1);
        clientesSelector.Should().HaveCount(1);
        usuariosSelector.Should().HaveCount(1);
        AssertAnonymousProperty(clientesSelector[0], "Id", "10");
        AssertAnonymousProperty(usuariosSelector[0], "Rol", "comercial");
    }

    [Fact]
    public async Task GetTiposRelacion_CuandoHayCatalogo_DevuelveSelectorSemantico()
    {
        TipoRelacionContactoCatalogo[] tiposRelacion =
        [
            BuildTipoRelacionContacto(3, "CLIENTE_CONTACTO", "Cliente / contacto", true),
            BuildTipoRelacionContacto(4, "REFERENTE", "Referente", false)
        ];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(tiposRelacionContacto: tiposRelacion));

        var result = await controller.GetTiposRelacion(true, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(1);
        AssertAnonymousProperty(items[0], "Codigo", "CLIENTE_CONTACTO");
        AssertAnonymousProperty(items[0], "Descripcion", "Cliente / contacto");
    }

    [Fact]
    public async Task CerrarOportunidadGanada_CuandoTieneExito_DevuelvePayloadActualizado()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CloseCrmOportunidadGanadaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        CrmOportunidad[] oportunidades = [BuildCrmOportunidad(8, 10, "Pipeline", "cerrado_ganado")];
        var controller = CreateController(mediator, BuildDb(oportunidadesCrm: oportunidades));

        var result = await controller.CerrarOportunidadGanada(8, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", "8");
    }

    [Fact]
    public async Task ReasignarOportunidad_CuandoFaltaResponsable_DevuelveBadRequest()
    {
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb());

        var result = await controller.ReasignarOportunidad(8, new CrmReassignRequest(""), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CompletarTarea_CuandoTieneExito_DevuelvePayloadActualizado()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CompleteCrmTareaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var tarea = CrmTarea.Crear(10, null, 7, "Seguimiento", "Detalle", "seguimiento", new DateOnly(2026, 3, 5), "media", "completada", new DateOnly(2026, 3, 6), null);
        SetProperty(tarea, nameof(CrmTarea.Id), 11L);
        var controller = CreateController(mediator, BuildDb(tareasCrm: [tarea]));

        var result = await controller.CompletarTarea(11, new CrmCompleteTaskRequest(new DateTimeOffset(new DateTime(2026, 3, 6, 0, 0, 0, DateTimeKind.Utc))), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", "11");
        AssertAnonymousProperty(ok.Value!, "Estado", "completada");
    }

    [Fact]
    public async Task GetUsuarioCrmById_CuandoExiste_DevuelvePayload()
    {
        Usuario[] usuarios = [BuildUsuario(7, "juan.perez", "Juan Perez", "juan@crm.com", true)];
        CrmUsuarioPerfil[] perfiles = [BuildCrmUsuarioPerfil(1, 7, "supervisor", true)];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(usuarios: usuarios, usuariosCrm: perfiles));

        var result = await controller.GetUsuarioCrmById(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", "7");
        AssertAnonymousProperty(ok.Value!, "Nombre", "Juan");
    }

    [Fact]
    public async Task GetSeguimientos_AplicaFiltrosYOrdenaPorFechaDescEId()
    {
        CrmSeguimiento[] seguimientos =
        [
            BuildSeguimiento(1, 10, 20, 30, 40, 50, new DateOnly(2026, 3, 1), "Uno", new DateOnly(2026, 3, 10), 60),
            BuildSeguimiento(2, 10, 20, 30, 40, 50, new DateOnly(2026, 3, 2), "Dos", new DateOnly(2026, 3, 11), 60),
            BuildSeguimiento(3, 10, 21, 30, 40, 50, new DateOnly(2026, 3, 3), "Tres", new DateOnly(2026, 3, 12), 60),
            BuildSeguimiento(4, 11, 20, 30, 40, 51, new DateOnly(2026, 3, 4), "Cuatro", new DateOnly(2026, 3, 13), 60)
        ];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(seguimientos: seguimientos));

        var result = await controller.GetSeguimientos(10, 20, 50, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Id", 2L);
        AssertAnonymousProperty(items[1], "Id", 1L);
    }

    [Fact]
    public async Task CreateSeguimiento_CuandoTieneExito_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateCrmSeguimientoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(22L));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.CreateSeguimiento(new CrmSeguimientoRequest(10, 20, 30, 40, 50, new DateOnly(2026, 3, 1), "Gestion", new DateOnly(2026, 3, 10), 60), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(CrmController.GetSeguimientos));
        AssertAnonymousProperty(created.Value!, "Id", 22L);
    }

    [Fact]
    public async Task UpdateSeguimiento_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateCrmSeguimientoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Seguimiento 22 no encontrado."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.UpdateSeguimiento(22, new CrmSeguimientoUpdateRequest("Gestion", new DateOnly(2026, 3, 10)), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteSeguimiento_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteCrmSeguimientoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.DeleteSeguimiento(22, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    private static CrmController CreateController(IMediator mediator, IApplicationDbContext db)
    {
        return new CrmController(mediator, db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static IApplicationDbContext BuildDb(
        IEnumerable<Contacto>? contactos = null,
        IEnumerable<TipoRelacionContactoCatalogo>? tiposRelacionContacto = null,
        IEnumerable<Tercero>? terceros = null,
        IEnumerable<CrmCliente>? clientesCrm = null,
        IEnumerable<CrmContacto>? crmContactos = null,
        IEnumerable<CrmTipoComunicado>? tiposComunicado = null,
        IEnumerable<CrmMotivo>? motivos = null,
        IEnumerable<CrmInteres>? intereses = null,
        IEnumerable<CrmCampana>? campanas = null,
        IEnumerable<CrmComunicado>? comunicados = null,
        IEnumerable<CrmSeguimiento>? seguimientos = null,
        IEnumerable<CrmOportunidad>? oportunidadesCrm = null,
        IEnumerable<CrmInteraccion>? interaccionesCrm = null,
        IEnumerable<CrmTarea>? tareasCrm = null,
        IEnumerable<CrmSegmento>? segmentosCrm = null,
        IEnumerable<CrmSegmentoMiembro>? segmentosMiembrosCrm = null,
        IEnumerable<Usuario>? usuarios = null,
        IEnumerable<CrmUsuarioPerfil>? usuariosCrm = null,
        IEnumerable<Sucursal>? sucursales = null)
    {
        var db = Substitute.For<IApplicationDbContext>();
        var contactosDbSet = MockDbSetHelper.CreateMockDbSet(contactos);
        var tiposRelacionContactoDbSet = MockDbSetHelper.CreateMockDbSet(tiposRelacionContacto);
        var tercerosDbSet = MockDbSetHelper.CreateMockDbSet(terceros);
        var clientesCrmDbSet = MockDbSetHelper.CreateMockDbSet(clientesCrm);
        var crmContactosDbSet = MockDbSetHelper.CreateMockDbSet(crmContactos);
        var tiposComunicadoDbSet = MockDbSetHelper.CreateMockDbSet(tiposComunicado);
        var motivosDbSet = MockDbSetHelper.CreateMockDbSet(motivos);
        var interesesDbSet = MockDbSetHelper.CreateMockDbSet(intereses);
        var campanasDbSet = MockDbSetHelper.CreateMockDbSet(campanas);
        var comunicadosDbSet = MockDbSetHelper.CreateMockDbSet(comunicados);
        var seguimientosDbSet = MockDbSetHelper.CreateMockDbSet(seguimientos);
        var oportunidadesCrmDbSet = MockDbSetHelper.CreateMockDbSet(oportunidadesCrm);
        var interaccionesCrmDbSet = MockDbSetHelper.CreateMockDbSet(interaccionesCrm);
        var tareasCrmDbSet = MockDbSetHelper.CreateMockDbSet(tareasCrm);
        var segmentosCrmDbSet = MockDbSetHelper.CreateMockDbSet(segmentosCrm);
        var segmentosMiembrosCrmDbSet = MockDbSetHelper.CreateMockDbSet(segmentosMiembrosCrm);
        var usuariosDbSet = MockDbSetHelper.CreateMockDbSet(usuarios);
        var usuariosCrmDbSet = MockDbSetHelper.CreateMockDbSet(usuariosCrm);
        var sucursalesDbSet = MockDbSetHelper.CreateMockDbSet(sucursales);

        db.Contactos.Returns(contactosDbSet);
        db.TiposRelacionesContacto.Returns(tiposRelacionContactoDbSet);
        db.Terceros.Returns(tercerosDbSet);
        db.CrmClientes.Returns(clientesCrmDbSet);
        db.CrmContactos.Returns(crmContactosDbSet);
        db.CrmTiposComunicado.Returns(tiposComunicadoDbSet);
        db.CrmMotivos.Returns(motivosDbSet);
        db.CrmIntereses.Returns(interesesDbSet);
        db.CrmCampanas.Returns(campanasDbSet);
        db.CrmComunicados.Returns(comunicadosDbSet);
        db.CrmSeguimientos.Returns(seguimientosDbSet);
        db.CrmOportunidades.Returns(oportunidadesCrmDbSet);
        db.CrmInteracciones.Returns(interaccionesCrmDbSet);
        db.CrmTareas.Returns(tareasCrmDbSet);
        db.CrmSegmentos.Returns(segmentosCrmDbSet);
        db.CrmSegmentosMiembros.Returns(segmentosMiembrosCrmDbSet);
        db.Usuarios.Returns(usuariosDbSet);
        db.CrmUsuariosPerfiles.Returns(usuariosCrmDbSet);
        db.Sucursales.Returns(sucursalesDbSet);
        return db;
    }

    private static Contacto BuildContacto(long id, long personaId, long personaContactoId, long? tipoRelacionId)
    {
        var entity = Contacto.Crear(personaId, personaContactoId, tipoRelacionId);
        SetProperty(entity, nameof(Contacto.Id), id);
        return entity;
    }

    private static TipoRelacionContactoCatalogo BuildTipoRelacionContacto(long id, string codigo, string descripcion, bool activo)
    {
        var entity = TipoRelacionContactoCatalogo.Crear(codigo, descripcion, null);
        SetProperty(entity, nameof(TipoRelacionContactoCatalogo.Id), id);
        if (!activo)
            entity.Desactivar(null);
        return entity;
    }

    private static CrmTipoComunicado BuildTipoComunicado(long id, string codigo, string descripcion, bool activo)
    {
        var entity = CrmTipoComunicado.Crear(codigo, descripcion, null);
        SetProperty(entity, nameof(CrmTipoComunicado.Id), id);
        if (!activo)
            entity.Desactivar(null);
        return entity;
    }

    private static CrmMotivo BuildMotivo(long id, string codigo, string descripcion, bool activo)
    {
        var entity = CrmMotivo.Crear(codigo, descripcion, null);
        SetProperty(entity, nameof(CrmMotivo.Id), id);
        if (!activo)
            entity.Desactivar(null);
        return entity;
    }

    private static CrmInteres BuildInteres(long id, string codigo, string descripcion, bool activo)
    {
        var entity = CrmInteres.Crear(codigo, descripcion, null);
        SetProperty(entity, nameof(CrmInteres.Id), id);
        if (!activo)
            entity.Desactivar(null);
        return entity;
    }

    private static CrmCampana BuildCampana(long id, long sucursalId, string nombre, DateOnly fechaInicio, DateOnly fechaFin, decimal? presupuesto, bool activa)
    {
        var entity = CrmCampana.Crear(sucursalId, nombre, null, fechaInicio, fechaFin, presupuesto, null);
        SetProperty(entity, nameof(CrmCampana.Id), id);
        if (!activa)
            entity.Cerrar(null);
        return entity;
    }

    private static CrmComunicado BuildComunicado(long id, long sucursalId, long terceroId, long? campanaId, long? tipoId, DateOnly fecha, string asunto, string? contenido, long? usuarioId)
    {
        var entity = CrmComunicado.Crear(sucursalId, terceroId, campanaId, tipoId, fecha, asunto, contenido, usuarioId);
        SetProperty(entity, nameof(CrmComunicado.Id), id);
        return entity;
    }

    private static CrmSeguimiento BuildSeguimiento(long id, long sucursalId, long terceroId, long? motivoId, long? interesId, long? campanaId, DateOnly fecha, string descripcion, DateOnly? proximaAccion, long? usuarioId)
    {
        var entity = CrmSeguimiento.Crear(sucursalId, terceroId, motivoId, interesId, campanaId, fecha, descripcion, proximaAccion, usuarioId);
        SetProperty(entity, nameof(CrmSeguimiento.Id), id);
        return entity;
    }

    private static Tercero BuildTercero(long id, string razonSocial, string nroDocumento, string? email)
    {
        var entity = Tercero.Crear($"CLI-{id}", razonSocial, 1, nroDocumento, 1, true, false, false, null, null);
        entity.Actualizar(razonSocial, razonSocial, 1, null, null, email, null, ZuluIA_Back.Domain.ValueObjects.Domicilio.Vacio(), null, null, null, null, null, null, true, null, false, 0m, null, false, 0m, null, null);
        SetProperty(entity, nameof(Tercero.Id), id);
        return entity;
    }

    private static CrmCliente BuildCrmCliente(long terceroId, string tipoCliente, string segmento, string? industria, string pais, string? provincia, string? ciudad, string? direccion, string origenCliente, string estadoRelacion, long? responsableId = null)
    {
        return CrmCliente.Crear(terceroId, tipoCliente, segmento, industria, pais, provincia, ciudad, direccion, origenCliente, estadoRelacion, responsableId, null, null);
    }

    private static CrmContacto BuildCrmContacto(long id, long clienteId, string nombre, string apellido, string? cargo)
    {
        var entity = CrmContacto.Crear(clienteId, nombre, apellido, cargo, "ana@test.com", "0991000000", "email", "activo", null, null);
        SetProperty(entity, nameof(CrmContacto.Id), id);
        return entity;
    }

    private static CrmOportunidad BuildCrmOportunidad(long id, long clienteId, string titulo, string etapa)
    {
        var entity = CrmOportunidad.Crear(clienteId, null, titulo, etapa, 50, 1000m, "USD", new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), null, "web", null, null, null);
        SetProperty(entity, nameof(CrmOportunidad.Id), id);
        return entity;
    }

    private static CrmOportunidad BuildCrmOportunidad(long id, long clienteId, string titulo, string etapa, DateOnly fechaEstimadaCierre, decimal montoEstimado, long? responsableId)
    {
        var entity = CrmOportunidad.Crear(clienteId, null, titulo, etapa, 50, montoEstimado, "USD", fechaEstimadaCierre.AddDays(-15), fechaEstimadaCierre, responsableId, "web", null, null, null);
        SetProperty(entity, nameof(CrmOportunidad.Id), id);
        return entity;
    }

    private static CrmInteraccion BuildCrmInteraccion(long id, long clienteId, string tipoInteraccion, string canal)
    {
        var entity = CrmInteraccion.Crear(clienteId, null, null, tipoInteraccion, canal, new DateTimeOffset(new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc)), 7, "exitosa", "Llamada inicial", "[]", null);
        SetProperty(entity, nameof(CrmInteraccion.Id), id);
        return entity;
    }

    private static CrmInteraccion BuildCrmInteraccion(long id, long clienteId, string tipoInteraccion, string canal, DateTimeOffset fechaHora, long usuarioResponsableId)
    {
        var entity = CrmInteraccion.Crear(clienteId, null, null, tipoInteraccion, canal, fechaHora, usuarioResponsableId, "exitosa", "Llamada inicial", "[]", null);
        SetProperty(entity, nameof(CrmInteraccion.Id), id);
        return entity;
    }

    private static CrmCampana BuildCampana(long id, long sucursalId, string nombre, DateOnly fechaInicio, DateOnly fechaFin, decimal? presupuesto, decimal? presupuestoGastado, long? responsableId, int leadsGenerados, int oportunidadesGeneradas, int negociosGanados, bool activa)
    {
        var entity = CrmCampana.Crear(sucursalId, nombre, null, fechaInicio, fechaFin, presupuesto, null, "email", "generacion_leads", null, presupuestoGastado, responsableId, null, leadsGenerados, oportunidadesGeneradas, negociosGanados);
        SetProperty(entity, nameof(CrmCampana.Id), id);
        if (!activa)
            entity.Cerrar(null);
        return entity;
    }

    private static CrmTarea BuildCrmTarea(long id, long clienteId, string titulo)
    {
        var entity = CrmTarea.Crear(clienteId, null, 7, titulo, "Detalle", "seguimiento", new DateOnly(2026, 3, 5), "media", "pendiente", null, null);
        SetProperty(entity, nameof(CrmTarea.Id), id);
        return entity;
    }

    private static CrmSegmento BuildCrmSegmento(long id, string nombre, string? descripcion, string criteriosJson, string tipoSegmento)
    {
        var entity = CrmSegmento.Crear(nombre, descripcion, criteriosJson, tipoSegmento, null);
        SetProperty(entity, nameof(CrmSegmento.Id), id);
        return entity;
    }

    private static CrmSegmentoMiembro BuildCrmSegmentoMiembro(long id, long segmentoId, long clienteId)
    {
        var entity = CrmSegmentoMiembro.Crear(segmentoId, clienteId, null);
        SetProperty(entity, nameof(CrmSegmentoMiembro.Id), id);
        return entity;
    }

    private static Sucursal BuildSucursal(long id, bool casaMatriz)
    {
        var entity = Sucursal.Crear("Casa Central", "80012345-6", 1, 1, 1, casaMatriz, null);
        SetProperty(entity, nameof(Sucursal.Id), id);
        return entity;
    }

    private static Usuario BuildUsuario(long id, string userName, string? nombreCompleto, string? email, bool activo)
    {
        var entity = Usuario.Crear(userName, nombreCompleto, email, null, null);
        SetProperty(entity, nameof(Usuario.Id), id);
        if (!activo)
            entity.Desactivar(null);
        return entity;
    }

    private static CrmUsuarioPerfil BuildCrmUsuarioPerfil(long id, long usuarioId, string rol, bool activo)
    {
        var entity = CrmUsuarioPerfil.Crear(usuarioId, rol, null, null);
        SetProperty(entity, nameof(CrmUsuarioPerfil.Id), id);
        if (!activo)
            entity.Actualizar(rol, null, false, null);
        return entity;
    }

    private static void SetProperty(object target, string propertyName, object? value)
    {
        target.GetType().GetProperty(propertyName)!.SetValue(target, value);
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object? expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}