using MediatR;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public record ReplaceTerceroVentanasCobranzaCommand(
    long TerceroId,
    IReadOnlyList<ReplaceTerceroVentanaCobranzaItem> Ventanas) : IRequest<Result<IReadOnlyList<TerceroVentanaCobranzaDto>>>;

public record ReplaceTerceroVentanaCobranzaItem(
    long? Id,
    string Dia,
    string? Franja,
    string? Canal,
    string? Responsable,
    bool Principal,
    int? Orden);
