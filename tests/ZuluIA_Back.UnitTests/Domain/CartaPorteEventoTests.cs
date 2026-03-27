using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Domain;

public class CartaPorteEventoTests
{
    [Fact]
    public void Registrar_DebeGuardarEventoYEstado()
    {
        var evento = CartaPorteEvento.Registrar(
            1,
            2,
            TipoEventoCartaPorte.CtgError,
            EstadoCartaPorte.CtgSolicitado,
            EstadoCartaPorte.CtgError,
            DateOnly.FromDateTime(DateTime.Today),
            "AFIP no disponible",
            null,
            2,
            7);

        evento.TipoEvento.Should().Be(TipoEventoCartaPorte.CtgError);
        evento.EstadoAnterior.Should().Be(EstadoCartaPorte.CtgSolicitado);
        evento.EstadoNuevo.Should().Be(EstadoCartaPorte.CtgError);
        evento.IntentoCtg.Should().Be(2);
    }
}
