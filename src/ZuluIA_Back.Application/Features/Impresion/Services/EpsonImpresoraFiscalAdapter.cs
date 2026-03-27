using System.Text;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;
using ZuluIA_Back.Application.Features.Impresion.DTOs;
using ZuluIA_Back.Application.Features.Impresion.Enums;
using ZuluIA_Back.Application.Features.Impresion.Interfaces;

namespace ZuluIA_Back.Application.Features.Impresion.Services;

public class EpsonImpresoraFiscalAdapter : IImpresoraFiscalAdapter
{
    public MarcaImpresoraFiscal Marca => MarcaImpresoraFiscal.Epson;

    public ResultadoImpresionFiscalDto Imprimir(ComprobanteDetalleDto comprobante)
    {
        var sb = new StringBuilder();
        sb.AppendLine("EPSON|ABRIR_COMPROBANTE");
        sb.AppendLine($"NUMERO|{comprobante.NumeroFormateado}");
        sb.AppendLine($"CLIENTE|{comprobante.TerceroRazonSocial}|{comprobante.TerceroCuit}");
        foreach (var item in comprobante.Items)
            sb.AppendLine($"ITEM|{item.Descripcion}|{item.Cantidad:0.####}|{item.PrecioUnitario:0.00}|{item.TotalLinea:0.00}");
        sb.AppendLine($"TOTAL|{comprobante.Total:0.00}");
        sb.AppendLine("EPSON|CERRAR_COMPROBANTE");

        return new ResultadoImpresionFiscalDto
        {
            Marca = Marca.ToString().ToUpperInvariant(),
            ComprobanteId = comprobante.Id,
            NumeroComprobante = comprobante.NumeroFormateado,
            Mensaje = "Comprobante enviado a impresora fiscal Epson.",
            PayloadFiscal = sb.ToString()
        };
    }
}
