using FluentAssertions;
using MediatR;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Application.Features.Terceros.Queries;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.UnitTests.Application;

public class GetTerceroByNroDocumentoQueryHandlerTests
{
    [Fact]
    public async Task Handle_CuandoDocumentoExiste_DeveRetornarTercero()
    {
        var repo = Substitute.For<ITerceroRepository>();
        var sender = Substitute.For<ISender>();

        var tercero = Tercero.Crear("CLI001", "Cliente SA", 1, "30-11111111-1", 1, true, false, false, null, null);
        typeof(Tercero).GetProperty(nameof(Tercero.Id))!.SetValue(tercero, 42L);

        repo.GetByNroDocumentoAsync("30-11111111-1", Arg.Any<CancellationToken>())
            .Returns(tercero);

        sender.Send(Arg.Any<GetTerceroByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(new TerceroDto
            {
                Id = 42,
                Legajo = "CLI001",
                RazonSocial = "Cliente SA",
                NroDocumento = "30-11111111-1"
            }));

        var handler = new GetTerceroByNroDocumentoQueryHandler(repo, sender);

        var result = await handler.Handle(
            new GetTerceroByNroDocumentoQuery("30-11111111-1"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(42);
        result.Value.NroDocumento.Should().Be("30-11111111-1");
        await sender.Received(1).Send(
            Arg.Is<GetTerceroByIdQuery>(q => q.Id == 42),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CuandoDocumentoNoExiste_DebeRetornarFailure()
    {
        var repo = Substitute.For<ITerceroRepository>();
        var sender = Substitute.For<ISender>();

        repo.GetByNroDocumentoAsync("99-99999999-9", Arg.Any<CancellationToken>())
            .Returns((Tercero?)null);

        var handler = new GetTerceroByNroDocumentoQueryHandler(repo, sender);

        var result = await handler.Handle(
            new GetTerceroByNroDocumentoQuery("99-99999999-9"),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("No se encontró");
        result.Error.Should().Contain("99-99999999-9");
        await sender.DidNotReceive().Send(Arg.Any<GetTerceroByIdQuery>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_CuandoDocumentoVacio_DebeRetornarFailure(string? documento)
    {
        var repo = Substitute.For<ITerceroRepository>();
        var sender = Substitute.For<ISender>();

        var handler = new GetTerceroByNroDocumentoQueryHandler(repo, sender);

        var result = await handler.Handle(
            new GetTerceroByNroDocumentoQuery(documento!),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("no puede estar vacío");
        await repo.DidNotReceive().GetByNroDocumentoAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
