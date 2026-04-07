using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Integraciones;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Domain;

public class BatchProgramacionTests
{
    [Fact]
    public void MarcarEjecutada_DebeActualizarProximaEjecucion()
    {
        var inicio = new DateTimeOffset(2025, 1, 1, 10, 0, 0, TimeSpan.Zero);
        var batch = BatchProgramacion.Crear(TipoProcesoIntegracion.FacturacionAutomatica, "Scheduler", 30, inicio, "{}", null, null);

        batch.MarcarEjecutada(inicio, null);

        batch.UltimaEjecucion.Should().Be(inicio);
        batch.ProximaEjecucion.Should().Be(inicio.AddMinutes(30));
    }

    [Fact]
    public void RegistrarError_DebeReprogramarProximaEjecucion()
    {
        var inicio = new DateTimeOffset(2025, 1, 1, 10, 0, 0, TimeSpan.Zero);
        var batch = BatchProgramacion.Crear(TipoProcesoIntegracion.FacturacionAutomatica, "Scheduler", 30, inicio, "{}", null, null);

        batch.RegistrarError(inicio, 15, "error", null);

        batch.UltimaEjecucion.Should().Be(inicio);
        batch.ProximaEjecucion.Should().Be(inicio.AddMinutes(15));
        batch.Observacion.Should().Be("error");
    }
}
