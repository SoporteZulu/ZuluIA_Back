using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.RRHH;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Domain;

public class EmpleadoTests
{
    private static Empleado CrearEmpleado() =>
        Empleado.Crear(1, 1, "EMP001", "Analista", null, DateOnly.FromDateTime(DateTime.Today), 100000m, 1);

    [Fact]
    public void Crear_ConDatosValidos_DebeCrearEmpleadoActivo()
    {
        var empleado = CrearEmpleado();

        empleado.Estado.Should().Be(EstadoEmpleado.Activo);
        empleado.Legajo.Should().Be("EMP001");
        empleado.Cargo.Should().Be("Analista");
        empleado.FechaEgreso.Should().BeNull();
    }

    [Fact]
    public void Crear_LegajoConEspacios_DebeNormalizarse()
    {
        var empleado = Empleado.Crear(1, 1, "  emp-test  ", "Cargo", null,
            DateOnly.FromDateTime(DateTime.Today), 0m, 1);

        empleado.Legajo.Should().Be("EMP-TEST");
    }

    [Fact]
    public void Egresar_EmpleadoActivo_DebeRegistrarEgreso()
    {
        var empleado = CrearEmpleado();
        var fechaEgreso = DateOnly.FromDateTime(DateTime.Today);

        empleado.Egresar(fechaEgreso);

        empleado.Estado.Should().Be(EstadoEmpleado.Inactivo);
        empleado.FechaEgreso.Should().Be(fechaEgreso);
    }

    [Fact]
    public void Actualizar_ConDatosValidos_DebeActualizar()
    {
        var empleado = CrearEmpleado();

        empleado.Actualizar("Desarrollador Senior", "IT", 150000m, 1);

        empleado.Cargo.Should().Be("Desarrollador Senior");
        empleado.Area.Should().Be("IT");
        empleado.SueldoBasico.Should().Be(150000m);
    }

    [Fact]
    public void SuspenderYReactivar_DebeCambiarEstado()
    {
        var empleado = CrearEmpleado();

        empleado.Suspender();
        empleado.Reactivar();

        empleado.Estado.Should().Be(EstadoEmpleado.Activo);
    }
}
