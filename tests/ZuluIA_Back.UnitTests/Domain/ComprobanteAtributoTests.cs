using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Comprobantes;

namespace ZuluIA_Back.UnitTests.Domain;

public class ComprobanteAtributoTests
{
    [Fact]
    public void Crear_ConClaveVacia_DebeLanzarExcepcion()
    {
        var act = () => ComprobanteAtributo.Crear(1, " ", "valor", "texto", null);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*clave del atributo*");
    }

    [Fact]
    public void ActualizarValor_DebeNormalizarEspacios()
    {
        var atributo = ComprobanteAtributo.Crear(1, "ChoferAuxiliar", "valor", "texto", null);

        atributo.ActualizarValor("  nuevo valor  ", null);

        atributo.Valor.Should().Be("nuevo valor");
    }
}
