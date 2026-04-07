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
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.Domain.Entities.Sucursales;
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
    private readonly IApplicationDbContext _db;
    private readonly IEmpleadoRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly CreateEmpleadoCommandHandler _handler;

    public CreateEmpleadoCommandHandlerTests()
    {
        _db      = Substitute.For<IApplicationDbContext>();
        _repo    = Substitute.For<IEmpleadoRepository>();
        _uow     = Substitute.For<IUnitOfWork>();
        _handler = new CreateEmpleadoCommandHandler(_db, _repo, _uow);

        var terceros = MockDbSetHelper.CreateMockDbSet(new[] { BuildTercero(1) });
        var sucursales = MockDbSetHelper.CreateMockDbSet(new[] { BuildSucursal(1) });
        var monedas = MockDbSetHelper.CreateMockDbSet(new[] { BuildMoneda(1) });
        _db.Terceros.Returns(terceros);
        _db.Sucursales.Returns(sucursales);
        _db.Monedas.Returns(monedas);
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

    private static Tercero BuildTercero(long id)
    {
        var tercero = Tercero.Crear("EMP001", "Empleado", 1, "20123456789", 1, false, false, true, 1, null);
        typeof(Tercero).BaseType!.GetProperty("Id")!.SetValue(tercero, id);
        return tercero;
    }

    private static Sucursal BuildSucursal(long id)
    {
        var sucursal = Sucursal.Crear("Casa Central", "20123456789", 1, 1, 1, true, null);
        typeof(Sucursal).BaseType!.GetProperty("Id")!.SetValue(sucursal, id);
        return sucursal;
    }

    private static Moneda BuildMoneda(long id)
    {
        var moneda = (Moneda)Activator.CreateInstance(typeof(Moneda), nonPublic: true)!;
        typeof(Moneda).GetProperty(nameof(Moneda.Id))!.SetValue(moneda, id);
        return moneda;
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
        var empleado = Empleado.Crear(1, 1, "EMP001", "Analista", null, new DateOnly(2024, 1, 1), 100_000m, 1);
        typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id))!.SetValue(empleado, 1L);

        var moneda = (Moneda)Activator.CreateInstance(typeof(Moneda), nonPublic: true)!;
        typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id))!.SetValue(moneda, 1L);
        _db.LiquidacionesSueldo.Returns(dbSet);
        _db.Empleados.Returns(MockDbSetHelper.CreateMockDbSet([empleado]));
        _db.Monedas.Returns(MockDbSetHelper.CreateMockDbSet([moneda]));
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
        _db.TasasInteres.Returns(MockDbSetHelper.CreateMockDbSet<TasaInteres>());

        var result = await Sut().Handle(new DesactivarTasaInteresCommand(99, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("no encontrada");
    }

    [Fact]
    public async Task Handle_TasaExistente_LaDesactivaYPersiste()
    {
        var tasa = TasaInteres.Crear("Mora", 5m, new DateOnly(2026, 4, 1), null, null);
        typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id))!.SetValue(tasa, 1L);
        _db.TasasInteres.Returns(MockDbSetHelper.CreateMockDbSet([tasa]));

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
        _db.TasasInteres.Returns(MockDbSetHelper.CreateMockDbSet<TasaInteres>());

        var result = await Sut().Handle(new ActivarTasaInteresCommand(99, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("no encontrada");
    }

    [Fact]
    public async Task Handle_TasaExistente_LaActivaYPersiste()
    {
        var tasa = TasaInteres.Crear("Mora", 5m, new DateOnly(2026, 4, 1), null, null);
        typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id))!.SetValue(tasa, 1L);
        tasa.Desactivar(userId: null);
        _db.TasasInteres.Returns(MockDbSetHelper.CreateMockDbSet([tasa]));

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
