using MediatR;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Terceros.Queries;

/// <summary>
/// Obtiene el detalle completo de un tercero por su Id.
/// Equivalente a la apertura del formulario de ABM de Clientes/Proveedores
/// del VB6 al hacer doble click en una fila de la grilla.
/// Retorna Result&lt;TerceroDto&gt; en lugar de TerceroDto? para alinearse
/// con el patrón Result del proyecto y poder distinguir entre
/// "no encontrado" (error semántico) y excepción técnica.
/// </summary>
public record GetTerceroByIdQuery(long Id)
    : IRequest<Result<TerceroDto>>;