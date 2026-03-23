using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using FluentAssertions;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Configuracion.Commands;
using ZuluIA_Back.Application.Features.Configuracion.DTOs;
using ZuluIA_Back.Application.Features.Configuracion.Queries;
using ZuluIA_Back.Application.Features.Finanzas.Commands;
using ZuluIA_Back.Application.Features.Finanzas.Queries;
using ZuluIA_Back.Application.Features.OrdenesPreparacion.Commands;
using ZuluIA_Back.Application.Features.OrdenesPreparacion.DTOs;
using ZuluIA_Back.Application.Features.OrdenesPreparacion.Queries;
using ZuluIA_Back.Application.Features.Proyectos.Commands;
using ZuluIA_Back.Application.Features.RRHH.Commands;
using ZuluIA_Back.Application.Features.TasasInteres.Commands;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Entities.Configuracion;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Entities.Logistica;
using ZuluIA_Back.Domain.Entities.Proyectos;
using ZuluIA_Back.Domain.Entities.RRHH;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.Services;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Application.Handlers;

// ═══════════════════════════════════════════════════════════════════════════════
// CONFIGURACIÓN
// ═══════════════════════════════════════════════════════════════════════════════

public class SetConfiguracionCommandHandlerTests
{
    private readonly IConfiguracionRepository _repo;
    private readonly SetConfiguracionCommandHandler _handler;

    public SetConfiguracionCommandHandlerTests()
    {
        _repo    = Substitute.For<IConfiguracionRepository>();
        _handler = new SetConfiguracionCommandHandler(_repo);
    }

    [Fact]
    public async Task Handle_CampoVacio_RetornaFailure()
    {
        var cmd = new SetConfiguracionCommand(string.Empty, "valor");
        var result = await _handler.Handle(cmd, CancellationToken.None);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_CampoEspacios_RetornaFailure()
    {
        var cmd = new SetConfiguracionCommand("   ", "valor");
        var result = await _handler.Handle(cmd, CancellationToken.None);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_CampoValido_LlamaUpsertYRetornaSuccess()
    {
        _repo.UpsertAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
             .Returns(Task.CompletedTask);

        var cmd = new SetConfiguracionCommand("MI_PARAM", "valor");
        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpsertAsync("MI_PARAM", "valor", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValorNulo_TambienPermitido()
    {
        _repo.UpsertAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
             .Returns(Task.CompletedTask);

        var cmd = new SetConfiguracionCommand("PARAM", null);
        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}

public class GetConfiguracionQueryHandlerTests
{
    private readonly IConfiguracionRepository _repo;
    private readonly IMapper _mapper;
    private readonly GetConfiguracionQueryHandler _handler;

    public GetConfiguracionQueryHandlerTests()
    {
        _repo    = Substitute.For<IConfiguracionRepository>();
        _mapper  = Substitute.For<IMapper>();
        _handler = new GetConfiguracionQueryHandler(_repo, _mapper);
    }

    [Fact]
    public async Task Handle_ListaVacia_RetornaListaVacia()
    {
        _repo.GetAllAsync(Arg.Any<CancellationToken>())
             .Returns(new List<ConfiguracionSistema>());
        _mapper.Map<IReadOnlyList<ConfiguracionDto>>(Arg.Any<object>())
               .Returns(Array.Empty<ConfiguracionDto>());

        var result = await _handler.Handle(new GetConfiguracionQuery(), CancellationToken.None);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ConParametros_LlamaMapper()
    {
        var config = ConfiguracionSistema.Crear("PARAM", "valor", 1, null);
        _repo.GetAllAsync(Arg.Any<CancellationToken>())
             .Returns(new List<ConfiguracionSistema> { config });
        var dtos = new List<ConfiguracionDto> { new ConfiguracionDto() };
        _mapper.Map<IReadOnlyList<ConfiguracionDto>>(Arg.Any<object>())
               .Returns(dtos.AsReadOnly());

        var result = await _handler.Handle(new GetConfiguracionQuery(), CancellationToken.None);
        result.Should().HaveCount(1);
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
// RRHH
// ═══════════════════════════════════════════════════════════════════════════════

public class CreateEmpleadoCommandHandlerTests
{
    private readonly IEmpleadoRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly CreateEmpleadoCommandHandler _handler;

    public CreateEmpleadoCommandHandlerTests()
    {
        _repo    = Substitute.For<IEmpleadoRepository>();
        _uow     = Substitute.For<IUnitOfWork>();
        _handler = new CreateEmpleadoCommandHandler(_repo, _uow);
    }

    private static CreateEmpleadoCommand ComandoValido() => new(
        TerceroId:    1,
        SucursalId:   1,
        Legajo:       "EMP001",
        Cargo:        "Analista",
        Area:         null,
        FechaIngreso: DateOnly.FromDateTime(DateTime.Today),
        SueldoBasico: 100_000m,
        MonedaId:     1);

    [Fact]
    public async Task Handle_LegajoDuplicado_RetornaFailure()
    {
        _repo.ExisteLegajoAsync("EMP001", null, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _handler.Handle(ComandoValido(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("EMP001");
    }

    [Fact]
    public async Task Handle_LegajoNuevo_CreaEmpleadoYRetornaSuccess()
    {
        _repo.ExisteLegajoAsync("EMP001", null, Arg.Any<CancellationToken>()).Returns(false);
        _repo.AddAsync(Arg.Any<Empleado>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _uow.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(ComandoValido(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).AddAsync(Arg.Any<Empleado>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class CreateLiquidacionSueldoCommandHandlerTests
{
    private readonly IApplicationDbContext _db;
    private readonly IUnitOfWork _uow;
    private readonly CreateLiquidacionSueldoCommandHandler _handler;

    public CreateLiquidacionSueldoCommandHandlerTests()
    {
        _db  = Substitute.For<IApplicationDbContext>();
        _uow = Substitute.For<IUnitOfWork>();
        _handler = new CreateLiquidacionSueldoCommandHandler(_db, _uow);
    }

    private static CreateLiquidacionSueldoCommand ComandoValido() => new(
        EmpleadoId:       1,
        SucursalId:       1,
        Anio:             2024,
        Mes:              6,
        SueldoBasico:     100_000m,
        TotalHaberes:     120_000m,
        TotalDescuentos:  20_000m,
        MonedaId:         1,
        Observacion:      null);

    [Fact]
    public async Task Handle_PeriodoDuplicado_RetornaFailure()
    {
        var liqExistente = new List<LiquidacionSueldo>().AsQueryable();
        // Retorna true para AnyAsync (periodo ya existe)
        var mockDbSet = MockDbSetHelper.CreateMockDbSet(new List<LiquidacionSueldo>());

        // Reconfigure to return true from AnyAsync
        _db.LiquidacionesSueldo.Returns(mockDbSet);
        // Force AnyAsync to return true by customizing
        // Since MockDbSetHelper doesn't easily mock AnyAsync for specific predicates,
        // we test via an existing record:
        var existing = LiquidacionSueldo.Crear(1, 1, 2024, 6, 100000m, 120000m, 20000m, 1, null);
        var dbSetWithRecord = MockDbSetHelper.CreateMockDbSet(new List<LiquidacionSueldo> { existing });
        _db.LiquidacionesSueldo.Returns(dbSetWithRecord);

        var result = await _handler.Handle(ComandoValido(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("2024");
    }

    [Fact]
    public async Task Handle_PeriodoNuevo_CreaLiquidacionYRetornaSuccess()
    {
        var dbSet = MockDbSetHelper.CreateMockDbSet(new List<LiquidacionSueldo>());
        _db.LiquidacionesSueldo.Returns(dbSet);
        _uow.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(ComandoValido(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
// PROYECTOS
// ═══════════════════════════════════════════════════════════════════════════════

public class DeactivateTareaEstimadaCommandHandlerTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();

    private DeactivateTareaEstimadaCommandHandler Sut() => new(_db);

    [Fact]
    public async Task Handle_TareaNoEncontrada_RetornaFailure()
    {
        var tareas = MockDbSetHelper.CreateMockDbSet<TareaEstimada>([]);
        _db.TareasEstimadas.Returns(tareas);

        var result = await Sut().Handle(new DeactivateTareaEstimadaCommand(99), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("no encontrada");
    }

    [Fact]
    public async Task Handle_TareaExistente_LaDesactivaYPersiste()
    {
        var tarea = TareaEstimada.Crear(1, 1, null, "Tarea", new DateOnly(2026, 4, 1), new DateOnly(2026, 4, 2), 2m, null, null);
        SetProperty(tarea, nameof(TareaEstimada.Id), 51L);

        var tareas = MockDbSetHelper.CreateMockDbSet(new[] { tarea });
        _db.TareasEstimadas.Returns(tareas);

        var result = await Sut().Handle(new DeactivateTareaEstimadaCommand(51), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        tarea.Activa.Should().BeFalse();
        await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static void SetProperty(object target, string propertyName, object? value)
    {
        var property = target.GetType().GetProperty(propertyName);
        property.Should().NotBeNull();
        property!.SetValue(target, value);
    }
}

public class ActivateTareaEstimadaCommandHandlerTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();

    private ActivateTareaEstimadaCommandHandler Sut() => new(_db);

    [Fact]
    public async Task Handle_TareaNoEncontrada_RetornaFailure()
    {
        var tareas = MockDbSetHelper.CreateMockDbSet<TareaEstimada>([]);
        _db.TareasEstimadas.Returns(tareas);

        var result = await Sut().Handle(new ActivateTareaEstimadaCommand(99), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("no encontrada");
    }

    [Fact]
    public async Task Handle_TareaExistente_LaActivaYPersiste()
    {
        var tarea = TareaEstimada.Crear(1, 1, null, "Tarea", new DateOnly(2026, 4, 1), new DateOnly(2026, 4, 2), 2m, null, null);
        SetProperty(tarea, nameof(TareaEstimada.Id), 50L);
        tarea.Desactivar(userId: null);

        var tareas = MockDbSetHelper.CreateMockDbSet(new[] { tarea });
        _db.TareasEstimadas.Returns(tareas);

        var result = await Sut().Handle(new ActivateTareaEstimadaCommand(50), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        tarea.Activa.Should().BeTrue();
        await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static void SetProperty(object target, string propertyName, object? value)
    {
        var property = target.GetType().GetProperty(propertyName);
        property.Should().NotBeNull();
        property!.SetValue(target, value);
    }
}

public class TareaEstimadaLifecycleCommandValidatorTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ActivateValidator_IdInvalido_RetornaError(long id)
    {
        var validator = new ActivateTareaEstimadaCommandValidator();

        var result = validator.Validate(new ActivateTareaEstimadaCommand(id));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == "Id");
    }

    [Fact]
    public void ActivateValidator_IdValido_Pasa()
    {
        var validator = new ActivateTareaEstimadaCommandValidator();

        var result = validator.Validate(new ActivateTareaEstimadaCommand(1));

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DeactivateValidator_IdInvalido_RetornaError(long id)
    {
        var validator = new DeactivateTareaEstimadaCommandValidator();

        var result = validator.Validate(new DeactivateTareaEstimadaCommand(id));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == "Id");
    }

    [Fact]
    public void DeactivateValidator_IdValido_Pasa()
    {
        var validator = new DeactivateTareaEstimadaCommandValidator();

        var result = validator.Validate(new DeactivateTareaEstimadaCommand(1));

        result.IsValid.Should().BeTrue();
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
// TASAS DE INTERES
// ═══════════════════════════════════════════════════════════════════════════════

public class DesactivarTasaInteresCommandHandlerTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();

    private DesactivarTasaInteresCommandHandler Sut() => new(_db);

    [Fact]
    public async Task Handle_TasaNoEncontrada_RetornaFailure()
    {
        _db.TasasInteres.FindAsync(Arg.Any<object[]>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<TasaInteres?>((TasaInteres?)null));

        var result = await Sut().Handle(new DesactivarTasaInteresCommand(99, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("no encontrada");
    }

    [Fact]
    public async Task Handle_TasaExistente_LaDesactivaYPersiste()
    {
        var tasa = TasaInteres.Crear("Mora", 5m, new DateOnly(2026, 4, 1), null, null);

        _db.TasasInteres.FindAsync(Arg.Any<object[]>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<TasaInteres?>(tasa));

        var result = await Sut().Handle(new DesactivarTasaInteresCommand(1, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        tasa.Activo.Should().BeFalse();
        await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class ActivarTasaInteresCommandHandlerTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();

    private ActivarTasaInteresCommandHandler Sut() => new(_db);

    [Fact]
    public async Task Handle_TasaNoEncontrada_RetornaFailure()
    {
        _db.TasasInteres.FindAsync(Arg.Any<object[]>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<TasaInteres?>((TasaInteres?)null));

        var result = await Sut().Handle(new ActivarTasaInteresCommand(99, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("no encontrada");
    }

    [Fact]
    public async Task Handle_TasaExistente_LaActivaYPersiste()
    {
        var tasa = TasaInteres.Crear("Mora", 5m, new DateOnly(2026, 4, 1), null, null);
        tasa.Desactivar(userId: null);

        _db.TasasInteres.FindAsync(Arg.Any<object[]>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<TasaInteres?>(tasa));

        var result = await Sut().Handle(new ActivarTasaInteresCommand(1, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        tasa.Activo.Should().BeTrue();
        await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class TasaInteresCommandValidatorTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ActivarValidator_IdInvalido_RetornaError(long id)
    {
        var validator = new ActivarTasaInteresCommandValidator();

        var result = validator.Validate(new ActivarTasaInteresCommand(id, null));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == "Id");
    }

    [Fact]
    public void ActivarValidator_IdValido_Pasa()
    {
        var validator = new ActivarTasaInteresCommandValidator();

        var result = validator.Validate(new ActivarTasaInteresCommand(1, null));

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DesactivarValidator_IdInvalido_RetornaError(long id)
    {
        var validator = new DesactivarTasaInteresCommandValidator();

        var result = validator.Validate(new DesactivarTasaInteresCommand(id, null));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == "Id");
    }

    [Fact]
    public void DesactivarValidator_IdValido_Pasa()
    {
        var validator = new DesactivarTasaInteresCommandValidator();

        var result = validator.Validate(new DesactivarTasaInteresCommand(1, null));

        result.IsValid.Should().BeTrue();
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
// ÓRDENES DE PREPARACIÓN
// ═══════════════════════════════════════════════════════════════════════════════

public class CreateOrdenPreparacionCommandHandlerTests
{
    private readonly IApplicationDbContext _db;
    private readonly IUnitOfWork _uow;
    private readonly CreateOrdenPreparacionCommandHandler _handler;

    public CreateOrdenPreparacionCommandHandlerTests()
    {
        _db  = Substitute.For<IApplicationDbContext>();
        _uow = Substitute.For<IUnitOfWork>();
        _handler = new CreateOrdenPreparacionCommandHandler(_db, _uow);

        var mockSet = MockDbSetHelper.CreateMockDbSet(new List<OrdenPreparacion>());
        _db.OrdenesPreparacion.Returns(mockSet);
        _uow.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
    }

    [Fact]
    public async Task Handle_SinDetalles_RetornaFailure()
    {
        var cmd = new CreateOrdenPreparacionCommand(
            SucursalId:           1,
            ComprobanteOrigenId:  null,
            TerceroId:            null,
            Fecha:                DateOnly.FromDateTime(DateTime.Today),
            Observacion:          null,
            Detalles:             new List<CreateOrdenPreparacionDetalleDto>());

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("detalle");
    }

    [Fact]
    public async Task Handle_ConDetalles_LlamaSaveChangesDosVecesYRetornaSuccess()
    {
        var detalles = new List<CreateOrdenPreparacionDetalleDto>
        {
            new CreateOrdenPreparacionDetalleDto(ItemId: 1, DepositoId: 1, Cantidad: 5m, Observacion: null)
        };

        var cmd = new CreateOrdenPreparacionCommand(
            SucursalId:           1,
            ComprobanteOrigenId:  null,
            TerceroId:            null,
            Fecha:                DateOnly.FromDateTime(DateTime.Today),
            Observacion:          null,
            Detalles:             detalles);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        // SaveChanges called twice: once after Create (to get ID), once after AgregarDetalle
        await _uow.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class AnularOrdenPreparacionCommandHandlerTests
{
    private readonly IRepository<OrdenPreparacion> _repo;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly AnularOrdenPreparacionCommandHandler _handler;

    public AnularOrdenPreparacionCommandHandlerTests()
    {
        _repo        = Substitute.For<IRepository<OrdenPreparacion>>();
        _uow         = Substitute.For<IUnitOfWork>();
        _currentUser = Substitute.For<ICurrentUserService>();
        _currentUser.UserId.Returns(1L);
        _handler = new AnularOrdenPreparacionCommandHandler(_repo, _uow, _currentUser);
    }

    [Fact]
    public async Task Handle_OrdenNoEncontrada_RetornaFailure()
    {
        _repo.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns((OrdenPreparacion?)null);

        var result = await _handler.Handle(new AnularOrdenPreparacionCommand(99), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("99");
    }

    [Fact]
    public async Task Handle_OrdenPendiente_AnulaYRetornaSuccess()
    {
        var orden = OrdenPreparacion.Crear(1, null, null, DateOnly.FromDateTime(DateTime.Today), null, null);
        _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(orden);
        _uow.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(new AnularOrdenPreparacionCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repo.Received(1).Update(Arg.Any<OrdenPreparacion>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class ConfirmarOrdenPreparacionCommandHandlerTests
{
    private readonly IRepository<OrdenPreparacion> _repo;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ConfirmarOrdenPreparacionCommandHandler _handler;

    public ConfirmarOrdenPreparacionCommandHandlerTests()
    {
        _repo        = Substitute.For<IRepository<OrdenPreparacion>>();
        _uow         = Substitute.For<IUnitOfWork>();
        _currentUser = Substitute.For<ICurrentUserService>();
        _currentUser.UserId.Returns(1L);
        _handler = new ConfirmarOrdenPreparacionCommandHandler(_repo, _uow, _currentUser);
    }

    [Fact]
    public async Task Handle_OrdenNoEncontrada_RetornaFailure()
    {
        _repo.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns((OrdenPreparacion?)null);

        var result = await _handler.Handle(new ConfirmarOrdenPreparacionCommand(99), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_OrdenPendiente_ConfirmaYRetornaSuccess()
    {
        var orden = OrdenPreparacion.Crear(1, null, null, DateOnly.FromDateTime(DateTime.Today), null, null);
        orden.AgregarDetalle(1, 1, 5m);
        orden.IniciarPreparacion(null);
        _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(orden);
        _uow.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(new ConfirmarOrdenPreparacionCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repo.Received(1).Update(Arg.Any<OrdenPreparacion>());
    }
}

public class GetOrdenPreparacionByIdQueryHandlerTests
{
    private readonly IApplicationDbContext _db;
    private readonly GetOrdenPreparacionByIdQueryHandler _handler;

    public GetOrdenPreparacionByIdQueryHandlerTests()
    {
        _db      = Substitute.For<IApplicationDbContext>();
        _handler = new GetOrdenPreparacionByIdQueryHandler(_db);
    }

    [Fact]
    public async Task Handle_OrdenNoExiste_RetornaFailure()
    {
        var mockSet = MockDbSetHelper.CreateMockDbSet(new List<OrdenPreparacion>());
        _db.OrdenesPreparacion.Returns(mockSet);

        var result = await _handler.Handle(new GetOrdenPreparacionByIdQuery(99), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("99");
    }

    [Fact]
    public async Task Handle_OrdenExiste_RetornaDto()
    {
        var orden = OrdenPreparacion.Crear(1, null, null, DateOnly.FromDateTime(DateTime.Today), null, null);
        var mockSet = MockDbSetHelper.CreateMockDbSet(new List<OrdenPreparacion> { orden });
        _db.OrdenesPreparacion.Returns(mockSet);

        var result = await _handler.Handle(new GetOrdenPreparacionByIdQuery(orden.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }
}

public class GetOrdenesPreparacionPagedQueryHandlerTests
{
    private readonly IApplicationDbContext _db;
    private readonly GetOrdenesPreparacionPagedQueryHandler _handler;

    public GetOrdenesPreparacionPagedQueryHandlerTests()
    {
        _db      = Substitute.For<IApplicationDbContext>();
        _handler = new GetOrdenesPreparacionPagedQueryHandler(_db);

        var mockSet = MockDbSetHelper.CreateMockDbSet(new List<OrdenPreparacion>());
        _db.OrdenesPreparacion.Returns(mockSet);
    }

    [Fact]
    public async Task Handle_SinRegistros_RetornaResultadoVacio()
    {
        var query = new GetOrdenesPreparacionPagedQuery(1, 20, null, null, null, null, null);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
// FINANZAS — COBROS
// ═══════════════════════════════════════════════════════════════════════════════

public class RegistrarCobroCommandHandlerTests
{
    private readonly ICobroRepository _cobroRepo;
    private readonly IComprobanteRepository _comprobanteRepo;
    private readonly IImputacionRepository _imputacionRepo;
    private readonly CuentaCorrienteService _ctaCteService;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly RegistrarCobroCommandHandler _handler;

    public RegistrarCobroCommandHandlerTests()
    {
        _cobroRepo       = Substitute.For<ICobroRepository>();
        _comprobanteRepo = Substitute.For<IComprobanteRepository>();
        _imputacionRepo  = Substitute.For<IImputacionRepository>();
        _ctaCteService   = Substitute.For<CuentaCorrienteService>(
            Substitute.For<ICuentaCorrienteRepository>(),
            Substitute.For<IMovimientoCtaCteRepository>());
        _uow             = Substitute.For<IUnitOfWork>();
        _currentUser     = Substitute.For<ICurrentUserService>();
        _currentUser.UserId.Returns(1L);

        _handler = new RegistrarCobroCommandHandler(
            _cobroRepo, _comprobanteRepo, _imputacionRepo,
            _ctaCteService, _uow, _currentUser);
    }

    [Fact]
    public async Task Handle_SinMedios_RetornaFailure()
    {
        var cmd = new RegistrarCobroCommand(
            SucursalId:           1,
            TerceroId:            1,
            Fecha:                DateOnly.FromDateTime(DateTime.Today),
            MonedaId:             1,
            Cotizacion:           1m,
            Observacion:          null,
            Medios:               new List<MedioCobroInput>(),
            ComprobantesAImputar: new List<ComprobanteAImputarInput>());

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("medio");
    }

    [Fact]
    public async Task Handle_ConMedios_GuardaCobroYRetornaSuccess()
    {
        var medios = new List<MedioCobroInput>
        {
            new MedioCobroInput(CajaId: 1, FormaPagoId: 1, ChequeId: null, Importe: 1000m, MonedaId: 1, Cotizacion: 1m)
        };

        _cobroRepo.AddAsync(Arg.Any<Cobro>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _uow.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var cmd = new RegistrarCobroCommand(
            SucursalId:           1,
            TerceroId:            1,
            Fecha:                DateOnly.FromDateTime(DateTime.Today),
            MonedaId:             1,
            Cotizacion:           1m,
            Observacion:          null,
            Medios:               medios,
            ComprobantesAImputar: new List<ComprobanteAImputarInput>());

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _cobroRepo.Received(1).AddAsync(Arg.Any<Cobro>(), Arg.Any<CancellationToken>());
    }
}

public class AnularCobroFinanzasCommandHandlerTests
{
    private readonly ICobroRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly AnularCobroCommandHandler _handler;

    public AnularCobroFinanzasCommandHandlerTests()
    {
        _repo        = Substitute.For<ICobroRepository>();
        _uow         = Substitute.For<IUnitOfWork>();
        _currentUser = Substitute.For<ICurrentUserService>();
        _currentUser.UserId.Returns(1L);
        _handler = new AnularCobroCommandHandler(_repo, _uow, _currentUser);
    }

    [Fact]
    public async Task Handle_CobroNoEncontrado_RetornaFailure()
    {
        _repo.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns((Cobro?)null);

        var result = await _handler.Handle(new AnularCobroCommand(99), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("99");
    }

    [Fact]
    public async Task Handle_CobroActivo_AnulaYRetornaSuccess()
    {
        var cobro = Cobro.Crear(1, 1, DateOnly.FromDateTime(DateTime.Today), 1, 1m, null, 1L);
        _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(cobro);
        _uow.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(new AnularCobroCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repo.Received(1).Update(Arg.Any<Cobro>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class GetCobrosPagedQueryHandlerTests
{
    private readonly ICobroRepository _repo;
    private readonly IApplicationDbContext _db;
    private readonly GetCobrosPagedQueryHandler _handler;

    public GetCobrosPagedQueryHandlerTests()
    {
        _repo    = Substitute.For<ICobroRepository>();
        _db      = Substitute.For<IApplicationDbContext>();
        _handler = new GetCobrosPagedQueryHandler(_repo, _db);

        _repo.GetPagedAsync(
            Arg.Any<int>(), Arg.Any<int>(),
            Arg.Any<long?>(), Arg.Any<long?>(),
            Arg.Any<DateOnly?>(), Arg.Any<DateOnly?>(),
            Arg.Any<CancellationToken>())
        .Returns(new PagedResult<Cobro>(new List<Cobro>(), 1, 20, 0));

        var mockTerceros18 = MockDbSetHelper.CreateMockDbSet(new List<Tercero>());

        _db.Terceros.Returns(mockTerceros18);
        var mockMonedas19 = MockDbSetHelper.CreateMockDbSet(
            new List<ZuluIA_Back.Domain.Entities.Referencia.Moneda>());
        _db.Monedas.Returns(mockMonedas19);
    }

    [Fact]
    public async Task Handle_SinRegistros_RetornaPageVacio()
    {
        var query = new GetCobrosPagedQuery(1, 20, null, null, null, null);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}

public class GetCobroDetalleQueryHandlerTests
{
    private readonly ICobroRepository _repo;
    private readonly IApplicationDbContext _db;
    private readonly GetCobroDetalleQueryHandler _handler;

    public GetCobroDetalleQueryHandlerTests()
    {
        _repo    = Substitute.For<ICobroRepository>();
        _db      = Substitute.For<IApplicationDbContext>();
        _handler = new GetCobroDetalleQueryHandler(_repo, _db);

        var mockTerceros20 = MockDbSetHelper.CreateMockDbSet(new List<Tercero>());

        _db.Terceros.Returns(mockTerceros20);
        var mockMonedas21 = MockDbSetHelper.CreateMockDbSet(
            new List<ZuluIA_Back.Domain.Entities.Referencia.Moneda>());
        _db.Monedas.Returns(mockMonedas21);
        var mockCajasCuentasBancarias22 = MockDbSetHelper.CreateMockDbSet(
            new List<ZuluIA_Back.Domain.Entities.Finanzas.CajaCuentaBancaria>());
        _db.CajasCuentasBancarias.Returns(mockCajasCuentasBancarias22);
        var mockFormasPago23 = MockDbSetHelper.CreateMockDbSet(
            new List<ZuluIA_Back.Domain.Entities.Referencia.FormaPago>());
        _db.FormasPago.Returns(mockFormasPago23);
    }

    [Fact]
    public async Task Handle_CobroNoExiste_RetornaNull()
    {
        _repo.GetByIdConMediosAsync(99, Arg.Any<CancellationToken>()).Returns((Cobro?)null);

        var result = await _handler.Handle(new GetCobroDetalleQuery(99), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_CobroExiste_RetornaDto()
    {
        var cobro = Cobro.Crear(1, 1, DateOnly.FromDateTime(DateTime.Today), 1, 1m, null, 1L);
        _repo.GetByIdConMediosAsync(cobro.Id, Arg.Any<CancellationToken>()).Returns(cobro);

        var result = await _handler.Handle(new GetCobroDetalleQuery(cobro.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.TerceroId.Should().Be(1);
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
// FINANZAS — PAGOS
// ═══════════════════════════════════════════════════════════════════════════════

public class RegistrarPagoCommandHandlerTests
{
    private readonly IPagoRepository _pagoRepo;
    private readonly IComprobanteRepository _comprobanteRepo;
    private readonly IImputacionRepository _imputacionRepo;
    private readonly CuentaCorrienteService _ctaCteService;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IApplicationDbContext _db;
    private readonly RegistrarPagoCommandHandler _handler;

    public RegistrarPagoCommandHandlerTests()
    {
        _pagoRepo        = Substitute.For<IPagoRepository>();
        _comprobanteRepo = Substitute.For<IComprobanteRepository>();
        _imputacionRepo  = Substitute.For<IImputacionRepository>();
        _ctaCteService   = Substitute.For<CuentaCorrienteService>(
            Substitute.For<ICuentaCorrienteRepository>(),
            Substitute.For<IMovimientoCtaCteRepository>());
        _uow             = Substitute.For<IUnitOfWork>();
        _currentUser     = Substitute.For<ICurrentUserService>();
        _currentUser.UserId.Returns(1L);
        _db              = Substitute.For<IApplicationDbContext>();

        var mockRetenciones24 = MockDbSetHelper.CreateMockDbSet(
            new List<ZuluIA_Back.Domain.Entities.Finanzas.Retencion>());

        _db.Retenciones.Returns(mockRetenciones24);

        _handler = new RegistrarPagoCommandHandler(
            _pagoRepo, _comprobanteRepo, _imputacionRepo,
            _ctaCteService, _uow, _currentUser, _db);
    }

    [Fact]
    public async Task Handle_SinMedios_RetornaFailure()
    {
        var cmd = new RegistrarPagoCommand(
            SucursalId:           1,
            TerceroId:            1,
            Fecha:                DateOnly.FromDateTime(DateTime.Today),
            MonedaId:             1,
            Cotizacion:           1m,
            Observacion:          null,
            Medios:               new List<MedioPagoInput>(),
            Retenciones:          new List<RetencionInput>(),
            ComprobantesAImputar: new List<ComprobanteAImputarInput>());

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("medio");
    }

    [Fact]
    public async Task Handle_ConMedios_GuardaPagoYRetornaSuccess()
    {
        var medios = new List<MedioPagoInput>
        {
            new MedioPagoInput(CajaId: 1, FormaPagoId: 1, ChequeId: null, Importe: 1000m, MonedaId: 1, Cotizacion: 1m)
        };

        _pagoRepo.AddAsync(Arg.Any<Pago>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _uow.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var cmd = new RegistrarPagoCommand(
            SucursalId:           1,
            TerceroId:            1,
            Fecha:                DateOnly.FromDateTime(DateTime.Today),
            MonedaId:             1,
            Cotizacion:           1m,
            Observacion:          null,
            Medios:               medios,
            Retenciones:          new List<RetencionInput>(),
            ComprobantesAImputar: new List<ComprobanteAImputarInput>());

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _pagoRepo.Received(1).AddAsync(Arg.Any<Pago>(), Arg.Any<CancellationToken>());
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
// FINANZAS — CUENTA CORRIENTE
// ═══════════════════════════════════════════════════════════════════════════════

public class GetCuentaCorrienteTerceroQueryHandlerTests
{
    private readonly ICuentaCorrienteRepository _repo;
    private readonly IApplicationDbContext _db;
    private readonly GetCuentaCorrienteTerceroQueryHandler _handler;

    public GetCuentaCorrienteTerceroQueryHandlerTests()
    {
        _repo    = Substitute.For<ICuentaCorrienteRepository>();
        _db      = Substitute.For<IApplicationDbContext>();
        _handler = new GetCuentaCorrienteTerceroQueryHandler(_repo, _db);

        var mockMonedas25 = MockDbSetHelper.CreateMockDbSet(
            new List<ZuluIA_Back.Domain.Entities.Referencia.Moneda>());

        _db.Monedas.Returns(mockMonedas25);
        var mockTerceros26 = MockDbSetHelper.CreateMockDbSet(new List<Tercero>());
        _db.Terceros.Returns(mockTerceros26);
    }

    [Fact]
    public async Task Handle_SinCuentas_RetornaListaVacia()
    {
        _repo.GetByTerceroAsync(1, Arg.Any<CancellationToken>())
             .Returns(new List<CuentaCorriente>());

        var result = await _handler.Handle(
            new GetCuentaCorrienteTerceroQuery(1, null), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ConCuentas_RetornaListaFiltrada()
    {
        var cuentas = new List<CuentaCorriente>
        {
            CuentaCorriente.Crear(1, null, 1),
            CuentaCorriente.Crear(1, 2L, 1)
        };
        _repo.GetByTerceroAsync(1, Arg.Any<CancellationToken>()).Returns(cuentas);

        var result = await _handler.Handle(
            new GetCuentaCorrienteTerceroQuery(1, null), CancellationToken.None);

        result.Should().HaveCount(2);
    }
}

public class GetMovimientosCtaCteQueryHandlerTests
{
    private readonly IMovimientoCtaCteRepository _repo;
    private readonly IApplicationDbContext _db;
    private readonly GetMovimientosCtaCteQueryHandler _handler;

    public GetMovimientosCtaCteQueryHandlerTests()
    {
        _repo    = Substitute.For<IMovimientoCtaCteRepository>();
        _db      = Substitute.For<IApplicationDbContext>();
        _handler = new GetMovimientosCtaCteQueryHandler(_repo, _db);

        _repo.GetPagedAsync(
            Arg.Any<int>(), Arg.Any<int>(),
            Arg.Any<long>(), Arg.Any<long?>(), Arg.Any<long?>(),
            Arg.Any<DateOnly?>(), Arg.Any<DateOnly?>(),
            Arg.Any<CancellationToken>())
        .Returns(new PagedResult<MovimientoCtaCte>(
            new List<MovimientoCtaCte>(), 1, 20, 0));

        var mockMonedas27 = MockDbSetHelper.CreateMockDbSet(
            new List<ZuluIA_Back.Domain.Entities.Referencia.Moneda>());

        _db.Monedas.Returns(mockMonedas27);
        var mockComprobantes28 = MockDbSetHelper.CreateMockDbSet(
            new List<Comprobante>());
        _db.Comprobantes.Returns(mockComprobantes28);
    }

    [Fact]
    public async Task Handle_SinMovimientos_RetornaPageVacio()
    {
        var query = new GetMovimientosCtaCteQuery(1, 20, 1, null, null, null, null);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}
