namespace ZuluIA_Back.Domain.Enums;

public enum EstadoCheque
{
    Cartera,
    Depositado,
    Acreditado,
    Rechazado,
    Entregado,
    Anulado,      // Para cheques propios anulados
    Endosado,     // Para cheques endosados a terceros
    EnTransito    // Entre depósito y acreditación
}