namespace ZuluIA_Back.Application.Features.Produccion.DTOs;

public class FormulaProduccionDto
{
    public long Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public long ItemResultadoId { get; set; }
    public string ItemResultadoCodigo { get; set; } = string.Empty;
    public string ItemResultadoDescripcion { get; set; } = string.Empty;
    public decimal CantidadResultado { get; set; }
    public long? UnidadMedidaId { get; set; }
    public string? UnidadMedidaDescripcion { get; set; }
    public bool Activo { get; set; }
    public string? Observacion { get; set; }
    public IReadOnlyList<FormulaIngredienteDto> Ingredientes { get; set; } = [];
}

public class FormulaIngredienteDto
{
    public long Id { get; set; }
    public long ItemId { get; set; }
    public string ItemCodigo { get; set; } = string.Empty;
    public string ItemDescripcion { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public long? UnidadMedidaId { get; set; }
    public string? UnidadMedidaDescripcion { get; set; }
    public bool EsOpcional { get; set; }
    public short Orden { get; set; }
}