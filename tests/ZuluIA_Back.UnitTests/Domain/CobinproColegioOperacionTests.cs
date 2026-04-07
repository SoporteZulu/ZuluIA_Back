using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Colegio;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Domain;

public class CobinproColegioOperacionTests
{
    [Fact]
    public void Confirmar_DebeCambiarEstadoAConfirmado()
    {
        var operacion = CobinproColegioOperacion.Registrar(1, 2, 3, 4, new DateOnly(2025, 1, 1), 100m, "REF1", null, null);

        operacion.Confirmar("ok", null);

        operacion.Estado.Should().Be(EstadoCobinproColegio.Confirmado);
    }
}
