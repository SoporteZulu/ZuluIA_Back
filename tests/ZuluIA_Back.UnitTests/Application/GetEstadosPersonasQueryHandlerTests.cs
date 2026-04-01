using FluentAssertions;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Features.Terceros.Queries;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.UnitTests.Application;

public class GetEstadosPersonasQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenSoloActivos_ReturnsOnlyActiveStates()
    {
        var repo = Substitute.For<IRepository<EstadoPersonaCatalogo>>();
        var habilitado = EstadoPersonaCatalogo.Crear("Habilitado", userId: null);
        var suspendido = EstadoPersonaCatalogo.Crear("Suspendido", userId: null);
        suspendido.Desactivar(userId: null);

        repo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IList<EstadoPersonaCatalogo>>([habilitado, suspendido]));

        var handler = new GetEstadosPersonasQueryHandler(repo);

        var result = await handler.Handle(new GetEstadosPersonasQuery(SoloActivos: true), CancellationToken.None);

        result.Select(x => x.Descripcion).Should().Equal("Habilitado");
    }
}
