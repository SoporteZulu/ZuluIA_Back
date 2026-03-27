using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Domain;

public class ChequeHistorialTests
{
    [Fact]
    public void Registrar_DebePersistirOperacionYEstados()
    {
        var historial = ChequeHistorial.Registrar(
            1, 2, 3,
            TipoOperacionCheque.Entrega,
            EstadoCheque.Cartera,
            EstadoCheque.Entregado,
            DateOnly.FromDateTime(DateTime.Today),
            null,
            "Entrega a tercero",
            99);

        historial.Operacion.Should().Be(TipoOperacionCheque.Entrega);
        historial.EstadoAnterior.Should().Be(EstadoCheque.Cartera);
        historial.EstadoNuevo.Should().Be(EstadoCheque.Entregado);
        historial.TerceroId.Should().Be(3);
    }
}
