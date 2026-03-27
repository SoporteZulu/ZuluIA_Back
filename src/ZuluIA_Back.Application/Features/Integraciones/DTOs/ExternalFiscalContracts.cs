namespace ZuluIA_Back.Application.Features.Integraciones.DTOs;

public sealed record AfipWsfeRequestContract(
    long ComprobanteId,
    long SucursalId,
    long? PuntoFacturacionId,
    short PuntoNumero,
    long TipoComprobanteId,
    decimal Total,
    DateOnly Fecha,
    string CuitEmisor,
    string? CertificadoAlias,
    string Ambiente,
    string Endpoint,
    string Operacion);

public sealed record CtgSolicitudRequestContract(
    long CartaPorteId,
    long? ComprobanteId,
    string CuitRemitente,
    string CuitDestinatario,
    string? CuitTransportista,
    string Ambiente,
    string Endpoint,
    DateOnly FechaSolicitud,
    string? Observacion,
    bool EsReintento);

public sealed record CtgConsultaRequestContract(
    long CartaPorteId,
    string? NroCtgActual,
    string Ambiente,
    string Endpoint,
    DateOnly FechaConsulta,
    string? NroCtg,
    string? Error,
    string? Observacion);

public sealed record CtgSolicitudResponseContract(bool Exitoso, string Estado, string? Mensaje, string? CodigoError = null);
public sealed record CtgConsultaResponseContract(string? NroCtg, string? Error, string? Observacion, string? CodigoError = null);

public sealed record SifenProcesarRequestContract(
    long ComprobanteId,
    long SucursalId,
    long PuntoFacturacionId,
    long? TimbradoFiscalId,
    string? CodigoSeguridad,
    string Ambiente,
    string Endpoint,
    string? Observacion);

public sealed record SifenProcesarResponseContract(bool Confirmada, string CodigoSeguridad, string ResponsePayload, string? Mensaje, string? CodigoError);

public sealed record DeuceProcesarRequestContract(
    long ComprobanteId,
    long SucursalId,
    long PuntoFacturacionId,
    string ReferenciaExterna,
    string Ambiente,
    string Endpoint,
    string? Observacion);

public sealed record DeuceProcesarResponseContract(bool Confirmada, string ReferenciaExterna, string ResponsePayload, string? Mensaje, string? CodigoError);
