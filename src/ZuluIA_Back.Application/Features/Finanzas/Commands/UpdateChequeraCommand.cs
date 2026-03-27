using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Finanzas.Commands;

public record UpdateChequeraCommand(
    long Id,
    string Banco,
    string NroCuenta,
    string? Observacion) : IRequest<Result>;