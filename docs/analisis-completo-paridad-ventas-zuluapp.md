# Análisis Completo: Paridad Funcional Ventas/Operaciones - zuluApp → ZuluIA_Back

**Fecha:** 2026-03-20  
**Objetivo:** Cerrar al 100% el backend de ventas para alcanzar paridad funcional total con `C:\Zulu\zuluApp`

## Resumen Ejecutivo

Este documento consolida el análisis de todas las vistas, formularios y operaciones de ventas del legado `zuluApp` para garantizar que el backend moderno cubra el 100% de la funcionalidad antes de avanzar el frontend.

## 1. ESTRUCTURA DE DATOS - COMPROBANTES VENTAS

### 1.1 Campos Identificados en zuluApp

#### Tabla COMPROBANTES (base)
```sql
-- Identificación
Id_Comprobante (PK)
Id_TipoComprobante (FK TiposComprobante)
Id_Sucursal (FK Sucursales)
Id_PuntoFacturacion (FK PuntosFacturacion)
PrefijoComprobante (int, ej: 0001)
NumeroComprobante (int, ej: 00000123)
FechaEmision (date)
FechaVencimiento (date, nullable)
Estado (int: 0=Borrador, 1=Emitido, 2=Anulado, 3=Fiscalizado)

-- Tercero
Id_Tercero (FK Terceros)
LegajoPersona (string)
DenominacionSocialPersona (string, snapshot)
DomicilioPersona (string, snapshot completo)
CondicionIvaPersona (string, snapshot)

-- Comercial
Id_Vendedor (FK Terceros)
NombreVendedor (string)
LegajoVendedor (string)
PorcentajeComisionVendedor (decimal)
ImporteComisionVendedor (decimal, calculado)
Id_Cobrador (FK Terceros)
NombreCobrador (string)
LegajoCobrador (string)
PorcentajeComisionCobrador (decimal)
Id_ZonaComercial (FK ZonasComerciales)
Id_ListaPrecios (FK ListasPrecios)
Id_CondicionPago (FK CondicionesPago)
PlazoDias (int)
Id_CanalVenta (FK CanalVenta: 1=Mostrador, 2=Web, 3=Telefónico, 4=Vendedor)

-- Monetario
Id_Moneda (FK Monedas)
Cotizacion (decimal)
Subtotal (decimal)
DescuentoImporte (decimal)
DescuentoPorcentaje (decimal)
RecargoImporte (decimal)
RecargoPorcentaje (decimal)
NetoGravado (decimal)
NetoNoGravado (decimal)
IvaRi (decimal)
IvaRni (decimal)
Percepciones (decimal)
Retenciones (decimal)
Total (decimal)
Saldo (decimal)

-- Fiscal AFIP (Argentina)
Cae (string)
Caea (string)
FechaVtoCae (date)
EstadoAfip (int)
UltimoErrorAfip (string)
FechaUltimaConsultaAfip (datetime)
QrData (string)

-- Fiscal SIFEN (Paraguay)
TimbradoId (long, FK Timbrados)
NroTimbrado (string)
EstadoSifen (int)
SifenCodigoRespuesta (string)
SifenMensajeRespuesta (string)
SifenTrackingId (string)
SifenCdc (string)
SifenNumeroLote (string)
SifenFechaRespuesta (datetime)

-- Logística (remitos principalmente)
Id_Transporte (FK Terceros)
ChoferNombre (string)
ChoferDni (string)
PatVehiculo (string)
PatAcoplado (string)
RutaLogistica (string)
DomicilioEntrega (string)
ObservacionesLogisticas (text)
FechaEstimadaEntrega (date)
FechaRealEntrega (date)
FirmaConformidad (image/binary)
NombreQuienRecibe (string)
PesoTotal (decimal)
VolumenTotal (decimal)
Bultos (int)
TipoEmbalaje (string)

-- Observaciones
Observacion (text, pública/cliente)
ObservacionInterna (text, interna operación)
ObservacionFiscal (text, específica para fiscalización)

-- Origen/Vínculo
Id_ComprobanteOrigen (FK Comprobantes, nullable)

-- Auditoría
UsuarioCreador (string)
FechaCreacion (datetime)
UsuarioModificador (string)
FechaModificacion (datetime)
```

#### Tabla CMP_COT (Carta Oficial Transporte - Paraguay)
```sql
Cot_id (PK, bigint identity)
cmp_id (FK Comprobantes)
Cot_fecha (date, NOT NULL) -- Fecha vigencia COT
Cot_Valor (nvarchar(50), NOT NULL) -- Número COT (mínimo 6 caracteres)
Cot_Descripcion (nvarchar(500), nullable)

-- Validaciones críticas:
-- 1. Cot_fecha >= FechaEmision del comprobante
-- 2. len(Cot_Valor) >= 6
-- 3. Campo obligatorio para remitos con transporte
```

#### Tabla VTA_CMP_ITEMS (detalle items/renglones)
```sql
Id_Item_Comprobante (PK)
Id_Comprobante (FK Comprobantes)
Orden (int)
Id_Item (FK Items)
CodigoItem (string, snapshot)
Descripcion (string)
Concepto (text, observación específica del renglón)
Cantidad (decimal)
CantidadBonificada (decimal)
CantidadSecundaria (decimal, unidad alternativa)
Id_UnidadMedida (FK UnidadesMedida)
PrecioUnitario (decimal)
PrecioListaOriginal (decimal, antes de descuentos)
DescuentoPorcentaje (decimal)
DescuentoImporte (decimal)
Id_AlicuotaIva (FK AlicuotasIva)
PorcentajeIva (decimal)
SubtotalNeto (decimal)
IvaImporte (decimal)
TotalLinea (decimal)
Id_Deposito (FK Depositos)
Lote (string)
Serie (string)
FechaVencimiento (date)
EsGravado (bit)
ComisionVendedorRenglon (decimal, si se calcula por línea)

-- Vínculo con pedido origen (si aplica)
Id_ItemPedidoOrigen (FK, nullable)
CantidadCumplidaDelPedido (decimal, cuánto de este renglón cumple el pedido)
```

#### Tabla VTA_CMP_ITEMS_ATRIBUTOS (atributos dinámicos por renglón)
```sql
Id_Atributo (PK)
Id_Item_Comprobante (FK VTA_CMP_ITEMS)
Id_AtributoComercial (FK AtributosComerci ales)
CodigoAtributo (string)
DescripcionAtributo (string)
Valor (string)
```

#### Tabla CMP_ATRIBUTOS_REMITO (atributos de cabecera remito)
```sql
Id_Atributo (PK)
Id_Comprobante (FK Comprobantes)
Clave (string)
Valor (string)
Descripcion (string)
```

---

## 2. PEDIDOS / NOTAS DE PEDIDO

### 2.1 Entidad Específica PEDIDOS (no es solo borrador de comprobante)

zuluApp maneja pedidos como **entidad propia** con lógica independiente:

#### Tabla PEDIDOS (o VTA_NOTASPEDIDO)
```sql
Id_Pedido (PK)
NumeroPedido (int)
PrefijoPedido (int)
FechaEmision (date)
FechaCompromisoEntrega (date, NOT NULL)
FechaAprobacion (date, nullable)
Id_Sucursal (FK)
Id_Tercero (FK)
Id_Vendedor (FK)
Id_Cobrador (FK)
Id_Moneda (FK)
Cotizacion (decimal)
EstadoPedido (int: 0=Pendiente, 1=Aprobado, 2=Rechazado, 3=EnPreparacion, 4=CumplidoParcial, 5=CumplidoTotal, 6=Anulado, 7=Cerrado)
Prioridad (int: 1=Alta, 2=Media, 3=Baja)
StockReservado (bit, indica si reserva stock)
UsuarioAprobador (string, nullable)
MotivoRechazo (text, nullable)
Id_CanalOrigen (int: web, teléfono, mostrador, vendedor)
Observacion (text)
Total (decimal)
UsuarioCreador (string)
FechaCreacion (datetime)
```

#### Tabla PEDIDOS_ITEMS (renglones con seguimiento de cumplimiento)
```sql
Id_Item_Pedido (PK)
Id_Pedido (FK PEDIDOS)
Orden (int)
Id_Item (FK Items)
Descripcion (string)
Cantidad (decimal, cantidad pedida)
CantidadEntregada (decimal, cuánto se ha entregado acumulado)
CantidadPendiente (decimal, calculado: Cantidad - CantidadEntregada)
EstadoItemPedido (int: 0=NoEntregado, 1=EntregaCompleta, 2=EntregaParcial, -1=Sobrepasada)
Atraso (bit, si FechaCompromisoEntrega < hoy y CantidadPendiente > 0)
PrecioUnitario (decimal)
DescuentoPorcentaje (decimal)
SubtotalNeto (decimal)
IvaImporte (decimal)
TotalLinea (decimal)
Id_Deposito (FK, desde dónde se entregará)
```

### 2.2 Flujos y Operaciones de Pedidos

#### Crear Pedido
- Validar cliente, sucursal, items
- Estado inicial: `Pendiente` (0)
- Si `StockReservado = true`, reservar cantidades en cada depósito
- Calcular totales

#### Aprobar Pedido
- Cambiar estado a `Aprobado` (1)
- Registrar `UsuarioAprobador` y `FechaAprobacion`
- Si no aprobado: `Rechazado` (2) + `MotivoRechazo`

#### Convertir a Remito/Factura
- Permitir conversión parcial o total
- Por cada item seleccionado:
  - Crear renglón en documento destino
  - Registrar vínculo `Id_ItemPedidoOrigen`
  - Actualizar `CantidadEntregada` del item del pedido
  - Recalcular `CantidadPendiente`
  - Actualizar `EstadoItemPedido`
- Si todos los items están en `EntregaCompleta`: `EstadoPedido = CumplidoTotal` (5)
- Si al menos uno está cumplido parcial: `EstadoPedido = CumplidoParcial` (4)
- Si ninguno entregado: sigue `Pendiente` o `EnPreparacion`

#### Cerrar Pedido
- Manualmente o masivo
- Cambia `EstadoPedido = Cerrado` (7)
- Libera stock reservado si aplica
- No se puede convertir a documentos después de cerrado
- Casos de uso: pedido cancelado por cliente, pedido vencido

#### Consultar Estado
Vistas principales:
- **VTAESTADONOTASPEDIDO_Listado.asp**:
  - Filtros: cliente, sucursal, fecha entrega, estado item, atraso, número pedido, código/concepto producto
  - Columnas: número, fecha, cliente, estado pedido, items, cantidades, cumplimiento
- **VTAConsultaNotaPedidoVinculadas_Listado.asp**:
  - Muestra cadena documental: Pedido → Remitos → Facturas
  - Cantidad cumplida por renglón en cada documento
- **VTAESTADONOTASPEDIDOEXPEDICION_Listado.asp**:
  - Vista orientada a logística/expedición
  - Filtros adicionales: depósito, ruta, transporte, fecha estimada entrega

---

## 3. REMITOS DE VENTA

### 3.1 Tipos de Remito en zuluApp

1. **Remito Valorizado**:
   - Impacta cuenta corriente del cliente (débito)
   - Impacta stock (egreso)
   - Puede facturarse posteriormente o quedar como documento final
   - Equivalente a "factura sin fiscal"

2. **Remito No Valorizado**:
   - NO impacta cuenta corriente
   - Impacta stock (egreso)
   - Siempre debe acompañarse de factura posterior
   - Uso típico: entrega con factura posterior, consignación

### 3.2 Campos Específicos de Remitos

Además de los campos base de COMPROBANTES, remitos requieren:

#### Logística Obligatoria
- `Id_Transporte` (FK Terceros): transportista
- `ChoferNombre`, `ChoferDni`: datos del conductor
- `PatVehiculo`, `PatAcoplado`: patentes
- `RutaLogistica`: descripción ruta
- `DomicilioEntrega`: dirección completa entrega (puede diferir del domicilio fiscal del cliente)
- `ObservacionesLogisticas`: observaciones internas logística

#### Estado Entrega
```sql
EstadoEntregaRemito (int):
  0 = Pendiente (creado, no despachado)
  1 = EnTránsito (despachado, no entregado)
  2 = Entregado (conformidad cliente)
  3 = EntregadoParcial (entrega parcial, resto pendiente)
  4 = Rechazado (cliente rechazó recepción)
  5 = Devuelto (retornó a depósito)
```

#### Conformidad
- `FechaRealEntrega` (date): cuándo se entregó efectivamente
- `FirmaConformidad` (image/varbinary): firma digital o escaneada
- `NombreQuienRecibe` (string): nombre persona que recibió
- `DniQuienRecibe` (string): documento de quien recibió

#### Atributos de Carga
- `PesoTotal` (decimal, kg)
- `VolumenTotal` (decimal, m³)
- `Bultos` (int): cantidad de bultos/pallets
- `TipoEmbalaje` (string): caja, pallet, bolsa, granel, etc.
- `SeguroTransporte` (bit): si tiene seguro contratado
- `ValorDeclarado` (decimal): valor declarado para seguro

#### COT (Carta Oficial Transporte - Paraguay)
**CRÍTICO** para remitos en Paraguay:
- Tabla `CMP_COT` (1:1 con Comprobante)
- `Cot_Valor` (string, mínimo 6 caracteres): número COT
- `Cot_fecha` (date): vigencia COT (debe ser >= fecha remito)
- `Cot_Descripcion` (string, opcional)

**Validaciones zuluApp:**
```vbscript
if Cot_Valor = "" then
    Error = "Debe ingresar numero de COT."
end if

if len(Cot_Valor) < 6 then
    Error = "El numero de COT ingresado tiene menos de 6 caracteres."
end if

if Cot_fecha < FechaEmision then
    Error = "La fecha ingresada es menor a la fecha de emisión del Remito."
end if
```

#### Atributos Dinámicos de Remito
Tabla `CMP_ATRIBUTOS_REMITO`:
- Formulario `frmRemitos_Atributos.frm` permite agregar metadatos libres
- Ejemplos: número guía courier, código interno logística, observaciones especiales

### 3.3 Flujos de Remito

#### Crear Remito desde Pedido
1. Seleccionar pedido origen
2. Seleccionar items a entregar (total o parcial)
3. Ingresar datos logísticos (transporte, chofer, etc.)
4. Si es Paraguay: ingresar COT obligatorio
5. Validar stock disponible en depósito
6. Al confirmar:
   - Crear comprobante tipo Remito
   - Vincular `Id_ComprobanteOrigen = Id_Pedido`
   - Por cada renglón: vincular `Id_ItemPedidoOrigen`
   - Actualizar cantidades entregadas del pedido
   - Actualizar `EstadoItemPedido` de los renglones del pedido
   - Si `EsValorizado`: generar movimiento cuenta corriente (débito cliente)
   - Generar movimiento stock (egreso depósito)
   - Estado inicial remito: `Pendiente` (0)

#### Despachar Remito
- Cambiar `EstadoEntregaRemito` a `EnTránsito` (1)
- Registrar fecha/hora despacho
- Opcional: generar documento impreso para chofer

#### Confirmar Entrega
- Cambiar `EstadoEntregaRemito` a `Entregado` (2)
- Registrar `FechaRealEntrega`, `NombreQuienRecibe`, `DniQuienRecibe`
- Capturar `FirmaConformidad` (firma digital)

#### Facturar desde Remito
- Crear factura con `Id_ComprobanteOrigen = Id_Remito`
- Heredar renglones del remito
- Si remito era no valorizado: al facturar se genera el débito en cuenta corriente
- Si remito era valorizado: factura solo fiscaliza, no impacta cuenta corriente nuevamente

---

## 4. FACTURAS DE VENTA

### 4.1 Tipos de Factura

1. **Factura Directa**: sin documento origen, venta completa en un paso
2. **Factura desde Pedido**: convierte pedido aprobado, puede ser parcial
3. **Factura desde Remito(s)**: fiscaliza uno o múltiples remitos previos
4. **Pre-Factura**: borrador numerado temporalmente, luego se confirma o anula
5. **Factura Electrónica**: integrada con AFIP (Argentina) o SIFEN (Paraguay)

### 4.2 Campos Específicos Facturas

#### Comisiones
- `PorcentajeComisionVendedor` (decimal)
- `ImporteComisionVendedor` (decimal, calculado: Total * % / 100)
- `PorcentajeComisionCobrador` (decimal)
- `ImporteComisionCobrador` (decimal, calculado)
- Opción: calcular comisión por renglón (`ComisionVendedorRenglon` en items)

#### Anticipos Aplicados
Tabla `ANTICIPOS_APLICADOS_FACTURA`:
```sql
Id_AnticipoAplicado (PK)
Id_Factura (FK Comprobantes)
Id_Recibo (FK Comprobantes, tipo Recibo Anticipo)
ImporteAplicado (decimal)
FechaAplicacion (datetime)
```
Lógica:
- Cliente puede tener recibos anticipados (pagos previos sin factura)
- Al emitir factura, se pueden aplicar estos anticipos
- Reduce el `Saldo` de la factura
- El recibo anticipo queda "consumido" o parcialmente consumido

#### Vínculo Origen Multiple
- Una factura puede consolidar varios remitos
- Tabla auxiliar `FACTURA_REMITOS_VINCULADOS` (M:N):
```sql
Id_Vinculo (PK)
Id_Factura (FK Comprobantes)
Id_Remito (FK Comprobantes)
FechaVinculo (datetime)
```

#### Condición de Pago
- `Id_CondicionPago` (FK CondicionesPago)
- Ejemplos: Contado, 30 días, 60 días, Consignación
- Define `PlazoDias` que determina `FechaVencimiento`
- Integrado con cuenta corriente para cálculo de vencimientos

### 4.3 Flujos de Factura

#### Crear Pre-Factura
- Estado `Borrador` o `PreFactura`
- Numera con serie temporal (ej: PRE-00001)
- No impacta stock ni cuenta corriente
- No obtiene CAE/CDC
- Permite edición completa

#### Confirmar/Emitir Factura
1. Validar datos completos
2. Numerar definitivamente (prefijo + número de facturación)
3. Generar movimiento cuenta corriente (débito cliente)
4. Si no viene de remito: generar movimiento stock (egreso)
5. Si aplica comisiones: registrar movimiento comisiones
6. Cambiar estado a `Emitido`
7. Si configurado: solicitar fiscal (CAE/CDC) automáticamente

#### Fiscalizar Electrónicamente
##### AFIP (Argentina)
1. Endpoint: `POST /api/comprobantes/{id}/autorizar-afip`
2. Validar que `Estado = Emitido`
3. Construir request WSFE con datos del comprobante
4. Enviar a AFIP web service
5. Si OK: registrar CAE, FechaVtoCae, QrData, cambiar `EstadoAfip = Aceptado`
6. Si error: registrar `UltimoErrorAfip`, `EstadoAfip = Rechazado`
7. Auditar en `AfipWsfeAudit`

##### SIFEN (Paraguay)
1. Endpoint: `POST /api/comprobantes/{id}/enviar-sifen`
2. Validar `TimbradoId` vigente
3. Construir XML SIFEN con datos del comprobante
4. Enviar a SIFEN web service
5. Si OK: registrar `SifenCdc`, `SifenTrackingId`, `EstadoSifen = Aceptado`
6. Si error: registrar `SifenMensajeRespuesta`, `EstadoSifen = Rechazado`
7. Auditar en tabla específica SIFEN

#### Anular Factura
- Si ya fiscalizada: requiere nota de crédito
- Si borrador/emitida sin fiscal: anulación directa
- Reversa movimientos: stock, cuenta corriente, comisiones
- Estado final: `Anulado`

---

## 5. NOTAS DE CRÉDITO Y DÉBITO

### 5.1 Nota de Crédito

Documento que **reduce/anula** una factura previa.

#### Motivos de NC (catálogo)
```sql
MOTIVOS_NOTA_CREDITO:
  1 = Devolución de mercadería
  2 = Descuento por volumen
  3 = Bonificación comercial
  4 = Error en facturación
  5 = Anulación total de factura
  6 = Ajuste de precio
```

#### Campos Específicos NC
- `Id_ComprobanteOrigen` (FK Comprobantes, factura que se acredita)
- `Id_MotivoNotaCredito` (FK)
- `AfectaStock` (bit): si es por devolución, re-ingresa stock
- `AfectaCuentaCorriente` (bit): típicamente siempre true, acredita al cliente
- Renglones: pueden ser todos o parciales de la factura origen
- En renglones: `CantidadDevuelta`, `MotivoDevolucionRenglon`

#### Flujos NC
1. **Desde Factura**: seleccionar factura origen, elegir items a acreditar
2. **Por Devolución**: cliente devuelve mercadería, genera remito devolución + NC
3. **Por Ajuste**: error administrativo, bonificación, etc.

Impactos:
- Movimiento cuenta corriente: crédito cliente (reduce saldo deudor)
- Si `AfectaStock = true`: movimiento ingreso a depósito
- Reduce el `Saldo` de la factura origen (puede quedar en 0)
- Fiscal: obtiene CAE/CDC propio (es documento fiscal independiente)

### 5.2 Nota de Débito

Documento que **incrementa** una factura previa o genera nuevo débito.

#### Motivos de ND (catálogo)
```sql
MOTIVOS_NOTA_DEBITO:
  1 = Intereses por mora
  2 = Gastos administrativos
  3 = Recargo por servicio adicional
  4 = Ajuste de precio (aumento)
  5 = Flete no facturado originalmente
```

#### Campos Específicos ND
- `Id_ComprobanteOrigen` (FK Comprobantes, factura que se debita)
- `Id_MotivoNotaDebito` (FK)
- `AfectaCuentaCorriente` (bit): típicamente true, debita al cliente
- Renglones: pueden ser nuevos conceptos o ajustes

#### Flujos ND
1. **Desde Factura**: seleccionar factura origen, agregar conceptos/ajustes
2. **Independiente**: crear ND sin origen específico (raro)

Impactos:
- Movimiento cuenta corriente: débito cliente (aumenta saldo deudor)
- Incrementa el `Saldo` de la factura origen (o genera saldo propio)
- Fiscal: obtiene CAE/CDC propio

---

## 6. CUENTA CORRIENTE Y COBRANZAS

### 6.1 Movimientos Cuenta Corriente

Tabla `MOVIMIENTOS_CUENTA_CORRIENTE`:
```sql
Id_Movimiento (PK)
Id_Tercero (FK)
Id_Comprobante (FK, nullable si es movimiento manual)
TipoMovimiento (int: 1=Débito, 2=Crédito)
Concepto (string)
Importe (decimal)
Saldo (decimal, saldo acumulado después del movimiento)
Fecha (date)
FechaVencimiento (date, para facturas/débitos)
Estado (int: 0=Pendiente, 1=Cobrado/Saldado, 2=Vencido, 3=Mora)
UsuarioCreador (string)
FechaCreacion (datetime)
```

Reglas:
- **Débito**: facturas, notas de débito, ajustes débito → aumentan deuda del cliente
- **Crédito**: notas de crédito, recibos, pagos → disminuyen deuda del cliente
- Cálculo `Estado`:
  - `Pendiente`: si `Saldo > 0` y `FechaVencimiento >= hoy`
  - `Vencido`: si `Saldo > 0` y `FechaVencimiento < hoy` y `(hoy - FechaVencimiento) <= 30 días`
  - `Mora`: si `Saldo > 0` y `(hoy - FechaVencimiento) > 30 días`
  - `Cobrado`: si `Saldo = 0`

### 6.2 Recibos de Cobranza

Documento que registra pago del cliente.

#### Tipos de Recibo
1. **Recibo Contado**: pago inmediato, puede ser contra factura o anticipo
2. **Recibo Anticipo**: pago adelantado, sin factura aún (se aplica después)
3. **Recibo Cuenta Corriente**: pago de facturas pendientes, con imputación

#### Medios de Pago
Tabla `RECIBO_MEDIOS_PAGO` (1:N con Recibo):
```sql
Id_MedioPago (PK)
Id_Recibo (FK)
TipoMedioPago (int: 1=Efectivo, 2=Cheque, 3=Transferencia, 4=Tarjeta, 5=Otro)
Importe (decimal)

-- Si tipo = Cheque:
Id_Cheque (FK Cheques)
NumeroCheque (string)
BancoCheque (string)
FechaCheque (date)
FechaVencimientoCheque (date)

-- Si tipo = Transferencia:
NumeroTransferencia (string)
BancoOrigen (string)
BancoDestino (string)
FechaTransferencia (date)

-- Si tipo = Tarjeta:
NumeroTarjeta (string, últimos 4 dígitos)
TipoTarjeta (string: Visa, Mastercard, etc.)
CuponTarjeta (string)
LoteTarjeta (string)
```

#### Imputaciones en Recibo
Tabla `IMPUTACIONES` (muchas-a-muchas entre comprobantes):
```sql
Id_Imputacion (PK)
Id_ComprobanteOrigen (FK Comprobantes, el recibo)
Id_ComprobanteDestino (FK Comprobantes, la factura/débito)
Importe (decimal, cuánto del recibo se aplica a esa factura)
Fecha (date)
UsuarioCreador (string)
FechaCreacion (datetime)
```

Reglas:
- Suma de `Imputaciones.Importe` del recibo ≤ `Total` del recibo
- Al imputar: reduce `Saldo` de la factura destino
- Si factura `Saldo = 0`: cambiar estado a `Cobrado`

### 6.3 Flujos de Cobranza

#### Cobro Inmediato (contra factura)
1. Crear Recibo con `TotalRecibo = TotalFactura`
2. Registrar medio(s) de pago
3. Crear imputación automática `Recibo → Factura`
4. Factura `Saldo = 0`, estado `Cobrado`

#### Cobro Anticipo
1. Crear Recibo sin `Id_ComprobanteDestino`
2. Registrar medios de pago
3. Recibo queda como "anticipo disponible" para cliente
4. Cuando se emite factura posterior:
   - Seleccionar anticipos disponibles
   - Crear imputación `Recibo → Factura`
   - Reduce `Saldo` de factura

#### Cobro Cuenta Corriente
1. Consultar facturas pendientes del cliente
2. Seleccionar facturas a cancelar (total o parcial)
3. Ingresar medios de pago
4. Por cada factura seleccionada:
   - Crear imputación `Recibo → Factura` con importe asignado
   - Actualizar `Saldo` de factura
5. Si `TotalRecibo > suma imputaciones`: queda remanente como anticipo

---

## 7. OPERACIONES MASIVAS

### 7.1 Cierre de Pedidos Masivos

**Formularios zuluApp:**
- `VTACERRARNOTASPEDIDO_Todas.asp`: cierra TODOS los pedidos
- `VTACERRARNOTASPEDIDO_Masivo.asp`: cierra pedidos con filtros

**Filtros disponibles:**
- Sucursal
- Cliente
- Fecha compromiso desde/hasta
- Estado pedido (solo Pendiente, Aprobado, CumplidoParcial)
- Vendedor

**Lógica:**
1. Filtrar pedidos según criterios
2. Mostrar preview de pedidos a cerrar
3. Confirmar acción
4. Por cada pedido:
   - Cambiar `EstadoPedido = Cerrado` (7)
   - Liberar stock reservado (si aplica)
   - Auditar usuario + fecha
5. Generar log de operación masiva

### 7.2 Facturación Masiva desde Remitos

**Formulario zuluApp:** `frmFacturacionMasiva.frm`

**Filtros:**
- Cliente
- Sucursal
- Fecha remito desde/hasta
- Solo remitos sin factura
- Solo remitos no valorizados (requieren factura obligatoria)

**Lógica:**
1. Listar remitos elegibles (sin factura, no anulados)
2. Agrupar por cliente (opcional)
3. Por cada grupo:
   - Crear factura con `Id_ComprobanteOrigen = primer remito`
   - Vincular todos los remitos del grupo (tabla `FACTURA_REMITOS_VINCULADOS`)
   - Consolidar renglones de todos los remitos
   - Si remitos eran no valorizados: generar movimiento cuenta corriente
   - Fiscalizar (opcional, puede ser posterior)
4. Generar reporte de facturas creadas

### 7.3 Facturación Automática Programada

**Formulario zuluApp:** `frmFacturacionAutomatica.frm`

**Configuración:**
- Criterios: clientes con pedidos aprobados mayores a X días
- Criterios: clientes con remitos sin factura mayores a X días
- Frecuencia: diaria, semanal
- Hora ejecución: 02:00 AM (típicamente)

**Lógica:**
1. Job programado ejecuta query con criterios
2. Por cada cliente/documento elegible:
   - Crear factura automáticamente
   - Aplicar reglas de cliente (lista precios, condición pago, descuentos)
   - Fiscalizar si configurado
   - Enviar email notificación (opcional)
3. Generar log de ejecución
4. Notificar errores a administradores

---

## 8. CONSULTAS Y REPORTES

### 8.1 Monitor de Ventas

**Formulario zuluApp:** `frmMonitorVentas.frm`

**Datos mostrados:**
- Ventas del día/mes/año por vendedor
- Ventas por sucursal
- Ventas por zona comercial
- Ranking de productos más vendidos
- Clientes con mayor facturación
- Comisiones generadas por vendedor
- Objetivos vs real (si hay objetivos configurados)

**Filtros:**
- Rango fechas
- Sucursal
- Vendedor
- Zona comercial
- Tipo comprobante (facturas, remitos valorizados)
- Moneda (convertir todo a moneda base)

### 8.2 Estado Cuenta Corriente Cliente

**Formulario zuluApp:** `frmImpresionCuentaCorriente.frm`

**Columnas:**
- Fecha
- Tipo comprobante
- Número comprobante
- Concepto
- Debe (débitos)
- Haber (créditos)
- Saldo acumulado
- Vencimiento
- Estado (Pendiente/Vencido/Mora/Cobrado)
- Días mora (si vencido)

**Filtros:**
- Cliente
- Rango fechas
- Solo pendientes / solo cobrados / todos
- Solo vencidos

**Exportación:**
- PDF
- Excel
- Envío por email

### 8.3 Consulta de Pedidos Vinculados

**Vista zuluApp:** `VTAConsultaNotaPedidoVinculadas_Listado.asp`

**Estructura:**
```
Pedido #12345
├─ Remito #001-00000678 (items: 3, cumplimiento: 100%)
│  └─ Factura #001-00001234 (fiscalizada, CAE: 12345678901234)
├─ Remito #001-00000679 (items: 2, cumplimiento: 50%)
│  └─ (sin factura)
└─ (items pendientes: 1)
```

**Datos por nivel:**
- Pedido: número, fecha, cliente, total, estado
- Remito: número, fecha, items incluidos, cantidad cumplida por item
- Factura: número, fecha, CAE/CDC, total, saldo

---

## 9. GAPS CRÍTICOS IDENTIFICADOS EN BACKEND ACTUAL

### 9.1 Entidad Comprobante

| Campo | Existe | Falta | Prioridad |
|-------|--------|-------|-----------|
| Vendedor, Cobrador, Comisiones | ❌ | ✅ | **ALTA** |
| ZonaComercial, ListaPrecios, CondicionPago | ❌ | ✅ | **ALTA** |
| CanalVenta | ❌ | ✅ | MEDIA |
| DescuentoPorcentaje, RecargoPorcentaje, RecargoImporte | ❌ | ✅ | ALTA |
| ObservacionInterna, ObservacionFiscal | ❌ | ✅ | MEDIA |
| Logística completa (ruta, peso, volumen, bultos, embalaje) | ⚠️ Parcial | ✅ | ALTA (remitos) |
| EstadoEntregaRemito | ❌ | ✅ | ALTA (remitos) |
| DniQuienRecibe, FirmaConformidad | ❌ | ✅ | MEDIA |
| UsuarioCreador, UsuarioModificador | ❌ | ✅ | ALTA (auditoría) |
| DomicilioTerceroSnapshot | ❌ | ✅ | MEDIA |

### 9.2 Entidad ComprobanteItem

| Campo | Existe | Falta | Prioridad |
|-------|--------|-------|-----------|
| Lote, Serie, FechaVencimiento | ❌ | ✅ | ALTA |
| UnidadMedidaId, CantidadSecundaria | ❌ | ✅ | MEDIA |
| Concepto (observación renglón) | ❌ | ✅ | MEDIA |
| PrecioListaOriginal | ❌ | ✅ | MEDIA |
| ComisionVendedorRenglon | ❌ | ✅ | BAJA |
| Id_ItemPedidoOrigen, CantidadCumplidaDelPedido | ❌ | ✅ | **ALTA** |

### 9.3 Tabla CMP_COT (Paraguay)

| Elemento | Existe | Falta | Prioridad |
|----------|--------|-------|-----------|
| Entidad ComprobanteTransporte | ❌ | ✅ | **CRÍTICA** |
| Cot_Valor (número COT) | ❌ | ✅ | CRÍTICA |
| Cot_fecha (vigencia) | ❌ | ✅ | CRÍTICA |
| Validación longitud ≥ 6 | ❌ | ✅ | CRÍTICA |
| Validación fecha ≥ fecha emisión | ❌ | ✅ | CRÍTICA |

### 9.4 Pedidos como Entidad Independiente

| Elemento | Existe | Falta | Prioridad |
|----------|--------|-------|-----------|
| Entidad Pedido (no solo borrador) | ❌ | ✅ | **CRÍTICA** |
| Entidad PedidoItem con seguimiento | ❌ | ✅ | CRÍTICA |
| EstadoPedido (Pendiente, Aprobado, etc.) | ❌ | ✅ | CRÍTICA |
| EstadoItemPedido (NoEntregado, Parcial, etc.) | ❌ | ✅ | CRÍTICA |
| FechaCompromisoEntrega | ❌ | ✅ | ALTA |
| Prioridad | ❌ | ✅ | MEDIA |
| StockReservado, reserva efectiva | ❌ | ✅ | ALTA |
| Aprobación (usuario, fecha, motivo rechazo) | ❌ | ✅ | ALTA |
| CanalOrigen | ❌ | ✅ | MEDIA |
| CantidadEntregada, CantidadPendiente por item | ❌ | ✅ | CRÍTICA |
| Atraso (flag si vencido) | ❌ | ✅ | MEDIA |

### 9.5 Vínculo Pedido → Remito/Factura

| Elemento | Existe | Falta | Prioridad |
|----------|--------|-------|-----------|
| Vínculo renglón a renglón | ❌ | ✅ | **CRÍTICA** |
| CantidadCumplida por conversión | ❌ | ✅ | CRÍTICA |
| Actualización automática EstadoItemPedido | ❌ | ✅ | CRÍTICA |
| Query consulta vinculaciones | ❌ | ✅ | ALTA |

### 9.6 Anticipos y Aplicación

| Elemento | Existe | Falta | Prioridad |
|----------|--------|-------|-----------|
| Tabla AnticiposAplicadosFactura | ❌ | ✅ | ALTA |
| Comando aplicar anticipo a factura | ❌ | ✅ | ALTA |
| Query anticipos disponibles por cliente | ❌ | ✅ | ALTA |
| DTO con anticipos aplicados en factura | ❌ | ✅ | MEDIA |

### 9.7 Vínculo Remito(s) → Factura

| Elemento | Existe | Falta | Prioridad |
|----------|--------|-------|-----------|
| Tabla FacturaRemitosVinculados (M:N) | ❌ | ✅ | ALTA |
| Factura consolidando múltiples remitos | ❌ | ✅ | ALTA |
| Query remitos sin factura | ❌ | ✅ | ALTA |

### 9.8 Operaciones Masivas

| Elemento | Existe | Falta | Prioridad |
|----------|--------|-------|-----------|
| Comando CerrarPedidosMasivo | ❌ | ✅ | ALTA |
| Comando FacturarRemitosM asivo | ❌ | ✅ | ALTA |
| Job FacturacionAutomatica | ❌ | ✅ | MEDIA |
| Auditoría operaciones masivas | ❌ | ✅ | MEDIA |

### 9.9 Queries y Filtros

| Consulta | Existe | Falta | Prioridad |
|----------|--------|-------|-----------|
| GetPedidosConFiltrosLegacy | ❌ | ✅ | **ALTA** |
| GetRemitosConFiltrosCOT | ❌ | ✅ | ALTA |
| GetPedidosVinculados (árbol documental) | ❌ | ✅ | ALTA |
| GetEstadoCumplimientoPedido | ❌ | ✅ | ALTA |
| GetRemitosLogisticaExpedicion | ❌ | ✅ | MEDIA |
| GetAnticiposDisponibles | ❌ | ✅ | MEDIA |
| GetRemitosSinFacturar | ❌ | ✅ | ALTA |

### 9.10 DTOs

| DTO | Existe | Falta | Prioridad |
|-----|--------|-------|-----------|
| VendedorSelectorDto | ❌ | ✅ | ALTA |
| CobradorSelectorDto | ❌ | ✅ | ALTA |
| ZonaComercialDto | ❌ | ✅ | ALTA |
| ListaPreciosDto | ❌ | ✅ | ALTA |
| CondicionPagoDto | ❌ | ✅ | ALTA |
| CanalVentaDto | ❌ | ✅ | MEDIA |
| PedidoDetalleDto (específico) | ❌ | ✅ | **CRÍTICA** |
| PedidoItemDto con cumplimiento | ❌ | ✅ | CRÍTICA |
| ComprobanteTransporteDto (COT) | ❌ | ✅ | CRÍTICA |
| RemitoCabAtributoDto | ❌ | ✅ | MEDIA |
| AnticipoAplicadoDto | ❌ | ✅ | MEDIA |
| ComprobanteDetalleDto extendido | ⚠️ Parcial | ✅ | ALTA |

---

## 10. RESUMEN PRIORIZACIÓN

### CRÍTICO (Bloquea frontend nuevo)
1. **Entidad Pedido independiente** con estados propios, seguimiento cumplimiento, aprobación
2. **Vínculo renglón a renglón** Pedido → Remito/Factura con cantidades cumplidas
3. **ComprobanteTransporte (COT)** para remitos Paraguay con validaciones
4. **Queries pedidos con filtros legacy** (estado item, atraso, cliente, etc.)
5. **Campos vendedor/cobrador/comisiones** en Comprobante y DTO
6. **Lote/Serie/Vencimiento** en ComprobanteItem

### ALTA (Debe completarse antes de release)
1. **ZonaComercial, ListaPrecios, CondicionPago** en Comprobante
2. **Logística completa remitos** (peso, volumen, bultos, estado entrega)
3. **Descuento%/Recargo% globales** en Comprobante
4. **Query remitos sin facturar** y **facturación masiva desde remitos**
5. **Anticipos aplicados** a facturas
6. **Cierre pedidos masivo** con filtros
7. **Auditoría completa** (usuario creador/modificador)

### MEDIA (Puede posponerse si hay presión)
1. **ObservacionInterna/Fiscal** separadas
2. **CanalVenta**
3. **RemitoCabAtributo** (metadatos dinámicos)
4. **Job facturación automática**
5. **Consulta pedidos vinculados** (árbol documental completo)
6. **DniQuienRecibe, FirmaConformidad** en remitos

### BAJA (Nice to have)
1. **ComisionVendedorRenglon** (cálculo por línea vs global)
2. **DomicilioTerceroSnapshot** en comprobante
3. **Reportes avanzados** (monitor ventas, ranking productos)

---

## 11. PLAN DE IMPLEMENTACIÓN RECOMENDADO

### Fase 1: Pedidos Completos (CRÍTICO)
**Duración estimada:** 3-4 días
1. Crear entidad `Pedido` y `PedidoItem` con todos los campos
2. Crear enums `EstadoPedido`, `EstadoItemPedido`
3. Implementar comandos:
   - `CreatePedidoCommand`
   - `UpdatePedidoCommand`
   - `AprobarPedidoCommand`
   - `RechazarPedidoCommand`
   - `CerrarPedidoCommand`
   - `CerrarPedidosMasivoCommand`
4. Implementar queries:
   - `GetPedidosPagedQuery` (con todos los filtros legacy)
   - `GetPedidoByIdQuery`
   - `GetEstadoCumplimientoPedidoQuery`
5. Crear DTOs: `PedidoDetalleDto`, `PedidoListDto`, `PedidoItemDto`
6. Migración DB: tablas `Pedidos`, `PedidosItems`

### Fase 2: Conversión Pedido → Remito/Factura (CRÍTICO)
**Duración estimada:** 2-3 días
1. Modificar `ComprobanteItem` para agregar:
   - `PedidoItemId` (FK nullable)
   - `CantidadCumplidaDelPedido`
2. Implementar `ConvertirPedidoADocumentoCommand`:
   - Validar cantidades disponibles
   - Crear comprobante destino (remito o factura)
   - Vincular renglones con `PedidoItemId`
   - Actualizar `CantidadEntregada` en `PedidoItem`
   - Recalcular estados pedido y items
3. Query `GetPedidosVinculadosQuery` (árbol documental)
4. Migración DB: columnas en `ComprobantesItems`

### Fase 3: COT Paraguay (CRÍTICO)
**Duración estimada:** 1-2 días
1. Crear entidad `ComprobanteTransporte`
2. Validadores:
   - `CotValidator`: longitud ≥ 6, fecha ≥ fecha emisión
3. Integrar en `CreateComprobanteCommand` y `EmitirRemitoCommand`
4. DTO: `ComprobanteTransporteDto`
5. Migración DB: tabla `ComprobantesTransporte`

### Fase 4: Campos Comerciales Comprobante (ALTA)
**Duración estimada:** 2 días
1. Agregar a `Comprobante`:
   - `VendedorId`, `CobradorId`
   - `PorcentajeComisionVendedor`, `ImporteComisionVendedor`
   - `PorcentajeComisionCobrador`
   - `ZonaComercialId`, `ListaPreciosId`, `CondicionPagoId`, `PlazoDias`
   - `DescuentoPorcentaje`, `RecargoPorcentaje`, `RecargoImporte`
   - `CanalVentaId`
   - `ObservacionInterna`, `ObservacionFiscal`
   - `UsuarioCreador`, `UsuarioModificador`
2. Actualizar `ComprobanteDetalleDto` para exponer todos estos campos
3. Actualizar validators
4. Migración DB: columnas en `Comprobantes`

### Fase 5: Logística Remitos Completa (ALTA)
**Duración estimada:** 2 días
1. Agregar a `Comprobante`:
   - `RutaLogistica`
   - `PesoTotal`, `VolumenTotal`, `Bultos`, `TipoEmbalaje`
   - `SeguroTransporte`, `ValorDeclarado`
   - `EstadoEntregaRemito` (enum)
   - `DniQuienRecibe`
2. Crear entidad `RemitoCabAtributo` (atributos dinámicos)
3. Actualizar `EmitirRemitoCommand` con validaciones logísticas
4. Comandos:
   - `DespacharRemitoCommand` (cambiar estado a EnTránsito)
   - `ConfirmarEntregaRemitoCommand` (cambiar a Entregado, capturar conformidad)
5. Migración DB: columnas + tabla `RemitosAtributos`

### Fase 6: Lote/Serie/Vencimiento en Items (CRÍTICO)
**Duración estimada:** 1 día
1. Agregar a `ComprobanteItem`:
   - `Lote`, `Serie`, `FechaVencimiento`
   - `UnidadMedidaId`, `CantidadSecundaria`
   - `Concepto` (observación renglón)
   - `PrecioListaOriginal`
2. Actualizar `ComprobanteItemDto`
3. Actualizar validators (validar vencimiento si producto lo requiere)
4. Migración DB: columnas en `ComprobantesItems`

### Fase 7: Anticipos y Aplicación (ALTA)
**Duración estimada:** 1-2 días
1. Crear entidad `AnticipoAplicado`
2. Comandos:
   - `AplicarAnticipoAFacturaCommand`
   - `QuitarAnticipoDeFacturaCommand`
3. Query `GetAnticiposDisponiblesQuery`
4. Actualizar `ComprobanteDetalleDto` con colección `AnticiposAplicados`
5. Migración DB: tabla `AnticiposAplicados`

### Fase 8: Vínculo Múltiple Remitos → Factura (ALTA)
**Duración estimada:** 1 día
1. Crear entidad `FacturaRemitoVinculo` (M:N)
2. Modificar `ConvertirDocumentoVentaCommand` para soportar múltiples remitos origen
3. Query `GetRemitosSinFacturarQuery`
4. Comando `FacturarRemitosMasivoCommand`
5. Migración DB: tabla `FacturasRemitosVinculados`

### Fase 9: DTOs Catálogos Comerciales (ALTA)
**Duración estimada:** 1 día
1. Crear DTOs:
   - `VendedorSelectorDto`
   - `CobradorSelectorDto`
   - `ZonaComercialDto`
   - `ListaPreciosDto`
   - `CondicionPagoDto`
   - `CanalVentaDto`
2. Queries selector:
   - `GetVendedoresActivosQuery`
   - `GetCobradoresActivosQuery`
   - `GetZonasComercialesQuery`
   - `GetListasPreciosQuery`
   - `GetCondicionesPagoQuery`

### Fase 10: Tests y Smoke (ALTA)
**Duración estimada:** 2 días
1. Unit tests críticos:
   - Pedido: crear, aprobar, cerrar
   - Conversión pedido → remito/factura
   - Aplicar anticipo
   - Validación COT
2. Integration tests:
   - Flujo completo pedido → remito → factura
   - Imputaciones
3. Smoke tests:
   - Queries con filtros
   - DTOs sin nulls inesperados
4. Compilar, run tests, fix issues

---

## 12. CRITERIOS DE ACEPTACIÓN

### Pedidos
- ✅ Puedo crear pedido con cliente, items, fecha compromiso
- ✅ Puedo aprobar/rechazar pedido con motivo
- ✅ Puedo convertir pedido a remito parcial o total
- ✅ Puedo convertir pedido a factura parcial o total
- ✅ Al convertir, cantidades se actualizan correctamente en pedido origen
- ✅ Puedo consultar estado cumplimiento con filtros legacy (cliente, fecha, atraso, item)
- ✅ Puedo cerrar pedidos masivamente con filtros
- ✅ Puedo ver árbol documental: Pedido → Remitos → Facturas

### Remitos
- ✅ Puedo crear remito valorizado/no valorizado desde pedido
- ✅ Puedo ingresar datos logísticos completos (transporte, chofer, patentes, etc.)
- ✅ Para Paraguay: COT es obligatorio y valida longitud ≥ 6 y fecha ≥ fecha emisión
- ✅ Puedo despachar remito (cambiar a EnTránsito)
- ✅ Puedo confirmar entrega con datos receptor
- ✅ Puedo consultar remitos con filtros: COT, depósito, estado entrega

### Facturas
- ✅ Puedo crear factura desde pedido
- ✅ Puedo crear factura desde uno o múltiples remitos
- ✅ Puedo aplicar anticipos del cliente a factura
- ✅ Datos comerciales completos: vendedor, cobrador, comisiones, zona, lista precios
- ✅ Factura muestra descuento% y recargo% además de importes
- ✅ DTO de factura expone todos los datos visibles en zuluApp

### Cuenta Corriente
- ✅ Factura genera movimiento débito en cuenta corriente
- ✅ NC genera movimiento crédito
- ✅ ND genera movimiento débito
- ✅ Recibo con imputaciones reduce saldo de facturas
- ✅ Puedo consultar estado cuenta corriente con filtros

### Operaciones Masivas
- ✅ Puedo cerrar pedidos masivamente con preview
- ✅ Puedo facturar remitos masivamente agrupando por cliente
- ✅ Auditoría registra usuario + fecha de operaciones masivas

---

## 13. CONCLUSIÓN

Este documento consolida **todos** los campos, entidades, flujos y operaciones identificados en `zuluApp` para el módulo de ventas/operaciones.

**Estado actual backend:** Cubre ~40-50% de la funcionalidad legacy.

**Gaps críticos:** Pedidos como entidad propia, vínculo renglón a renglón, COT Paraguay, campos comerciales completos, lote/serie/vencimiento.

**Plan sugerido:** 10 fases, ~15-20 días desarrollo total para alcanzar paridad 100%.

**Siguiente paso:** Implementar Fase 1 (Pedidos Completos) y validar con stakeholders antes de avanzar.

---

**Fin del análisis.**
