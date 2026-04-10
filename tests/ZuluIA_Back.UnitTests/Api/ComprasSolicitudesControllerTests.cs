using System.Runtime.Serialization;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.Domain.Entities.Stock;
using ZuluIA_Back.Domain.Entities.Sucursales;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class ComprasSolicitudesControllerTests
{
    [Fact]
    public async Task GetSolicitudes_CuandoHayCasoCritico_DevuelveProyeccionEnriquecida()
    {
        var controller = CreateController(BuildDb(
            [CrearStock(100, 20, 0m)],
            [CrearItem(100, "MAT-001", "Acero", 2, 3m, 10m, 2m, 7)],
            [CrearDeposito(20, 5, "Central")],
            [CrearSucursal(5, "Casa Central")],
            [CrearUnidad(2, "unid")],
            [CrearCategoria(7, "Insumos")]
        ));

        var result = await controller.GetSolicitudes(null, null, null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var rows = ok.Value.Should().BeAssignableTo<IReadOnlyList<CompraSolicitudResumenDto>>().Subject;
        rows.Should().ContainSingle();
        rows[0].Codigo.Should().Be("MAT-001");
        rows[0].Severidad.Should().Be("critica");
        rows[0].Sugerido.Should().Be(10m);
        rows[0].CostoEstimado.Should().Be(20m);
        rows[0].EstadoReposicion.Should().Be("Reposición inmediata requerida");
        rows[0].CategoriaDescripcion.Should().Be("Insumos");
        rows[0].UnidadMedidaDescripcion.Should().Be("unid");
    }

    [Fact]
    public async Task GetSolicitudes_CuandoFiltraPorDepositoYSeveridad_AplicaCompatibilidad()
    {
        var controller = CreateController(BuildDb(
            [
                CrearStock(100, 20, 0m),
                CrearStock(101, 21, 4m)
            ],
            [
                CrearItem(100, "MAT-001", "Acero", 2, 3m, 8m, 2m, null),
                CrearItem(101, "MAT-002", "Tornillo", 2, 5m, 8m, 1m, null)
            ],
            [
                CrearDeposito(20, 5, "Central"),
                CrearDeposito(21, 5, "Secundario")
            ],
            [CrearSucursal(5, "Casa Central")],
            [CrearUnidad(2, "unid")],
            []
        ));

        var result = await controller.GetSolicitudes(5, 20, "Crítica", CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var rows = ok.Value.Should().BeAssignableTo<IReadOnlyList<CompraSolicitudResumenDto>>().Subject;
        rows.Should().ContainSingle();
        rows[0].DepositoId.Should().Be(20);
        rows[0].Severidad.Should().Be("critica");
    }

    private static ComprasController CreateController(IApplicationDbContext db)
    {
        var controller = new ComprasController(Substitute.For<IMediator>(), db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }

    private static IApplicationDbContext BuildDb(
        IEnumerable<StockItem> stock,
        IEnumerable<Item> items,
        IEnumerable<Deposito> depositos,
        IEnumerable<Sucursal> sucursales,
        IEnumerable<UnidadMedida> unidades,
        IEnumerable<CategoriaItem> categorias)
    {
        var db = Substitute.For<IApplicationDbContext>();
        db.Stock.Returns(MockDbSetHelper.CreateMockDbSet(stock));
        db.Items.Returns(MockDbSetHelper.CreateMockDbSet(items));
        db.Depositos.Returns(MockDbSetHelper.CreateMockDbSet(depositos));
        db.Sucursales.Returns(MockDbSetHelper.CreateMockDbSet(sucursales));
        db.UnidadesMedida.Returns(MockDbSetHelper.CreateMockDbSet(unidades));
        db.CategoriasItems.Returns(MockDbSetHelper.CreateMockDbSet(categorias));
        return db;
    }

    private static StockItem CrearStock(long itemId, long depositoId, decimal cantidad)
    {
        var stock = StockItem.Crear(itemId, depositoId, cantidad);
        SetProperty(stock, nameof(StockItem.Id), itemId * 100 + depositoId);
        return stock;
    }

    private static Item CrearItem(long id, string codigo, string descripcion, long unidadId, decimal stockMinimo, decimal? stockMaximo, decimal precioCosto, long? categoriaId)
    {
        var item = (Item)FormatterServices.GetUninitializedObject(typeof(Item));
        SetProperty(item, nameof(Item.Id), id);
        SetProperty(item, nameof(Item.Codigo), codigo);
        SetProperty(item, nameof(Item.Descripcion), descripcion);
        SetProperty(item, nameof(Item.UnidadMedidaId), unidadId);
        SetProperty(item, nameof(Item.StockMinimo), stockMinimo);
        SetProperty(item, nameof(Item.StockMaximo), stockMaximo);
        SetProperty(item, nameof(Item.PrecioCosto), precioCosto);
        SetProperty(item, nameof(Item.CategoriaId), categoriaId);
        SetProperty(item, nameof(Item.ManejaStock), true);
        SetProperty(item, nameof(Item.Activo), true);
        return item;
    }

    private static Deposito CrearDeposito(long id, long sucursalId, string descripcion)
    {
        var deposito = Deposito.Crear(sucursalId, descripcion);
        SetProperty(deposito, nameof(Deposito.Id), id);
        return deposito;
    }

    private static Sucursal CrearSucursal(long id, string descripcion)
    {
        var sucursal = Sucursal.Crear(descripcion, "80000000-0", 1, 1, 1, true, 1);
        SetProperty(sucursal, nameof(Sucursal.Id), id);
        SetProperty(sucursal, nameof(Sucursal.NombreFantasia), descripcion);
        return sucursal;
    }

    private static UnidadMedida CrearUnidad(long id, string descripcion)
    {
        var unidad = UnidadMedida.Crear(descripcion, descripcion, descripcion);
        SetProperty(unidad, nameof(UnidadMedida.Id), id);
        return unidad;
    }

    private static CategoriaItem CrearCategoria(long id, string descripcion)
    {
        var categoria = CategoriaItem.Crear(null, descripcion, descripcion, 1, null, 1);
        SetProperty(categoria, nameof(CategoriaItem.Id), id);
        return categoria;
    }

    private static void SetProperty(object target, string propertyName, object? value)
    {
        target.GetType().GetProperty(propertyName)!.SetValue(target, value);
    }
}
