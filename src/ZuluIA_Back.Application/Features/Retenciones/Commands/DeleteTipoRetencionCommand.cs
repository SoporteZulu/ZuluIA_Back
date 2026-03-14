using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Retenciones.Commands;

public record DeleteTipoRetencionCommand(long Id) : IRequest<Result>;
