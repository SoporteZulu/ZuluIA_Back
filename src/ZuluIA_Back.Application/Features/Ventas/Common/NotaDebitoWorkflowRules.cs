using System.Linq.Expressions;
using ZuluIA_Back.Domain.Entities.Referencia;

namespace ZuluIA_Back.Application.Features.Ventas.Common;

internal static class NotaDebitoWorkflowRules
{
    public static Expression<Func<TipoComprobante, bool>> TipoComprobantePredicate()
        => tipo => tipo.Activo
            && tipo.EsVenta
            && (tipo.Codigo.ToUpper().Contains("ND")
                || tipo.Descripcion.ToUpper().Contains("NOTA DE DEBITO")
                || tipo.Descripcion.ToUpper().Contains("NOTA DE DÉBITO"));

    public static bool EsNotaDebitoVenta(TipoComprobante tipo)
    {
        ArgumentNullException.ThrowIfNull(tipo);

        return tipo.Activo
            && tipo.EsVenta
            && (tipo.Codigo.Contains("ND", StringComparison.OrdinalIgnoreCase)
                || tipo.Descripcion.Contains("NOTA DE DEBITO", StringComparison.OrdinalIgnoreCase)
                || tipo.Descripcion.Contains("NOTA DE DÉBITO", StringComparison.OrdinalIgnoreCase));
    }
}
