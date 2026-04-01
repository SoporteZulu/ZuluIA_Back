using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Common.Mappings;
using ZuluIA_Back.Application.Features.Terceros.Queries;
using ZuluIA_Back.Domain.Entities.Geografia;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.ValueObjects;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Application;

public class GetClientesActivosQueryHandlerTests
{
    [Fact]
    public async Task Handle_DebeRetornarUbicacionCompuestaEnSelector()
    {
        var repo = Substitute.For<ITerceroRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        var mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();

        var tercero = Tercero.Crear("CLI001", "Empresa SA", 1, "30-11111111-1", 1, true, false, false, null, null);
        tercero.SetDomicilio(Domicilio.Crear("Av. Siempre Viva", "742", null, null, "5000", 5, 7, 2));

        var provincia = CreateProvincia(2, "Cordoba");
        var localidad = CreateLocalidad(5, 2, "Capital");
        var barrio = CreateBarrio(7, 5, "Centro");

        repo.GetClientesActivosAsync(null, Arg.Any<CancellationToken>())
            .Returns([tercero]);
        var provincias = MockDbSetHelper.CreateMockDbSet([provincia]);
        var localidades = MockDbSetHelper.CreateMockDbSet([localidad]);
        var barrios = MockDbSetHelper.CreateMockDbSet([barrio]);
        db.Provincias.Returns(provincias);
        db.Localidades.Returns(localidades);
        db.Barrios.Returns(barrios);

        var handler = new GetClientesActivosQueryHandler(repo, db, mapper);

        var result = await handler.Handle(new GetClientesActivosQuery(), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].GeografiaCompleta.Should().Be("Centro / Capital / Cordoba");
        result[0].UbicacionCompleta.Should().Be("Av. Siempre Viva 742 - Centro / Capital / Cordoba - CP 5000");
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

    private static Barrio CreateBarrio(long id, long localidadId, string descripcion)
    {
        var entity = Barrio.Crear(localidadId, descripcion);
        typeof(Barrio).GetProperty(nameof(Barrio.Id))!.SetValue(entity, id);
        return entity;
    }
}
