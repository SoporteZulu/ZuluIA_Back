using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Cajas.Commands;

public record AbrirCajaCommand(long Id, DateOnly FechaApertura, decimal SaldoInicial) : IRequest<Result>;