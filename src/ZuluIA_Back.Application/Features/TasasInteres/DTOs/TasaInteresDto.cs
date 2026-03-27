namespace ZuluIA_Back.Application.Features.TasasInteres.DTOs;

public record TasaInteresDto(
    long Id,
    string Descripcion,
    decimal TasaMensual,
    DateOnly FechaDesde,
    DateOnly? FechaHasta,
    bool Activo);
