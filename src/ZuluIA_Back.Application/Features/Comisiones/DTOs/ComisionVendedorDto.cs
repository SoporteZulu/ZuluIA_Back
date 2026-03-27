namespace ZuluIA_Back.Application.Features.Comisiones.DTOs;

public record ComisionVendedorListDto(
    long Id,
    long SucursalId,
    long VendedorId,
    int Periodo,
    decimal MontoBase,
    decimal PorcentajeComision,
    decimal MontoComision,
    string Estado);

public record ComisionVendedorDto(
    long Id,
    long SucursalId,
    long VendedorId,
    int Periodo,
    decimal MontoBase,
    decimal PorcentajeComision,
    decimal MontoComision,
    string Estado,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
