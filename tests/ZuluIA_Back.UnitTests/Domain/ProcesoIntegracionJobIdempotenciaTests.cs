using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Integraciones;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Domain;

public class ProcesoIntegracionJobIdempotenciaTests
{
    [Fact]
    public void Crear_DebeNormalizarClaveIdempotencia()
    {
        var job = ProcesoIntegracionJob.Crear(TipoProcesoIntegracion.FacturacionMasiva, "Facturación", 1, null, null, " lot-001 ");
        job.ClaveIdempotencia.Should().Be("LOT-001");
    }
}
