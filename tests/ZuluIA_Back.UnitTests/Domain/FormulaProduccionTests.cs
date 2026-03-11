using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Produccion;

namespace ZuluIA_Back.UnitTests.Domain;

public class FormulaProduccionTests
{
    private static FormulaProduccion CrearFormula() =>
        FormulaProduccion.Crear("FP001", "Fórmula Test", 1, 10m, null, null, null);

    private static FormulaIngrediente CrearIngrediente(long itemId = 1) =>
        FormulaIngrediente.Crear(0, itemId, 2m, null, false, (short)1);

    [Fact]
    public void Crear_ConDatosValidos_DebeCrearFormula()
    {
        var formula = FormulaProduccion.Crear("fp-001", "Fórmula de Prueba", 1, 5m, null, null, null);

        formula.Codigo.Should().Be("FP-001");
        formula.Descripcion.Should().Be("Fórmula de Prueba");
        formula.CantidadResultado.Should().Be(5m);
        formula.Activo.Should().BeTrue();
        formula.Ingredientes.Should().BeEmpty();
    }

    [Fact]
    public void AgregarIngrediente_DebeAgregarIngrediente()
    {
        var formula = CrearFormula();
        var ingrediente = CrearIngrediente();

        formula.AgregarIngrediente(ingrediente);

        formula.Ingredientes.Should().HaveCount(1);
    }

    [Fact]
    public void RemoverIngrediente_Existente_DebeRemoverlo()
    {
        var formula = CrearFormula();
        formula.AgregarIngrediente(CrearIngrediente(itemId: 5));

        formula.RemoverIngrediente(5);

        formula.Ingredientes.Should().BeEmpty();
    }

    [Fact]
    public void Desactivar_FormulaActiva_DebeDesactivarla()
    {
        var formula = CrearFormula();

        formula.Desactivar(null);

        formula.Activo.Should().BeFalse();
    }

    [Fact]
    public void Crear_CodigoConEspacios_DebeNormalizarse()
    {
        var formula = FormulaProduccion.Crear("  fp-test  ", "Fórmula", 1, 1m, null, null, null);

        formula.Codigo.Should().Be("FP-TEST");
    }
}
