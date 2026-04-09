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

public class ComprobantesControllerRemitosComprasTests
{
    [Fact]
    public async Task GetRemitosCompras_CuandoHayDiferencia_DevuelvePendienteConOrdenYItemsComparados()
    {
        var origen = CrearComprobante(50, 1, 99, false, 0m, null, null, 5);
        var remito = CrearComprobante(60, 2, 99, false, 0m, 50, 300, 5);
        var controller = CreateController(BuildDb(
            [origen, remito],
            [
                CrearTipoComprobante(1, "Orden compra", true),
                CrearTipoComprobante(2, "Remito compra", true)
            ],
            [CrearTercero(99, "Proveedor Uno"), CrearTercero(300, "Transporte Norte")],
            [CrearUsuario(5, "Sofía Quiroga")],
            [CrearOrden(9, 50, EstadoOrdenCompra.Pendiente)],
            [
                CrearComprobanteItem(500, 50, 1000, "Plancha acero", 120m, 1, null, 0),
                CrearComprobanteItem(600, 60, 1000, "Plancha acero", 96m, 1, 20, 0)
            ],
            [CrearItem(1000, "MAT-ACR-12", 1)],
            [CrearUnidad(1, "unid")],
            [CrearDeposito(20, "Central")]));

        var result = await controller.GetRemitosCompras(null, null, null, null, null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var rows = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        rows.Should().ContainSingle();
        AssertAnonymousProperty(rows[0], "Tipo", "No valorizado");
        AssertAnonymousProperty(rows[0], "Estado", "PENDIENTE");
        AssertAnonymousProperty(rows[0], "OrdenCompraReferencia", "OC-9");
        AssertAnonymousProperty(rows[0], "Transportista", "Transporte Norte");
    }

    [Fact]
    public async Task GetRemitosCompras_CuandoOrdenYaFueRecibida_DevuelveRecibidoYValorizado()
    {
        var origen = CrearComprobante(70, 1, 99, true, 250m, null, null, 5);
        var remito = CrearComprobante(80, 2, 99, true, 250m, 70, null, 5);
        var controller = CreateController(BuildDb(
            [origen, remito],
            [
                CrearTipoComprobante(1, "Orden compra", true),
                CrearTipoComprobante(2, "Remito compra", true)
            ],
            [CrearTercero(99, "Proveedor Uno")],
            [CrearUsuario(5, "Mariano Cid")],
            [CrearOrden(10, 70, EstadoOrdenCompra.Recibida)],
            [
                CrearComprobanteItem(700, 70, 2000, "Servicio montaje", 1m, 2, null, 0),
                CrearComprobanteItem(800, 80, 2000, "Servicio montaje", 1m, 2, 21, 0)
            ],
            [CrearItem(2000, "SER-MONT", 2)],
            [CrearUnidad(2, "srv")],
            [CrearDeposito(21, "Obra Delta")]));

        var result = await controller.GetRemitosCompras(null, null, null, null, true, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var row = ((IEnumerable)ok.Value!).Cast<object>().Single();
        AssertAnonymousProperty(row, "Tipo", "Valorizado");
        AssertAnonymousProperty(row, "Estado", "RECIBIDO");
        AssertAnonymousProperty(row, "Deposito", "Obra Delta");
        AssertAnonymousProperty(row, "ResponsableRecepcion", "Mariano Cid");
    }

    [Fact]
    public async Task GetRemitosCompras_CuandoFiltraTipoYEstado_AceptaAlias()
    {
        var origen = CrearComprobante(90, 1, 99, true, 250m, null, null, 5);
        var remito = CrearComprobante(91, 2, 99, true, 250m, 90, null, 5);
        var controller = CreateController(BuildDb(
            [origen, remito],
            [
                CrearTipoComprobante(1, "Orden compra", true),
                CrearTipoComprobante(2, "Remito compra", true)
            ],
            [CrearTercero(99, "Proveedor Uno")],
            [CrearUsuario(5, "Mariano Cid")],
            [CrearOrden(11, 90, EstadoOrdenCompra.Recibida)],
            [
                CrearComprobanteItem(900, 90, 2000, "Servicio montaje", 1m, 2, null, 0),
                CrearComprobanteItem(901, 91, 2000, "Servicio montaje", 1m, 2, 21, 0)
            ],
            [CrearItem(2000, "SER-MONT", 2)],
            [CrearUnidad(2, "srv")],
            [CrearDeposito(21, "Obra Delta")]));

        var result = await controller.GetRemitosCompras(null, null, "RECIBIDA", "Valorizada", true, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ((IEnumerable)ok.Value!).Cast<object>().Should().ContainSingle();
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
        IEnumerable<ComprobanteItem> comprobantesItems,
        IEnumerable<Item> items,
        IEnumerable<UnidadMedida> unidades,
        IEnumerable<Deposito> depositos)
    {
        var db = Substitute.For<IApplicationDbContext>();
        db.Comprobantes.Returns(MockDbSetHelper.CreateMockDbSet(comprobantes));
        db.TiposComprobante.Returns(MockDbSetHelper.CreateMockDbSet(tipos));
        db.Terceros.Returns(MockDbSetHelper.CreateMockDbSet(terceros));
        db.Usuarios.Returns(MockDbSetHelper.CreateMockDbSet(usuarios));
        db.OrdenesCompraMeta.Returns(MockDbSetHelper.CreateMockDbSet(ordenes));
        db.ComprobantesItems.Returns(MockDbSetHelper.CreateMockDbSet(comprobantesItems));
        db.Items.Returns(MockDbSetHelper.CreateMockDbSet(items));
        db.UnidadesMedida.Returns(MockDbSetHelper.CreateMockDbSet(unidades));
        db.Depositos.Returns(MockDbSetHelper.CreateMockDbSet(depositos));
        return db;
    }

    private static Comprobante CrearComprobante(long id, long tipoId, long terceroId, bool esValorizado, decimal total, long? origenId, long? transporteId, long createdBy)
    {
        var comprobante = (Comprobante)FormatterServices.GetUninitializedObject(typeof(Comprobante));
        SetProperty(comprobante, nameof(Comprobante.Id), id);
        SetProperty(comprobante, nameof(Comprobante.SucursalId), 1L);
        SetProperty(comprobante, nameof(Comprobante.TipoComprobanteId), tipoId);
        SetProperty(comprobante, nameof(Comprobante.TerceroId), terceroId);
        SetProperty(comprobante, nameof(Comprobante.MonedaId), 1L);
        SetProperty(comprobante, nameof(Comprobante.Numero), new NroComprobante(1, id));
        SetProperty(comprobante, nameof(Comprobante.Fecha), new DateOnly(2026, 4, 10));
        SetProperty(comprobante, nameof(Comprobante.Total), total);
        SetProperty(comprobante, nameof(Comprobante.Saldo), total);
        SetProperty(comprobante, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        SetProperty(comprobante, nameof(Comprobante.EsValorizado), esValorizado);
        SetProperty(comprobante, nameof(Comprobante.ComprobanteOrigenId), origenId);
        SetProperty(comprobante, nameof(Comprobante.TransporteId), transporteId);
        SetProperty(comprobante, nameof(Comprobante.CreatedBy), createdBy);
        SetProperty(comprobante, nameof(Comprobante.NombreQuienRecibe), (string?)null);
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
        SetProperty(tipo, nameof(TipoComprobante.AfectaCuentaCorriente), true);
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

    private static OrdenCompraMeta CrearOrden(long id, long comprobanteId, EstadoOrdenCompra estado)
    {
        var orden = OrdenCompraMeta.Crear(comprobanteId, 99, new DateOnly(2026, 4, 20), "Entrega", 1m);
        SetProperty(orden, nameof(OrdenCompraMeta.Id), id);
        SetProperty(orden, nameof(OrdenCompraMeta.EstadoOc), estado);
        return orden;
    }

    private static ComprobanteItem CrearComprobanteItem(long id, long comprobanteId, long itemId, string descripcion, decimal cantidad, long unidadId, long? depositoId, short orden)
    {
        var item = (ComprobanteItem)FormatterServices.GetUninitializedObject(typeof(ComprobanteItem));
        SetProperty(item, nameof(ComprobanteItem.Id), id);
        SetProperty(item, nameof(ComprobanteItem.ComprobanteId), comprobanteId);
        SetProperty(item, nameof(ComprobanteItem.ItemId), itemId);
        SetProperty(item, nameof(ComprobanteItem.Descripcion), descripcion);
        SetProperty(item, nameof(ComprobanteItem.Cantidad), cantidad);
        SetProperty(item, nameof(ComprobanteItem.UnidadMedidaId), unidadId);
        SetProperty(item, nameof(ComprobanteItem.DepositoId), depositoId);
        SetProperty(item, nameof(ComprobanteItem.Orden), orden);
        return item;
    }

    private static Item CrearItem(long id, string codigo, long unidadId)
    {
        var item = (Item)FormatterServices.GetUninitializedObject(typeof(Item));
        SetProperty(item, nameof(Item.Id), id);
        SetProperty(item, nameof(Item.Codigo), codigo);
        SetProperty(item, nameof(Item.UnidadMedidaId), unidadId);
        return item;
    }

    private static UnidadMedida CrearUnidad(long id, string disminutivo)
    {
        var unidad = (UnidadMedida)FormatterServices.GetUninitializedObject(typeof(UnidadMedida));
        SetProperty(unidad, nameof(UnidadMedida.Id), id);
        SetProperty(unidad, nameof(UnidadMedida.Descripcion), disminutivo);
        SetProperty(unidad, nameof(UnidadMedida.Disminutivo), disminutivo);
        return unidad;
    }

    private static Deposito CrearDeposito(long id, string descripcion)
    {
        var deposito = (Deposito)FormatterServices.GetUninitializedObject(typeof(Deposito));
        SetProperty(deposito, nameof(Deposito.Id), id);
        SetProperty(deposito, nameof(Deposito.Descripcion), descripcion);
        return deposito;
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
