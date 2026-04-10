using System.Runtime.Serialization;
using FluentAssertions;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Compras.Queries;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Compras;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Entities.Usuarios;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Application.Compras;

public class ComprasQueryHandlersTests
{
    [Fact]
    public async Task GetCotizacionesPaged_EnriqueceProveedorEstadoLegacyYCantidad()
    {
        var repo = Substitute.For<ICotizacionCompraRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        var cotizacion = CrearCotizacion(41, 10, 7, EstadoCotizacionCompra.Aceptada, 5);
        repo.GetPagedAsync(2, 30, 10, 7, "ACEPTADA", Arg.Any<CancellationToken>())
            .Returns(new PagedResult<CotizacionCompra>([cotizacion], 2, 30, 1));
        db.Terceros.Returns(MockDbSetHelper.CreateMockDbSet([CrearTercero(7, "Proveedor Uno")]));

        var handler = new GetCotizacionesCompraPagedQueryHandler(repo, db);

        var result = await handler.Handle(new GetCotizacionesCompraPagedQuery(2, 30, 10, 7, "ACEPTADA"), CancellationToken.None);

        result.Page.Should().Be(2);
        result.PageSize.Should().Be(30);
        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items[0].Proveedor.Should().Be("Proveedor Uno");
        result.Items[0].RequisicionReferencia.Should().Be("REQ-5");
        result.Items[0].EstadoLegacy.Should().Be("APROBADA");
        result.Items[0].CantidadItems.Should().Be(1);
    }

    [Fact]
    public async Task GetCotizacionDetalle_EnriqueceProveedorYCodigoItem()
    {
        var repo = Substitute.For<ICotizacionCompraRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        var cotizacion = CrearCotizacion(42, 10, 8, EstadoCotizacionCompra.Pendiente, 6);
        repo.GetByIdConItemsAsync(42, Arg.Any<CancellationToken>()).Returns(cotizacion);
        db.Terceros.Returns(MockDbSetHelper.CreateMockDbSet([CrearTercero(8, "Proveedor Dos")]));
        db.Items.Returns(MockDbSetHelper.CreateMockDbSet([CrearItem(100, "MAT-001")]));

        var handler = new GetCotizacionCompraDetalleQueryHandler(repo, db);

        var result = await handler.Handle(new GetCotizacionCompraDetalleQuery(42), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Proveedor.Should().Be("Proveedor Dos");
        result.RequisicionReferencia.Should().Be("REQ-6");
        result.EstadoLegacy.Should().Be("ENVIADA");
        result.Items.Should().ContainSingle();
        result.Items[0].Codigo.Should().Be("MAT-001");
        result.Items[0].Subtotal.Should().Be(result.Items[0].Total);
    }

    [Fact]
    public async Task GetRequisicionesPaged_EnriqueceSolicitanteYEstadoLegacy()
    {
        var repo = Substitute.For<IRequisicionCompraRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        var requisicion = CrearRequisicion(51, 15, EstadoRequisicion.Procesada);
        repo.GetPagedAsync(1, 20, 10, 15, "PROCESADA", Arg.Any<CancellationToken>())
            .Returns(new PagedResult<RequisicionCompra>([requisicion], 1, 20, 1));
        db.Usuarios.Returns(MockDbSetHelper.CreateMockDbSet([CrearUsuario(15, "Paula Gómez")]));

        var handler = new GetRequisicionesCompraPagedQueryHandler(repo, db);

        var result = await handler.Handle(new GetRequisicionesCompraPagedQuery(1, 20, 10, 15, "PROCESADA"), CancellationToken.None);

        result.Items.Should().ContainSingle();
        result.Items[0].Solicitante.Should().Be("Paula Gómez");
        result.Items[0].EstadoLegacy.Should().Be("COTIZADA");
        result.Items[0].RequisicionReferencia.Should().Be("REQ-51");
        result.Items[0].Motivo.Should().Be(result.Items[0].Descripcion);
    }

    [Fact]
    public async Task GetRequisicionDetalle_EnriqueceSolicitanteYCodigoItem()
    {
        var repo = Substitute.For<IRequisicionCompraRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        var requisicion = CrearRequisicion(52, 16, EstadoRequisicion.Borrador);
        repo.GetByIdConItemsAsync(52, Arg.Any<CancellationToken>()).Returns(requisicion);
        db.Usuarios.Returns(MockDbSetHelper.CreateMockDbSet([CrearUsuario(16, "Lucía Márquez")]));
        db.Items.Returns(MockDbSetHelper.CreateMockDbSet([CrearItem(101, "TOR-001")]));

        var handler = new GetRequisicionCompraDetalleQueryHandler(repo, db);

        var result = await handler.Handle(new GetRequisicionCompraDetalleQuery(52), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Solicitante.Should().Be("Lucía Márquez");
        result.EstadoLegacy.Should().Be("ABIERTA");
        result.Items.Should().ContainSingle();
        result.Items[0].Codigo.Should().Be("TOR-001");
    }

    private static CotizacionCompra CrearCotizacion(long id, long sucursalId, long proveedorId, EstadoCotizacionCompra estado, long? requisicionId)
    {
        var cotizacion = CotizacionCompra.Crear(sucursalId, requisicionId, proveedorId, new DateOnly(2026, 4, 20), new DateOnly(2026, 4, 25), "Observación", 1);
        cotizacion.AgregarItem(CotizacionCompraItem.Crear(id, 100, "Insumo", 2, 15));
        if (estado == EstadoCotizacionCompra.Aceptada)
            cotizacion.Aceptar(1);
        else if (estado == EstadoCotizacionCompra.Rechazada)
            cotizacion.Rechazar(1);
        else if (estado == EstadoCotizacionCompra.Procesada)
        {
            cotizacion.Aceptar(1);
            cotizacion.MarcarProcesada(1);
        }

        SetProperty(cotizacion, nameof(CotizacionCompra.Id), id);
        SetProperty(cotizacion.Items.Single(), nameof(CotizacionCompraItem.Id), id * 10);
        return cotizacion;
    }

    private static RequisicionCompra CrearRequisicion(long id, long solicitanteId, EstadoRequisicion estado)
    {
        var requisicion = RequisicionCompra.Crear(10, solicitanteId, new DateOnly(2026, 4, 20), "Reposición crítica", "Observación", 1);
        requisicion.AgregarItem(RequisicionCompraItem.Crear(id, 101, "Tornillo", 4, "unid", "Detalle"));
        if (estado == EstadoRequisicion.Enviada)
            requisicion.Enviar(1);
        else if (estado == EstadoRequisicion.Aprobada)
        {
            requisicion.Enviar(1);
            requisicion.Aprobar(1);
        }
        else if (estado == EstadoRequisicion.Procesada)
        {
            requisicion.Enviar(1);
            requisicion.Aprobar(1);
            requisicion.MarcarProcesada(1);
        }
        else if (estado == EstadoRequisicion.Cancelada)
            requisicion.Cancelar(1);

        SetProperty(requisicion, nameof(RequisicionCompra.Id), id);
        SetProperty(requisicion.Items.Single(), nameof(RequisicionCompraItem.Id), id * 10);
        return requisicion;
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
}
