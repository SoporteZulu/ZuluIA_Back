using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Fiscal;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Domain;

public class SalidaRegulatoriaTests
{
    [Fact]
    public void MarcarPresentada_DebeCambiarEstado()
    {
        var salida = SalidaRegulatoria.Crear(TipoSalidaRegulatoria.Hechauka, 1, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 31), "hka.txt", "contenido", null);
        salida.MarcarPresentada(null);
        salida.Estado.Should().Be(EstadoSalidaRegulatoria.Presentada);
    }
}
