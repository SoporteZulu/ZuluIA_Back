using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Collections;
using System.Runtime.Serialization;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Entities.Usuarios;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.ValueObjects;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class ComprobantesControllerAjustesComprasTests
{
    [Fact]
    public async Task GetNotasCreditoCompras_CuandoHayNotaAplicada_DevuelveVistaEsperada()
    {
        var nota = CrearComprobante(101, 2, 99, 0m, true, MotivoDevolucion.DiferenciaPrecio, 40, 5, "Bonificación por calidad", "Observación NC");
        var origen = CrearComprobante(40, 1, 99, 42000m, false, null, null, 5, null, null);
        var imputacion = Imputacion.Crear(101, 40, 42000m, new DateOnly(2026, 4, 20), 1);
        SetProperty(imputacion, nameof(Imputacion.Id), 9001L);
        var controller = CreateController(BuildDb(
            [nota, origen],
            [CrearTipoComprobante(1, "Factura compra", true), CrearTipoComprobante(2, "Nota crédito compra", true)],
            [CrearTercero(99, "Proveedor Uno")],
            [CrearUsuario(5, "Lucía Márquez")],
            [CrearOrden(12, 40)],
            [imputacion],
            [CrearComprobanteItem(1, 101, "Bonificación por mercadería observada", 42000m)]));

        var result = await controller.GetNotasCreditoCompras(null, null, null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var row = ((IEnumerable)ok.Value!).Cast<object>().Single();
        AssertAnonymousProperty(row, "Proveedor", "Proveedor Uno");
        AssertAnonymousProperty(row, "Estado", "APLICADA");
        AssertAnonymousProperty(row, "ComprobanteReferencia", "0001-00000040");
        AssertAnonymousProperty(row, "OrdenCompraReferencia", "OC-12");
    }

    [Fact]
    public async Task GetNotasCreditoCompras_CuandoFiltraAplicado_AceptaAliasMasculino()
    {
        var nota = CrearComprobante(111, 2, 99, 0m, true, MotivoDevolucion.DiferenciaPrecio, 41, 5, "Bonificación por calidad", "Observación NC");
        var origen = CrearComprobante(41, 1, 99, 42000m, false, null, null, 5, null, null);
        var imputacion = Imputacion.Crear(111, 41, 42000m, new DateOnly(2026, 4, 20), 1);
        SetProperty(imputacion, nameof(Imputacion.Id), 9002L);
        var controller = CreateController(BuildDb(
            [nota, origen],
            [CrearTipoComprobante(1, "Factura compra", true), CrearTipoComprobante(2, "Nota crédito compra", true)],
            [CrearTercero(99, "Proveedor Uno")],
            [CrearUsuario(5, "Lucía Márquez")],
            [CrearOrden(13, 41)],
            [imputacion],
            [CrearComprobanteItem(10, 111, "Bonificación aplicada", 42000m)]));

        var result = await controller.GetNotasCreditoCompras(null, null, "APLICADO", CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ((IEnumerable)ok.Value!).Cast<object>().Should().ContainSingle();
    }

    [Fact]
    public async Task GetAjustesCompras_CuandoHayAjusteDebito_DevuelveTipoYCircuito()
    {
        var ajuste = CrearComprobante(201, 3, 99, 27500m, false, null, 41, 7, null, "Ajuste comercial");
        var origen = CrearComprobante(41, 1, 99, 27500m, false, null, null, 7, null, null);
        var controller = CreateController(BuildDb(
            [ajuste, origen],
            [CrearTipoComprobante(1, "Factura compra", true), CrearTipoComprobante(3, "Ajuste compra", true)],
            [CrearTercero(99, "Proveedor Dos")],
            [CrearUsuario(7, "Paula Gómez")],
            [CrearOrden(18, 41)],
            [],
            [CrearComprobanteItem(2, 201, "Diferencia precio filtros de aire", 27500m)]));

        var result = await controller.GetAjustesCompras(null, null, "Débito", CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var row = ((IEnumerable)ok.Value!).Cast<object>().Single();
        AssertAnonymousProperty(row, "Tipo", "Débito");
        AssertAnonymousProperty(row, "Proveedor", "Proveedor Dos");
        AssertAnonymousProperty(row, "OrdenCompraReferencia", "OC-18");
        AssertAnonymousProperty(row, "RequiereNotaCredito", false);
    }

    [Fact]
    public async Task GetAjustesCompras_CuandoFiltraDebito_SinAcento_AceptaAlias()
    {
        var ajuste = CrearComprobante(211, 3, 99, 27500m, false, null, 42, 7, null, "Ajuste comercial");
        var origen = CrearComprobante(42, 1, 99, 27500m, false, null, null, 7, null, null);
        var controller = CreateController(BuildDb(
            [ajuste, origen],
            [CrearTipoComprobante(1, "Factura compra", true), CrearTipoComprobante(3, "Ajuste compra", true)],
            [CrearTercero(99, "Proveedor Dos")],
            [CrearUsuario(7, "Paula Gómez")],
            [CrearOrden(19, 42)],
            [],
            [CrearComprobanteItem(20, 211, "Diferencia precio filtros", 27500m)]));

        var result = await controller.GetAjustesCompras(null, null, "Debito", CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ((IEnumerable)ok.Value!).Cast<object>().Should().ContainSingle();
    }

    [Fact]
    public async Task GetAjustesCompras_CuandoHayRemitoCompra_NoLoIncluyeEnVistaEconomica()
    {
        var ajuste = CrearComprobante(301, 3, 99, 12000m, false, null, 51, 7, null, "Ajuste comercial");
        var remito = CrearComprobante(302, 4, 99, 0m, false, null, 51, 7, null, "Remito operativo");
        var origen = CrearComprobante(51, 1, 99, 12000m, false, null, null, 7, null, null);
        var controller = CreateController(BuildDb(
            [ajuste, remito, origen],
            [
                CrearTipoComprobante(1, "Factura compra", true),
                CrearTipoComprobante(3, "Ajuste compra", true),
                CrearTipoComprobante(4, "Remito compra", true)
            ],
            [CrearTercero(99, "Proveedor Dos")],
            [CrearUsuario(7, "Paula Gómez")],
            [CrearOrden(18, 51)],
            [],
            [CrearComprobanteItem(3, 301, "Diferencia precio filtros de aire", 12000m)]));

        var result = await controller.GetAjustesCompras(null, null, null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var rows = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        rows.Should().ContainSingle();
        AssertAnonymousProperty(rows[0], "Id", 301L);
    }

    private static ComprobantesController CreateController(IApplicationDbContext db)
    {
        var controller = new ComprobantesController(Substitute.For<IMediator>(), db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }

    private static IApplicationDbContext BuildDb(
        IEnumerable<Comprobante> comprobantes,
        IEnumerable<TipoComprobante> tipos,
        IEnumerable<Tercero> terceros,
        IEnumerable<Usuario> usuarios,
        IEnumerable<OrdenCompraMeta> ordenes,
        IEnumerable<Imputacion> imputaciones,
        IEnumerable<ComprobanteItem> items)
    {
        var db = Substitute.For<IApplicationDbContext>();
        db.Comprobantes.Returns(MockDbSetHelper.CreateMockDbSet(comprobantes));
        db.TiposComprobante.Returns(MockDbSetHelper.CreateMockDbSet(tipos));
        db.Terceros.Returns(MockDbSetHelper.CreateMockDbSet(terceros));
        db.Usuarios.Returns(MockDbSetHelper.CreateMockDbSet(usuarios));
        db.OrdenesCompraMeta.Returns(MockDbSetHelper.CreateMockDbSet(ordenes));
        db.Imputaciones.Returns(MockDbSetHelper.CreateMockDbSet(imputaciones));
        db.ComprobantesItems.Returns(MockDbSetHelper.CreateMockDbSet(items));
        return db;
    }

    private static Comprobante CrearComprobante(long id, long tipoId, long terceroId, decimal saldo, bool acreditaCc, MotivoDevolucion? motivo, long? origenId, long createdBy, string? observacionDevolucion, string? observacion)
    {
        var comprobante = (Comprobante)FormatterServices.GetUninitializedObject(typeof(Comprobante));
        SetProperty(comprobante, nameof(Comprobante.Id), id);
        SetProperty(comprobante, nameof(Comprobante.SucursalId), 1L);
        SetProperty(comprobante, nameof(Comprobante.TipoComprobanteId), tipoId);
        SetProperty(comprobante, nameof(Comprobante.TerceroId), terceroId);
        SetProperty(comprobante, nameof(Comprobante.MonedaId), 1L);
        SetProperty(comprobante, nameof(Comprobante.Numero), new NroComprobante(1, id));
        SetProperty(comprobante, nameof(Comprobante.Fecha), new DateOnly(2026, 4, 20));
        SetProperty(comprobante, nameof(Comprobante.Total), 42000m);
        SetProperty(comprobante, nameof(Comprobante.Saldo), saldo);
        SetProperty(comprobante, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        SetProperty(comprobante, nameof(Comprobante.AcreditaCuentaCorriente), acreditaCc);
        SetProperty(comprobante, nameof(Comprobante.MotivoDevolucion), motivo);
        SetProperty(comprobante, nameof(Comprobante.ComprobanteOrigenId), origenId);
        SetProperty(comprobante, nameof(Comprobante.CreatedBy), createdBy);
        SetProperty(comprobante, nameof(Comprobante.ObservacionDevolucion), observacionDevolucion);
        SetProperty(comprobante, nameof(Comprobante.Observacion), observacion);
        return comprobante;
    }

    private static TipoComprobante CrearTipoComprobante(long id, string descripcion, bool esCompra)
    {
        var tipo = (TipoComprobante)FormatterServices.GetUninitializedObject(typeof(TipoComprobante));
        SetProperty(tipo, nameof(TipoComprobante.Id), id);
        SetProperty(tipo, nameof(TipoComprobante.Descripcion), descripcion);
        SetProperty(tipo, nameof(TipoComprobante.Codigo), descripcion.ToUpperInvariant().Replace(' ', '_'));
        SetProperty(tipo, nameof(TipoComprobante.Activo), true);
        SetProperty(tipo, nameof(TipoComprobante.EsCompra), esCompra);
        SetProperty(tipo, nameof(TipoComprobante.EsVenta), !esCompra);
        return tipo;
    }

    private static Tercero CrearTercero(long id, string razonSocial)
    {
        var tercero = (Tercero)FormatterServices.GetUninitializedObject(typeof(Tercero));
        SetProperty(tercero, nameof(Tercero.Id), id);
        SetProperty(tercero, nameof(Tercero.RazonSocial), razonSocial);
        return tercero;
    }

    private static Usuario CrearUsuario(long id, string nombre)
    {
        var usuario = (Usuario)FormatterServices.GetUninitializedObject(typeof(Usuario));
        SetProperty(usuario, nameof(Usuario.Id), id);
        SetProperty(usuario, nameof(Usuario.UserName), nombre.ToLowerInvariant().Replace(' ', '.'));
        SetProperty(usuario, nameof(Usuario.NombreCompleto), nombre);
        return usuario;
    }

    private static OrdenCompraMeta CrearOrden(long id, long comprobanteId)
    {
        var orden = OrdenCompraMeta.Crear(comprobanteId, 99, new DateOnly(2026, 4, 25), "Entrega", 1m);
        SetProperty(orden, nameof(OrdenCompraMeta.Id), id);
        return orden;
    }

    private static ComprobanteItem CrearComprobanteItem(long id, long comprobanteId, string descripcion, decimal totalLinea)
    {
        var item = (ComprobanteItem)FormatterServices.GetUninitializedObject(typeof(ComprobanteItem));
        SetProperty(item, nameof(ComprobanteItem.Id), id);
        SetProperty(item, nameof(ComprobanteItem.ComprobanteId), comprobanteId);
        SetProperty(item, nameof(ComprobanteItem.Descripcion), descripcion);
        SetProperty(item, nameof(ComprobanteItem.TotalLinea), totalLinea);
        SetProperty(item, nameof(ComprobanteItem.Orden), (short)0);
        return item;
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
