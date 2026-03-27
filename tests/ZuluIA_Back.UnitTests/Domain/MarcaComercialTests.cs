using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Comercial;

namespace ZuluIA_Back.UnitTests.Domain;

public class MarcaComercialTests
{
    [Fact]
    public void Crear_DebeNormalizarCodigo()
    {
        var marca = MarcaComercial.Crear(" mk-1 ", "Marca", null);
        marca.Codigo.Should().Be("MK-1");
    }
}
