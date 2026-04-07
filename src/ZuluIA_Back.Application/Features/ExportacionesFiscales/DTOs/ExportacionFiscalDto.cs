namespace ZuluIA_Back.Application.Features.ExportacionesFiscales.DTOs;

public record CitiVentasLineaDto(
    string FechaComprobante,
    string TipoComprobante,
    string PuntoVenta,
    string NroComprobante,
    string NroComprobanteHasta,
    string CodigoDocumento,
    string NroDocumento,
    string Cognombre,
    decimal ImporteTotal,
    decimal ImporteTotalConceptoNoGravado,
    decimal PercepcionNoCategorizados,
    decimal ImporteExento,
    decimal ImportePercepcionesNacionales,
    decimal ImportePercepcionesIibb,
    decimal ImportePercepcionesMunicipales,
    decimal ImporteImpuestosInternos,
    string CodigoMoneda,
    decimal TipoCambio,
    int CantidadAlicuotasIva,
    string CodigoOperacion,
    decimal OtrosTributos,
    string FechaVtoCpte);

public record CitiComprasLineaDto(
    string FechaComprobante,
    string TipoComprobante,
    string PuntoVenta,
    string NroComprobante,
    string CodigoDocDespacho,
    string NroDespachoImportacion,
    string CodigoDocumentoVendedor,
    string NroIdVendedor,
    string Cognombre,
    decimal ImporteTotal,
    decimal ImporteTotalConceptoNoGravado,
    decimal ImporteExento,
    decimal ImportePercepcionesDni,
    decimal ImportePercepcionesIibb,
    decimal ImportePercepcionesMunicipales,
    decimal ImporteImpuestosInternos,
    decimal ImporteOtrosTributos,
    int CantidadAlicuotasIva,
    string CodigoOperacion,
    decimal CreditoFiscalComputablePeriodoActual,
    decimal OtrosAtributosCompra,
    long? CuitEmisorCorredorInterviene,
    string DenominacionEmisorCorredor,
    decimal ComisionesHonorarios);

public record IibbPercepcionLineaDto(
    string Cuit,
    string RazonSocial,
    DateOnly FechaPercepcion,
    string TipoOperacion,
    decimal BaseImponible,
    decimal AlicuotaPercepcion,
    decimal Importe,
    string NroComprobante);

public record RetencionLineaDto(
    string Cuit,
    string RazonSocial,
    DateOnly FechaRetencion,
    string TipoRetencion,
    string NroComprobante,
    decimal BaseImponible,
    decimal AlicuotaRetencion,
    decimal Importe);

public record ExportacionArchivoResultDto(
    string NombreArchivo,
    string Contenido,
    int CantidadRegistros);
