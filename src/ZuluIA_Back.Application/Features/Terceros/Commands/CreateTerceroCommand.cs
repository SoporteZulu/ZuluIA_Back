using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public record CreateTerceroCommand(
    string Legajo,
    string RazonSocial,
    string? NombreFantasia,
    long TipoDocumentoId,
    string NroDocumento,
    long CondicionIvaId,
    long? CategoriaId,
    bool EsCliente,
    bool EsProveedor,
    string? Calle,
    string? Nro,
    string? CodigoPostal,
    long? LocalidadId,
    long? BarrioId,
    string? Telefono,
    string? Celular,
    string? Email,
    string? Web,
    long? MonedaId,
    decimal? LimiteCredito,
    long? SucursalId
) : IRequest<Result<long>>;