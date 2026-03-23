using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public record UpdatePuntoFacturacionCommand(long Id, long TipoId, string Descripcion) : IRequest<Result>;