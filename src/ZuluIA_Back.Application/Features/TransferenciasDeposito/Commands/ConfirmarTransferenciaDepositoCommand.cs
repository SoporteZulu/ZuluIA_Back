using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.TransferenciasDeposito.Commands;

public record ConfirmarTransferenciaDepositoCommand(long Id) : IRequest<Result>;
