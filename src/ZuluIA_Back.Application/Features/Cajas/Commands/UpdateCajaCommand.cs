using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Cajas.Commands;

public record UpdateCajaCommand(
    long Id,
    string Descripcion,
    long TipoId,
    long MonedaId,
    bool EsCaja,
    string? Banco,
    string? NroCuenta,
    string? Cbu,
    long? UsuarioId
) : IRequest<Result>;