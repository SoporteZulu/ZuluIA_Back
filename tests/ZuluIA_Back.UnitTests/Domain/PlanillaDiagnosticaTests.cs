using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Diagnosticos;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Domain;

public class PlanillaDiagnosticaTests
{
    [Fact]
    public void Evaluar_DebeCambiarEstadoYResultado()
    {
        var planilla = PlanillaDiagnostica.Crear(1, "Planilla", DateOnly.FromDateTime(DateTime.Today), null, null);
        planilla.Evaluar(82.5m, "ok", null);
        planilla.Estado.Should().Be(EstadoPlanillaDiagnostica.Evaluada);
        planilla.ResultadoTotal.Should().Be(82.5m);
    }
}
