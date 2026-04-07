using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Finanzas.Commands;

public record ActivateTarjetaTipoCommand(long Id) : IRequest<Result>;
