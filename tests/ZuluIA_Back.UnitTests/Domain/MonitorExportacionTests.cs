using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Integraciones;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Domain;

public class MonitorExportacionTests
{
    [Fact]
    public void Actualizar_DebeGuardarUltimoEstadoYPendientes()
    {
        var monitor = MonitorExportacion.Crear("VENTAS", "Ventas", null);
        monitor.Actualizar(1, EstadoProcesoIntegracion.Finalizado, 3, "ok", null);
        monitor.RegistrosPendientes.Should().Be(3);
        monitor.UltimoEstado.Should().Be(EstadoProcesoIntegracion.Finalizado);
    }
}
