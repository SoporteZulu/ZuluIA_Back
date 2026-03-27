using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

internal static class TerceroFiscalRules
{
    public static async Task<string?> ValidateAsync(
        IApplicationDbContext db,
        long condicionIvaId,
        long tipoDocumentoId,
        string nroDocumento,
        TipoPersoneriaTercero tipoPersoneria,
        string? claveFiscal,
        string? valorClaveFiscal,
        CancellationToken ct)
    {
        var condicionIvaCodigo = await db.CondicionesIva
            .AsNoTracking()
            .Where(x => x.Id == condicionIvaId)
            .Select(x => (short?)x.Codigo)
            .FirstOrDefaultAsync(ct);

        if (!condicionIvaCodigo.HasValue)
            return "La condición fiscal seleccionada no existe.";

        var tipoDocumentoCodigo = await db.TiposDocumento
            .AsNoTracking()
            .Where(x => x.Id == tipoDocumentoId)
            .Select(x => (short?)x.Codigo)
            .FirstOrDefaultAsync(ct);

        if (!tipoDocumentoCodigo.HasValue)
            return "El tipo de documento seleccionado no existe.";

        var digits = new string((nroDocumento ?? string.Empty).Where(char.IsDigit).ToArray());
        var requiereClaveFiscal = condicionIvaCodigo.Value is 1 or 4 or 6;
        var esConsumidorFinal = condicionIvaCodigo.Value == 5;

        if (requiereClaveFiscal)
        {
            if (string.IsNullOrWhiteSpace(claveFiscal) || string.IsNullOrWhiteSpace(valorClaveFiscal))
                return "La condición fiscal seleccionada requiere clave fiscal y valor de clave fiscal.";

            if (string.IsNullOrWhiteSpace(nroDocumento))
                return "Debe especificar un documento para la condición fiscal seleccionada.";

            if (digits.Length != 11)
                return "Para la condición fiscal seleccionada el documento debe tener 11 dígitos.";

            if (tipoDocumentoCodigo.Value is not 80 and not 86)
                return "Para la condición fiscal seleccionada el tipo de documento debe ser CUIT o CUIL.";
        }

        if (esConsumidorFinal && string.IsNullOrWhiteSpace(nroDocumento) && tipoDocumentoCodigo.Value != 99)
            return "Para consumidor final sin documento el tipo de documento debe ser CONSUMIDOR FINAL.";

        if (tipoDocumentoCodigo.Value == 96 && digits.Length == 11)
            return "Un documento de 11 dígitos no puede registrarse como DNI.";

        if (tipoDocumentoCodigo.Value is 80 or 86 && digits.Length > 0 && digits.Length != 11)
            return "Un CUIT/CUIL debe tener 11 dígitos.";

        if (tipoPersoneria == TipoPersoneriaTercero.Juridica && tipoDocumentoCodigo.Value == 96 && !string.IsNullOrWhiteSpace(nroDocumento))
            return "Una persona jurídica no debería identificarse con DNI. Use CUIT/CUIL o CONSUMIDOR FINAL según corresponda.";

        return null;
    }
}
