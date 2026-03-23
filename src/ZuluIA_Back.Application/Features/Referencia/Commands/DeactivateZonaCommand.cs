using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Referencia.Commands;

public record DeactivateZonaCommand(long Id) : IRequest<Result>;