using MediatR;
using ZuluIA_Back.Application.Features.Impresion.DTOs;
using ZuluIA_Back.Application.Features.Impresion.Enums;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Impresion.Commands;

public record ImprimirComprobanteFiscalCommand(long ComprobanteId, MarcaImpresoraFiscal Marca) : IRequest<Result<ResultadoImpresionFiscalDto>>;
