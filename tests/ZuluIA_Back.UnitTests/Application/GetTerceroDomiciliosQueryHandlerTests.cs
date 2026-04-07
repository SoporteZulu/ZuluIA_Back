using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Common.Mappings;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Application.Features.Terceros.Queries;
using ZuluIA_Back.Domain.Entities.Geografia;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Application;

public class GetTerceroDomiciliosQueryHandlerTests
{
    [Fact]
    public async Task Handle_DebeRetornarDomiciliosOrdenados()
    {
        var db = Substitute.For<IApplicationDbContext>();
        var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        var mapper = mapperConfig.CreateMapper();

        var tercero = Tercero.Crear("CLI001", "Empresa SA", 1, "30-11111111-1", 1, true, false, false, null, null);
        typeof(Tercero).GetProperty(nameof(Tercero.Id))!.SetValue(tercero, 1L);
        var tipoDomicilio = CreateTipoDomicilio(1, "Legal");
        var provincia = CreateProvincia(2, "Cordoba");
        var localidad = CreateLocalidad(5, provincia.Id, "Capital");
        var domicilioSecundario = PersonaDomicilio.Crear(tercero.Id, tipoDomicilioId: 1, provinciaId: provincia.Id, localidadId: localidad.Id, calle: "Depósito", barrio: "Centro", orden: 1, esDefecto: false);
        var domicilioPrincipal = PersonaDomicilio.Crear(tercero.Id, tipoDomicilioId: 1, provinciaId: provincia.Id, localidadId: localidad.Id, calle: "Casa central", barrio: "General Paz", codigoPostal: "5000", orden: 2, esDefecto: true);

        db.Terceros.Returns(MockDbSetHelper.CreateMockDbSet([tercero]));
        db.PersonasDomicilios.Returns(MockDbSetHelper.CreateMockDbSet([domicilioSecundario, domicilioPrincipal]));
        db.TiposDomicilio.Returns(MockDbSetHelper.CreateMockDbSet([tipoDomicilio]));
        db.Provincias.Returns(MockDbSetHelper.CreateMockDbSet([provincia]));
        db.Localidades.Returns(MockDbSetHelper.CreateMockDbSet([localidad]));

        var handler = new GetTerceroDomiciliosQueryHandler(db, mapper);

        var result = await handler.Handle(new GetTerceroDomiciliosQuery(tercero.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value![0].Calle.Should().Be("Casa central");
        result.Value[0].EsDefecto.Should().BeTrue();
        result.Value[0].TipoDomicilioDescripcion.Should().Be("Legal");
        result.Value[0].GeografiaCompleta.Should().Be("General Paz / Capital / Cordoba");
        result.Value[0].UbicacionCompleta.Should().Be("Casa central - General Paz / Capital / Cordoba - CP 5000");
    }

    private static Provincia CreateProvincia(long id, string descripcion)
    {
        var entity = Provincia.Crear(1, "CBA", descripcion);
        typeof(Provincia).GetProperty(nameof(Provincia.Id))!.SetValue(entity, id);
        return entity;
    }

    private static Localidad CreateLocalidad(long id, long provinciaId, string descripcion)
    {
        var entity = Localidad.Crear(provinciaId, descripcion);
        typeof(Localidad).GetProperty(nameof(Localidad.Id))!.SetValue(entity, id);
        return entity;
    }

    private static TipoDomicilioCatalogo CreateTipoDomicilio(long id, string descripcion)
    {
        var entity = (TipoDomicilioCatalogo)Activator.CreateInstance(typeof(TipoDomicilioCatalogo), nonPublic: true)!;
        typeof(TipoDomicilioCatalogo).GetProperty(nameof(TipoDomicilioCatalogo.Id))!.SetValue(entity, id);
        typeof(TipoDomicilioCatalogo).GetProperty(nameof(TipoDomicilioCatalogo.Descripcion))!.SetValue(entity, descripcion);
        return entity;
    }
}
