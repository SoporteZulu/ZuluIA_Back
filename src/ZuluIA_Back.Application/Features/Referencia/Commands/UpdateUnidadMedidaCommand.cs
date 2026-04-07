using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Referencia.Commands;

public record UpdateUnidadMedidaCommand(
    long Id,
    string Descripcion,
    string? Disminutivo,
    decimal Multiplicador,
    bool EsUnidadBase,
    long? UnidadBaseId) : IRequest<Result>;