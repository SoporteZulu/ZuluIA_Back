using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Produccion;

public class FormulaProduccion : AuditableEntity
{
    public string Codigo { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public long ItemResultadoId { get; private set; }
    public decimal CantidadResultado { get; private set; }
    public long? UnidadMedidaId { get; private set; }
    public bool Activo { get; private set; } = true;
    public string? Observacion { get; private set; }

    private readonly List<FormulaIngrediente> _ingredientes = [];
    public IReadOnlyCollection<FormulaIngrediente> Ingredientes => _ingredientes.AsReadOnly();

    private FormulaProduccion() { }

    public static FormulaProduccion Crear(
        string codigo,
        string descripcion,
        long itemResultadoId,
        decimal cantidadResultado,
        long? unidadMedidaId,
        string? observacion,
        long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        if (cantidadResultado <= 0)
            throw new InvalidOperationException(
                "La cantidad resultado debe ser mayor a 0.");

        var formula = new FormulaProduccion
        {
            Codigo             = codigo.Trim().ToUpperInvariant(),
            Descripcion        = descripcion.Trim(),
            ItemResultadoId    = itemResultadoId,
            CantidadResultado  = cantidadResultado,
            UnidadMedidaId     = unidadMedidaId,
            Observacion        = observacion?.Trim(),
            Activo             = true
        };

        formula.SetCreated(userId);
        return formula;
    }

    public void AgregarIngrediente(FormulaIngrediente ingrediente)
    {
        if (_ingredientes.Any(x => x.ItemId == ingrediente.ItemId))
            throw new InvalidOperationException(
                $"El ítem ID {ingrediente.ItemId} ya existe en la fórmula.");

        _ingredientes.Add(ingrediente);
        SetUpdated(null);
    }

    public void RemoverIngrediente(long itemId)
    {
        var ingrediente = _ingredientes.FirstOrDefault(x => x.ItemId == itemId);
        if (ingrediente is not null)
        {
            _ingredientes.Remove(ingrediente);
            SetUpdated(null);
        }
    }

    public void Actualizar(
        string descripcion,
        decimal cantidadResultado,
        string? observacion,
        long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        if (cantidadResultado <= 0)
            throw new InvalidOperationException(
                "La cantidad resultado debe ser mayor a 0.");

        Descripcion       = descripcion.Trim();
        CantidadResultado = cantidadResultado;
        Observacion       = observacion?.Trim();
        SetUpdated(userId);
    }

    public void Desactivar(long? userId)
    {
        Activo = false;
        SetDeleted();
        SetUpdated(userId);
    }

    public void Activar(long? userId)
    {
        Activo = true;
        SetUpdated(userId);
    }
}