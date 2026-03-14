using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.OrdenesPreparacion.DTOs;

public class OrdenPreparacionDto
{
    public long Id { get; init; }
    public long SucursalId { get; init; }
    public long? ComprobanteOrigenId { get; init; }
    public long? TerceroId { get; init; }
    public DateOnly Fecha { get; init; }
    public EstadoOrdenPreparacion Estado { get; init; }
    public string? Observacion { get; init; }
    public DateOnly? FechaConfirmacion { get; init; }
    public IReadOnlyList<OrdenPreparacionDetalleDto> Detalles { get; init; } = [];
}

public class OrdenPreparacionListDto
{
    public long Id { get; init; }
    public long SucursalId { get; init; }
    public long? TerceroId { get; init; }
    public long? ComprobanteOrigenId { get; init; }
    public DateOnly Fecha { get; init; }
    public EstadoOrdenPreparacion Estado { get; init; }
    public string? Observacion { get; init; }
    public DateOnly? FechaConfirmacion { get; init; }
    public int CantidadRenglones { get; init; }
}

public record OrdenPreparacionDetalleDto(
    long Id,
    long ItemId,
    long DepositoId,
    decimal Cantidad,
    decimal CantidadEntregada,
    bool EstaCompleto,
    string? Observacion
);

public record CreateOrdenPreparacionDetalleDto(
    long ItemId,
    long DepositoId,
    decimal Cantidad,
    string? Observacion
);
