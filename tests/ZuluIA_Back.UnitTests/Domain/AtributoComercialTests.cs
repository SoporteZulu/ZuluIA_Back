using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Comercial;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Domain;

public class AtributoComercialTests
{
    [Fact]
    public void Crear_DebeGuardarTipoDato()
    {
        var atributo = AtributoComercial.Crear("ATTR1", "Atributo", TipoDatoAtributoComercial.Texto, null);
        atributo.TipoDato.Should().Be(TipoDatoAtributoComercial.Texto);
    }

    [Fact]
    public void ValidarValor_NumeroInvalido_DebeLanzarExcepcion()
    {
        var atributo = AtributoComercial.Crear("ATTR1", "Atributo", TipoDatoAtributoComercial.Numero, null);
        var action = () => atributo.ValidarValor("abc");
        action.Should().Throw<InvalidOperationException>();
    }
}
