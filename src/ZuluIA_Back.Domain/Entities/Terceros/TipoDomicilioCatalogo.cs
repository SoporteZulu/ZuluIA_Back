using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Terceros;

/// <summary>
/// Catálogo legacy del tipo de domicilio usado por `PER_DOMICILIO`.
/// Referencia histórica del combo `TIPODOMICILIO` (`tdom_id`) en `frmCliente`.
/// </summary>
public class TipoDomicilioCatalogo : BaseEntity
{
    public string Descripcion { get; private set; } = string.Empty;

    private TipoDomicilioCatalogo() { }
}
