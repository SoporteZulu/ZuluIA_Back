using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Miembros.Commands;

public record CreateMiembroCommand(
    string Legajo,
    string Nombre,
    long TipoDocumentoId,
    string NroDocumento,
    long CondicionIvaId,
    long? SucursalId) : IRequest<Result<long>>;