using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Contratos.Commands;

public record CancelarContratoCommand(long Id, string? Observacion) : IRequest<Result>;
