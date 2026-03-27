using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Logistica;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Domain;

public class TransferenciaDepositoTests
{
    [Fact]
    public void Confirmar_ConDetalle_DebeCambiarEstado()
    {
        var transferencia = TransferenciaDeposito.Crear(1, 10, 20, new DateOnly(2025, 1, 1), null, null);
        transferencia.AgregarDetalle(5, 2m);
        transferencia.Confirmar(new DateOnly(2025, 1, 2), null);
        transferencia.Estado.Should().Be(EstadoTransferenciaDeposito.Confirmada);
    }

    [Fact]
    public void VincularOrdenPreparacion_DebeAsignarReferencia()
    {
        var tr = TransferenciaDeposito.Crear(1, 1, 2, new DateOnly(2025, 1, 1), null, null);

        tr.VincularOrdenPreparacion(15, null);

        tr.OrdenPreparacionId.Should().Be(15);
    }
}
