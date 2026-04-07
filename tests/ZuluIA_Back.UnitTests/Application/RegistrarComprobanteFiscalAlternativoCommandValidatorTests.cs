using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;
using ZuluIA_Back.Application.Features.PuntoVenta.Commands;
using ZuluIA_Back.Application.Features.PuntoVenta.Enums;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Application;

public class RegistrarComprobanteFiscalAlternativoCommandValidatorTests
{
    private readonly RegistrarComprobanteFiscalAlternativoCommandValidator _validator = new();

    [Fact]
    public void Validar_DeuceSinReferencia_DebeTenerError()
    {
        var cmd = new RegistrarComprobanteFiscalAlternativoCommand(
            CanalOperacionPuntoVenta.Pos,
            TipoFlujoFiscalAlternativo.Deuce,
            1,
            1,
            1,
            new DateOnly(2025, 1, 1),
            null,
            1,
            1,
            1m,
            0m,
            null,
            true,
            null,
            [new ComprobanteItemInput(1, "Item", 1m, 0, 100, 0m, 1, null, 1)]);

        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.ReferenciaExterna);
    }
}
