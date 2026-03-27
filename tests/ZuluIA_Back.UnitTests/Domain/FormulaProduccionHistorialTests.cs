using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Produccion;

namespace ZuluIA_Back.UnitTests.Domain;

public class FormulaProduccionHistorialTests
{
    [Fact]
    public void Registrar_DebeGuardarVersionYSnapshot()
    {
        var historial = FormulaProduccionHistorial.Registrar(1, 2, "FP1", "Formula", 10m, "{ }", "update", null);

        historial.Version.Should().Be(2);
        historial.Motivo.Should().Be("update");
    }
}
