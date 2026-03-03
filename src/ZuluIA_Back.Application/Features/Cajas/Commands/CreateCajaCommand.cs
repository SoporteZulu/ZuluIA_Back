using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Cajas.Commands;

public record CreateCajaCommand(
    long SucursalId,
    long TipoId,
    string Descripcion,
    long MonedaId,
    bool EsCaja,
    string? Banco,
    string? NroCuenta,
    string? Cbu,
    long? UsuarioId
) : IRequest<Result<long>>;