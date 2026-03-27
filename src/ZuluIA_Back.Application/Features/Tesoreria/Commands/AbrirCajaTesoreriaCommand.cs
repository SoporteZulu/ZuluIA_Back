using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Tesoreria.Commands;

public record AbrirCajaTesoreriaCommand(
    long CajaId,
    DateOnly FechaApertura,
    decimal SaldoInicial,
    string? Observacion
) : IRequest<Result<long>>;
