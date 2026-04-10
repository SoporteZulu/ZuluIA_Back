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
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.ValueObjects;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class ImputacionesControllerComprasTests
{
    [Fact]
    public async Task GetCompras_CuandoHayCompraPendiente_DevuelveResumenConReferencias()
    {
        var controller = CreateController(BuildDb(
            [CrearComprobante(10, 1, 99, 7, 1200m, 1200m, "Compra pendiente", null)],
            [CrearTipoComprobante(1, "Factura compra", true, true)],
            [CrearTercero(99, "Proveedor Uno")],
            [CrearMoneda(7, "ARS")],
            [CrearUsuario(5, "Usuario Compras")],
            [CrearOrden(55, 10, 99)],
            [CrearComprobante(11, 2, 99, 7, 1200m, 0m, "Recepción", 10)],
            []));

        var result = await controller.GetCompras(null, null, null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var rows = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        rows.Should().ContainSingle();
        AssertAnonymousProperty(rows[0], "Estado", "PENDIENTE");
        AssertAnonymousProperty(rows[0], "Proveedor", "Proveedor Uno");
        AssertAnonymousProperty(rows[0], "OrdenCompraReferencia", "OC-55");
        AssertAnonymousProperty(rows[0], "RecepcionReferencia", "0001-00000011");
    }

    [Fact]
    public async Task GetCompras_CuandoHayImputacionActiva_DevuelveEstadoObservadaYDistribucion()
    {
        var imputacion = Imputacion.Crear(20, 10, 300m, new DateOnly(2026, 4, 15), 1);
        SetProperty(imputacion, nameof(Imputacion.Id), 77L);

        var controller = CreateController(BuildDb(
            [
                CrearComprobante(10, 1, 99, 7, 1000m, 700m, "Compra observada", null),
                CrearComprobante(20, 3, 150, 7, 300m, 0m, "Pago", null)
            ],
            [
                CrearTipoComprobante(1, "Factura compra", true, true),
                CrearTipoComprobante(3, "Pago proveedor", false, false)
            ],
            [
                CrearTercero(99, "Proveedor Uno"),
                CrearTercero(150, "Tesorería")
            ],
            [CrearMoneda(7, "ARS")],
            [CrearUsuario(5, "Usuario Compras")],
            [],
            [],
            [imputacion]));

        var result = await controller.GetCompras(null, null, null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var row = ((IEnumerable)ok.Value!).Cast<object>().Single();
        AssertAnonymousProperty(row, "Estado", "OBSERVADA");
        AssertAnonymousProperty(row, "Cuenta", "Pago proveedor");
        AssertAnonymousProperty(row, "CentroCosto", "Tesorería");
    }

    private static ImputacionesController CreateController(IApplicationDbContext db)
    {
        var controller = new ImputacionesController(
            Substitute.For<IMediator>(),
            Substitute.For<IImputacionRepository>(),
            db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }

    private static IApplicationDbContext BuildDb(
        IEnumerable<Comprobante> comprobantes,
        IEnumerable<TipoComprobante> tipos,
        IEnumerable<Tercero> terceros,
        IEnumerable<Moneda> monedas,
        IEnumerable<Usuario> usuarios,
        IEnumerable<OrdenCompraMeta> ordenes,
        IEnumerable<Comprobante> recepciones,
        IEnumerable<Imputacion> imputaciones)
    {
        var db = Substitute.For<IApplicationDbContext>();
        db.Comprobantes.Returns(MockDbSetHelper.CreateMockDbSet(comprobantes.Concat(recepciones)));
        db.TiposComprobante.Returns(MockDbSetHelper.CreateMockDbSet(tipos));
        db.Terceros.Returns(MockDbSetHelper.CreateMockDbSet(terceros));
        db.Monedas.Returns(MockDbSetHelper.CreateMockDbSet(monedas));
        db.Usuarios.Returns(MockDbSetHelper.CreateMockDbSet(usuarios));
        db.OrdenesCompraMeta.Returns(MockDbSetHelper.CreateMockDbSet(ordenes));
        db.Imputaciones.Returns(MockDbSetHelper.CreateMockDbSet(imputaciones));
        return db;
    }

    private static Comprobante CrearComprobante(long id, long tipoId, long terceroId, long monedaId, decimal total, decimal saldo, string? observacion, long? origenId)
    {
        var comprobante = (Comprobante)FormatterServices.GetUninitializedObject(typeof(Comprobante));
        SetProperty(comprobante, nameof(Comprobante.Id), id);
        SetProperty(comprobante, nameof(Comprobante.SucursalId), 1L);
        SetProperty(comprobante, nameof(Comprobante.TipoComprobanteId), tipoId);
        SetProperty(comprobante, nameof(Comprobante.TerceroId), terceroId);
        SetProperty(comprobante, nameof(Comprobante.MonedaId), monedaId);
        SetProperty(comprobante, nameof(Comprobante.Numero), new NroComprobante(1, id));
        SetProperty(comprobante, nameof(Comprobante.Fecha), new DateOnly(2026, 4, 10));
        SetProperty(comprobante, nameof(Comprobante.Total), total);
        SetProperty(comprobante, nameof(Comprobante.Saldo), saldo);
        SetProperty(comprobante, nameof(Comprobante.Estado), EstadoComprobante.Emitido);
        SetProperty(comprobante, nameof(Comprobante.CreatedBy), 5L);
        SetProperty(comprobante, nameof(Comprobante.CreatedAt), new DateTimeOffset(2026, 4, 10, 10, 0, 0, TimeSpan.Zero));
        SetProperty(comprobante, nameof(Comprobante.Observacion), observacion);
        SetProperty(comprobante, nameof(Comprobante.ComprobanteOrigenId), origenId);
        return comprobante;
    }

    private static TipoComprobante CrearTipoComprobante(long id, string descripcion, bool esCompra, bool afectaCuentaCorriente)
    {
        var tipo = (TipoComprobante)FormatterServices.GetUninitializedObject(typeof(TipoComprobante));
        SetProperty(tipo, nameof(TipoComprobante.Id), id);
        SetProperty(tipo, nameof(TipoComprobante.Descripcion), descripcion);
        SetProperty(tipo, nameof(TipoComprobante.Codigo), descripcion.ToUpperInvariant().Replace(' ', '_'));
        SetProperty(tipo, nameof(TipoComprobante.Activo), true);
        SetProperty(tipo, nameof(TipoComprobante.EsCompra), esCompra);
        SetProperty(tipo, nameof(TipoComprobante.EsVenta), !esCompra);
        SetProperty(tipo, nameof(TipoComprobante.AfectaCuentaCorriente), afectaCuentaCorriente);
        return tipo;
    }

    private static Tercero CrearTercero(long id, string razonSocial)
    {
        var tercero = (Tercero)FormatterServices.GetUninitializedObject(typeof(Tercero));
        SetProperty(tercero, nameof(Tercero.Id), id);
        SetProperty(tercero, nameof(Tercero.RazonSocial), razonSocial);
        return tercero;
    }

    private static Moneda CrearMoneda(long id, string codigo)
    {
        var moneda = (Moneda)FormatterServices.GetUninitializedObject(typeof(Moneda));
        SetProperty(moneda, nameof(Moneda.Id), id);
        SetProperty(moneda, nameof(Moneda.Codigo), codigo);
        SetProperty(moneda, nameof(Moneda.Activa), true);
        return moneda;
    }

    private static Usuario CrearUsuario(long id, string nombre)
    {
        var usuario = (Usuario)FormatterServices.GetUninitializedObject(typeof(Usuario));
        SetProperty(usuario, nameof(Usuario.Id), id);
        SetProperty(usuario, nameof(Usuario.UserName), nombre.ToLowerInvariant().Replace(' ', '.'));
        SetProperty(usuario, nameof(Usuario.NombreCompleto), nombre);
        return usuario;
    }

    private static OrdenCompraMeta CrearOrden(long id, long comprobanteId, long proveedorId)
    {
        var orden = OrdenCompraMeta.Crear(comprobanteId, proveedorId, new DateOnly(2026, 4, 20), "Entrega", 1m);
        SetProperty(orden, nameof(OrdenCompraMeta.Id), id);
        return orden;
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
