using MediatR;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public record ReplaceTerceroDomiciliosCommand(
    long TerceroId,
    IReadOnlyList<ReplaceTerceroDomicilioItem> Domicilios) : IRequest<Result<IReadOnlyList<TerceroDomicilioDto>>>;

public record ReplaceTerceroDomicilioItem(
    long? Id,
    long? TipoDomicilioId,
    long? ProvinciaId,
    long? LocalidadId,
    string? Calle,
    string? Barrio,
    string? CodigoPostal,
    string? Observacion,
    bool EsDefecto,
    int? Orden);
