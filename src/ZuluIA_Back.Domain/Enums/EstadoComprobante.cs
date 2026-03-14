namespace ZuluIA_Back.Domain.Enums;

public enum EstadoComprobante
{
    Borrador,
    Emitido,
    PagadoParcial,
    Pagado,
    Anulado,
    /// <summary>Presupuesto que fue convertido a comprobante definitivo.</summary>
    Convertido
}