using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Fiscal.Commands;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Application;

public class GenerarSalidaRegulatoriaCommandValidatorTests
{
    private readonly GenerarSalidaRegulatoriaCommandValidator _validator = new();

    [Fact]
    public void Validar_NombreArchivoVacio_DebeTenerError()
    {
        var cmd = new GenerarSalidaRegulatoriaCommand(TipoSalidaRegulatoria.RentasBsAs, 1, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 31), string.Empty);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.NombreArchivo);
    }
}
