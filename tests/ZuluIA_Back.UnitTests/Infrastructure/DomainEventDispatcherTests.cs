using FluentAssertions;
using MediatR;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Infrastructure.Services;

namespace ZuluIA_Back.UnitTests.Infrastructure;

public class DomainEventDispatcherTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly DomainEventDispatcher _sut;

    public DomainEventDispatcherTests()
    {
        _sut = new DomainEventDispatcher(_mediator);
    }

    [Fact]
    public async Task DispatchAsync_SinEventos_NoLlamaAlMediator()
    {
        await _sut.DispatchAsync([], CancellationToken.None);

        await _mediator.DidNotReceiveWithAnyArgs().Publish(default!, default);
    }

    [Fact]
    public async Task DispatchAsync_DosEventos_LlamaAlMediatorDosveces()
    {
        var ev1 = Substitute.For<IDomainEvent>();
        var ev2 = Substitute.For<IDomainEvent>();

        await _sut.DispatchAsync([ev1, ev2], CancellationToken.None);

        await _mediator.Received(1).Publish(ev1, Arg.Any<CancellationToken>());
        await _mediator.Received(1).Publish(ev2, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DispatchAsync_PasaCancellationToken_AlMediator()
    {
        var ev = Substitute.For<IDomainEvent>();
        using var cts = new CancellationTokenSource();

        await _sut.DispatchAsync([ev], cts.Token);

        await _mediator.Received(1).Publish(ev, cts.Token);
    }
}
