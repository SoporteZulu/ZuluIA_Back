using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Configuracion.Commands;
using ZuluIA_Back.Application.Features.Configuracion.DTOs;
using ZuluIA_Back.Application.Features.Configuracion.Queries;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class ConfiguracionControllerTests
{
    [Fact]
    public async Task GetParametros_DevuelveOkYMandaQueryCorrecta()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        IReadOnlyList<ConfiguracionDto> data = [new() { Id = 1, Campo = "empresa", Valor = "Zulu", TipoDato = 1, Descripcion = "Nombre" }];
        mediator.Send(Arg.Any<GetConfiguracionQuery>(), Arg.Any<CancellationToken>())
            .Returns(data);
        var controller = CreateController(mediator, db);

        var result = await controller.GetParametros(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(data);
        await mediator.Received(1).Send(Arg.Any<GetConfiguracionQuery>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetParametro_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var command = new SetConfiguracionCommand("", "valor");
        mediator.Send(command, Arg.Any<CancellationToken>())
            .Returns(Result.Failure("El campo es obligatorio."));
        var controller = CreateController(mediator, db);

        var result = await controller.SetParametro(command, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task SetParametro_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var command = new SetConfiguracionCommand("empresa", "Zulu");
        mediator.Send(command, Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.SetParametro(command, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(command, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetMonedas_DevuelveSoloActivasOrdenadas()
    {
        var monedas = MockDbSetHelper.CreateMockDbSet([
            BuildMoneda(1, "USD", "Dolar", "$", false, true),
            BuildMoneda(2, "ARS", "Peso", "$", false, true),
            BuildMoneda(3, "EUR", "Euro", "€", false, false)
        ]);
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(db => db.Monedas.Returns(monedas)));

        var result = await controller.GetMonedas(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ok.Value.Should().BeAssignableTo<System.Collections.IEnumerable>().Subject.Cast<object>().ToList();
        data.Should().HaveCount(2);
        AssertAnonymousProperty(data[0], "Id", 1L);
        AssertAnonymousProperty(data[0], "Codigo", "USD");
        AssertAnonymousProperty(data[0], "Simbolo", "$");
        AssertAnonymousProperty(data[0], "SinDecimales", false);
        AssertAnonymousProperty(data[1], "Id", 2L);
    }

    [Fact]
    public async Task GetCondicionesIva_OrdenaPorCodigo()
    {
        var condiciones = MockDbSetHelper.CreateMockDbSet([
            BuildCondicionIva(2, 5, "Responsable inscripto"),
            BuildCondicionIva(1, 1, "Consumidor final")
        ]);
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(db => db.CondicionesIva.Returns(condiciones)));

        var result = await controller.GetCondicionesIva(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ok.Value.Should().BeAssignableTo<System.Collections.IEnumerable>().Subject.Cast<object>().ToList();
        data.Should().HaveCount(2);
        AssertAnonymousProperty(data[0], "Id", 1L);
        AssertAnonymousProperty(data[0], "Codigo", (short)1);
        AssertAnonymousProperty(data[1], "Codigo", (short)5);
    }

    [Fact]
    public async Task GetTiposDocumento_OrdenaPorCodigo()
    {
        var tiposDocumento = MockDbSetHelper.CreateMockDbSet([
            BuildTipoDocumento(2, 96, "DNI"),
            BuildTipoDocumento(1, 80, "CUIT")
        ]);
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(db => db.TiposDocumento.Returns(tiposDocumento)));

        var result = await controller.GetTiposDocumento(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ok.Value.Should().BeAssignableTo<System.Collections.IEnumerable>().Subject.Cast<object>().ToList();
        data.Should().HaveCount(2);
        AssertAnonymousProperty(data[0], "Codigo", (short)80);
        AssertAnonymousProperty(data[0], "Descripcion", "CUIT");
        AssertAnonymousProperty(data[1], "Codigo", (short)96);
    }

    [Fact]
    public async Task GetTiposComprobante_DevuelveSoloActivosOrdenados()
    {
        var tipos = MockDbSetHelper.CreateMockDbSet([
            BuildTipoComprobante(2, "FCB", "Factura B", true, false, false),
            BuildTipoComprobante(1, "FCA", "Factura A", true, true, true),
            BuildTipoComprobante(3, "NCI", "Nota interna", false, false, false)
        ]);
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(db => db.TiposComprobante.Returns(tipos)));

        var result = await controller.GetTiposComprobante(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ok.Value.Should().BeAssignableTo<System.Collections.IEnumerable>().Subject.Cast<object>().ToList();
        data.Should().HaveCount(2);
        AssertAnonymousProperty(data[0], "Id", 1L);
        AssertAnonymousProperty(data[0], "Codigo", "FCA");
        AssertAnonymousProperty(data[0], "EsVenta", true);
        AssertAnonymousProperty(data[0], "AfectaStock", true);
        AssertAnonymousProperty(data[1], "Codigo", "FCB");
    }

    [Fact]
    public async Task GetAlicuotasIva_OrdenaPorPorcentaje()
    {
        var alicuotas = MockDbSetHelper.CreateMockDbSet([
            BuildAlicuotaIva(2, 21, "General", 21),
            BuildAlicuotaIva(1, 10, "Reducida", 10)
        ]);
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(db => db.AlicuotasIva.Returns(alicuotas)));

        var result = await controller.GetAlicuotasIva(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ok.Value.Should().BeAssignableTo<System.Collections.IEnumerable>().Subject.Cast<object>().ToList();
        data.Should().HaveCount(2);
        AssertAnonymousProperty(data[0], "Porcentaje", 10L);
        AssertAnonymousProperty(data[0], "Codigo", (short)10);
        AssertAnonymousProperty(data[1], "Porcentaje", 21L);
    }

    [Fact]
    public async Task GetUnidadesMedida_OrdenaPorDescripcion()
    {
        var unidades = MockDbSetHelper.CreateMockDbSet([
            BuildUnidadMedida(2, "LT", "Litro"),
            BuildUnidadMedida(1, "KG", "Kilogramo")
        ]);
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(db => db.UnidadesMedida.Returns(unidades)));

        var result = await controller.GetUnidadesMedida(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ok.Value.Should().BeAssignableTo<System.Collections.IEnumerable>().Subject.Cast<object>().ToList();
        AssertAnonymousProperty(data[0], "Descripcion", "Kilogramo");
    }

    [Fact]
    public async Task GetFormasPago_DevuelveSoloActivasOrdenadas()
    {
        var formas = MockDbSetHelper.CreateMockDbSet([
            BuildFormaPago(2, "Transferencia", true),
            BuildFormaPago(1, "Efectivo", true),
            BuildFormaPago(3, "Cheque", false)
        ]);
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(db => db.FormasPago.Returns(formas)));

        var result = await controller.GetFormasPago(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ok.Value.Should().BeAssignableTo<System.Collections.IEnumerable>().Subject.Cast<object>().ToList();
        data.Should().HaveCount(2);
        AssertAnonymousProperty(data[0], "Id", 1L);
        AssertAnonymousProperty(data[0], "Descripcion", "Efectivo");
        AssertAnonymousProperty(data[1], "Descripcion", "Transferencia");
    }

    [Fact]
    public async Task GetCategoriasTerceros_OrdenaPorDescripcion()
    {
        var categorias = MockDbSetHelper.CreateMockDbSet([
            BuildCategoriaTercero(2, "Mayorista", true),
            BuildCategoriaTercero(1, "Minorista", false)
        ]);
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(db => db.CategoriasTerceros.Returns(categorias)));

        var result = await controller.GetCategoriasTerceros(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ok.Value.Should().BeAssignableTo<System.Collections.IEnumerable>().Subject.Cast<object>().ToList();
        AssertAnonymousProperty(data[0], "Descripcion", "Mayorista");
        AssertAnonymousProperty(data[0], "EsImportador", true);
    }

    private static ConfiguracionController CreateController(IMediator mediator, IApplicationDbContext db)
    {
        return new ConfiguracionController(mediator, db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static IApplicationDbContext BuildDb(Action<IApplicationDbContext> configure)
    {
        var db = Substitute.For<IApplicationDbContext>();
        configure(db);
        return db;
    }

    private static Moneda BuildMoneda(long id, string codigo, string descripcion, string simbolo, bool sinDecimales, bool activa)
    {
        var entity = CreateInstance<Moneda>();
        SetProperty(entity, nameof(Moneda.Id), id);
        SetProperty(entity, nameof(Moneda.Codigo), codigo);
        SetProperty(entity, nameof(Moneda.Descripcion), descripcion);
        SetProperty(entity, nameof(Moneda.Simbolo), simbolo);
        SetProperty(entity, nameof(Moneda.SinDecimales), sinDecimales);
        SetProperty(entity, nameof(Moneda.Activa), activa);
        return entity;
    }

    private static CondicionIva BuildCondicionIva(long id, short codigo, string descripcion)
    {
        var entity = CreateInstance<CondicionIva>();
        SetProperty(entity, nameof(CondicionIva.Id), id);
        SetProperty(entity, nameof(CondicionIva.Codigo), codigo);
        SetProperty(entity, nameof(CondicionIva.Descripcion), descripcion);
        return entity;
    }

    private static TipoDocumento BuildTipoDocumento(long id, short codigo, string descripcion)
    {
        var entity = CreateInstance<TipoDocumento>();
        SetProperty(entity, nameof(TipoDocumento.Id), id);
        SetProperty(entity, nameof(TipoDocumento.Codigo), codigo);
        SetProperty(entity, nameof(TipoDocumento.Descripcion), descripcion);
        return entity;
    }

    private static TipoComprobante BuildTipoComprobante(long id, string codigo, string descripcion, bool activo, bool esVenta, bool afectaStock)
    {
        var entity = CreateInstance<TipoComprobante>();
        SetProperty(entity, nameof(TipoComprobante.Id), id);
        SetProperty(entity, nameof(TipoComprobante.Codigo), codigo);
        SetProperty(entity, nameof(TipoComprobante.Descripcion), descripcion);
        SetProperty(entity, nameof(TipoComprobante.Activo), activo);
        SetProperty(entity, nameof(TipoComprobante.EsVenta), esVenta);
        SetProperty(entity, nameof(TipoComprobante.EsCompra), !esVenta);
        SetProperty(entity, nameof(TipoComprobante.EsInterno), false);
        SetProperty(entity, nameof(TipoComprobante.AfectaStock), afectaStock);
        SetProperty(entity, nameof(TipoComprobante.AfectaCuentaCorriente), true);
        SetProperty(entity, nameof(TipoComprobante.GeneraAsiento), true);
        SetProperty(entity, nameof(TipoComprobante.TipoAfip), (short?)1);
        SetProperty(entity, nameof(TipoComprobante.LetraAfip), 'A');
        return entity;
    }

    private static AlicuotaIva BuildAlicuotaIva(long id, short codigo, string descripcion, long porcentaje)
    {
        var entity = CreateInstance<AlicuotaIva>();
        SetProperty(entity, nameof(AlicuotaIva.Id), id);
        SetProperty(entity, nameof(AlicuotaIva.Codigo), codigo);
        SetProperty(entity, nameof(AlicuotaIva.Descripcion), descripcion);
        SetProperty(entity, nameof(AlicuotaIva.Porcentaje), porcentaje);
        return entity;
    }

    private static UnidadMedida BuildUnidadMedida(long id, string codigo, string descripcion)
    {
        var entity = UnidadMedida.Crear(codigo, descripcion);
        SetProperty(entity, nameof(UnidadMedida.Id), id);
        return entity;
    }

    private static FormaPago BuildFormaPago(long id, string descripcion, bool activa)
    {
        var entity = CreateInstance<FormaPago>();
        SetProperty(entity, nameof(FormaPago.Id), id);
        SetProperty(entity, nameof(FormaPago.Descripcion), descripcion);
        SetProperty(entity, nameof(FormaPago.Activa), activa);
        return entity;
    }

    private static CategoriaTercero BuildCategoriaTercero(long id, string descripcion, bool esImportador)
    {
        var entity = CategoriaTercero.Crear(descripcion, esImportador);
        SetProperty(entity, nameof(CategoriaTercero.Id), id);
        return entity;
    }

    private static T CreateInstance<T>() where T : class
    {
        return (T)Activator.CreateInstance(typeof(T), true)!;
    }

    private static void SetProperty(object entity, string propertyName, object? value)
    {
        entity.GetType().GetProperty(propertyName)!.SetValue(entity, value);
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}