using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Reportes.Commands;
using ZuluIA_Back.Application.Features.Reportes.Enums;

namespace ZuluIA_Back.UnitTests.Application;

public class ExportarReporteCommandValidatorTests
{
    private readonly ExportarReporteCommandValidator _validator = new();

    [Fact]
    public void Validar_ContableSinEjercicio_DebeTenerError()
    {
        var cmd = new ExportarReporteCommand(TipoReporteParametrizado.Contable, FormatoExportacionReporte.Csv, null, null, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 31));
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.EjercicioId);
    }
}
