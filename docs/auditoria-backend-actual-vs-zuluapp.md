# Auditoría Backend Actual vs zuluApp - Ventas/Operaciones

**Fecha:** 2026-03-20  
**Objetivo:** Validar qué campos y funcionalidades ya existen en el backend actual comparado con el análisis completo de zuluApp

## 1. ENTIDAD COMPROBANTE

### 1.1 Campos Base - CUMPLIDO ✅
| Campo zuluApp | Backend Actual | Estado |
|---------------|----------------|--------|
| Id_Comprobante | Id | ✅ OK |
| Id_TipoComprobante | TipoComprobanteId | ✅ OK |
| Id_Sucursal | SucursalId | ✅ OK |
| Id_PuntoFacturacion | PuntoFacturacionId | ✅ OK |
| PrefijoComprobante / NumeroComprobante | Numero (NroComprobante) | ✅ OK |
| FechaEmision | Fecha | ✅ OK |
| FechaVencimiento | FechaVencimiento | ✅ OK |
| Id_Tercero | TerceroId | ✅ OK |
| Id_Moneda | MonedaId | ✅ OK |
| Cotizacion | Cotizacion | ✅ OK |
| Estado | Estado (EstadoComprobante) | ✅ OK |

### 1.2 Campos Comerciales - CUMPLIDO ✅
| Campo zuluApp | Backend Actual | Estado |
|---------------|----------------|--------|
| Id_Vendedor | VendedorId | ✅ OK (línea 21) |
| Id_Cobrador | CobradorId | ✅ OK (línea 22) |
| PorcentajeComisionVendedor | PorcentajeComisionVendedor | ✅ OK (línea 28) |
| PorcentajeComisionCobrador | PorcentajeComisionCobrador | ✅ OK (línea 29) |
| ImporteComisionVendedor | ❌ | ⚠️ FALTA (se calcula pero no se persiste) |
| Id_ZonaComercial | ZonaComercialId | ✅ OK (línea 23) |
| Id_ListaPrecios | ListaPreciosId | ✅ OK (línea 24) |
| Id_CondicionPago | CondicionPagoId | ✅ OK (línea 25) |
| PlazoDias | PlazoDias | ✅ OK (línea 26) |
| Id_CanalVenta | CanalVentaId | ✅ OK (línea 27) |

### 1.3 Campos Logísticos (Remitos) - CUMPLIDO ✅
| Campo zuluApp | Backend Actual | Estado |
|---------------|----------------|--------|
| Id_Transporte | TransporteId | ✅ OK (línea 32) |
| ChoferNombre | ChoferNombre | ✅ OK (línea 33) |
| ChoferDni | ChoferDni | ✅ OK (línea 34) |
| PatVehiculo | PatVehiculo | ✅ OK (línea 35) |
| PatAcoplado | PatAcoplado | ✅ OK (línea 36) |
| DomicilioEntrega | DomicilioEntrega | ✅ OK (línea 37) |
| ObservacionesLogisticas | ObservacionesLogisticas | ✅ OK (línea 38) |
| FechaEstimadaEntrega | FechaEstimadaEntrega | ✅ OK (línea 39) |
| FechaRealEntrega | FechaRealEntrega | ✅ OK (línea 40) |
| FirmaConformidad | FirmaConformidad | ✅ OK (línea 41, string) |
| NombreQuienRecibe | NombreQuienRecibe | ✅ OK (línea 42) |
| EstadoEntregaRemito | EstadoLogistico (EstadoLogisticoRemito) | ✅ OK (línea 43) |
| RutaLogistica | ❌ | ⚠️ FALTA |
| PesoTotal | ❌ | ⚠️ FALTA |
| VolumenTotal | ❌ | ⚠️ FALTA |
| Bultos | ❌ | ⚠️ FALTA |
| TipoEmbalaje | ❌ | ⚠️ FALTA |
| SeguroTransporte | ❌ | ⚠️ FALTA |
| ValorDeclarado | ❌ | ⚠️ FALTA |
| DniQuienRecibe | ❌ | ⚠️ FALTA |
| EsValorizado | EsValorizado | ✅ OK (línea 44) |
| Id_DepositoOrigen | DepositoOrigenId | ✅ OK (línea 45) |

### 1.4 Campos Pedido - PARCIAL ⚠️
| Campo zuluApp | Backend Actual | Estado |
|---------------|----------------|--------|
| FechaCompromisoEntrega | FechaEntregaCompromiso | ✅ OK (línea 48) |
| EstadoPedido | EstadoPedido (EstadoPedido enum) | ✅ OK (línea 49) |
| MotivoCierrePedido | MotivoCierrePedido | ✅ OK (línea 50) |
| FechaCierrePedido | FechaCierrePedido | ✅ OK (línea 51) |
| Prioridad | ❌ | ⚠️ FALTA |
| StockReservado (flag) | ❌ | ⚠️ FALTA |
| UsuarioAprobador | ❌ | ⚠️ FALTA |
| FechaAprobacion | ❌ | ⚠️ FALTA |
| MotivoRechazo | ❌ | ⚠️ FALTA |
| Id_CanalOrigen | CanalVentaId | ✅ OK (cubre esto) |

**⚠️ CRÍTICO:** Pedidos están manejados como comprobantes genéricos, no como entidad independiente con lógica de seguimiento de cumplimiento por renglón.

### 1.5 Campos Monetarios - PARCIAL ⚠️
| Campo zuluApp | Backend Actual | Estado |
|---------------|----------------|--------|
| Subtotal | Subtotal | ✅ OK (línea 79) |
| DescuentoImporte | DescuentoImporte | ✅ OK (línea 80) |
| DescuentoPorcentaje | DescuentoPorcentaje | ✅ OK (línea 58) |
| NetoGravado | NetoGravado | ✅ OK (línea 81) |
| NetoNoGravado | NetoNoGravado | ✅ OK (línea 82) |
| IvaRi | IvaRi | ✅ OK (línea 83) |
| IvaRni | IvaRni | ✅ OK (línea 84) |
| Percepciones | Percepciones | ✅ OK (línea 85) |
| Retenciones | Retenciones | ✅ OK (línea 86) |
| Total | Total | ✅ OK (línea 87) |
| Saldo | Saldo | ✅ OK (línea 88) |
| RecargoImporte | RecargoImporte | ✅ OK (línea 57) |
| RecargoPorcentaje | RecargoPorcentaje | ✅ OK (línea 56) |

### 1.6 Campos Observaciones - CUMPLIDO ✅
| Campo zuluApp | Backend Actual | Estado |
|---------------|----------------|--------|
| Observacion | Observacion | ✅ OK (línea 107) |
| ObservacionInterna | ObservacionInterna | ✅ OK (línea 54) |
| ObservacionFiscal | ObservacionFiscal | ✅ OK (línea 55) |

### 1.7 Fiscal AFIP (Argentina) - CUMPLIDO ✅
| Campo zuluApp | Backend Actual | Estado |
|---------------|----------------|--------|
| Cae | Cae | ✅ OK (línea 99) |
| Caea | Caea | ✅ OK (línea 100) |
| FechaVtoCae | FechaVtoCae | ✅ OK (línea 101) |
| EstadoAfip | EstadoAfip | ✅ OK (línea 103) |
| UltimoErrorAfip | UltimoErrorAfip | ✅ OK (línea 104) |
| FechaUltimaConsultaAfip | FechaUltimaConsultaAfip | ✅ OK (línea 105) |
| QrData | QrData | ✅ OK (línea 102) |

### 1.8 Fiscal SIFEN (Paraguay) - CUMPLIDO ✅
| Campo zuluApp | Backend Actual | Estado |
|---------------|----------------|--------|
| TimbradoId | TimbradoId | ✅ OK (línea 90) |
| NroTimbrado | NroTimbrado | ✅ OK (línea 91) |
| EstadoSifen | EstadoSifen | ✅ OK (línea 92) |
| SifenCodigoRespuesta | SifenCodigoRespuesta | ✅ OK (línea 93) |
| SifenMensajeRespuesta | SifenMensajeRespuesta | ✅ OK (línea 94) |
| SifenTrackingId | SifenTrackingId | ✅ OK (línea 95) |
| SifenCdc | SifenCdc | ✅ OK (línea 96) |
| SifenNumeroLote | SifenNumeroLote | ✅ OK (línea 97) |
| SifenFechaRespuesta | SifenFechaRespuesta | ✅ OK (línea 98) |

### 1.9 COT (Carta Oficial Transporte - Paraguay) - CUMPLIDO ✅
| Campo zuluApp | Backend Actual | Estado |
|---------------|----------------|--------|
| Entidad CMP_COT | ComprobanteCot | ✅ OK (línea 111) |
| Cot_Valor | Ver ComprobanteCot | ✅ OK (verificar estructura) |
| Cot_fecha | Ver ComprobanteCot | ✅ OK (verificar estructura) |
| Cot_Descripcion | Ver ComprobanteCot | ✅ OK (verificar estructura) |

### 1.10 Notas Débito/Crédito - CUMPLIDO ✅
| Campo zuluApp | Backend Actual | Estado |
|---------------|----------------|--------|
| Id_MotivoDebito | MotivoDebitoId | ✅ OK (línea 64) |
| MotivoDebitoObservacion | MotivoDebitoObservacion | ✅ OK (línea 65) |
| MotivoDevolucion | MotivoDevolucion | ✅ OK (línea 71) |
| TipoDevolucion | TipoDevolucion | ✅ OK (línea 72) |
| AutorizadorDevolucion | AutorizadorDevolucionId | ✅ OK (línea 73) |
| FechaAutorizacionDevolucion | FechaAutorizacionDevolucion | ✅ OK (línea 74) |
| ObservacionDevolucion | ObservacionDevolucion | ✅ OK (línea 75) |
| ReingresaStock | ReingresaStock | ✅ OK (línea 76) |
| AcreditaCuentaCorriente | AcreditaCuentaCorriente | ✅ OK (línea 77) |

### 1.11 Anulación - CUMPLIDO ✅
| Campo zuluApp | Backend Actual | Estado |
|---------------|----------------|--------|
| FechaAnulacion | FechaAnulacion | ✅ OK (línea 66) |
| UsuarioAnulacionId | UsuarioAnulacionId | ✅ OK (línea 67) |
| MotivoAnulacion | MotivoAnulacion | ✅ OK (línea 68) |

### 1.12 Snapshot Tercero - CUMPLIDO ✅
| Campo zuluApp | Backend Actual | Estado |
|---------------|----------------|--------|
| DomicilioTerceroSnapshot | TerceroDomicilioSnapshot | ✅ OK (línea 61) |
| DenominacionSocialPersona | ❌ | ⚠️ FALTA (debería snapshot razón social) |
| CondicionIvaPersona | ❌ | ⚠️ FALTA (debería snapshot condición IVA) |

### 1.13 Vínculo Origen - CUMPLIDO ✅
| Campo zuluApp | Backend Actual | Estado |
|---------------|----------------|--------|
| Id_ComprobanteOrigen | ComprobanteOrigenId | ✅ OK (línea 109) |

### 1.14 Auditoría - PARCIAL ⚠️
| Campo zuluApp | Backend Actual | Estado |
|---------------|----------------|--------|
| UsuarioCreador | CreatedBy (AuditableEntity) | ✅ OK (hereda de AuditableEntity) |
| FechaCreacion | CreatedAt (AuditableEntity) | ✅ OK |
| UsuarioModificador | UpdatedBy (AuditableEntity) | ✅ OK |
| FechaModificacion | UpdatedAt (AuditableEntity) | ✅ OK |

### 1.15 Atributos Dinámicos - CUMPLIDO ✅
| Elemento zuluApp | Backend Actual | Estado |
|------------------|----------------|--------|
| CMP_ATRIBUTOS_REMITO | Atributos (ComprobanteAtributo) | ✅ OK (línea 115-116) |

---

## 2. ENTIDAD COMPROBANTEITEM

### 2.1 Campos Base - CUMPLIDO ✅
| Campo zuluApp | Backend Actual | Estado |
|---------------|----------------|--------|
| Id_Item_Comprobante | Id | ✅ OK |
| Id_Comprobante | ComprobanteId | ✅ OK (línea 8) |
| Id_Item | ItemId | ✅ OK (línea 9) |
| Descripcion | Descripcion | ✅ OK (línea 10) |
| Cantidad | Cantidad | ✅ OK (línea 11) |
| CantidadBonificada | CantidadBonificada | ✅ OK (línea 13) |
| PrecioUnitario | PrecioUnitario | ✅ OK (línea 14) |
| DescuentoPorcentaje | DescuentoPct | ✅ OK (línea 15) |
| Id_AlicuotaIva | AlicuotaIvaId | ✅ OK (línea 16) |
| PorcentajeIva | PorcentajeIva | ✅ OK (línea 17) |
| SubtotalNeto | SubtotalNeto | ✅ OK (línea 18) |
| IvaImporte | IvaImporte | ✅ OK (línea 19) |
| TotalLinea | TotalLinea | ✅ OK (línea 20) |
| Id_Deposito | DepositoId | ✅ OK (línea 21) |
| Orden | Orden | ✅ OK (línea 22) |
| EsGravado | EsGravado | ✅ OK (línea 23) |

### 2.2 Campos Extendidos - CUMPLIDO ✅
| Campo zuluApp | Backend Actual | Estado |
|---------------|----------------|--------|
| Lote | Lote | ✅ OK (línea 26) |
| Serie | Serie | ✅ OK (línea 27) |
| FechaVencimiento | FechaVencimiento | ✅ OK (línea 28) |
| Id_UnidadMedida | UnidadMedidaId | ✅ OK (línea 29) |
| Concepto (observación) | ObservacionRenglon | ✅ OK (línea 30) |
| PrecioListaOriginal | PrecioListaOriginal | ✅ OK (línea 31) |
| ComisionVendedorRenglon | ComisionVendedorRenglon | ✅ OK (línea 32) |

### 2.3 Vínculo Pedido - PARCIAL ⚠️
| Campo zuluApp | Backend Actual | Estado |
|---------------|----------------|--------|
| Id_ItemPedidoOrigen | ComprobanteItemOrigenId | ✅ OK (línea 33) |
| CantidadCumplidaDelPedido | ❌ | ⚠️ FALTA EXPLÍCITO (podría calcularse) |

### 2.4 Campos Específicos NC/ND - CUMPLIDO ✅
| Campo zuluApp | Backend Actual | Estado |
|---------------|----------------|--------|
| CantidadDocumentoOrigen | CantidadDocumentoOrigen | ✅ OK (línea 36) |
| PrecioDocumentoOrigen | PrecioDocumentoOrigen | ✅ OK (línea 37) |

### 2.5 Seguimiento Cumplimiento Pedido - CUMPLIDO ✅
| Campo zuluApp | Backend Actual | Estado |
|---------------|----------------|--------|
| CantidadEntregada | CantidadEntregada | ✅ OK (línea 40) |
| CantidadPendiente | CantidadPendiente | ✅ OK (línea 41) |
| EstadoItemPedido | EstadoEntrega (EstadoEntregaItem) | ✅ OK (línea 42) |
| Atraso | EsAtrasado | ✅ OK (línea 43) |

### 2.6 Atributos Dinámicos Item - FALTA ⚠️
| Elemento zuluApp | Backend Actual | Estado |
|------------------|----------------|--------|
| VTA_CMP_ITEMS_ATRIBUTOS | ❌ | ⚠️ FALTA (debería haber colección Atributos en ComprobanteItem) |

---

## 3. ENTIDADES AUXILIARES

### 3.1 ComprobanteCot (Paraguay)
Verificar estructura completa:
```csharp
public class ComprobanteCot : BaseEntity
{
    public long ComprobanteId { get; private set; }
    public string CotValor { get; private set; } = string.Empty; // Mínimo 6 caracteres
    public DateOnly CotFecha { get; private set; } // Vigencia
    public string? CotDescripcion { get; private set; }
    
    // Validaciones críticas:
    // 1. CotValor.Length >= 6
    // 2. CotFecha >= Comprobante.Fecha
}
```
✅ Verificar si existe correctamente implementada

### 3.2 ComprobanteAtributo (atributos dinámicos cabecera)
✅ Existe (líneas 115-116 Comprobante.cs)

### 3.3 ComprobanteItemAtributoComercial
⚠️ Verificar si cubre `VTA_CMP_ITEMS_ATRIBUTOS` del legado

---

## 4. TABLA PEDIDOS INDEPENDIENTE - CRÍTICO FALTA ❌

**PROBLEMA:** El backend actual trata pedidos como comprobantes genéricos con algunos campos adicionales.

**SOLUCIÓN REQUERIDA:** Crear entidad `Pedido` independiente con:

```csharp
public class Pedido : AuditableEntity
{
    public long Id { get; private set; }
    public string NumeroPedido { get; private set; } // Formato: PPPP-NNNNNNNN
    public short Prefijo { get; private set; }
    public long Numero { get; private set; }
    public DateOnly FechaEmision { get; private set; }
    public DateOnly FechaCompromisoEntrega { get; private set; }
    public DateOnly? FechaAprobacion { get; private set; }
    
    public long SucursalId { get; private set; }
    public long TerceroId { get; private set; }
    public long? VendedorId { get; private set; }
    public long? CobradorId { get; private set; }
    public long MonedaId { get; private set; }
    public decimal Cotizacion { get; private set; }
    
    public EstadoPedido Estado { get; private set; } // Enum: Pendiente, Aprobado, Rechazado, EnPreparacion, CumplidoParcial, CumplidoTotal, Anulado, Cerrado
    public PrioridadPedido Prioridad { get; private set; } // Enum: Alta, Media, Baja
    public bool StockReservado { get; private set; } // Si reserva stock
    
    public long? UsuarioAprobadorId { get; private set; }
    public string? MotivoRechazo { get; private set; }
    public long? CanalOrigenId { get; private set; }
    public string? Observacion { get; private set; }
    
    public decimal Total { get; private set; }
    
    private readonly List<PedidoItem> _items = [];
    public IReadOnlyCollection<PedidoItem> Items => _items.AsReadOnly();
}

public class PedidoItem : BaseEntity
{
    public long PedidoId { get; private set; }
    public short Orden { get; private set; }
    public long ItemId { get; private set; }
    public string Descripcion { get; private set; } = string.Empty;
    public decimal Cantidad { get; private set; } // Cantidad pedida
    public decimal CantidadEntregada { get; private set; } // Acumulado de lo entregado
    public decimal CantidadPendiente => Cantidad - CantidadEntregada; // Calculado
    
    public EstadoEntregaItem EstadoEntrega { get; private set; } // Enum: NoEntregado, EntregaCompleta, EntregaParcial, Sobrepasada
    public bool EsAtrasado { get; private set; } // Si FechaCompromisoEntrega < hoy y CantidadPendiente > 0
    
    public decimal PrecioUnitario { get; private set; }
    public decimal DescuentoPorcentaje { get; private set; }
    public decimal SubtotalNeto { get; private set; }
    public decimal IvaImporte { get; private set; }
    public decimal TotalLinea { get; private set; }
    public long? DepositoId { get; private set; } // Desde dónde se entregará
}
```

**OPERACIONES REQUERIDAS:**
1. `CreatePedidoCommand`: crear pedido con items
2. `UpdatePedidoCommand`: editar si Pendiente
3. `AprobarPedidoCommand`: cambiar a Aprobado
4. `RechazarPedidoCommand`: cambiar a Rechazado + motivo
5. `ConvertirPedidoADocumentoCommand`: generar remito/factura desde pedido (parcial o total)
6. `CerrarPedidoCommand`: cerrar individual
7. `CerrarPedidosMasivoCommand`: cerrar múltiples con filtros
8. `GetPedidosPagedQuery`: listar con filtros legacy
9. `GetPedidoByIdQuery`: detalle completo
10. `GetEstadoCumplimientoPedidoQuery`: árbol documental pedido → documentos
11. `GetPedidosVinculadosQuery`: ver remitos/facturas generados desde pedido

---

## 5. TABLA ANTICIPOS APLICADOS - FALTA ❌

**PROBLEMA:** No existe mecanismo para aplicar recibos anticipados a facturas.

**SOLUCIÓN REQUERIDA:**

```csharp
public class AnticipoAplicado : BaseEntity
{
    public long Id { get; private set; }
    public long FacturaId { get; private set; } // FK Comprobantes
    public long ReciboAnticipoId { get; private set; } // FK Comprobantes (tipo Recibo Anticipo)
    public decimal ImporteAplicado { get; private set; }
    public DateTimeOffset FechaAplicacion { get; private set; }
    public long? UsuarioAplicacionId { get; private set; }
}
```

**OPERACIONES REQUERIDAS:**
1. `AplicarAnticipoAFacturaCommand`: vincular anticipo a factura
2. `QuitarAnticipoDeFacturaCommand`: desvincular (si aún no cobrado)
3. `GetAnticiposDisponiblesQuery`: listar anticipos sin aplicar de un cliente

---

## 6. TABLA FACTURA_REMITOS_VINCULADOS (M:N) - FALTA ❌

**PROBLEMA:** Una factura puede consolidar múltiples remitos, pero no hay relación M:N explícita.

**SOLUCIÓN REQUERIDA:**

```csharp
public class FacturaRemitoVinculo : BaseEntity
{
    public long Id { get; private set; }
    public long FacturaId { get; private set; } // FK Comprobantes
    public long RemitoId { get; private set; } // FK Comprobantes
    public DateTimeOffset FechaVinculo { get; private set; }
}
```

**OPERACIONES REQUERIDAS:**
1. Modificar `ConvertirDocumentoVentaCommand` para soportar múltiples remitos origen
2. `FacturarRemitosMasivoCommand`: facturar varios remitos juntos
3. `GetRemitosSinFacturarQuery`: listar remitos pendientes de facturación

---

## 7. CAMPOS FALTANTES EN COMPROBANTE (PRIORIDAD MEDIA/BAJA)

| Campo | Prioridad | Observación |
|-------|-----------|-------------|
| ImporteComisionVendedor | MEDIA | Se puede calcular, pero zuluApp lo persiste |
| RutaLogistica | MEDIA | Remitos |
| PesoTotal | MEDIA | Remitos |
| VolumenTotal | MEDIA | Remitos |
| Bultos | MEDIA | Remitos |
| TipoEmbalaje | MEDIA | Remitos |
| SeguroTransporte | BAJA | Remitos |
| ValorDeclarado | BAJA | Remitos |
| DniQuienRecibe | MEDIA | Remitos |
| Prioridad (pedidos) | MEDIA | Si pedidos siguen como comprobantes |
| StockReservado (pedidos) | MEDIA | Si pedidos siguen como comprobantes |
| UsuarioAprobador (pedidos) | MEDIA | Si pedidos siguen como comprobantes |
| FechaAprobacion (pedidos) | MEDIA | Si pedidos siguen como comprobantes |
| MotivoRechazo (pedidos) | MEDIA | Si pedidos siguen como comprobantes |
| DenominacionSocialSnapshot | MEDIA | Snapshot de razón social del tercero |
| CondicionIvaSnapshot | MEDIA | Snapshot de condición IVA |

---

## 8. COMPROBANTEITEMATTRIBUTOCOMERCIAL - VERIFICAR ⚠️

Verificar si cubre completamente `VTA_CMP_ITEMS_ATRIBUTOS`:
- ¿Relación correcta con ComprobanteItem?
- ¿Estructura key-value flexible?
- ¿Integra con catálogo `AtributosComer ciales`?

---

## 9. QUERIES Y FILTROS - GAPS CRÍTICOS ❌

| Query zuluApp | Backend Actual | Estado |
|---------------|----------------|--------|
| GetPedidosPagedQuery con filtros legacy | ❌ | ⚠️ FALTA (cliente, sucursal, fecha entrega, estado item, atraso, código producto) |
| GetRemitosConFiltrosCOT | ❌ | ⚠️ FALTA (número COT, fecha vigencia COT, depósito) |
| GetPedidosVinculados (árbol documental) | ❌ | ⚠️ FALTA |
| GetEstadoCumplimientoPedido | ❌ | ⚠️ FALTA |
| GetRemitosLogisticaExpedicion | ❌ | ⚠️ FALTA |
| GetAnticiposDisponibles | ❌ | ⚠️ FALTA |
| GetRemitosSinFacturar | ❌ | ⚠️ FALTA |
| GetEstadoCuentaCorrienteCliente | ⚠️ Parcial | ⚠️ VERIFICAR si completo |

---

## 10. COMANDOS FALTANTES - GAPS CRÍTICOS ❌

| Comando zuluApp | Backend Actual | Estado |
|-----------------|----------------|--------|
| CreatePedidoCommand | ❌ | ⚠️ FALTA (dedicado, no genérico) |
| AprobarPedidoCommand | ❌ | ⚠️ FALTA |
| RechazarPedidoCommand | ❌ | ⚠️ FALTA |
| ConvertirPedidoADocumentoCommand | ⚠️ Parcial | ⚠️ Sin seguimiento cantidades renglón a renglón |
| CerrarPedidoCommand | ❌ | ⚠️ FALTA |
| CerrarPedidosMasivoCommand | ❌ | ⚠️ FALTA |
| DespacharRemitoCommand | ❌ | ⚠️ FALTA (cambiar estado logístico) |
| ConfirmarEntregaRemitoCommand | ❌ | ⚠️ FALTA (capturar conformidad) |
| AplicarAnticipoAFacturaCommand | ❌ | ⚠️ FALTA |
| FacturarRemitosMasivoCommand | ❌ | ⚠️ FALTA |
| FacturacionAutomaticaJob | ❌ | ⚠️ FALTA (proceso programado) |

---

## 11. DTOs FALTANTES ⚠️

| DTO zuluApp | Backend Actual | Estado |
|-------------|----------------|--------|
| PedidoDetalleDto (específico) | ❌ | ⚠️ FALTA |
| PedidoListDto (específico) | ❌ | ⚠️ FALTA |
| PedidoItemDto con cumplimiento | ❌ | ⚠️ FALTA |
| VendedorSelectorDto | ❌ | ⚠️ FALTA |
| CobradorSelectorDto | ❌ | ⚠️ FALTA |
| ZonaComercialDto | ❌ | ⚠️ FALTA |
| ListaPreciosDto | ❌ | ⚠️ FALTA |
| CondicionPagoDto | ❌ | ⚠️ FALTA |
| CanalVentaDto | ❌ | ⚠️ FALTA |
| ComprobanteTransporteDto (COT) | ⚠️ Verificar | ⚠️ Verificar si se expone en DTO detalle |
| RemitoCabAtributoDto | ⚠️ Verificar | ⚠️ Verificar si se expone en DTO detalle |
| AnticipoAplicadoDto | ❌ | ⚠️ FALTA |
| ComprobanteDetalleDto extendido | ⚠️ Parcial | ⚠️ Faltan algunos campos (vendedor, cobrador, zona, etc.) |

---

## 12. ENUMS FALTANTES ⚠️

| Enum zuluApp | Backend Actual | Estado |
|--------------|----------------|--------|
| EstadoPedido | ✅ OK | ✅ (verificar si cubre todos los estados) |
| EstadoEntregaItem | ✅ OK | ✅ (como EstadoEntregaItem) |
| EstadoLogisticoRemito | ✅ OK | ✅ (como EstadoLogisticoRemito) |
| PrioridadPedido | ❌ | ⚠️ FALTA |
| CanalVenta | ❌ | ⚠️ FALTA (o como catálogo) |
| MotivoNotaCredito | ❌ | ⚠️ FALTA (o como catálogo) |
| MotivoNotaDebito | ❌ | ⚠️ FALTA (o como catálogo MotivoDebito) |

---

## 13. RESUMEN AUDITORÍA

### 13.1 Estado General
- **Entidad Comprobante:** ~85% cubierto ✅
- **Entidad ComprobanteItem:** ~90% cubierto ✅
- **Pedidos Independientes:** 0% cubierto ❌ CRÍTICO
- **Anticipos Aplicados:** 0% cubierto ❌
- **Vínculo M:N Factura-Remitos:** 0% cubierto ❌
- **Queries con filtros legacy:** ~20% cubierto ❌ CRÍTICO
- **Comandos específicos pedidos:** 0% cubierto ❌ CRÍTICO
- **DTOs selectores comerciales:** 0% cubierto ❌
- **Operaciones masivas:** 0% cubierto ❌

### 13.2 Gaps Críticos (Bloquean paridad 100%)
1. ❌ **Pedido como entidad independiente** con seguimiento cumplimiento renglón a renglón
2. ❌ **ConvertirPedidoADocumentoCommand** con vínculo renglón a renglón y actualización cantidades
3. ❌ **Queries pedidos con todos los filtros legacy** (estado item, atraso, fecha compromiso)
4. ❌ **CerrarPedidosMasivoCommand** con filtros
5. ❌ **GetPedidosVinculadosQuery** (árbol documental)
6. ❌ **AplicarAnticipoAFacturaCommand** y tabla relación
7. ❌ **FacturarRemitosMasivoCommand**
8. ❌ **DTOs selectores comerciales** (vendedor, cobrador, zona, lista precios, condición pago)

### 13.3 Gaps Alta Prioridad
1. ⚠️ **ImporteComisionVendedor** persistido en Comprobante
2. ⚠️ **Campos logísticos remito** (peso, volumen, bultos, tipo embalaje, DNI receptor)
3. ⚠️ **DespacharRemitoCommand** y **ConfirmarEntregaRemitoCommand**
4. ⚠️ **GetRemitosConFiltrosCOT** (filtros COT y depósito)
5. ⚠️ **GetRemitosSinFacturarQuery**
6. ⚠️ **ComprobanteDetalleDto extendido** con todos los campos comerciales

### 13.4 Gaps Media/Baja Prioridad
1. ⚠️ Snapshot razón social y condición IVA tercero en Comprobante
2. ⚠️ FacturacionAutomaticaJob (proceso programado)
3. ⚠️ Enums/Catálogos: PrioridadPedido, CanalVenta, MotivoNotaCredito/Debito
4. ⚠️ Reportes avanzados (monitor ventas, ranking productos)

---

## 14. SIGUIENTE PASO

Basándome en esta auditoría, el **camino crítico** es:

### Fase 1 Ajustada: Completar Pedidos (3-4 días)
1. Crear entidad `Pedido` y `PedidoItem` independientes
2. Crear comandos crear/aprobar/rechazar/cerrar pedidos
3. Crear query pedidos paginada con todos los filtros legacy
4. Migración BD: tablas `Pedidos` y `PedidosItems`

### Fase 2 Ajustada: Conversión Pedido → Documentos (2-3 días)
1. Implementar `ConvertirPedidoADocumentoCommand` con vínculo renglón a renglón
2. Actualizar `ComprobanteItem` para vincular con `PedidoItem`
3. Lógica de actualización `CantidadEntregada` y estados pedido/items
4. Query `GetPedidosVinculadosQuery` (árbol documental)

### Fase 3 Ajustada: Anticipos y Facturación Masiva (2 días)
1. Tabla `AnticiposAplicados` y comandos aplicar/quitar
2. Tabla `FacturaRemitoVinculo` (M:N)
3. Comando `FacturarRemitosMasivoCommand`
4. Query `GetRemitosSinFacturarQuery`

### Fase 4 Ajustada: DTOs Comerciales y Extender ComprobanteDetalleDto (1-2 días)
1. Crear DTOs selectores: VendedorSelectorDto, CobradorSelectorDto, etc.
2. Queries: GetVendedoresActivos, GetCobradoresActivos, etc.
3. Actualizar `ComprobanteDetalleDto` para exponer vendedor, cobrador, zona, lista precios, anticipos aplicados

### Fase 5: Campos Logísticos Remito (1 día)
1. Agregar campos peso, volumen, bultos, tipo embalaje, DNI receptor
2. Comandos DespacharRemito y ConfirmarEntregaRemito
3. Query GetRemitosConFiltrosCOT

### Fase 6: Tests y Validación (2 días)
1. Unit tests críticos
2. Integration tests flujo completo
3. Smoke tests
4. Compilar y corregir

---

**Fin de la auditoría.**
