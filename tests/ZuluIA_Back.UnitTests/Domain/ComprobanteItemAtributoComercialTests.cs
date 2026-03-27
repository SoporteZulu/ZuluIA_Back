using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Comercial;

namespace ZuluIA_Back.UnitTests.Domain;

public class ComprobanteItemAtributoComercialTests
{
    [Fact]
    public void Crear_DebeAsignarValor()
    {
        var atributo = ComprobanteItemAtributoComercial.Crear(1, 2, "Lote 123", null);
        atributo.Valor.Should().Be("Lote 123");
        atributo.AtributoComercialId.Should().Be(2);
    }
}
