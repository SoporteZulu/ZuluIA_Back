using ZuluIA_Back.Application.Features.Comprobantes.DTOs;
using ZuluIA_Back.Application.Features.Impresion.DTOs;
using ZuluIA_Back.Application.Features.Impresion.Enums;

namespace ZuluIA_Back.Application.Features.Impresion.Interfaces;

public interface IImpresoraFiscalAdapter
{
    MarcaImpresoraFiscal Marca { get; }
    ResultadoImpresionFiscalDto Imprimir(ComprobanteDetalleDto comprobante);
}
