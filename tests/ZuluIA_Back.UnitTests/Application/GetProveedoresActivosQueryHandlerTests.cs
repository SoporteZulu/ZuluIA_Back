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

public class GetProveedoresActivosQueryHandlerTests
{
    [Fact]
    public async Task Handle_DebeRetornarUbicacionCompuestaEnSelector()
    {
        var repo = Substitute.For<ITerceroRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        var mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();

        var tercero = Tercero.Crear("PRO001", "Proveedor SA", 1, "30-22222222-2", 1, false, true, false, null, null);
        tercero.SetDomicilio(Domicilio.Crear("Ruta 9", "100", null, null, "3100", 8, null, 3));

        var provincia = CreateProvincia(3, "Entre Rios");
        var localidad = CreateLocalidad(8, 3, "Parana");

        repo.GetProveedoresActivosAsync(null, Arg.Any<CancellationToken>())
            .Returns([tercero]);
        db.Provincias.Returns(MockDbSetHelper.CreateMockDbSet([provincia]));
        db.Localidades.Returns(MockDbSetHelper.CreateMockDbSet([localidad]));
        db.Barrios.Returns(MockDbSetHelper.CreateMockDbSet<Barrio>());

        var handler = new GetProveedoresActivosQueryHandler(repo, db, mapper);

        var result = await handler.Handle(new GetProveedoresActivosQuery(), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].GeografiaCompleta.Should().Be("Parana / Entre Rios");
        result[0].UbicacionCompleta.Should().Be("Ruta 9 100 - Parana / Entre Rios - CP 3100");
    }

    private static Provincia CreateProvincia(long id, string descripcion)
    {
        var entity = Provincia.Crear(1, "ERI", descripcion);
        typeof(Provincia).GetProperty(nameof(Provincia.Id))!.SetValue(entity, id);
        return entity;
    }

    private static Localidad CreateLocalidad(long id, long provinciaId, string descripcion)
    {
        var entity = Localidad.Crear(provinciaId, descripcion);
        typeof(Localidad).GetProperty(nameof(Localidad.Id))!.SetValue(entity, id);
        return entity;
    }
}
