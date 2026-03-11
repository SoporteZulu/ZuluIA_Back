using MediatR;
using ZuluIA_Back.Application.Features.Facturacion.DTOs;

namespace ZuluIA_Back.Application.Features.Facturacion.Queries;

public enum TipoLibroIva { Ventas, Compras }

public record GetLibroIvaQuery(
    long SucursalId,
    DateOnly Desde,
    DateOnly Hasta,
    TipoLibroIva Tipo)
    : IRequest<LibroIvaDto>;