using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Contratos;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Domain;

public class ContratoTests
{
    [Fact]
    public void Renovar_DebeActualizarPeriodoYEstado()
    {
        var contrato = Contrato.Crear(1, 1, 1, "CT-1", "Contrato", new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 31), 100m, false, null, null);
        contrato.Renovar(new DateOnly(2025, 2, 28), 120m, null, null);
        contrato.Estado.Should().Be(EstadoContrato.Renovado);
        contrato.Importe.Should().Be(120m);
        contrato.FechaInicio.Should().Be(new DateOnly(2025, 2, 1));
    }

    [Fact]
    public void Finalizar_DebeCambiarEstado()
    {
        var contrato = Contrato.Crear(1, 1, 1, "CT-1", "Contrato", new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 31), 100m, false, null, null);

        contrato.Finalizar("fin", null);

        contrato.Estado.Should().Be(EstadoContrato.Finalizado);
    }
}
