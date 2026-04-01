using FluentAssertions;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Features.Terceros.Queries;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.UnitTests.Application;

public class GetTiposDomicilioQueryHandlerTests
{
    [Fact]
    public async Task Handle_DebeOrdenarPorDescripcion()
    {
        var repo = Substitute.For<IRepository<TipoDomicilioCatalogo>>();
        var fiscal = CreateTipoDomicilio(2, "Fiscal");
        var legal = CreateTipoDomicilio(1, "Legal");

        repo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IList<TipoDomicilioCatalogo>>([fiscal, legal]));

        var handler = new GetTiposDomicilioQueryHandler(repo);

        var result = await handler.Handle(new GetTiposDomicilioQuery(), CancellationToken.None);

        result.Select(x => x.Descripcion).Should().Equal("Fiscal", "Legal");
    }

    private static TipoDomicilioCatalogo CreateTipoDomicilio(long id, string descripcion)
    {
        var entity = (TipoDomicilioCatalogo)Activator.CreateInstance(typeof(TipoDomicilioCatalogo), nonPublic: true)!;
        typeof(TipoDomicilioCatalogo).GetProperty(nameof(TipoDomicilioCatalogo.Id))!.SetValue(entity, id);
        typeof(TipoDomicilioCatalogo).GetProperty(nameof(TipoDomicilioCatalogo.Descripcion))!.SetValue(entity, descripcion);
        return entity;
    }
}
