using Microsoft.AspNetCore.Routing;
using System.Text.RegularExpressions;

namespace ZuluIA_Back.Api.Utilities;

/// <summary>
/// Converts PascalCase controller names to kebab-case route tokens.
/// e.g.  OrdenesTrabajosController → api/ordenes-trabajos
///       PlanCuentasController     → api/plan-cuentas
/// </summary>
public partial class KebabCaseRouteTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value)
    {
        if (value is not string s) return null;
        return CamelCaseToKebab().Replace(s, "$1-$2").ToLowerInvariant();
    }

    [GeneratedRegex("([a-z])([A-Z])")]
    private static partial Regex CamelCaseToKebab();
}
