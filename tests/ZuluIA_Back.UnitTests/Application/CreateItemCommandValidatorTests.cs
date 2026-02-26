using FluentValidation.TestHelper;
using Xunit;
using ZuluIA_Back.Application.Features.Items.Commands;

namespace ZuluIA_Back.UnitTests.Application;

public class CreateItemCommandValidatorTests
{
    private readonly CreateItemCommandValidator _validator = new();

    private static CreateItemCommand ComandoValido() => new(
        Codigo: "PROD001",
        CodigoBarras: null,
        Descripcion: "Notebook",
        DescripcionAdicional: null,
        CategoriaId: null,
        UnidadMedidaId: 1,
        AlicuotaIvaId: 1,
        MonedaId: 1,
        EsProducto: true,
        EsServicio: false,
        ManejaStock: true,
        PrecioCosto: 100m,
        PrecioVenta: 200m,
        StockMinimo: 5m,
        StockMaximo: 100m,
        CodigoAfip: null,
        SucursalId: null
    );

    [Fact]
    public void Validar_ComandoValido_NoDebeHaveErrores()
    {
        var result = _validator.TestValidate(ComandoValido());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validar_ProductoYServicio_DebeHaveError()
    {
        var cmd = ComandoValido() with { EsProducto = true, EsServicio = true };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public void Validar_NiProductoNiServicio_DebeHaveError()
    {
        var cmd = ComandoValido() with { EsProducto = false, EsServicio = false };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public void Validar_PrecioCostoNegativo_DebeHaveError()
    {
        var cmd = ComandoValido() with { PrecioCosto = -1m };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.PrecioCosto);
    }

    [Fact]
    public void Validar_StockMaximoMenorQueMinimo_DebeHaveError()
    {
        var cmd = ComandoValido() with { StockMinimo = 100m, StockMaximo = 10m };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.StockMaximo);
    }
}