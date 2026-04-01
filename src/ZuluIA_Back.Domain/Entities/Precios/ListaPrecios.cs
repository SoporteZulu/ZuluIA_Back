using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Precios;

public class ListaPrecios : AuditableEntity
{
    public string Descripcion { get; private set; } = string.Empty;
    public long MonedaId { get; private set; }
    public DateOnly? VigenciaDesde { get; private set; }
    public DateOnly? VigenciaHasta { get; private set; }
    public bool Activa { get; private set; } = true;
    public bool EsPorDefecto { get; private set; }
    public long? ListaPadreId { get; private set; }
    public int Prioridad { get; private set; }
    public string? Observaciones { get; private set; }

    private readonly List<ListaPreciosItem> _items = [];
    public IReadOnlyCollection<ListaPreciosItem> Items => _items.AsReadOnly();

    public ListaPrecios? ListaPadre { get; private set; }

    private ListaPrecios() { }

    public static ListaPrecios Crear(
        string descripcion,
        long monedaId,
        DateOnly? vigenciaDesde,
        DateOnly? vigenciaHasta,
        long? userId,
        bool esPorDefecto = false,
        long? listaPadreId = null,
        int prioridad = 0,
        string? observaciones = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        if (vigenciaDesde.HasValue && vigenciaHasta.HasValue
            && vigenciaHasta < vigenciaDesde)
            throw new InvalidOperationException(
                "La fecha de vigencia hasta no puede ser anterior a la fecha de vigencia desde.");

        if (prioridad < 0)
            throw new ArgumentException("La prioridad no puede ser negativa.", nameof(prioridad));

        var lista = new ListaPrecios
        {
            Descripcion    = descripcion.Trim(),
            MonedaId       = monedaId,
            VigenciaDesde  = vigenciaDesde,
            VigenciaHasta  = vigenciaHasta,
            EsPorDefecto   = esPorDefecto,
            ListaPadreId   = listaPadreId,
            Prioridad      = prioridad,
            Observaciones  = observaciones?.Trim(),
            Activa         = true
        };

        lista.SetCreated(userId);
        return lista;
    }

    public void Actualizar(
        string descripcion,
        long monedaId,
        DateOnly? vigenciaDesde,
        DateOnly? vigenciaHasta,
        long? userId,
        bool esPorDefecto = false,
        long? listaPadreId = null,
        int prioridad = 0,
        string? observaciones = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        if (vigenciaDesde.HasValue && vigenciaHasta.HasValue
            && vigenciaHasta < vigenciaDesde)
            throw new InvalidOperationException(
                "La fecha de vigencia hasta no puede ser anterior a la fecha de vigencia desde.");

        if (prioridad < 0)
            throw new ArgumentException("La prioridad no puede ser negativa.", nameof(prioridad));

        if (listaPadreId.HasValue && listaPadreId.Value == Id)
            throw new InvalidOperationException("Una lista no puede ser su propia lista padre.");

        Descripcion   = descripcion.Trim();
        MonedaId      = monedaId;
        VigenciaDesde = vigenciaDesde;
        VigenciaHasta = vigenciaHasta;
        EsPorDefecto  = esPorDefecto;
        ListaPadreId  = listaPadreId;
        Prioridad     = prioridad;
        Observaciones = observaciones?.Trim();
        SetUpdated(userId);
    }

    /// <summary>
    /// Agrega o actualiza el precio de un ítem en la lista.
    /// Si el ítem ya existe, actualiza precio y descuento.
    /// Si no existe, lo agrega.
    /// </summary>
    public void UpsertItem(long itemId, decimal precio, decimal descuentoPct)
    {
        if (precio < 0)
            throw new InvalidOperationException("El precio no puede ser negativo.");

        if (descuentoPct < 0 || descuentoPct > 100)
            throw new InvalidOperationException("El descuento debe estar entre 0 y 100.");

        var existente = _items.FirstOrDefault(x => x.ItemId == itemId);

        if (existente is not null)
            existente.ActualizarPrecio(precio, descuentoPct);
        else
            _items.Add(ListaPreciosItem.Crear(Id, itemId, precio, descuentoPct));
    }

    /// <summary>
    /// Elimina un ítem de la lista por itemId.
    /// Retorna false si el ítem no existía.
    /// </summary>
    public bool RemoverItem(long itemId)
    {
        var item = _items.FirstOrDefault(x => x.ItemId == itemId);
        if (item is null) return false;
        _items.Remove(item);
        return true;
    }

    /// <summary>
    /// Obtiene el precio de un ítem, o null si no está en la lista.
    /// </summary>
    public ListaPreciosItem? ObtenerPrecioItem(long itemId) =>
        _items.FirstOrDefault(x => x.ItemId == itemId);

    public void Desactivar(long? userId)
    {
        Activa = false;
        SetDeleted();
        SetUpdated(userId);
    }

    public void Activar(long? userId)
    {
        Activa = true;
        SetUpdated(userId);
    }

    /// <summary>
    /// Indica si la lista está vigente en una fecha dada.
    /// </summary>
    public bool EstaVigente(DateOnly fecha)
    {
        if (!Activa) return false;
        if (VigenciaDesde.HasValue && fecha < VigenciaDesde.Value) return false;
        if (VigenciaHasta.HasValue && fecha > VigenciaHasta.Value) return false;
        return true;
    }
}