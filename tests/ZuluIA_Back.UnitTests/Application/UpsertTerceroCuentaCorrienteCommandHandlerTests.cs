using FluentAssertions;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Terceros.Commands;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Application;

public class UpsertTerceroCuentaCorrienteCommandHandlerTests
{
    [Fact]
    public async Task Handle_DebePersistirLimitesSeparados()
    {
        var db = Substitute.For<IApplicationDbContext>();
        var currentUser = Substitute.For<ICurrentUserService>();
        var uow = Substitute.For<IUnitOfWork>();
        currentUser.UserId.Returns(1L);

        var tercero = Tercero.Crear("CLI001", "Empresa SA", 1, "30-11111111-1", 1, true, false, false, null, null);
        var perfil = TerceroPerfilComercial.Crear(tercero.Id, null);

        db.Terceros.Returns(MockDbSetHelper.CreateMockDbSet([tercero]));
        db.TercerosPerfilesComerciales.Returns(MockDbSetHelper.CreateMockDbSet([perfil]));

        var handler = new UpsertTerceroCuentaCorrienteCommandHandler(db, currentUser, uow);
        var command = new UpsertTerceroCuentaCorrienteCommand(
            tercero.Id,
            150m,
            new DateOnly(2024, 3, 1),
            new DateOnly(2024, 9, 30),
            300m,
            new DateOnly(2024, 1, 1),
            new DateOnly(2024, 12, 31));

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.LimiteSaldo.Should().Be(150m);
        result.Value.LimiteCreditoTotal.Should().Be(300m);
        tercero.LimiteCredito.Should().Be(300m);
        perfil.SaldoMaximoVigente.Should().Be(150m);
        await uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
