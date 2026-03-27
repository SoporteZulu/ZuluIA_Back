using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Integraciones.Commands;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Application;

public class EjecutarSyncIntegracionCommandValidatorTests
{
    private readonly EjecutarSyncIntegracionCommandValidator _validator = new();

    [Fact]
    public void Validar_CodigoMonitorVacio_DebeTenerError()
    {
        var result = _validator.TestValidate(new EjecutarSyncIntegracionCommand(TipoProcesoIntegracion.Sincronizacion, string.Empty, "desc", null));
        result.ShouldHaveValidationErrorFor(x => x.CodigoMonitor);
    }

    [Fact]
    public void Validar_ClaveIdempotenciaMuyLarga_DebeTenerError()
    {
        var cmd = new EjecutarSyncIntegracionCommand(
            TipoProcesoIntegracion.Sincronizacion,
            "SYNC_VENTAS",
            "desc",
            null,
            new string('A', 101));

        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.ClaveIdempotencia);
    }
}
