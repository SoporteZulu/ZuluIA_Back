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
using ZuluIA_Back.Domain.Entities.Terceros;
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
    public async Task CreateCampana_CuandoTieneExito_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateCrmCampanaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(14L));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.CreateCampana(new CrmCampanaRequest(10, "Camp", null, new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), 100), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(CrmController.GetCampanas));
        AssertAnonymousProperty(created.Value!, "Id", 14L);
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
        IEnumerable<CrmTipoComunicado>? tiposComunicado = null,
        IEnumerable<CrmMotivo>? motivos = null,
        IEnumerable<CrmInteres>? intereses = null,
        IEnumerable<CrmCampana>? campanas = null,
        IEnumerable<CrmComunicado>? comunicados = null,
        IEnumerable<CrmSeguimiento>? seguimientos = null)
    {
        var db = Substitute.For<IApplicationDbContext>();
        var contactosDbSet = MockDbSetHelper.CreateMockDbSet(contactos);
        var tiposComunicadoDbSet = MockDbSetHelper.CreateMockDbSet(tiposComunicado);
        var motivosDbSet = MockDbSetHelper.CreateMockDbSet(motivos);
        var interesesDbSet = MockDbSetHelper.CreateMockDbSet(intereses);
        var campanasDbSet = MockDbSetHelper.CreateMockDbSet(campanas);
        var comunicadosDbSet = MockDbSetHelper.CreateMockDbSet(comunicados);
        var seguimientosDbSet = MockDbSetHelper.CreateMockDbSet(seguimientos);

        db.Contactos.Returns(contactosDbSet);
        db.CrmTiposComunicado.Returns(tiposComunicadoDbSet);
        db.CrmMotivos.Returns(motivosDbSet);
        db.CrmIntereses.Returns(interesesDbSet);
        db.CrmCampanas.Returns(campanasDbSet);
        db.CrmComunicados.Returns(comunicadosDbSet);
        db.CrmSeguimientos.Returns(seguimientosDbSet);
        return db;
    }

    private static Contacto BuildContacto(long id, long personaId, long personaContactoId, long? tipoRelacionId)
    {
        var entity = Contacto.Crear(personaId, personaContactoId, tipoRelacionId);
        SetProperty(entity, nameof(Contacto.Id), id);
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

    private static void SetProperty(object target, string propertyName, object? value)
    {
        target.GetType().GetProperty(propertyName)!.SetValue(target, value);
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object? expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}