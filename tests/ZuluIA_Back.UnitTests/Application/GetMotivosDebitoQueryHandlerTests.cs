using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Application;

public class GetMotivosDebitoQueryHandlerTests
{
    [Fact]
    public async Task Handle_SoloActivos_DebeRetornarSoloMotivosActivos()
    {
        var activos = MotivoDebito.Crear("NDPRECIO", "Diferencia de precio", false, true, true);
        var inactivo = MotivoDebito.Crear("NDFLETE", "Flete adicional", false, false, true);
        inactivo.Desactivar();

        var db = Substitute.For<IApplicationDbContext>();
        db.MotivosDebito.Returns(MockDbSetHelper.CreateMockDbSet([activos, inactivo]));

        var handler = new ZuluIA_Back.Application.Features.Ventas.Queries.GetMotivosDebitoQueryHandler(db);

        var result = await handler.Handle(new ZuluIA_Back.Application.Features.Ventas.Queries.GetMotivosDebitoQuery(), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("Diferencia de precio", result[0].Descripcion);
    }
}
