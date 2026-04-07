using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Referencia.Commands;

public record ActivateJurisdiccionCommand(long Id) : IRequest<Result>;