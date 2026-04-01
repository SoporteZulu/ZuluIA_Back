using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Logistica;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Domain;

public class TransferenciaDepositoTests
{
    [Fact]
    public void Crear_ConFechaDefault_LanzaExcepcion()
    {
        var act = () => TransferenciaDeposito.Crear(1, 10, 20, default, null, null);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*fecha*");
    }

    [Fact]
    public void Despachar_ConDetalle_DebeCambiarEstado()
    {
        var transferencia = TransferenciaDeposito.Crear(1, 10, 20, new DateOnly(2025, 1, 1), null, null);
        transferencia.AgregarDetalle(5, 2m);
        var fechaDespacho = new DateOnly(2025, 1, 2);

        transferencia.Despachar(fechaDespacho, null);

        transferencia.Estado.Should().Be(EstadoTransferenciaDeposito.EnTransito);
        transferencia.FechaDespacho.Should().Be(fechaDespacho);
    }

    [Fact]
    public void Confirmar_DesdeEnTransito_DebeCambiarEstado()
    {
        var transferencia = TransferenciaDeposito.Crear(1, 10, 20, new DateOnly(2025, 1, 1), null, null);
        transferencia.AgregarDetalle(5, 2m);
        transferencia.Despachar(new DateOnly(2025, 1, 2), null);
        transferencia.Confirmar(new DateOnly(2025, 1, 2), null);

        transferencia.Estado.Should().Be(EstadoTransferenciaDeposito.Confirmada);
    }

    [Fact]
    public void Confirmar_ConFechaAnteriorALaTransferencia_LanzaExcepcion()
    {
        var transferencia = TransferenciaDeposito.Crear(1, 10, 20, new DateOnly(2025, 1, 2), null, null);
        transferencia.AgregarDetalle(5, 2m);
        transferencia.Despachar(new DateOnly(2025, 1, 2), null);

        var act = () => transferencia.Confirmar(new DateOnly(2025, 1, 1), null);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*confirmación*anterior*");
    }

    [Fact]
    public void VincularOrdenPreparacion_DebeAsignarReferencia()
    {
        var tr = TransferenciaDeposito.Crear(1, 1, 2, new DateOnly(2025, 1, 1), null, null);

        tr.VincularOrdenPreparacion(15, null);

        tr.OrdenPreparacionId.Should().Be(15);
    }

    [Fact]
    public void VincularOrdenPreparacion_EnTransito_LanzaExcepcion()
    {
        var tr = TransferenciaDeposito.Crear(1, 1, 2, new DateOnly(2025, 1, 1), null, null);
        tr.AgregarDetalle(5, 2m);
        tr.Despachar(new DateOnly(2025, 1, 2), null);

        var act = () => tr.VincularOrdenPreparacion(15, null);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*borrador*");
    }

    [Fact]
    public void Anular_Confirmada_LanzaExcepcion()
    {
        var transferencia = TransferenciaDeposito.Crear(1, 10, 20, new DateOnly(2025, 1, 1), null, null);
        transferencia.AgregarDetalle(5, 2m);
        transferencia.Despachar(new DateOnly(2025, 1, 2), null);
        transferencia.Confirmar(new DateOnly(2025, 1, 2), null);

        var act = () => transferencia.Anular(null);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*confirmada*");
    }
}
