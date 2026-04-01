namespace ZuluIA_Back.Application.Features.Cheques.DTOs;

public class ChequePendienteDto
{
    public long Id { get; set; }
    public string NroCheque { get; set; } = string.Empty;
    public string Banco { get; set; } = string.Empty;
    public string? Titular { get; set; }
    public DateOnly FechaEmision { get; set; }
    public DateOnly FechaVencimiento { get; set; }
    public decimal Importe { get; set; }
    public string MonedaSimbolo { get; set; } = "$";
    public long CajaId { get; set; }
    public string CajaDescripcion { get; set; } = string.Empty;
    public long? TerceroId { get; set; }
    public string? TerceroRazonSocial { get; set; }
    public bool EsCruzado { get; set; }
    public bool EsALaOrden { get; set; }
    public int DiasHastaVencimiento { get; set; }
    public bool EstaVencido { get; set; }
}

public class ChequePropio : ChequeDto
{
    public string BancoDescripcion { get; set; } = string.Empty;
    public string NroCuenta { get; set; } = string.Empty;
    public bool EstaAnulado => Estado == "ANULADO";
}

public class ChequeConHistorialDto
{
    public ChequeDto Cheque { get; set; } = null!;
    public IReadOnlyList<ChequeHistorialDto> Historial { get; set; } = [];
}

public class ChequeRutaItemDto
{
    public long Id { get; set; }
    public DateTimeOffset Fecha { get; set; }
    public string Operacion { get; set; } = string.Empty;
    public string EstadoAnterior { get; set; } = string.Empty;
    public string EstadoNuevo { get; set; } = string.Empty;
    public long? ComprobanteId { get; set; }
    public string? ComprobanteNumero { get; set; }
    public string? ComprobanteTipo { get; set; }
    public long? UsuarioId { get; set; }
    public string? UsuarioNombre { get; set; }
    public string? Observacion { get; set; }
}

public class ChequeListadoSimpleDto
{
    public long Id { get; set; }
    public string NroCheque { get; set; } = string.Empty;
    public string Banco { get; set; } = string.Empty;
    public DateOnly FechaVencimiento { get; set; }
    public decimal Importe { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
}
