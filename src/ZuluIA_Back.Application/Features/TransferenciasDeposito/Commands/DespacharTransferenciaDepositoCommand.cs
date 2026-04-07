using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.TransferenciasDeposito.Commands;

public record DespacharTransferenciaDepositoCommand(long Id) : IRequest<Result>;
