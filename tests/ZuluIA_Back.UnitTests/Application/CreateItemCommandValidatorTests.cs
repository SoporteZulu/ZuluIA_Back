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
        AlicuotaIvaCompraId: null,
        MonedaId: 1,
        EsProducto: true,
        EsServicio: false,
        EsFinanciero: false,
        ManejaStock: true,
        PrecioCosto: 100m,
        PrecioVenta: 200m,
        StockMinimo: 5m,
        StockMaximo: 100m,
        PuntoReposicion: 10m,
        StockSeguridad: 2m,
        Peso: 1.5m,
        Volumen: 0.75m,
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

    [Fact]
    public void Validar_PorcentajeGananciaNegativo_DebeHaveError()
    {
        var cmd = ComandoValido() with { PorcentajeGanancia = -1m };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.PorcentajeGanancia);
    }

    [Fact]
    public void Validar_PorcentajeMaximoDescuentoFueraDeRango_DebeHaveError()
    {
        var cmd = ComandoValido() with { PorcentajeMaximoDescuento = 150m };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.PorcentajeMaximoDescuento);
    }

    [Fact]
    public void Validar_PesoNegativo_DebeHaveError()
    {
        var cmd = ComandoValido() with { Peso = -1m };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Peso);
    }

    [Fact]
    public void Validar_EsTrazableSinStock_DebeHaveError()
    {
        var cmd = ComandoValido() with { ManejaStock = false, EsTrazable = true };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public void Validar_ImpuestoInternoIdCero_DebeHaveError()
    {
        var cmd = ComandoValido() with { ImpuestoInternoId = 0 };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.ImpuestoInternoId);
    }

    [Fact]
    public void Validar_AtributosComercialesDuplicados_DebeHaveError()
    {
        var cmd = ComandoValido() with
        {
            AtributosComerciales =
            [
                new ItemAtributoComercialInput(1, "Rojo"),
                new ItemAtributoComercialInput(1, "Azul")
            ]
        };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public void Validar_ComponentesDuplicados_DebeHaveError()
    {
        var cmd = ComandoValido() with
        {
            Componentes =
            [
                new ItemComponenteInput(10, 1m, 1),
                new ItemComponenteInput(10, 2m, 1)
            ]
        };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveAnyValidationError();
    }
}