using MediatR;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Ventas.Commands;

public record UpsertRemitoCotCommand(
    long ComprobanteId,
    string Numero,
    DateOnly FechaVigencia,
    string? Descripcion) : IRequest<Result<ComprobanteCotDto>>;
