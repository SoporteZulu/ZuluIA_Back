using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Colegio;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Domain;

public class LoteColegioTests
{
    [Fact]
    public void MarcarEmitido_DebeCambiarEstadoYCantidad()
    {
        var lote = LoteColegio.Crear(1, "LOTE1", new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 31), null, null);
        lote.MarcarEmitido(3, null);
        lote.Estado.Should().Be(EstadoLoteColegio.Emitido);
        lote.CantidadCedulones.Should().Be(3);
    }

    [Fact]
    public void Actualizar_EnBorrador_DebeCambiarFechas()
    {
        var lote = LoteColegio.Crear(1, "LOTE1", new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 31), null, null);

        lote.Actualizar(new DateOnly(2025, 2, 1), new DateOnly(2025, 2, 28), "Obs", null);

        lote.FechaEmision.Should().Be(new DateOnly(2025, 2, 1));
        lote.FechaVencimiento.Should().Be(new DateOnly(2025, 2, 28));
        lote.Observacion.Should().Be("Obs");
    }

    [Fact]
    public void Cerrar_Emitido_DebeCambiarEstado()
    {
        var lote = LoteColegio.Crear(1, "LOTE1", new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 31), null, null);
        lote.MarcarEmitido(2, null);

        lote.Cerrar("Cierre", null);

        lote.Estado.Should().Be(EstadoLoteColegio.Cerrado);
    }
}
