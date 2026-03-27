using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.RRHH.Commands;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Application;

public class CambiarEstadoEmpleadoCommandValidatorTests
{
    private readonly CambiarEstadoEmpleadoCommandValidator _validator = new();

    [Fact]
    public void Validar_InactivarSinFecha_DebeTenerError()
    {
        var cmd = new CambiarEstadoEmpleadoCommand(1, EstadoEmpleado.Inactivo);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.FechaEgreso);
    }
}
