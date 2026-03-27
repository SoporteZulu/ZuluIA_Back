using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Diagnosticos;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Domain;

public class VariableDiagnosticaTests
{
    [Fact]
    public void AgregarOpcion_EnVariableOpcion_DebeAgregarla()
    {
        var variable = VariableDiagnostica.Crear(1, "VAR1", "Variable", TipoVariableDiagnostica.Opcion, true, 1m, null);
        variable.AgregarOpcion(VariableDiagnosticaOpcion.Crear(0, "OP1", "Opción 1", 50m, 1));
        variable.Opciones.Should().HaveCount(1);
    }
}
