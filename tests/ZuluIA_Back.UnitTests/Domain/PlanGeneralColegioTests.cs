using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Colegio;

namespace ZuluIA_Back.UnitTests.Domain;

public class PlanGeneralColegioTests
{
    [Fact]
    public void Crear_DebeNormalizarCodigo()
    {
        var plan = PlanGeneralColegio.Crear(1, 2, 3, 4, 5, " pg-01 ", "Plan", 100m, null, null);
        plan.Codigo.Should().Be("PG-01");
    }

    [Fact]
    public void Actualizar_DebeActualizarDescripcionEImporte()
    {
        var plan = PlanGeneralColegio.Crear(1, 2, 3, 4, 5, "PG-01", "Plan", 100m, null, null);

        plan.Actualizar(6, 7, 8, 9, " pg-02 ", "Plan actualizado", 250m, "Obs", null);

        plan.PlanPagoId.Should().Be(6);
        plan.Codigo.Should().Be("PG-02");
        plan.Descripcion.Should().Be("Plan actualizado");
        plan.ImporteBase.Should().Be(250m);
    }
}
