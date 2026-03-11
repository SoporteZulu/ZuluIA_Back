using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Produccion;

public class FormulaIngrediente : BaseEntity
{
    public long FormulaId { get; private set; }
    public long ItemId { get; private set; }
    public decimal Cantidad { get; private set; }
    public long? UnidadMedidaId { get; private set; }
    public bool EsOpcional { get; private set; }
    public short Orden { get; private set; }

    private FormulaIngrediente() { }

    public static FormulaIngrediente Crear(
        long formulaId,
        long itemId,
        decimal cantidad,
        long? unidadMedidaId,
        bool esOpcional,
        short orden)
    {
        if (cantidad <= 0)
            throw new InvalidOperationException(
                "La cantidad del ingrediente debe ser mayor a 0.");

        return new FormulaIngrediente
        {
            FormulaId      = formulaId,
            ItemId         = itemId,
            Cantidad       = cantidad,
            UnidadMedidaId = unidadMedidaId,
            EsOpcional     = esOpcional,
            Orden          = orden
        };
    }

    public void ActualizarCantidad(decimal cantidad)
    {
        if (cantidad <= 0)
            throw new InvalidOperationException(
                "La cantidad del ingrediente debe ser mayor a 0.");
        Cantidad = cantidad;
    }
}