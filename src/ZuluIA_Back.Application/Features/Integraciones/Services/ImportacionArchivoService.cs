using System.Globalization;
using System.Text;
using MediatR;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;
using ZuluIA_Back.Application.Features.Integraciones.Commands;
using ZuluIA_Back.Application.Features.Ventas.Common;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Integraciones.Services;

public class ImportacionArchivoService(
    ArchivoTabularParserService parser,
    ArchivoImportLayoutProfileService layoutProfileService,
    IMediator mediator)
{
    public async Task<Result<long>> ImportarClientesAsync(string nombreArchivo, string contenidoBase64, bool actualizarExistentes, string? observacion, CancellationToken ct)
    {
        var rows = layoutProfileService.NormalizeRows("CLIENTES", parser.Parse(nombreArchivo, Convert.FromBase64String(contenidoBase64)));
        var clientes = rows.Select(r => new ClienteImportacionInput(
            GetRequired(r, "Legajo"),
            GetRequired(r, "RazonSocial"),
            GetOptional(r, "NombreFantasia"),
            GetLong(r, "TipoDocumentoId"),
            GetRequired(r, "NroDocumento"),
            GetLong(r, "CondicionIvaId"),
            GetBool(r, "EsCliente", true),
            GetBool(r, "EsProveedor"),
            GetBool(r, "EsEmpleado"),
            GetOptional(r, "Calle"),
            GetOptional(r, "Nro"),
            GetOptional(r, "Piso"),
            GetOptional(r, "Dpto"),
            GetOptional(r, "CodigoPostal"),
            GetNullableLong(r, "LocalidadId"),
            GetNullableLong(r, "BarrioId"),
            GetOptional(r, "NroIngresosBrutos"),
            GetOptional(r, "NroMunicipal"),
            GetOptional(r, "Telefono"),
            GetOptional(r, "Celular"),
            GetOptional(r, "Email"),
            GetOptional(r, "Web"),
            GetNullableLong(r, "MonedaId"),
            GetNullableLong(r, "CategoriaId"),
            GetNullableDecimal(r, "LimiteCredito"),
            GetBool(r, "Facturable", true),
            GetNullableLong(r, "CobradorId"),
            GetDecimal(r, "PctComisionCobrador"),
            GetNullableLong(r, "VendedorId"),
            GetDecimal(r, "PctComisionVendedor"),
            GetOptional(r, "Observacion"),
            GetNullableLong(r, "SucursalId"))).ToList();

        return await mediator.Send(new ImportarClientesCommand(clientes, actualizarExistentes, observacion), ct);
    }

    public async Task<Result<long>> ImportarVentasAsync(string nombreArchivo, string contenidoBase64, string? observacion, CancellationToken ct)
    {
        var rows = layoutProfileService.NormalizeRows("VENTAS", parser.Parse(nombreArchivo, Convert.FromBase64String(contenidoBase64)));
        var ventas = rows.GroupBy(r => GetRequired(r, "ReferenciaExterna"), StringComparer.OrdinalIgnoreCase)
            .Select(g =>
            {
                var head = g.First();
                var items = g.Select((r, index) => new ComprobanteItemInput(
                    GetLong(r, "ItemId"),
                    GetOptional(r, "Descripcion"),
                    GetDecimal(r, "Cantidad", 1m),
                    (long)GetDecimal(r, "CantidadBonificada"),
                    (long)GetDecimal(r, "PrecioUnitario"),
                    GetDecimal(r, "DescuentoPct"),
                    GetLong(r, "AlicuotaIvaId"),
                    GetNullableLong(r, "DepositoId"),
                    (short)GetDecimal(r, "Orden", index + 1))).ToList();

                return new VentaImportacionInput(
                    GetRequired(head, "ReferenciaExterna"),
                    GetLong(head, "SucursalId"),
                    GetNullableLong(head, "PuntoFacturacionId"),
                    GetLong(head, "TipoComprobanteId"),
                    GetDate(head, "Fecha"),
                    GetNullableDate(head, "FechaVencimiento"),
                    GetLong(head, "TerceroId"),
                    GetLong(head, "MonedaId"),
                    GetDecimal(head, "Cotizacion", 1m),
                    GetDecimal(head, "Percepciones"),
                    GetOptional(head, "Observacion"),
                    GetNullableLong(head, "ComprobanteOrigenId"),
                    items,
                    GetBool(head, "Emitir", true),
                    ParseEnum(GetOptional(head, "OperacionStock"), OperacionStockVenta.Egreso),
                    ParseEnum(GetOptional(head, "OperacionCuentaCorriente"), OperacionCuentaCorrienteVenta.Debito));
            })
            .ToList();

        return await mediator.Send(new ImportarVentasCommand(ventas, observacion), ct);
    }

    private static string GetRequired(IReadOnlyDictionary<string, string?> row, string key)
        => row.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value.Trim()
            : throw new InvalidOperationException($"La columna '{key}' es obligatoria.");

    private static string? GetOptional(IReadOnlyDictionary<string, string?> row, string key)
        => row.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value.Trim() : null;

    private static long GetLong(IReadOnlyDictionary<string, string?> row, string key)
        => long.Parse(GetRequired(row, key), CultureInfo.InvariantCulture);

    private static long? GetNullableLong(IReadOnlyDictionary<string, string?> row, string key)
        => string.IsNullOrWhiteSpace(GetOptional(row, key)) ? null : long.Parse(GetOptional(row, key)!, CultureInfo.InvariantCulture);

    private static decimal GetDecimal(IReadOnlyDictionary<string, string?> row, string key, decimal defaultValue = 0m)
        => string.IsNullOrWhiteSpace(GetOptional(row, key)) ? defaultValue : decimal.Parse(GetOptional(row, key)!, CultureInfo.InvariantCulture);

    private static decimal? GetNullableDecimal(IReadOnlyDictionary<string, string?> row, string key)
        => string.IsNullOrWhiteSpace(GetOptional(row, key)) ? null : decimal.Parse(GetOptional(row, key)!, CultureInfo.InvariantCulture);

    private static bool GetBool(IReadOnlyDictionary<string, string?> row, string key, bool defaultValue = false)
        => string.IsNullOrWhiteSpace(GetOptional(row, key)) ? defaultValue : bool.Parse(GetOptional(row, key)!);

    private static DateOnly GetDate(IReadOnlyDictionary<string, string?> row, string key)
        => DateOnly.Parse(GetRequired(row, key), CultureInfo.InvariantCulture);

    private static DateOnly? GetNullableDate(IReadOnlyDictionary<string, string?> row, string key)
        => string.IsNullOrWhiteSpace(GetOptional(row, key)) ? null : DateOnly.Parse(GetOptional(row, key)!, CultureInfo.InvariantCulture);

    private static TEnum ParseEnum<TEnum>(string? value, TEnum defaultValue) where TEnum : struct
        => string.IsNullOrWhiteSpace(value) ? defaultValue : Enum.Parse<TEnum>(value, true);
}
