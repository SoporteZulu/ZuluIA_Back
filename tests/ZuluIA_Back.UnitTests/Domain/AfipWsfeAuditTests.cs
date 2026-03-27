using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Domain;

public class AfipWsfeAuditTests
{
    [Fact]
    public void Registrar_DebePersistirOperacionYPayloads()
    {
        var audit = AfipWsfeAudit.Registrar(1, 2, 3, TipoOperacionAfipWsfe.SolicitarCae, true, "req", "res", null, "cae", null, DateOnly.FromDateTime(DateTime.Today), null);

        audit.Operacion.Should().Be(TipoOperacionAfipWsfe.SolicitarCae);
        audit.RequestPayload.Should().Be("req");
        audit.ResponsePayload.Should().Be("res");
        audit.Cae.Should().Be("cae");
    }
}
