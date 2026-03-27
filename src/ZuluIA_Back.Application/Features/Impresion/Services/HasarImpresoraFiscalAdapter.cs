using System.Text;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;
using ZuluIA_Back.Application.Features.Impresion.DTOs;
using ZuluIA_Back.Application.Features.Impresion.Enums;
using ZuluIA_Back.Application.Features.Impresion.Interfaces;

namespace ZuluIA_Back.Application.Features.Impresion.Services;

public class HasarImpresoraFiscalAdapter : IImpresoraFiscalAdapter
{
    public MarcaImpresoraFiscal Marca => MarcaImpresoraFiscal.Hasar;

    public ResultadoImpresionFiscalDto Imprimir(ComprobanteDetalleDto comprobante)
    {
        var sb = new StringBuilder();
        sb.AppendLine("HASAR|INICIO");
        sb.AppendLine($"DOCUMENTO|{comprobante.NumeroFormateado}|{comprobante.Fecha:yyyy-MM-dd}");
        sb.AppendLine($"RECEPTOR|{comprobante.TerceroRazonSocial}|{comprobante.TerceroCuit}");
        foreach (var item in comprobante.Items)
            sb.AppendLine($"DETALLE|{item.ItemCodigo}|{item.Descripcion}|{item.Cantidad:0.####}|{item.TotalLinea:0.00}");
        sb.AppendLine($"IMPORTE_TOTAL|{comprobante.Total:0.00}");
        sb.AppendLine("HASAR|FIN");

        return new ResultadoImpresionFiscalDto
        {
            Marca = Marca.ToString().ToUpperInvariant(),
            ComprobanteId = comprobante.Id,
            NumeroComprobante = comprobante.NumeroFormateado,
            Mensaje = "Comprobante enviado a impresora fiscal Hasar.",
            PayloadFiscal = sb.ToString()
        };
    }
}
