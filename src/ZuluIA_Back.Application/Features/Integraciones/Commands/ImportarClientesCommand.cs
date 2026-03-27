using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Integraciones.Commands;

public record ClienteImportacionInput(
    string Legajo,
    string RazonSocial,
    string? NombreFantasia,
    long TipoDocumentoId,
    string NroDocumento,
    long CondicionIvaId,
    bool EsCliente,
    bool EsProveedor,
    bool EsEmpleado,
    string? Calle,
    string? Nro,
    string? Piso,
    string? Dpto,
    string? CodigoPostal,
    long? LocalidadId,
    long? BarrioId,
    string? NroIngresosBrutos,
    string? NroMunicipal,
    string? Telefono,
    string? Celular,
    string? Email,
    string? Web,
    long? MonedaId,
    long? CategoriaId,
    decimal? LimiteCredito,
    bool Facturable,
    long? CobradorId,
    decimal PctComisionCobrador,
    long? VendedorId,
    decimal PctComisionVendedor,
    string? Observacion,
    long? SucursalId);

public record ImportarClientesCommand(
    IReadOnlyList<ClienteImportacionInput> Clientes,
    bool ActualizarExistentes = true,
    string? Observacion = null) : IRequest<Result<long>>;
