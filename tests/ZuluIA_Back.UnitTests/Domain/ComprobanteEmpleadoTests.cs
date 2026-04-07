using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.RRHH;

namespace ZuluIA_Back.UnitTests.Domain;

public class ComprobanteEmpleadoTests
{
    [Fact]
    public void Crear_DebeNormalizarNumeroYTipo()
    {
        var comprobante = ComprobanteEmpleado.Crear(1, 2, 3, new DateOnly(2025, 1, 31), "recibo_sueldo", " rec-1 ", 100m, 1, null, null);
        comprobante.Tipo.Should().Be("RECIBO_SUELDO");
        comprobante.Numero.Should().Be("REC-1");
    }

    [Fact]
    public void Crear_TotalCero_DebeLanzarExcepcion()
    {
        var action = () => ComprobanteEmpleado.Crear(1, 2, 3, new DateOnly(2025, 1, 31), "recibo_sueldo", "rec-1", 0m, 1, null, null);
        action.Should().Throw<InvalidOperationException>();
    }
}
