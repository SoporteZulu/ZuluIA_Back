using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.TransferenciasDeposito.Commands;

public record AnularTransferenciaDepositoCommand(long Id) : IRequest<Result>;
