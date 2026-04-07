using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Compras.Commands;

public record EnviarRequisicionCompraCommand(long Id) : IRequest<Result>;

public record AprobarRequisicionCompraCommand(long Id) : IRequest<Result>;

public record RechazarRequisicionCompraCommand(long Id, string? Motivo) : IRequest<Result>;

public record CancelarRequisicionCompraCommand(long Id) : IRequest<Result>;
