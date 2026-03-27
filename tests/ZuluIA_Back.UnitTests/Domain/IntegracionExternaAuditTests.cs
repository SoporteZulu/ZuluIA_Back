using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Integraciones;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Domain;

public class IntegracionExternaAuditTests
{
    [Fact]
    public void Registrar_DebeGuardarClasificacionYAmbiente()
    {
        var audit = IntegracionExternaAudit.Registrar(
            ProveedorIntegracionExterna.AfipWsfe,
            "solicitar_cae",
            "COMPROBANTE",
            10,
            false,
            1,
            15000,
            false,
            250,
            "TEST",
            "https://test.afip.local/wsfe",
            "TIMEOUT",
            false,
            "{}",
            "{}",
            "timeout",
            null);

        audit.Operacion.Should().Be("SOLICITAR_CAE");
        audit.Ambiente.Should().Be("TEST");
        audit.CodigoError.Should().Be("TIMEOUT");
        audit.ErrorFuncional.Should().BeFalse();
    }
}
