using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Comprobantes;

namespace ZuluIA_Back.UnitTests.Domain;

public class ImputacionTests
{
    [Fact]
    public void Crear_ConDatosValidos_DebeCrearActiva()
    {
        var imputacion = Imputacion.Crear(1, 2, 100m, DateOnly.FromDateTime(DateTime.Today), 10);

        imputacion.Anulada.Should().BeFalse();
        imputacion.Importe.Should().Be(100m);
    }

    [Fact]
    public void Desimputar_Activa_DebeMarcarAnulada()
    {
        var imputacion = Imputacion.Crear(1, 2, 100m, DateOnly.FromDateTime(DateTime.Today), 10);

        imputacion.Desimputar(DateOnly.FromDateTime(DateTime.Today), "Reversión", 11);

        imputacion.Anulada.Should().BeTrue();
        imputacion.MotivoDesimputacion.Should().Be("Reversión");
        imputacion.FechaDesimputacion.Should().NotBeNull();
    }

    [Fact]
    public void Desimputar_YaDesimputada_DebeLanzarExcepcion()
    {
        var imputacion = Imputacion.Crear(1, 2, 100m, DateOnly.FromDateTime(DateTime.Today), 10);
        imputacion.Desimputar(DateOnly.FromDateTime(DateTime.Today), null, 11);

        var act = () => imputacion.Desimputar(DateOnly.FromDateTime(DateTime.Today), null, 12);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*ya fue desimputada*");
    }
}
