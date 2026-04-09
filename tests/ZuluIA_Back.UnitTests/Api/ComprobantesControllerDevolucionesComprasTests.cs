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
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Entities.Usuarios;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.ValueObjects;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class ComprobantesControllerDevolucionesComprasTests
{
    [Fact]
    public async Task GetDevolucionesCompras_CuandoHayDevolucionStock_DevuelveReferenciasVisibles()
    {
        var origen = CrearComprobante(40, 1, 99, false, false, null, null, 5, EstadoComprobante.Emitido);
        var remito = CrearComprobante(41, 2, 99, false, false, 40, null, 5, EstadoComprobante.Emitido);
        var devolucion = CrearComprobante(42, 3, 99, true, false, 40, MotivoDevolucion.ErrorEntrega, 5, EstadoComprobante.Emitido);
        var controller = CreateController(BuildDb(
            [origen, remito, devolucion],
            [
                CrearTipoComprobante(1, "Factura compra", true),
                CrearTipoComprobante(2, "Remito compra", true),
                CrearTipoComprobante(3, "Devolución compra", true)
            ],
            [CrearTercero(99, "Proveedor Uno")],
            [CrearUsuario(5, "Lucía Márquez")],
            [CrearOrden(12, 40)],
            [CrearComprobanteItem(1, 42, 1000, "Insumo devuelto", 3m)],
            [CrearItem(1000, "INS-001")]));

        var result = await controller.GetDevolucionesCompras(null, null, null, null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var row = ((IEnumerable)ok.Value!).Cast<object>().Single();
        AssertAnonymousProperty(row, "Tipo", "Stock");
        AssertAnonymousProperty(row, "Estado", "PROCESADA");
        AssertAnonymousProperty(row, "OrdenCompraReferencia", "OC-12");
        AssertAnonymousProperty(row, "RemitoReferencia", "0001-00000041");
        AssertAnonymousProperty(row, "RequiereNotaCredito", false);
    }

    [Fact]
    public async Task GetDevolucionesCompras_CuandoHayDevolucionEconomica_DevuelvePendienteYRequiereNc()
    {
        var origen = CrearComprobante(50, 1, 99, false, false, null, null, 7, EstadoComprobante.Emitido);
        var devolucion = CrearComprobante(51, 4, 99, false, true, 50, MotivoDevolucion.DiferenciaPrecio, 7, EstadoComprobante.Borrador);
        var controller = CreateController(BuildDb(
            [origen, devolucion],
            [
                CrearTipoComprobante(1, "Factura compra", true),
                CrearTipoComprobante(4, "Devolución económica compra", true)
            ],
            [CrearTercero(99, "Proveedor Dos")],
            [CrearUsuario(7, "Paula Gómez")],
            [CrearOrden(18, 50)],
            [CrearComprobanteItem(2, 51, 2000, "Servicio no ejecutado", 1m)],
            [CrearItem(2000, "SER-001")]));

        var result = await controller.GetDevolucionesCompras(null, null, "PENDIENTE", null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var row = ((IEnumerable)ok.Value!).Cast<object>().Single();
        AssertAnonymousProperty(row, "Tipo", "Económica");
        AssertAnonymousProperty(row, "Estado", "ABIERTA");
        AssertAnonymousProperty(row, "RequiereNotaCredito", true);
        AssertAnonymousProperty(row, "Responsable", "Paula Gómez");
    }

    [Fact]
    public async Task GetDevolucionesCompras_CuandoFiltraTipoSinAcento_AceptaAlias()
    {
        var origen = CrearComprobante(70, 1, 99, false, false, null, null, 7, EstadoComprobante.Emitido);
        var devolucion = CrearComprobante(71, 4, 99, false, true, 70, MotivoDevolucion.DiferenciaPrecio, 7, EstadoComprobante.Borrador);
        var controller = CreateController(BuildDb(
            [origen, devolucion],
            [
                CrearTipoComprobante(1, "Factura compra", true),
                CrearTipoComprobante(4, "Devolución económica compra", true)
            ],
            [CrearTercero(99, "Proveedor Dos")],
            [CrearUsuario(7, "Paula Gómez")],
            [CrearOrden(20, 70)],
            [CrearComprobanteItem(4, 71, 2000, "Servicio no ejecutado", 1m)],
            [CrearItem(2000, "SER-001")]));

        var result = await controller.GetDevolucionesCompras(null, null, null, "Economica", CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ((IEnumerable)ok.Value!).Cast<object>().Should().ContainSingle();
    }

    [Fact]
    public async Task GetDevolucionesCompras_CuandoHayAjusteEconomicoSinMetadatosDeDevolucion_NoLoIncluye()
    {
        var origen = CrearComprobante(60, 1, 99, false, false, null, null, 7, EstadoComprobante.Emitido);
        var ajuste = CrearComprobante(61, 5, 99, false, true, 60, null, 7, EstadoComprobante.Emitido);
        SetProperty(ajuste, nameof(Comprobante.ObservacionDevolucion), null);
        SetProperty(ajuste, nameof(Comprobante.Observacion), "Ajuste económico sin devolución");
        var controller = CreateController(BuildDb(
            [origen, ajuste],
            [
                CrearTipoComprobante(1, "Factura compra", true),
                CrearTipoComprobante(5, "Ajuste compra", true)
            ],
            [CrearTercero(99, "Proveedor Dos")],
            [CrearUsuario(7, "Paula Gómez")],
            [CrearOrden(18, 60)],
            [],
            []));

        var result = await controller.GetDevolucionesCompras(null, null, null, null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ((IEnumerable)ok.Value!).Cast<object>().Should().BeEmpty();
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
        IEnumerable<ComprobanteItem> items,
        IEnumerable<Item> catalogo)
    {
        var db = Substitute.For<IApplicationDbContext>();
        db.Comprobantes.Returns(MockDbSetHelper.CreateMockDbSet(comprobantes));
        db.TiposComprobante.Returns(MockDbSetHelper.CreateMockDbSet(tipos));
        db.Terceros.Returns(MockDbSetHelper.CreateMockDbSet(terceros));
        db.Usuarios.Returns(MockDbSetHelper.CreateMockDbSet(usuarios));
        db.OrdenesCompraMeta.Returns(MockDbSetHelper.CreateMockDbSet(ordenes));
        db.ComprobantesItems.Returns(MockDbSetHelper.CreateMockDbSet(items));
        db.Items.Returns(MockDbSetHelper.CreateMockDbSet(catalogo));
        return db;
    }

    private static Comprobante CrearComprobante(long id, long tipoId, long terceroId, bool reingresaStock, bool acreditaCuentaCorriente, long? origenId, MotivoDevolucion? motivo, long createdBy, EstadoComprobante estado)
    {
        var comprobante = (Comprobante)FormatterServices.GetUninitializedObject(typeof(Comprobante));
        SetProperty(comprobante, nameof(Comprobante.Id), id);
        SetProperty(comprobante, nameof(Comprobante.SucursalId), 1L);
        SetProperty(comprobante, nameof(Comprobante.TipoComprobanteId), tipoId);
        SetProperty(comprobante, nameof(Comprobante.TerceroId), terceroId);
        SetProperty(comprobante, nameof(Comprobante.MonedaId), 1L);
        SetProperty(comprobante, nameof(Comprobante.Numero), new NroComprobante(1, id));
        SetProperty(comprobante, nameof(Comprobante.Fecha), new DateOnly(2026, 4, 20));
        SetProperty(comprobante, nameof(Comprobante.Total), 1000m);
        SetProperty(comprobante, nameof(Comprobante.Saldo), estado == EstadoComprobante.Borrador ? 1000m : 0m);
        SetProperty(comprobante, nameof(Comprobante.Estado), estado);
        SetProperty(comprobante, nameof(Comprobante.ReingresaStock), reingresaStock);
        SetProperty(comprobante, nameof(Comprobante.AcreditaCuentaCorriente), acreditaCuentaCorriente);
        SetProperty(comprobante, nameof(Comprobante.ComprobanteOrigenId), origenId);
        SetProperty(comprobante, nameof(Comprobante.MotivoDevolucion), motivo);
        SetProperty(comprobante, nameof(Comprobante.CreatedBy), createdBy);
        SetProperty(comprobante, nameof(Comprobante.Observacion), "Observación devolución");
        SetProperty(comprobante, nameof(Comprobante.ObservacionDevolucion), motivo.HasValue || reingresaStock || acreditaCuentaCorriente ? "Resolución devolución" : null);
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

    private static ComprobanteItem CrearComprobanteItem(long id, long comprobanteId, long itemId, string descripcion, decimal cantidad)
    {
        var item = (ComprobanteItem)FormatterServices.GetUninitializedObject(typeof(ComprobanteItem));
        SetProperty(item, nameof(ComprobanteItem.Id), id);
        SetProperty(item, nameof(ComprobanteItem.ComprobanteId), comprobanteId);
        SetProperty(item, nameof(ComprobanteItem.ItemId), itemId);
        SetProperty(item, nameof(ComprobanteItem.Descripcion), descripcion);
        SetProperty(item, nameof(ComprobanteItem.Cantidad), cantidad);
        SetProperty(item, nameof(ComprobanteItem.Orden), (short)0);
        return item;
    }

    private static Item CrearItem(long id, string codigo)
    {
        var item = (Item)FormatterServices.GetUninitializedObject(typeof(Item));
        SetProperty(item, nameof(Item.Id), id);
        SetProperty(item, nameof(Item.Codigo), codigo);
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
