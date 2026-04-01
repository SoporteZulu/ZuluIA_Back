using FluentAssertions;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Terceros.Queries;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Application;

public class GetTerceroCuentaCorrienteQueryHandlerTests
{
    [Fact]
    public async Task Handle_DebeRetornarLimitesSeparadosDesdeTerceroYPerfil()
    {
        var db = Substitute.For<IApplicationDbContext>();
        var tercero = Tercero.Crear("CLI001", "Empresa SA", 1, "30-11111111-1", 1, true, false, false, null, null);
        tercero.ActualizarCuentaCorriente(250m, new DateOnly(2024, 1, 1), new DateOnly(2024, 12, 31), null);

        var perfil = TerceroPerfilComercial.Crear(tercero.Id, null);
        perfil.ActualizarCuentaCorriente(100m, new DateOnly(2024, 2, 1), new DateOnly(2024, 11, 30), null);

        db.Terceros.Returns(MockDbSetHelper.CreateMockDbSet([tercero]));
        db.TercerosPerfilesComerciales.Returns(MockDbSetHelper.CreateMockDbSet([perfil]));

        var handler = new GetTerceroCuentaCorrienteQueryHandler(db);

        var result = await handler.Handle(new GetTerceroCuentaCorrienteQuery(tercero.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.LimiteSaldo.Should().Be(100m);
        result.Value.LimiteCreditoTotal.Should().Be(250m);
        result.Value.VigenciaLimiteSaldoDesde.Should().Be(new DateOnly(2024, 2, 1));
        result.Value.VigenciaLimiteCreditoTotalHasta.Should().Be(new DateOnly(2024, 12, 31));
    }
}
