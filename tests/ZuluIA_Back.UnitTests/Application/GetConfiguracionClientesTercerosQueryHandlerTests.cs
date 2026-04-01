using FluentAssertions;
using MediatR;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Application.Features.Terceros.Queries;
using ZuluIA_Back.Domain.Entities.Comercial;
using ZuluIA_Back.Domain.Entities.Geografia;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.Domain.Entities.Sucursales;
using ZuluIA_Back.Domain.Entities.Usuarios;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Application;

public class GetConfiguracionClientesTercerosQueryHandlerTests
{
    [Fact]
    public async Task Handle_DebeAgruparConfiguracionBaseDeLaFicha()
    {
        var db = Substitute.For<IApplicationDbContext>();
        var sender = Substitute.For<ISender>();

        var catalogos = new CatalogosTercerosDto([], [], [], [], [], [], []);
        sender.Send(Arg.Any<GetCatalogosTercerosQuery>(), Arg.Any<CancellationToken>())
            .Returns(catalogos);

        var categoria = CreateCategoriaTercero(1, "Mayorista");
        var condicionIva = CreateCondicionIva(2, 1, "Responsable Inscripto");
        var tipoDocumento = CreateTipoDocumento(3, 80, "CUIT");
        var moneda = CreateMoneda(4, "ARS", "Peso", "$", false);
        var pais = CreatePais(5, "AR", "Argentina");
        var zona = ZonaComercial.Crear("NORTE", "Zona Norte", userId: null);
        typeof(ZonaComercial).GetProperty(nameof(ZonaComercial.Id))!.SetValue(zona, 6L);
        var sucursal = Sucursal.Crear("Casa Central", "30-11111111-1", 1, 1, 1, true, null);
        typeof(Sucursal).GetProperty(nameof(Sucursal.Id))!.SetValue(sucursal, 7L);
        var usuario = Usuario.Crear("vendedor.norte", "Vendedor Norte", "vendedor@zuluia.local", null, null);
        typeof(Usuario).GetProperty(nameof(Usuario.Id))!.SetValue(usuario, 8L);

        var categoriasTerceros = MockDbSetHelper.CreateMockDbSet([categoria]);
        var condicionesIva = MockDbSetHelper.CreateMockDbSet([condicionIva]);
        var tiposDocumento = MockDbSetHelper.CreateMockDbSet([tipoDocumento]);
        var monedas = MockDbSetHelper.CreateMockDbSet([moneda]);
        var paises = MockDbSetHelper.CreateMockDbSet([pais]);
        var zonasComerciales = MockDbSetHelper.CreateMockDbSet([zona]);
        var sucursales = MockDbSetHelper.CreateMockDbSet([sucursal]);
        var usuarios = MockDbSetHelper.CreateMockDbSet([usuario]);

        db.CategoriasTerceros.Returns(categoriasTerceros);
        db.CondicionesIva.Returns(condicionesIva);
        db.TiposDocumento.Returns(tiposDocumento);
        db.Monedas.Returns(monedas);
        db.Paises.Returns(paises);
        db.ZonasComerciales.Returns(zonasComerciales);
        db.Sucursales.Returns(sucursales);
        db.Usuarios.Returns(usuarios);

        var handler = new GetConfiguracionClientesTercerosQueryHandler(db, sender);

        var result = await handler.Handle(new GetConfiguracionClientesTercerosQuery(), CancellationToken.None);

        result.Catalogos.Should().BeSameAs(catalogos);
        result.CategoriasTerceros.Should().ContainSingle(x => x.Descripcion == "Mayorista");
        result.CondicionesIva.Should().ContainSingle(x => x.Descripcion == "Responsable Inscripto");
        result.TiposDocumento.Should().ContainSingle(x => x.Descripcion == "CUIT");
        result.Monedas.Should().ContainSingle(x => x.Codigo == "ARS");
        result.Paises.Should().ContainSingle(x => x.Descripcion == "Argentina");
        result.ZonasComerciales.Should().ContainSingle(x => x.Descripcion == "Zona Norte");
        result.Sucursales.Should().ContainSingle(x => x.RazonSocial == "Casa Central");
        result.Vendedores.Should().ContainSingle(x => x.UserName == "vendedor.norte");
        result.Cobradores.Should().ContainSingle(x => x.UserName == "vendedor.norte");
    }

    private static CategoriaTercero CreateCategoriaTercero(long id, string descripcion)
    {
        var entity = CategoriaTercero.Crear(descripcion);
        typeof(CategoriaTercero).GetProperty(nameof(CategoriaTercero.Id))!.SetValue(entity, id);
        return entity;
    }

    private static CondicionIva CreateCondicionIva(long id, short codigo, string descripcion)
    {
        var entity = (CondicionIva)Activator.CreateInstance(typeof(CondicionIva), nonPublic: true)!;
        typeof(CondicionIva).GetProperty(nameof(CondicionIva.Id))!.SetValue(entity, id);
        typeof(CondicionIva).GetProperty(nameof(CondicionIva.Codigo))!.SetValue(entity, codigo);
        typeof(CondicionIva).GetProperty(nameof(CondicionIva.Descripcion))!.SetValue(entity, descripcion);
        return entity;
    }

    private static TipoDocumento CreateTipoDocumento(long id, short codigo, string descripcion)
    {
        var entity = (TipoDocumento)Activator.CreateInstance(typeof(TipoDocumento), nonPublic: true)!;
        typeof(TipoDocumento).GetProperty(nameof(TipoDocumento.Id))!.SetValue(entity, id);
        typeof(TipoDocumento).GetProperty(nameof(TipoDocumento.Codigo))!.SetValue(entity, codigo);
        typeof(TipoDocumento).GetProperty(nameof(TipoDocumento.Descripcion))!.SetValue(entity, descripcion);
        return entity;
    }

    private static Moneda CreateMoneda(long id, string codigo, string descripcion, string simbolo, bool sinDecimales)
    {
        var entity = (Moneda)Activator.CreateInstance(typeof(Moneda), nonPublic: true)!;
        typeof(Moneda).GetProperty(nameof(Moneda.Id))!.SetValue(entity, id);
        typeof(Moneda).GetProperty(nameof(Moneda.Codigo))!.SetValue(entity, codigo);
        typeof(Moneda).GetProperty(nameof(Moneda.Descripcion))!.SetValue(entity, descripcion);
        typeof(Moneda).GetProperty(nameof(Moneda.Simbolo))!.SetValue(entity, simbolo);
        typeof(Moneda).GetProperty(nameof(Moneda.SinDecimales))!.SetValue(entity, sinDecimales);
        return entity;
    }

    private static Pais CreatePais(long id, string codigo, string descripcion)
    {
        var entity = Pais.Crear(codigo, descripcion);
        typeof(Pais).GetProperty(nameof(Pais.Id))!.SetValue(entity, id);
        return entity;
    }
}
