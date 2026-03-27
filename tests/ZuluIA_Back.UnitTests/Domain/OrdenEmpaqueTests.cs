using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Produccion;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Domain;

public class OrdenEmpaqueTests
{
    [Fact]
    public void Crear_DebeQuedarPendiente()
    {
        var orden = OrdenEmpaque.Crear(1, 2, 3, DateOnly.FromDateTime(DateTime.Today), 5m, "L1", null, null);

        orden.Estado.Should().Be(EstadoOrdenEmpaque.Pendiente);
    }
}
