using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using ZuluIA_Back.Application.Features.Compras.Commands;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Infrastructure.Persistence;
using ZuluIA_Back.Infrastructure.Persistence.Repositories;

namespace ZuluIA_Back.UnitTests.Application.Handlers;

public class CrearOrdenCompraDesdeComprobanteCommandHandlerTests
{
    [Fact]
    public async Task Handle_CuandoComprobanteEsValido_CreaOrdenCompraMeta()
    {
        await using var context = CreateContext();
        var comprobante = CrearComprobanteBase(terceroId: 25);
        context.Comprobantes.Add(comprobante);
        await context.SaveChangesAsync();

        var handler = new CrearOrdenCompraDesdeComprobanteCommandHandler(
            context,
            new ComprobanteRepository(context),
            new BaseRepository<OrdenCompraMeta>(context),
            new UnitOfWork(context));

        var result = await handler.Handle(
            new CrearOrdenCompraDesdeComprobanteCommand(
                comprobante.Id,
                25,
                new DateOnly(2026, 4, 20),
                "Entregar en depósito central"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var orden = await context.OrdenesCompraMeta.SingleAsync();
        orden.ComprobanteId.Should().Be(comprobante.Id);
        orden.ProveedorId.Should().Be(25);
        orden.FechaEntregaReq.Should().Be(new DateOnly(2026, 4, 20));
        orden.CondicionesEntrega.Should().Be("Entregar en depósito central");
        orden.CantidadTotal.Should().Be(3);
    }

    [Fact]
    public async Task Handle_CuandoComprobanteYaEstaVinculado_RetornaFailure()
    {
        await using var context = CreateContext();
        var comprobante = CrearComprobanteBase(terceroId: 25);
        context.Comprobantes.Add(comprobante);
        await context.SaveChangesAsync();

        context.OrdenesCompraMeta.Add(OrdenCompraMeta.Crear(comprobante.Id, 25, null, null, 3));
        await context.SaveChangesAsync();

        var handler = new CrearOrdenCompraDesdeComprobanteCommandHandler(
            context,
            new ComprobanteRepository(context),
            new BaseRepository<OrdenCompraMeta>(context),
            new UnitOfWork(context));

        var result = await handler.Handle(
            new CrearOrdenCompraDesdeComprobanteCommand(comprobante.Id, 25, null, null),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("ya está vinculado");
    }

    [Fact]
    public async Task Handle_CuandoProveedorNoCoincide_RetornaFailure()
    {
        await using var context = CreateContext();
        var comprobante = CrearComprobanteBase(terceroId: 25);
        context.Comprobantes.Add(comprobante);
        await context.SaveChangesAsync();

        var handler = new CrearOrdenCompraDesdeComprobanteCommandHandler(
            context,
            new ComprobanteRepository(context),
            new BaseRepository<OrdenCompraMeta>(context),
            new UnitOfWork(context));

        var result = await handler.Handle(
            new CrearOrdenCompraDesdeComprobanteCommand(comprobante.Id, 99, null, null),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("no coincide");
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static Comprobante CrearComprobanteBase(long terceroId)
    {
        var comprobante = Comprobante.Crear(
            1,
            null,
            1,
            1,
            1,
            new DateOnly(2026, 4, 10),
            null,
            terceroId,
            1,
            1,
            "Orden base",
            1);

        comprobante.AgregarItem(ComprobanteItem.Crear(
            0,
            10,
            "ITEM TEST",
            3,
            0,
            100,
            0,
            1,
            21,
            null,
            0));

        return comprobante;
    }
}
