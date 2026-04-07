namespace ZuluIA_Back.Application.Features.Terceros.Queries;

internal static class TerceroUbicacionFormatter
{
    public static string BuildGeografiaCompleta(
        string? provinciaDescripcion,
        string? localidadDescripcion,
        string? barrioDescripcion)
    {
        var partes = new[] { barrioDescripcion, localidadDescripcion, provinciaDescripcion }
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!.Trim())
            .ToList();

        return partes.Count == 0 ? string.Empty : string.Join(" / ", partes);
    }

    public static string BuildUbicacionCompleta(
        string? direccionCompleta,
        string? geografiaCompleta,
        string? codigoPostal)
    {
        var partes = new List<string>();

        if (!string.IsNullOrWhiteSpace(direccionCompleta))
            partes.Add(direccionCompleta.Trim());

        if (!string.IsNullOrWhiteSpace(geografiaCompleta))
            partes.Add(geografiaCompleta.Trim());

        if (!string.IsNullOrWhiteSpace(codigoPostal))
            partes.Add($"CP {codigoPostal.Trim()}");

        return partes.Count == 0 ? string.Empty : string.Join(" - ", partes);
    }
}
