using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Integraciones;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Domain;

public class ImpresionSpoolTrabajoTests
{
    [Fact]
    public void Completar_DebeMarcarTrabajoComoCompletado()
    {
        var trabajo = ImpresionSpoolTrabajo.Encolar(1, "FISCAL", "EPSON", null);
        trabajo.MarcarProcesando(null);
        trabajo.Completar("payload", null);

        trabajo.Estado.Should().Be(EstadoImpresionSpool.Completado);
        trabajo.PayloadGenerado.Should().Be("payload");
    }

    [Fact]
    public void Reencolar_DebeVolverAPendiente()
    {
        var trabajo = ImpresionSpoolTrabajo.Encolar(1, "FISCAL", "EPSON", null);
        trabajo.MarcarProcesando(null);
        trabajo.Fallar("error", null, null);

        trabajo.Reencolar(DateTimeOffset.UtcNow, null);

        trabajo.Estado.Should().Be(EstadoImpresionSpool.Pendiente);
        trabajo.MensajeError.Should().BeNull();
        trabajo.ProximoIntento.Should().NotBeNull();
    }
}
