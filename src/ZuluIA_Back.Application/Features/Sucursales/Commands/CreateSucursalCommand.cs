using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Sucursales.Commands;

public record CreateSucursalCommand(
    string RazonSocial,
    string? NombreFantasia,
    string Cuit,
    string? NroIngresosBrutos,
    long CondicionIvaId,
    long MonedaId,
    long PaisId,
    string? Calle,
    string? Nro,
    string? Piso,
    string? Dpto,
    string? CodigoPostal,
    long? LocalidadId,
    long? BarrioId,
    string? Telefono,
    string? Email,
    string? Web,
    string? Cbu,
    string? AliasCbu,
    string? Cai,
    short PuertoAfip,
    bool CasaMatriz
) : IRequest<Result<long>>;