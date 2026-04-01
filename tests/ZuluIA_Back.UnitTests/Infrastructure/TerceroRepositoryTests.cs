using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using ZuluIA_Back.Infrastructure.Persistence;
using ZuluIA_Back.Infrastructure.Persistence.Repositories;
using ZuluIA_Back.Domain.Entities.Terceros;

namespace ZuluIA_Back.UnitTests.Infrastructure;

public class TerceroRepositoryTests
{
    [Fact]
    public async Task GetPagedAsync_ConSoloActivosTrue_DebeRetornarSoloActivos()
    {
        await using var context = CreateContext();
        context.Terceros.AddRange(
            CreateCliente("CLI001", "Activo Uno"),
            CreateClienteInactivo("CLI002", "Inactivo Uno"));
        await context.SaveChangesAsync();

        var repository = new TerceroRepository(context);

        var result = await repository.GetPagedAsync(
            1,
            50,
            null,
            true,
            null,
            null,
            true,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items[0].Legajo.Should().Be("CLI001");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(null)]
    public async Task GetPagedAsync_ConSoloActivosFalseONull_DebeRetornarTodos(bool? soloActivos)
    {
        await using var context = CreateContext();
        context.Terceros.AddRange(
            CreateCliente("CLI001", "Activo Uno"),
            CreateClienteInactivo("CLI002", "Inactivo Uno"));
        await context.SaveChangesAsync();

        var repository = new TerceroRepository(context);

        var result = await repository.GetPagedAsync(
            1,
            50,
            null,
            true,
            null,
            null,
            soloActivos,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            CancellationToken.None);

        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Items.Select(x => x.Legajo).Should().BeEquivalentTo(["CLI001", "CLI002"]);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static Tercero CreateCliente(string legajo, string razonSocial)
        => Tercero.Crear(legajo, razonSocial, 1, "20111111111", 1, true, false, false, null, null);

    private static Tercero CreateClienteInactivo(string legajo, string razonSocial)
    {
        var tercero = Tercero.Crear(legajo, razonSocial, 1, "20999999999", 1, true, false, false, null, null);
        tercero.Desactivar(null);
        return tercero;
    }
}
