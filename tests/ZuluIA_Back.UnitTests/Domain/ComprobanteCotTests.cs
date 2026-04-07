using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Comprobantes;

namespace ZuluIA_Back.UnitTests.Domain;

public class ComprobanteCotTests
{
    [Fact]
    public void Crear_ConNumeroCorto_DebeLanzarExcepcion()
    {
        var act = () => ComprobanteCot.Crear(1, "12345", new DateOnly(2026, 3, 31), new DateOnly(2026, 3, 30), null, null);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*6 caracteres*");
    }

    [Fact]
    public void Crear_ConFechaAnteriorALaEmision_DebeLanzarExcepcion()
    {
        var act = () => ComprobanteCot.Crear(1, "123456", new DateOnly(2026, 3, 29), new DateOnly(2026, 3, 30), null, null);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*fecha de emisión del remito*");
    }
}
