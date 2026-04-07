using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using ZuluIA_Back.Infrastructure.Persistence;

namespace ZuluIA_Back.UnitTests.Infrastructure;

public class UnitOfWorkTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly UnitOfWork _sut;

    public UnitOfWorkTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().Options;
        _context = new AppDbContext(options);
        _sut = new UnitOfWork(_context);
    }

    [Fact]
    public async Task CommitTransactionAsync_SinTransaccionActiva_LanzaInvalidOperationException()
    {
        var act = async () => await _sut.CommitTransactionAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*transacción activa*");
    }

    [Fact]
    public async Task RollbackTransactionAsync_SinTransaccionActiva_LanzaInvalidOperationException()
    {
        var act = async () => await _sut.RollbackTransactionAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*transacción activa*");
    }

    [Fact]
    public void Dispose_LlamadoDosVeces_NoLanzaExcepcion()
    {
        var act = () =>
        {
            _sut.Dispose();
            _sut.Dispose();
        };

        act.Should().NotThrow();
    }

    public void Dispose()
    {
        _sut.Dispose();
        GC.SuppressFinalize(this);
    }
}
