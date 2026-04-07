using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Common.Mappings;
using ZuluIA_Back.Application.Features.Terceros.Commands;
using ZuluIA_Back.Domain.Entities.Geografia;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Application;

public class ReplaceTerceroDomiciliosCommandHandlerTests
{
    [Fact]
    public async Task Handle_DebeSincronizarDomicilioPrincipalConElDomicilioPorDefecto()
    {
        var db = Substitute.For<IApplicationDbContext>();
        var uow = Substitute.For<IUnitOfWork>();
        var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        var mapper = mapperConfig.CreateMapper();

        var tercero = Tercero.Crear("CLI001", "Empresa SA", 1, "30-11111111-1", 1, true, false, false, null, null);
        SetId(tercero, 10L);
        var localidad = CreateLocalidad(5, 1, "Cordoba");
        var barrio = CreateBarrio(7, 5, "Centro");
        var domicilioExistente = PersonaDomicilio.Crear(tercero.Id, 1, null, 5, "Viejo", null, "5000", null, 0, true);

        db.Terceros.Returns(MockDbSetHelper.CreateMockDbSet([tercero]));
        db.PersonasDomicilios.Returns(MockDbSetHelper.CreateMockDbSet([domicilioExistente]));
        db.Barrios.Returns(MockDbSetHelper.CreateMockDbSet([barrio]));
        db.Provincias.Returns(MockDbSetHelper.CreateMockDbSet<Provincia>());
        db.Localidades.Returns(MockDbSetHelper.CreateMockDbSet([localidad]));

        var handler = new ReplaceTerceroDomiciliosCommandHandler(db, uow, mapper);
        var command = new ReplaceTerceroDomiciliosCommand(
            tercero.Id,
            [new ReplaceTerceroDomicilioItem(domicilioExistente.Id, 1, null, 5, "Casa central", "Centro", "X5000", null, true, 0)]);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        tercero.Domicilio.Calle.Should().Be("Casa central");
        tercero.Domicilio.CodigoPostal.Should().Be("X5000");
        tercero.Domicilio.ProvinciaId.Should().Be(1);
        tercero.Domicilio.LocalidadId.Should().Be(5);
        tercero.Domicilio.BarrioId.Should().Be(7);
    }

    [Fact]
    public async Task Handle_MasDeTresDomicilios_RetornaFailure()
    {
        var db = Substitute.For<IApplicationDbContext>();
        var uow = Substitute.For<IUnitOfWork>();
        var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        var mapper = mapperConfig.CreateMapper();

        var tercero = Tercero.Crear("CLI001", "Empresa SA", 1, "30-11111111-1", 1, true, false, false, null, null);
        SetId(tercero, 10L);

        db.Terceros.Returns(MockDbSetHelper.CreateMockDbSet([tercero]));
        db.PersonasDomicilios.Returns(MockDbSetHelper.CreateMockDbSet<PersonaDomicilio>());
        db.Barrios.Returns(MockDbSetHelper.CreateMockDbSet<Barrio>());
        db.Provincias.Returns(MockDbSetHelper.CreateMockDbSet<Provincia>());
        db.Localidades.Returns(MockDbSetHelper.CreateMockDbSet<Localidad>());

        var handler = new ReplaceTerceroDomiciliosCommandHandler(db, uow, mapper);
        var command = new ReplaceTerceroDomiciliosCommand(
            tercero.Id,
            [
                new ReplaceTerceroDomicilioItem(null, null, null, null, "Domicilio 1", null, null, null, true, 0),
                new ReplaceTerceroDomicilioItem(null, null, null, null, "Domicilio 2", null, null, null, false, 1),
                new ReplaceTerceroDomicilioItem(null, null, null, null, "Domicilio 3", null, null, null, false, 2),
                new ReplaceTerceroDomicilioItem(null, null, null, null, "Domicilio 4", null, null, null, false, 3)
            ]);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("hasta 3 domicilios");
        await uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static Localidad CreateLocalidad(long id, long provinciaId, string descripcion)
    {
        var entity = Localidad.Crear(provinciaId, descripcion);
        typeof(Localidad).GetProperty(nameof(Localidad.Id))!.SetValue(entity, id);
        return entity;
    }

    private static void SetId(object entity, long id)
    {
        var property = entity.GetType().BaseType?.GetProperty("Id") ?? entity.GetType().GetProperty("Id");
        property.Should().NotBeNull();
        property!.SetValue(entity, id);
    }

    private static Barrio CreateBarrio(long id, long localidadId, string descripcion)
    {
        var entity = Barrio.Crear(localidadId, descripcion);
        typeof(Barrio).GetProperty(nameof(Barrio.Id))!.SetValue(entity, id);
        return entity;
    }
}
