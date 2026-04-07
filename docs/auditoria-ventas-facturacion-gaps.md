# Auditoría Contratos Ventas/Facturación - zuluApp vs ZuluIA_Back

**Fecha:** $(Get-Date)
**Objetivo:** Identificar gaps funcionales en DTOs de ventas/facturación comparando con zuluApp legacy

## 1. FACTURAS DE VENTA

### 1.1 Campos Legacy detectados en `frmFacturaVenta`

#### Cabecera de Factura
- **Identificación básica:** número, prefijo, letra, fecha, fecha vencimiento, sucursal, punto facturación
- **Tercero:** razón social, CUIT, condición IVA, domicilio completo
- **Comercial:** vendedor (nombre + legajo), cobrador (nombre + legajo), zona comercial, lista de precios
- **Logístico:** transporte, chofer, domicilio entrega, observaciones logísticas
- **Fiscal:** tipo comprobante, CAE, fecha vto CAE, timbrado (Paraguay), estado AFIP/SIFEN
- **Monetario:** moneda, cotización, condición de pago, plazo días
- **Documental:** comprobante origen (pedido/remito), estado (borrador/emitido/anulado/fiscal)
- **Comisiones:** % comisión vendedor, importe comisión, % comisión cobrador
- **Observaciones:** observación comercial, observación interna, observación fiscal

#### Detalle de Items
- Código item, descripción, cantidad, precio unitario, descuento %, IVA %, subtotal, total
- Depósito, lote, serie, vencimiento (si aplica)
- Atributos comerciales dinámicos por item
- Observaciones por renglón
- Bonificaciones aplicadas

#### Totales y Agregados
- Subtotal, descuento global, recargo global, percepciones, retenciones, IVA discriminado, total
- Saldo pendiente, pagos anticipos aplicados, pagos contra entrega
- Totales por alícuota IVA

#### Estados y Flujos
- **Borrador:** editable, no impacta stock ni cuenta corriente
- **Pre-factura:** reserva información pero no numera definitivamente
- **Emitida:** numerada, impacta stock/cuenta corriente, puede fiscalizarse
- **Fiscalizada:** CAE/CDC obtenido, no modificable
- **Anulada:** NC asociada o anulación directa

### 1.2 Campos Actuales en Backend (ComprobanteDetalleDto)

✅ **CUBIERTO:**
- Identificación: Id, Prefijo, Numero, NumeroFormateado, Fecha, FechaVencimiento
- Sucursal: SucursalId, SucursalRazonSocial, PuntoFacturacionId
- Tipo: TipoComprobanteId, Descripcion, Codigo
- Tercero: TerceroId, RazonSocial, Cuit, CondicionIva
- Moneda: MonedaId, Simbolo, Cotizacion
- Totales: Subtotal, DescuentoImporte, NetoGravado, NetoNoGravado, IvaRi, IvaRni, Percepciones, Retenciones, Total, Saldo
- Fiscal AFIP: Cae, Caea, FechaVtoCae, EstadoAfip, UltimoErrorAfip, FechaUltimaConsultaAfip, QrData
- Fiscal SIFEN: EstadoSifen, CodigoRespuesta, MensajeRespuesta, TrackingId, Cdc, NumeroLote, FechaRespuesta, TimbradoId, NroTimbrado
- Estado: Estado
- Items: colección Items con detalle
- Imputaciones: colección Imputaciones
- Observacion: Observacion
- Auditoría: CreatedAt, UpdatedAt

❌ **FALTANTE:**
1. **Vendedor:** VendedorId, VendedorNombre, VendedorLegajo, PorcentajeComisionVendedor, ImporteComisionVendedor
2. **Cobrador:** CobradorId, CobradorNombre, CobradorLegajo, PorcentajeComisionCobrador
3. **Zona comercial:** ZonaComercialId, ZonaComercialDescripcion
4. **Lista de precios:** ListaPreciosId, ListaPreciosDescripcion
5. **Condición de pago:** CondicionPagoId, CondicionPagoDescripcion, PlazoDias
6. **Logística:** TransporteId, TransporteRazonSocial, ChoferNombre, ChoferDni, DomicilioEntregaCompleto, ObservacionesLogisticas
7. **Comprobante origen:** ComprobanteOrigenId, ComprobanteOrigenNumero, ComprobanteOrigenTipo (solo guardamos ID, no exponemos detalle)
8. **Recargo global:** RecargoImporte, RecargoPorcentaje
9. **Descuento global %:** DescuentoPorcentaje (solo tenemos importe)
10. **Anticipos aplicados:** colección de pagos anticipados aplicados al comprobante
11. **Observación interna:** ObservacionInterna (separada de Observacion pública)
12. **Observación fiscal:** ObservacionFiscal
13. **Usuario creador:** UsuarioCreador, UsuarioModificador
14. **Domicilio tercero en momento factura:** DomicilioTercero (snapshot del momento)
15. **Canal de venta:** CanalVentaId (mostrador, web, telefónico, etc.)

⚠️ **PARCIAL:**
- **Estado documental:** existe EstadoComprobante pero no distingue claramente Borrador/PreFactura/Emitido/Fiscalizado
- **Vínculo origen-destino:** existe ComprobanteOrigenId pero no se expone descripción ni se navega fácilmente

### 1.3 Campos Faltantes en Items (ComprobanteItemDto)

❌ **FALTANTE:**
1. Lote, Serie, Vencimiento del producto
2. Unidad de medida (si difiere de la base del item)
3. ObservacionRenglon
4. PrecioListaOriginal (antes de descuentos)
5. ComisionVendedorRenglon (si se calcula por línea)
6. Marca comercial, jurisdicción, maestros auxiliares (si se replican por renglón)
7. Cantidad pendiente de entregar (si factura desde pedido parcialmente cumplido)
8. Renglón origen (si viene de remito/pedido, vincular al renglón específico)

---

## 2. PEDIDOS / NOTAS DE PEDIDO

### 2.1 Campos Legacy detectados en `frmNotaPedido`

Similar a factura pero con:
- **Estado pedido:** Pendiente, Aprobado, Rechazado, En Preparación, Cumplido Parcial, Cumplido Total, Anulado
- **Fecha compromiso:** FechaCompromisoEntrega
- **Prioridad:** PrioridadPedido (Alta, Media, Baja)
- **Reserva de stock:** StockReservado (flag o cantidad reservada por renglón)
- **Cumplimiento:** CantidadEntregada, CantidadPendiente por renglón
- **Aprobación:** UsuarioAprobador, FechaAprobacion, MotivoRechazo
- **Canal:** CanalOrigen (web, teléfono, mostrador, vendedor)

### 2.2 Estado Actual Backend

El backend actual trata pedidos como **borradores de comprobante genéricos**:
- `CrearBorradorVentaCommand` con tipoComprobanteId de "Nota de Pedido"
- No existe entidad específica `Pedido` con lógica propia
- No hay reserva de stock
- No hay estados propios de pedido
- No hay aprobación formal
- No hay seguimiento de cumplimiento por renglón

❌ **GAP CRÍTICO:** El pedido debe ser entidad/agregado propio con:
1. Estados propios (Pendiente, Aprobado, Rechazado, EnPreparacion, Parcial, Cumplido, Anulado)
2. Reserva y liberación de stock
3. Aprobación workflow
4. Seguimiento de cumplimiento parcial/total por renglón
5. Conversión a remito/factura con trazabilidad de cantidades

---

## 3. REMITOS DE VENTA

### 3.1 Campos Legacy en `frmRemitosVentasValorizados` / `frmRemitosVentasNoValorizados`

Además de campos comunes a factura:
- **Valorizado:** EsValorizado (flag que determina si impacta cuenta corriente)
- **Logística completa:**
  - Transporte, chofer, patente vehículo, patente acoplado
  - Domicilio entrega completo
  - Ruta, observaciones logísticas
  - Fecha estimada entrega, fecha real entrega
  - Firma conformidad, nombre quien recibe
- **Atributos de remito:**
  - Peso total, volumen total, bultos
  - Tipo embalaje, condiciones especiales
  - Seguro transporte, valor declarado
- **Estado entrega:**
  - Pendiente, En Tránsito, Entregado, Entregado Parcial, Rechazado, Devuelto
- **Vínculo pedido:** PedidoOrigenId, cantidades cumplidas por renglón del pedido

### 3.2 Estado Actual Backend

✅ Existe endpoint `EmitirRemito` con flags `EsValorizado` y `AfectaStock`

❌ **FALTANTE:**
1. Datos logísticos completos (transporte, chofer, patentes, ruta)
2. Atributos de remito (peso, volumen, bultos, embalaje)
3. Estado de entrega propio
4. Firma/recepción
5. Vínculo formal con pedido y actualización de cumplimiento
6. Entrega parcial con múltiples remitos por pedido

---

## 4. NOTAS DE CRÉDITO / DÉBITO

### 4.1 Campos Legacy en `frmNotaCreditoVenta` / `frmNotaDebitoVenta`

- **Vínculo origen:** FacturaOrigenId, FacturaOrigenNumero (no solo ID genérico)
- **Motivo tipificado:** MotivoNCId, MotivoNDId (catálogo cerrado de motivos fiscales/comerciales)
- **Detalle devolución:**
  - Por cada renglón: ItemFacturaOrigenId, CantidadDevuelta, MotivoDevolRenglon
- **Impacto stock:** ReingresaStock (flag por NC)
- **Impacto cuenta corriente:** AcreditaCuentaCorriente, DebitaCuentaCorriente
- **Observaciones:** ObservacionMotivo (textual extendida)

### 4.2 Estado Actual Backend

⚠️ **PARCIAL:** Existe `RegistrarDevolucionVentaCommand` con:
- ComprobanteOrigenId
- ReingresaStock
- AcreditaCuentaCorriente
- Pero NO expone: motivo tipificado, vínculo detallado por renglón origen

❌ **FALTANTE:**
1. Catálogo de motivos NC/ND
2. Vínculo renglón-a-renglón con factura origen
3. Exposición clara en DTO de detalle del vínculo con origen
4. Validación de cantidades devueltas vs facturadas

---

## 5. DEVOLUCIONES (Vista Independiente)

### 5.1 Legacy en `frmDevolucionVentaStock` / `frmDevolucionVentaNoValorizada`

- Listado específico de devoluciones (no mezcla con NC genéricas)
- Filtros por: cliente, fecha, item, motivo, usuario
- Estados: Pendiente, Autorizada, Rechazada, Procesada (NC emitida)
- Workflow de autorización previo a NC
- Diferencias por renglón (cantidad, importe, motivo)

### 5.2 Estado Actual Backend

❌ **GAP:** No existe endpoint dedicado de listado/gestión de devoluciones
- La UX actual depende de `useLegacyLocalCollection` (estado local)
- El comando `RegistrarDevolucionVentaCommand` existe pero no hay lectura estructurada

**REQUERIDO:**
1. Entidad `DevolucionVenta` separada o vista materializada
2. Query `GetDevolucionesPagedQuery`
3. Estados propios de devolución con workflow
4. Autorización previa a emisión NC

---

## 6. AJUSTES DE VENTAS

### 6.1 Legacy en `frmAjustesVentaCredito` / `frmAjustesVentaDebito` / `frmAjustesPV`

- Ajustes comerciales/fiscales post-factura
- Motivos tipificados (diferencia precio, error carga, bonificación posterior, etc.)
- Impacto selectivo en stock, cuenta corriente, IVA
- Autorización requerida por monto/tipo
- Auditoría completa

### 6.2 Estado Actual Backend

❌ **GAP TOTAL:** No existe módulo de ajustes de ventas

**REQUERIDO:**
1. Entidad `AjusteVenta`
2. Comandos `RegistrarAjusteCreditoCommand`, `RegistrarAjusteDebitoCommand`
3. Catálogo de motivos
4. Workflow de autorización
5. Impacto configurable en stock/CC/fiscal

---

## 7. LISTAS DE PRECIOS

### 7.1 Legacy en `frmListasPrecios`

- Lista con vigencia desde/hasta
- Herencia entre listas (lista base + ajustes)
- Precios especiales por cliente
- Precios especiales por canal (mostrador vs web)
- Precios especiales por vendedor
- Promociones con vigencia
- Motor de resolución: ¿qué precio aplicar?

### 7.2 Estado Actual Backend

⚠️ **PARCIAL:** Existe gestión básica de listas pero:

❌ **FALTANTE:**
1. Herencia entre listas
2. Precios especiales por cliente/canal/vendedor
3. Promociones con vigencia
4. **Motor de resolución de precio aplicable** (endpoint que dado cliente+item+fecha+cantidad retorna precio final)
5. Endpoints de impresión/exportación

---

## 8. DESCUENTOS COMERCIALES

### 8.1 Legacy en `frmDescuentos` / `frmDescuento_AM`

- Descuento por cliente + item con vigencia
- Descuento por categoría cliente + rubro item
- Prioridades de aplicación
- % máximo de descuento del cliente (tope)
- Validación en emisión de factura

### 8.2 Estado Actual Backend

❌ **GAP TOTAL:** No existe módulo de descuentos comerciales
- El campo `% máximo descuento` en Tercero no está expuesto formalmente
- No hay gestión de descuentos estructurados
- No hay validación en emisión de comprobantes

**REQUERIDO:**
1. Entidad `DescuentoComercial`
2. CRUD completo
3. Validación en `EmitirDocumentoVentaCommand`
4. Exposición de % máximo en `TerceroDto`

---

## 9. DASHBOARD Y KPIs VENTAS

### 9.1 Legacy en `frmMonitorVentas` / `frmVerificarObjetivosVenta`

- KPIs en tiempo real:
  - Ventas del día/mes/año por vendedor/zona/sucursal
  - Pedidos pendientes, remitos sin facturar
  - Facturas pendientes de cobro, deuda vencida
  - Objetivos vs real por vendedor
- Alertas operativas:
  - Clientes bloqueados
  - Stock bajo en items de venta
  - Documentos sin CAE
  - Pedidos atrasados

### 9.2 Estado Actual Backend

❌ **GAP:** No existe endpoint dedicado de dashboard/KPIs ventas

**REQUERIDO:**
1. `GET /api/ventas/dashboard` con métricas agregadas
2. Filtros por sucursal, vendedor, zona, período
3. Alertas operativas

---

## 10. CUENTA CORRIENTE Y AGING

### 10.1 Legacy en `frmImpresionCuentaCorriente` / `frmExportarCuentaCorrienteClientes`

- Saldo actual por cliente
- Movimientos detallados (facturas, NC, cobros)
- Aging por tramos (0-30, 31-60, 61-90, 91-120, >120 días)
- Filtros por cliente, vendedor, zona, sucursal
- Exportación Excel/PDF
- Impresión formal

### 10.2 Estado Actual Backend

⚠️ **PARCIAL:** Existe query base pero:

❌ **FALTANTE:**
1. Aging por tramos
2. Filtros por vendedor, zona
3. Endpoints de exportación/impresión
4. Consolidado por cliente con drill-down a movimientos

---

## 11. IMPUTACIONES AVANZADAS

### 11.1 Legacy en `frmImputacionesVentas` / `frmImputacionesVentasMasivas`

- Imputación simple
- Imputación masiva (múltiples cobros a múltiples facturas)
- Desimputación simple/masiva
- Vinculación/desvinculación manual de comprobantes
- Auditoría completa
- Filtros por tipo, fecha, estado, cliente, importe

### 11.2 Estado Actual Backend

⚠️ **PARCIAL:** Existen comandos básicos pero:

❌ **FALTANTE:**
1. Endpoint agregado para pantalla completa de imputaciones
2. Modelo consolidado para facilitar UX
3. Validación completa de saldo residual y parcialidades
4. Filtros avanzados

---

## RESUMEN EJECUTIVO DE GAPS

### CRÍTICOS (Bloquean reemplazo funcional)
1. **Vendedor/Cobrador/Zona en comprobantes:** no se exponen en DTO
2. **Logística en remitos:** sin transporte, chofer, patentes, datos entrega
3. **Pedidos sin lógica propia:** no hay reserva stock, aprobación, seguimiento cumplimiento
4. **Listas precios sin motor resolución:** no hay endpoint que calcule precio aplicable
5. **Descuentos comerciales inexistentes:** no hay gestión ni validación
6. **Devoluciones sin backend dedicado:** UX depende de estado local
7. **Ajustes de ventas inexistentes:** funcionalidad completa faltante

### ALTOS (Reducen valor operativo)
8. **Dashboard/KPIs ventas:** no hay endpoint agregado
9. **Aging cuenta corriente:** no expuesto
10. **Imputaciones sin endpoint agregado:** UX compleja sin backend consolidado
11. **Atributos logísticos remito:** peso, volumen, firma, estado entrega
12. **Motivos NC/ND tipificados:** no hay catálogo formal
13. **Vínculo renglón-a-renglón NC con factura origen:** no implementado
14. **Facturación masiva/automática:** existe base pero no completa

### MEDIOS (Mejoran UX pero no bloquean)
15. **Recargo global en factura**
16. **Descuento global % (solo tenemos importe)**
17. **Anticipos aplicados en factura**
18. **Observación interna/fiscal separadas**
19. **Usuario creador/modificador visible**
20. **Domicilio tercero snapshot en factura**
21. **Canal de venta**
22. **Lote/Serie/Vencimiento en renglón**
23. **Precio lista original antes de descuentos**
24. **Comisión por renglón**

---

## PRÓXIMOS PASOS RECOMENDADOS

### Paso 1: Ampliar DTOs de Comprobante (CRÍTICO)
Agregar a `ComprobanteDetalleDto` y `ComprobanteDto`:
- VendedorId, VendedorNombre, VendedorLegajo
- CobradorId, CobradorNombre, CobradorLegajo
- ZonaComercialId, ZonaComercialDescripcion
- ListaPreciosId, ListaPreciosDescripcion
- CondicionPagoId, CondicionPagoDescripcion, PlazoDias
- TransporteId, TransporteRazonSocial (para remitos)
- ChoferNombre, ChoferDni, DomicilioEntregaCompleto
- ComprobanteOrigenNumero, ComprobanteOrigenTipo (además del ID)
- CanalVentaId

### Paso 2: Crear Pedidos con Lógica Propia (CRÍTICO)
- Diseñar entidad/agregado `Pedido` separado de `Comprobante` genérico
- Estados propios, reserva stock, aprobación, cumplimiento por renglón
- Conversión a remito/factura con trazabilidad

### Paso 3: Motor Resolución Precio (CRÍTICO)
- Endpoint `GET /api/listas-precios/resolver?clienteId=X&itemId=Y&fecha=Z&cantidad=N`
- Retorna precio aplicable con regla de prioridad

### Paso 4: Descuentos Comerciales (CRÍTICO)
- Entidad `DescuentoComercial`, CRUD, validación en emisión facturas

### Paso 5: Devoluciones Backend Dedicado (ALTO)
- Query `GetDevolucionesPagedQuery`
- Estados propios, autorización previa a NC

### Paso 6: Ajustes de Ventas (ALTO)
- Entidad `AjusteVenta`, comandos, catálogo motivos, workflow autorización

### Paso 7: Dashboard/KPIs Ventas (ALTO)
- Endpoint agregado con métricas en tiempo real

### Paso 8: Aging y Cuenta Corriente Completa (ALTO)
- Aging por tramos, exportación, impresión

### Paso 9: Imputaciones Endpoint Agregado (MEDIO/ALTO)
- Modelo consolidado para UX sin estado local

### Paso 10: Atributos Logísticos Remito (MEDIO)
- Peso, volumen, firma, estado entrega

---

**Conclusión:** Existen **gaps críticos** que impiden reemplazo funcional completo de zuluApp. Priorizar pasos 1-6 antes de considerar paridad alcanzada.
