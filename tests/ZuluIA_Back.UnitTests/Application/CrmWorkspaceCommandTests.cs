using FluentAssertions;
using FluentValidation.TestHelper;
using NSubstitute;
using System.Reflection;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.CRM.Commands;
using ZuluIA_Back.Domain.Entities.CRM;
using ZuluIA_Back.Domain.Entities.Sucursales;
using ZuluIA_Back.Domain.Entities.Usuarios;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Application;

public class CrmWorkspaceCommandTests
{
    [Fact]
    public void CreateCrmClienteCommandValidator_CuandoTipoClienteEsInvalido_DebeTenerError()
    {
        var validator = new CreateCrmClienteCommandValidator();
        var result = validator.TestValidate(CreateClienteCommand() with { TipoCliente = "fantasma" });

        result.ShouldHaveValidationErrorFor(x => x.TipoCliente);
    }

    [Fact]
    public void CreateCrmOportunidadCommandValidator_CuandoEtapaEsInvalida_DebeTenerError()
    {
        var validator = new CreateCrmOportunidadCommandValidator();
        var result = validator.TestValidate(CreateOportunidadCommand() with { Etapa = "cerrada" });

        result.ShouldHaveValidationErrorFor(x => x.Etapa);
    }

    [Fact]
    public void CreateCrmSegmentoCommandValidator_CuandoElJsonDeCriteriosEsInvalido_DebeTenerError()
    {
        var validator = new CreateCrmSegmentoCommandValidator();
        var result = validator.TestValidate(new CreateCrmSegmentoCommand("VIP", null, "{", "dinamico"));

        result.ShouldHaveValidationErrorFor(x => x.CriteriosJson);
    }

    [Fact]
    public void CreateCrmUsuarioCommandValidator_CuandoRolEsInvalido_DebeTenerError()
    {
        var validator = new CreateCrmUsuarioCommandValidator();
        var result = validator.TestValidate(new CreateCrmUsuarioCommand("Ana", "Perez", "ana@test.com", "externo", "activo", null));

        result.ShouldHaveValidationErrorFor(x => x.Rol);
    }

    [Fact]
    public void GenerateLegajo_CuandoSeUsaEnCRM_DebeRespetarLaLongitudDeTerceros()
    {
        var helperType = typeof(CreateCrmClienteCommand).Assembly
            .GetType("ZuluIA_Back.Application.Features.CRM.Commands.CrmCommandPersistenceHelper");
        var legajo = (string)helperType!
            .GetMethod("GenerateLegajo", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!
            .Invoke(null, null)!;

        legajo.Should().StartWith("CRM");
        legajo.Length.Should().BeLessOrEqualTo(20);
    }

    [Fact]
    public async Task CreateCrmContactoCommandHandler_CuandoClienteNoExiste_DebeFallar()
    {
        var db = BuildDb();
        var handler = new CreateCrmContactoCommandHandler(db);

        var result = await handler.Handle(
            new CreateCrmContactoCommand(10, "Ana", "Lopez", null, null, null, "email", "activo", null),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Cliente CRM 10 no encontrado o inactivo");
    }

    [Fact]
    public async Task CreateCrmOportunidadCommandHandler_CuandoContactoEsDeOtroCliente_DebeFallar()
    {
        var db = BuildDb(
            clientes: [BuildCrmCliente(10)],
            contactos: [BuildCrmContacto(20, 11)],
            usuarios: [BuildUsuario(7, true)]);
        var handler = new CreateCrmOportunidadCommandHandler(db);

        var result = await handler.Handle(CreateOportunidadCommand(contactoPrincipalId: 20, responsableId: 7), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("no pertenece al cliente 10");
    }

    [Fact]
    public async Task CreateCrmInteraccionCommandHandler_CuandoOportunidadEsDeOtroCliente_DebeFallar()
    {
        var db = BuildDb(
            clientes: [BuildCrmCliente(10)],
            oportunidades: [BuildCrmOportunidad(30, 11)],
            usuarios: [BuildUsuario(7, true)]);
        var handler = new CreateCrmInteraccionCommandHandler(db);

        var result = await handler.Handle(
            new CreateCrmInteraccionCommand(10, null, 30, "llamada", "telefono", DateTimeOffset.UtcNow, 7, "exitosa", null, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("no pertenece al cliente 10");
    }

    [Fact]
    public async Task CreateCrmTareaCommandHandler_CuandoResponsableEstaInactivo_DebeFallar()
    {
        var db = BuildDb(
            clientes: [BuildCrmCliente(10)],
            usuarios: [BuildUsuario(7, false)]);
        var handler = new CreateCrmTareaCommandHandler(db);

        var result = await handler.Handle(
            new CreateCrmTareaCommand(10, null, 7, "Seguimiento", null, "seguimiento", new DateOnly(2026, 3, 10), "media", "pendiente", null),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("responsable CRM");
    }

    [Fact]
    public async Task CreateCrmCampanaCommandHandler_CuandoSucursalNoExiste_DebeFallar()
    {
        var db = BuildDb();
        var handler = new CreateCrmCampanaCommandHandler(db);

        var result = await handler.Handle(
            new CreateCrmCampanaCommand(10, "Camp", null, new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), 100m, "email", "generacion_leads", null, 0m, null, null, 0, 0, 0),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Sucursal 10 no encontrada o inactiva");
    }

    [Fact]
    public async Task AddCrmSegmentoClienteCommandHandler_CuandoSegmentoEsEstatico_AgregaMembresia()
    {
        var segmento = BuildCrmSegmento(4, "estatico");
        var cliente = BuildCrmCliente(10);
        var db = BuildDb(clientes: [cliente], segmentos: [segmento]);
        var handler = new AddCrmSegmentoClienteCommandHandler(db);

        var result = await handler.Handle(new AddCrmSegmentoClienteCommand(4, 10), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        ((IEnumerable<CrmSegmentoMiembro>)db.CrmSegmentosMiembros).Should().ContainSingle();
    }

    [Fact]
    public async Task AddCrmSegmentoClienteCommandHandler_CuandoSegmentoEsDinamico_DebeFallar()
    {
        var segmento = BuildCrmSegmento(4, "dinamico");
        var cliente = BuildCrmCliente(10);
        var db = BuildDb(clientes: [cliente], segmentos: [segmento]);
        var handler = new AddCrmSegmentoClienteCommandHandler(db);

        var result = await handler.Handle(new AddCrmSegmentoClienteCommand(4, 10), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("estáticos admiten membresía manual");
    }

    [Fact]
    public async Task RemoveCrmSegmentoClienteCommandHandler_CuandoExisteMembresia_LaDesactiva()
    {
        var segmento = BuildCrmSegmento(4, "estatico");
        var cliente = BuildCrmCliente(10);
        var miembro = BuildCrmSegmentoMiembro(1, 4, 10);
        var db = BuildDb(clientes: [cliente], segmentos: [segmento], segmentosMiembros: [miembro]);
        var handler = new RemoveCrmSegmentoClienteCommandHandler(db);

        var result = await handler.Handle(new RemoveCrmSegmentoClienteCommand(4, 10), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        miembro.Activo.Should().BeFalse();
    }

    [Fact]
    public async Task CloseCrmOportunidadGanadaCommandHandler_CuandoEstaAbierta_LaCierra()
    {
        var oportunidad = BuildCrmOportunidad(30, 10);
        var db = BuildDb(oportunidades: [oportunidad]);
        var handler = new CloseCrmOportunidadGanadaCommandHandler(db);

        var result = await handler.Handle(new CloseCrmOportunidadGanadaCommand(30), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        oportunidad.Etapa.Should().Be("cerrado_ganado");
        oportunidad.Probabilidad.Should().Be(100);
    }

    [Fact]
    public async Task ReassignCrmOportunidadCommandHandler_CuandoResponsableEsValido_Reasigna()
    {
        var oportunidad = BuildCrmOportunidad(30, 10);
        var db = BuildDb(oportunidades: [oportunidad], usuarios: [BuildUsuario(7, true)]);
        var handler = new ReassignCrmOportunidadCommandHandler(db);

        var result = await handler.Handle(new ReassignCrmOportunidadCommand(30, 7), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        oportunidad.ResponsableId.Should().Be(7);
    }

    [Fact]
    public async Task CompleteCrmTareaCommandHandler_CuandoLaTareaExiste_LaCompleta()
    {
        var tarea = BuildCrmTarea(40, 10, 30);
        var db = BuildDb(tareas: [tarea]);
        var handler = new CompleteCrmTareaCommandHandler(db);

        var result = await handler.Handle(new CompleteCrmTareaCommand(40, new DateOnly(2026, 3, 11)), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        tarea.Estado.Should().Be("completada");
        tarea.FechaCompletado.Should().Be(new DateOnly(2026, 3, 11));
    }

    [Fact]
    public async Task ReopenCrmTareaCommandHandler_CuandoLaTareaEstaCompletada_LaReabre()
    {
        var tarea = CrmTarea.Crear(10, null, 7, "Seguimiento", null, "seguimiento", new DateOnly(2026, 3, 10), "media", "completada", new DateOnly(2026, 3, 10), null);
        typeof(CrmTarea).GetProperty(nameof(CrmTarea.Id))!.SetValue(tarea, 40L);
        var db = BuildDb(tareas: [tarea]);
        var handler = new ReopenCrmTareaCommandHandler(db);

        var result = await handler.Handle(new ReopenCrmTareaCommand(40), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        tarea.Estado.Should().Be("pendiente");
        tarea.FechaCompletado.Should().BeNull();
    }

    [Fact]
    public async Task DeleteCrmClienteCommandHandler_CuandoTieneDependencias_DebeAplicarBajaEnCascada()
    {
        var perfil = BuildCrmCliente(10);
        var contacto = BuildCrmContacto(20, 10);
        var oportunidad = BuildCrmOportunidad(30, 10);
        var tarea = BuildCrmTarea(40, 10, 30);
        var interaccion = BuildCrmInteraccion(50, 10, 30);
        var db = BuildDb(
            clientes: [perfil],
            contactos: [contacto],
            oportunidades: [oportunidad],
            tareas: [tarea],
            interacciones: [interaccion]);
        var handler = new DeleteCrmClienteCommandHandler(db);

        var result = await handler.Handle(new DeleteCrmClienteCommand(10), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        perfil.Activo.Should().BeFalse();
        contacto.Activo.Should().BeFalse();
        oportunidad.Activa.Should().BeFalse();
        tarea.Activa.Should().BeFalse();
        ((IEnumerable<CrmInteraccion>)db.CrmInteracciones).Should().BeEmpty();
    }

    [Fact]
    public void CrmOportunidad_CuandoSeMarcaGanada_NormalizaProbabilidadYMotivo()
    {
        var oportunidad = CrmOportunidad.Crear(10, null, "Pipeline", "cerrado_ganado", 25, 1000m, "USD", new DateOnly(2026, 3, 1), null, null, "web", "No aplica", null, null);

        oportunidad.Probabilidad.Should().Be(100);
        oportunidad.MotivoPerdida.Should().BeNull();
    }

    [Fact]
    public void CrmOportunidad_CuandoEstaCerrada_NoPuedeReabrirse()
    {
        var oportunidad = CrmOportunidad.Crear(10, null, "Pipeline", "cerrado_ganado", 100, 1000m, "USD", new DateOnly(2026, 3, 1), null, null, "web", null, null, null);

        var act = () => oportunidad.Actualizar(10, null, "Pipeline", "lead", 50, 1000m, "USD", new DateOnly(2026, 3, 1), null, null, "web", null, null, null);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CrmTarea_CuandoNoEstaCompletada_LimpiaFechaCompletado()
    {
        var tarea = CrmTarea.Crear(10, null, 7, "Seguimiento", null, "seguimiento", new DateOnly(2026, 3, 10), "media", "pendiente", new DateOnly(2026, 3, 10), null);

        tarea.FechaCompletado.Should().BeNull();
    }

    [Fact]
    public void CrmTarea_CuandoEstaCompletada_SoloPuedeVolverAPendiente()
    {
        var tarea = CrmTarea.Crear(10, null, 7, "Seguimiento", null, "seguimiento", new DateOnly(2026, 3, 10), "media", "completada", new DateOnly(2026, 3, 10), null);

        var act = () => tarea.Actualizar(10, null, 7, "Seguimiento", null, "seguimiento", new DateOnly(2026, 3, 10), "media", "en_curso", null, null);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CrmTarea_CuandoSeReabreDesdeCompletada_LimpiaFechaCompletado()
    {
        var tarea = CrmTarea.Crear(10, null, 7, "Seguimiento", null, "seguimiento", new DateOnly(2026, 3, 10), "media", "completada", new DateOnly(2026, 3, 10), null);

        tarea.Actualizar(10, null, 7, "Seguimiento", null, "seguimiento", new DateOnly(2026, 3, 10), "media", "pendiente", null, null);

        tarea.Estado.Should().Be("pendiente");
        tarea.FechaCompletado.Should().BeNull();
    }

    private static CreateCrmClienteCommand CreateClienteCommand()
        => new("Acme", "prospecto", "pyme", null, null, "Paraguay", null, null, null, null, null, null, "web", "nuevo", null, null, null);

    private static CreateCrmOportunidadCommand CreateOportunidadCommand(long? contactoPrincipalId = null, long? responsableId = null)
        => new(10, contactoPrincipalId, "Pipeline", "lead", 50, 1000m, "USD", new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), responsableId, "web", null, null);

    private static IApplicationDbContext BuildDb(
        IEnumerable<CrmCliente>? clientes = null,
        IEnumerable<CrmContacto>? contactos = null,
        IEnumerable<CrmOportunidad>? oportunidades = null,
        IEnumerable<CrmInteraccion>? interacciones = null,
        IEnumerable<CrmTarea>? tareas = null,
        IEnumerable<CrmSegmentoMiembro>? segmentosMiembros = null,
        IEnumerable<Usuario>? usuarios = null,
        IEnumerable<CrmSegmento>? segmentos = null,
        IEnumerable<Sucursal>? sucursales = null)
    {
        var db = Substitute.For<IApplicationDbContext>();
        db.CrmClientes.Returns(MockDbSetHelper.CreateMockDbSet(clientes));
        db.CrmContactos.Returns(MockDbSetHelper.CreateMockDbSet(contactos));
        db.CrmOportunidades.Returns(MockDbSetHelper.CreateMockDbSet(oportunidades));
        db.CrmInteracciones.Returns(MockDbSetHelper.CreateMockDbSet(interacciones));
        db.CrmTareas.Returns(MockDbSetHelper.CreateMockDbSet(tareas));
        db.CrmSegmentosMiembros.Returns(MockDbSetHelper.CreateMockDbSet(segmentosMiembros));
        db.CrmSegmentos.Returns(MockDbSetHelper.CreateMockDbSet(segmentos));
        db.Usuarios.Returns(MockDbSetHelper.CreateMockDbSet(usuarios));
        db.Sucursales.Returns(MockDbSetHelper.CreateMockDbSet(sucursales));
        return db;
    }

    private static CrmCliente BuildCrmCliente(long terceroId)
        => CrmCliente.Crear(terceroId, "prospecto", "pyme", null, "Paraguay", null, null, null, "web", "nuevo", null, null, null);

    private static CrmContacto BuildCrmContacto(long id, long clienteId)
    {
        var entity = CrmContacto.Crear(clienteId, "Ana", "Lopez", null, null, null, "email", "activo", null, null);
        typeof(CrmContacto).GetProperty(nameof(CrmContacto.Id))!.SetValue(entity, id);
        return entity;
    }

    private static CrmOportunidad BuildCrmOportunidad(long id, long clienteId)
    {
        var entity = CrmOportunidad.Crear(clienteId, null, "Pipeline", "lead", 50, 1000m, "USD", new DateOnly(2026, 3, 1), null, null, "web", null, null, null);
        typeof(CrmOportunidad).GetProperty(nameof(CrmOportunidad.Id))!.SetValue(entity, id);
        return entity;
    }

    private static CrmTarea BuildCrmTarea(long id, long clienteId, long oportunidadId)
    {
        var entity = CrmTarea.Crear(clienteId, oportunidadId, 7, "Seguimiento", null, "seguimiento", new DateOnly(2026, 3, 10), "media", "pendiente", null, null);
        typeof(CrmTarea).GetProperty(nameof(CrmTarea.Id))!.SetValue(entity, id);
        return entity;
    }

    private static CrmInteraccion BuildCrmInteraccion(long id, long clienteId, long oportunidadId)
    {
        var entity = CrmInteraccion.Crear(clienteId, null, oportunidadId, "llamada", "telefono", DateTimeOffset.UtcNow, 7, "exitosa", null, "[]", null);
        typeof(CrmInteraccion).GetProperty(nameof(CrmInteraccion.Id))!.SetValue(entity, id);
        return entity;
    }

    private static Usuario BuildUsuario(long id, bool activo)
    {
        var entity = Usuario.Crear($"user.{id}", "Usuario CRM", $"user{id}@crm.com", null, null);
        typeof(Usuario).GetProperty(nameof(Usuario.Id))!.SetValue(entity, id);
        if (!activo)
            entity.Desactivar(null);
        return entity;
    }

    private static CrmSegmento BuildCrmSegmento(long id, string tipoSegmento)
    {
        var entity = CrmSegmento.Crear("VIP", null, "[]", tipoSegmento, null);
        typeof(CrmSegmento).GetProperty(nameof(CrmSegmento.Id))!.SetValue(entity, id);
        return entity;
    }

    private static CrmSegmentoMiembro BuildCrmSegmentoMiembro(long id, long segmentoId, long clienteId)
    {
        var entity = CrmSegmentoMiembro.Crear(segmentoId, clienteId, null);
        typeof(CrmSegmentoMiembro).GetProperty(nameof(CrmSegmentoMiembro.Id))!.SetValue(entity, id);
        return entity;
    }
}
