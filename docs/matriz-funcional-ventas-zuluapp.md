# Matriz funcional de `Ventas` contra `zuluApp`

## Objetivo
Esta matriz define el alcance funcional que debe cubrir el backend para que `C:\Zulu\ZuluIA_Front\app\ventas` pueda reemplazar al módulo legado de `C:\Zulu\zuluApp` sin faltantes de datos, operaciones ni trazabilidad.

## Regla de trabajo
- `zuluApp` es la referencia funcional principal.
- El criterio es `backend-first`: primero cerrar faltantes de API, DTOs, catálogos, estados, relaciones y validaciones; después ajustar el frontend.
- Una vista no se considera cerrada por poder listar o guardar: debe cubrir todos los datos visibles, acciones críticas, estados operativos, filtros, trazabilidad e impresión/exportación que el legado requería.

## Fuentes relevadas
### Frontend nuevo
- `app/ventas/page.tsx`
- `app/ventas/clientes/page.tsx`
- `app/ventas/cobros/page.tsx`
- `app/ventas/cheques/page.tsx`
- `app/ventas/cuenta-corriente/page.tsx`
- `app/ventas/descuentos/page.tsx`
- `app/ventas/devoluciones/page.tsx`
- `app/ventas/facturas/page.tsx`
- `app/ventas/imputaciones/page.tsx`
- `app/ventas/listas-precios/page.tsx`
- `app/ventas/notas-credito/page.tsx`
- `app/ventas/notas-debito/page.tsx`
- `app/ventas/operaciones/page.tsx`
- `app/ventas/pedidos/page.tsx`
- `app/ventas/productos/page.tsx`
- `app/ventas/puntos-facturacion/page.tsx`
- `app/ventas/remitos/page.tsx`
- `app/ventas/reportes/page.tsx`

### Legacy `zuluApp`
Formularios principales detectados para Ventas:
- `frmCliente`, `frmClienteResumido`, `frmImportadorClientes`
- `frmCobro`, `frmCobroVentanillaCP`, `frmCobroVentanillaCT`
- `frmChequesTerceroVenta`, `frmChequesTerceroDeposito`, `frmChequesTerceroRechazo`, `frmAuditoriaCheques`
- `frmFacturaVenta`, `frmPreFacturaVenta`, `frmFacturaVentaMonitor`, `frmFacturacionMasiva`, `frmFacturacionAutomatica`, `frmFacturaElectronica`
- `frmNotaPedido`
- `frmRemitosVentasValorizados`, `frmRemitosVentasNoValorizados`, `frmRemitos_Atributos`, `frmReportesRemitos`
- `frmNotaCreditoVenta`, `frmPreNotaCreditoVenta`, `frmNotaCreditoVentaForExport`, `frmNotaCreditoVentaImport`
- `frmNotaDebitoVenta`, `frmNotaDebitoVenta2`, `frmNotaDebitoVentaForExport`, `frmNotaDebitoVentaImport`
- `frmImputacionesVentas`, `frmImputacionesVentasMasivas`, `frmDesimputarComprobantesVentas`, `frmVincularComprobantesVenta`, `frmDesvincularComprobantesVenta`
- `frmListasPrecios`, `frmListasPreciosNuevoItem`, `frmListasPreciosOpciones`, `frmListasPreciosImprimir`
- `frmRegistrarComprobantePuntoVenta`
- `frmDescuentos`, `frmDescuento_AM`
- `frmAjustesVentaCredito`, `frmAjustesVentaDebito`, `frmAjustesPV`
- `frmDevolucionVentaStock`, `frmDevolucionVentaNoValorizada`
- `frmMonitorVentas`, `frmVerificarObjetivosVenta`, `frmExportarCuentaCorrienteClientes`, `frmImpresionCuentaCorriente`

### Backend actual relevado
Controladores/endpoints base ya existentes:
- `TercerosController`
- `ComprobantesController`
- `VentasController`
- `ImputacionesController`
- `ChequesController`
- `CajasController`
- `PuntosFacturacionController`
- `MaestrosComercialesController`
- `ReportesController`
- `OperacionesController`

## Leyenda de estado
- `REAL`: ya existe base backend utilizable.
- `PARCIAL`: existe base, pero faltan datos, relaciones o acciones para alcanzar al legado.
- `GAP`: el frontend todavía depende de local state, metadata insuficiente o falta backend dedicado.

---

## 1. Maestros comerciales

### 1.1 Clientes
- Vista front: `app/ventas/clientes/page.tsx`
- Hooks base: `useTerceros`, `useTercerosConfig`, `useGeografia`
- Legacy: `frmCliente`, `frmClienteResumido`, `frmImportadorClientes`
- Backend actual: `REAL/PARCIAL`
  - listado y detalle de terceros
  - perfil comercial ampliado
  - contactos múltiples
  - sucursales de entrega
  - transportes
  - ventanas de cobranza
  - catálogos de categorías/estados por rol
- Datos legacy que el backend debe garantizar al 100%:
  - identificación completa: legajo, razón social, nombre comercial, apellido, nombre, personería, tipo y nro documento
  - fiscal: condición IVA, clave fiscal, valor de clave fiscal, ingresos brutos, municipal, facturable
  - domicilio completo y geografía
  - clasificación general y por rol cliente/proveedor
  - vendedor, cobrador, moneda, límite de crédito, observaciones
  - rubro, subrubro, sector, zona comercial, condición venta, plazo cobro, riesgo crediticio
  - contactos, sucursales/puntos de entrega, transportes, ventanas de cobranza
  - estado operativo bloqueante/no bloqueante
  - historial y restricciones de operación en ventas
- Gaps backend a cerrar:
  - verificar si faltan catálogos auxiliares consumidos por la ficha legacy
  - validar importación de clientes y reglas masivas si el flujo sigue vigente
  - completar cualquier dato visible en `frmCliente` que hoy no viaje en DTO listado/detalle
- Criterio de cierre:
  - la pantalla nueva no debe requerir overlay local ni texto manual para suplir datos comerciales o fiscales del cliente

### 1.2 Productos
- Vista front: `app/ventas/productos/page.tsx`
- Hooks base: `useItems`, `useItemsConfig`
- Legacy relacionado: formularios de producto y atributos usados desde facturación/remitos
- Backend actual: `PARCIAL`
  - alta/edición/listado de items
  - atributos comerciales y maestros comerciales disponibles
- Datos/acciones a cubrir:
  - datos comerciales completos del item para ventas
  - atributos comerciales visibles y editables
  - stock útil para venta: disponible, comprometido, reservado, en tránsito si aplica
  - listas de precios asociadas
  - categorías, marcas, jurisdicciones, auxiliares comerciales
  - restricciones fiscales/comerciales por tipo de item
- Gaps backend:
  - consolidar DTO detalle de item orientado a ventas
  - exponer stock operativo real para pedidos/remitos/facturas
  - definir si faltan equivalencias, presentaciones o variantes del legado

### 1.3 Listas de precios
- Vista front: `app/ventas/listas-precios/page.tsx`
- Hooks base: `useListasPrecios`, `useItems`, `useItemsConfig`
- Legacy: `frmListasPrecios`, `frmListasPreciosNuevoItem`, `frmListasPreciosOpciones`, `frmListasPreciosImprimir`
- Backend actual: `PARCIAL`
- Cobertura backend presente:
  - alta, edición, baja de listas
  - alta, actualización y eliminación de renglones por item
- Cobertura requerida:
  - vigencia, lista por defecto, moneda, detalle por producto
  - herencia entre listas
  - promociones
  - precios especiales por cliente
  - precios especiales por canal
  - precios especiales por vendedor
  - impresión/exportación de listas
- Gaps backend:
  - motor de resolución de precio final aplicable
  - reglas de prioridad/precedencia
  - endpoints de impresión/exportación
- Prioridad: alta, porque impacta facturas, pedidos, notas y reportes

### 1.4 Puntos de facturación
- Vista front: `app/ventas/puntos-facturacion/page.tsx`
- Hook base: `usePuntosFacturacion`, `useTiposPuntoFacturacion`, `useSucursales`
- Legacy: `frmRegistrarComprobantePuntoVenta`
- Backend actual: `REAL/PARCIAL`
  - alta/edición/baja
  - tipos de punto
  - próximo número por punto y tipo de comprobante
  - configuración AFIP WSFE base
- Cobertura requerida:
  - numeración por comprobante/circuito
  - configuración CAI/CAE/CAEA o flujos alternativos según sucursal
  - restricciones por sucursal, tipo documental y modalidad fiscal
  - estado de habilitación operativa
  - auditoría de configuración
- Gaps backend:
  - validar si el legado diferenciaba numeradores más finos por documento/letra/canal
  - completar integraciones fiscales alternativas si el front debe mostrarlas

---

## 2. Circuito documental de ventas

### 2.1 Dashboard ventas
- Vista front: `app/ventas/page.tsx`
- Hooks base: `useComprobantes`, `useTerceros`, `useComprobantesConfig`
- Legacy: `frmMonitorVentas`, `frmVerificarObjetivosVenta`
- Backend actual: `PARCIAL`
- Cobertura requerida:
  - KPIs reales por documento, deuda, remitos, pedidos, facturas y cobranzas
  - alertas operativas
  - objetivos comerciales
  - pipeline o embudo documental si el legado lo exponía
  - pendientes críticos por sucursal, vendedor, cliente y circuito
- Gaps backend:
  - endpoint agregado específico de dashboard ventas
  - métricas por vendedor, zona, canal, sucursal y estado operativo

### 2.2 Facturas de venta
- Vista front: `app/ventas/facturas/page.tsx`
- Hooks base: `useComprobantes`, `useComprobantesConfig`, `usePuntosFacturacion`, `useSucursales`, `useTerceros`, `useItems`
- Legacy: `frmFacturaVenta`, `frmPreFacturaVenta`, `frmFacturaVentaMonitor`, `frmFacturacionMasiva`, `frmFacturacionAutomatica`, `frmFacturaElectronica`
- Backend actual: `REAL/PARCIAL`
  - emisión documental
  - detalle de comprobante
  - anulación
  - CAE/AFIP base
  - conversión de presupuesto
- Cobertura requerida:
  - borrador/pre-factura/factura emitida
  - facturación masiva
  - facturación automática
  - estados fiscales completos
  - trazabilidad pedido/remito/factura
  - reimpresión
  - auditoría fiscal y operativa
- Gaps backend:
  - revisar si la facturación masiva/automática ya cubre todo el legado o es solo primera capa
  - completar causas de error fiscal y estados intermedios visibles
  - formalizar relación documento origen/destino por renglón cuando aplique

### 2.3 Pedidos de venta
- Vista front: `app/ventas/pedidos/page.tsx`
- Hooks base: `useComprobantes({ esVenta: true })`, `useComprobantesConfig`, `useSucursales`, `useTerceros`, `useItems`
- Legacy: `frmNotaPedido`
- Backend actual: `PARCIAL`
  - creación como borrador documental
  - soporte de conversión documental general
- Cobertura requerida:
  - pedido formal
  - reserva de stock
  - aprobación/rechazo
  - cumplimiento parcial/total
  - conversión a remito y factura con trazabilidad
  - fechas compromiso, vendedor, canal, observaciones comerciales
- Gaps backend:
  - estado operativo propio del pedido
  - reserva y liberación de stock
  - seguimiento de cumplimiento por renglón

### 2.4 Remitos de venta
- Vista front: `app/ventas/remitos/page.tsx`
- Hooks base: `useComprobantes`, `useComprobantesConfig`, `useSucursales`, `useTerceros`, `useItems`
- Legacy: `frmRemitosVentasValorizados`, `frmRemitosVentasNoValorizados`, `frmRemitos_Atributos`, `frmReportesRemitos`
- Backend actual: `REAL/PARCIAL`
  - creación/emisión de remitos
  - emisión masiva
  - detalle y anulación
  - impacto de stock y, opcionalmente, cuenta corriente
- Cobertura requerida:
  - remito valorizado y no valorizado
  - datos logísticos completos: transporte, chofer, entrega, ruta, observación
  - entrega parcial
  - atributos de remito
  - firma/recepción
  - vínculo remito-factura
  - impresión específica
- Gaps backend:
  - datos logísticos y atributos por remito/renglón
  - cumplimiento parcial y estado logístico propio
  - vinculación avanzada remito-factura

### 2.5 Notas de crédito
- Vista front: `app/ventas/notas-credito/page.tsx`
- Hook base: comparte base con notas de débito
- Legacy: `frmNotaCreditoVenta`, `frmPreNotaCreditoVenta`, `frmNotaCreditoVentaForExport`, `frmNotaCreditoVentaImport`
- Backend actual: `PARCIAL`
  - emisión documental general
  - soporte de devoluciones en `VentasController`
- Cobertura requerida:
  - vínculo formal a factura origen
  - motivo fiscal/comercial tipificado
  - devolución exacta por renglón
  - impacto en stock y cuenta corriente
  - reimpresión
  - variantes de import/export si siguen vigentes
- Gaps backend:
  - referencia documental y causal estructurada
  - detalle de devolución por renglón
  - integración con flujos masivos/importados del legado

### 2.6 Notas de débito
- Vista front: `app/ventas/notas-debito/page.tsx`
- Base UI: reutiliza `notas-credito`
- Legacy: `frmNotaDebitoVenta`, `frmNotaDebitoVenta2`, `frmNotaDebitoVentaForExport`, `frmNotaDebitoVentaImport`
- Backend actual: `PARCIAL`
- Cobertura requerida:
  - vínculo formal con factura origen
  - motivo fiscal/comercial
  - cargos/diferencias por renglón si aplica
  - reimpresión y trazabilidad
- Gaps backend:
  - contrato backend específico de débito cuando difiera de crédito
  - referencias, causal y auditoría dedicada

### 2.7 Devoluciones
- Vista front: `app/ventas/devoluciones/page.tsx`
- Hooks base: `useLegacyLocalCollection`, `useComprobantes`
- Legacy: `frmDevolucionVentaStock`, `frmDevolucionVentaNoValorizada`
- Backend actual: `GAP/PARCIAL`
  - existe `RegistrarDevolucionVentaCommand`, pero la UX actual todavía conserva dependencia local
- Cobertura requerida:
  - devolución valorizada/no valorizada
  - ingreso o no de stock
  - crédito de cuenta corriente
  - relación con documento origen
  - diferencia por renglón
- Gaps backend:
  - revisar si DTOs y queries devuelven todo lo que la pantalla necesita
  - construir lectura/listado específico de devoluciones para reemplazar estado local

### 2.8 Ajustes de ventas
- Vista front: `app/ventas/ajustes/page.tsx`
- Hooks base: `useComprobantes`, `useLegacyLocalCollection`
- Legacy: `frmAjustesVentaCredito`, `frmAjustesVentaDebito`, `frmAjustesPV`
- Backend actual: `GAP`
- Cobertura requerida:
  - ajustes comerciales y/o fiscales
  - motivo estructurado
  - impacto en stock, cuenta corriente o fiscalidad según tipo
  - autorización/auditoría
- Gaps backend:
  - falta backend dedicado para que la vista deje de depender de colección local

---

## 3. Cobranzas y cuenta corriente

### 3.1 Cobros
- Vista front: `app/ventas/cobros/page.tsx`
- Hooks base: `useCajas`, `useComprobantes`, `useConfiguracion`, `useCobros`, `useSucursales`, `useTerceros`
- Legacy: `frmCobro`, `frmCobroVentanillaCP`, `frmCobroVentanillaCT`
- Backend actual: `REAL/PARCIAL`
  - registración de cobros
  - consulta de comprobantes con saldo pendiente
  - cajas y formas de pago base
- Cobertura requerida:
  - cobro con múltiples medios
  - anticipos
  - retenciones
  - diferencias y redondeos
  - recibos
  - cobranzas por ventanilla/circuito
  - aplicación parcial sobre múltiples comprobantes
- Gaps backend:
  - confirmar que la forma de pago, recibo y detalle de aplicación cubran todos los escenarios del legado
  - exponer detalle printable/exportable del recibo y su aplicación

### 3.2 Cheques
- Vista front: `app/ventas/cheques/page.tsx`
- Hooks base: `useCajas`, `useCheques`, `useSucursales`, `useTerceros`
- Legacy: `frmChequesTerceroVenta`, `frmChequesTerceroDeposito`, `frmChequesTerceroRechazo`, `frmAuditoriaCheques`
- Backend actual: `REAL/PARCIAL`
  - alta
  - cartera
  - depósito
  - acreditación
  - rechazo
  - entrega
  - historial y auditoría
- Cobertura requerida:
  - ciclo de vida completo del cheque
  - filtros operativos por caja, tercero, estado, banco, fechas
  - auditoría apta para pantalla de control
- Gaps backend:
  - revisar si faltan estados o transiciones del legacy
  - validar si se necesitan consultas agregadas por cartera/riesgo/vencimiento

### 3.3 Cuenta corriente
- Vista front: `app/ventas/cuenta-corriente/page.tsx`
- Hooks base: `useCuentaCorriente`, `useSucursales`, `useTerceros`
- Legacy: `frmImpresionCuentaCorriente`, `frmExportarCuentaCorrienteClientes`
- Backend actual: `PARCIAL`
- Cobertura requerida:
  - saldo actual
  - movimientos
  - vencimientos
  - aging por tramo
  - filtros por sucursal, cliente, vendedor, zona
  - exportación/impresión
- Gaps backend:
  - confirmar que la query actual exponga todas las columnas del legacy
  - incorporar exportación e impresión si aún no están materializadas

### 3.4 Imputaciones
- Vista front: `app/ventas/imputaciones/page.tsx`
- Hooks base: `useCobros`, `useComprobantes`, `useLegacyLocalCollection`, `useSucursales`
- Legacy: `frmImputacionesVentas`, `frmImputacionesVentasMasivas`, `frmDesimputarComprobantesVentas`, `frmVincularComprobantesVenta`, `frmDesvincularComprobantesVenta`
- Backend actual: `REAL/PARCIAL/GAP`
  - existen consultas por origen, destino e historial
  - existen imputación, desimputación y variantes masivas
  - la vista aún muestra dependencia parcial local
- Cobertura requerida:
  - imputar simple
  - imputar masivo
  - desimputar simple y masivo
  - vincular/desvincular
  - auditoría
  - filtros por tipo, fecha, estado y rol origen/destino
- Gaps backend:
  - construir endpoint o modelo agregado para la pantalla completa
  - remover dependencia local del frontend
  - confirmar comportamiento exacto legacy en saldo residual y parcialidades

---

## 4. Comercial avanzado y soporte operativo

### 4.1 Descuentos comerciales
- Vista front: `app/ventas/descuentos/page.tsx`
- Hooks base: `useDescuentosComerciales`, `useLegacyLocalCollection`, `useTerceros`, `useItems`
- Legacy: `frmDescuentos`, `frmDescuento_AM`
- Backend actual: `GAP/PARCIAL`
- Cobertura requerida:
  - descuentos por cliente
  - descuentos por item
  - descuentos por rubro/categoría
  - vigencias
  - prioridades
  - compatibilidad con listas, canal y vendedor
  - cálculo reutilizable en emisión documental
- Gaps backend:
  - la dependencia de colección local indica falta de paridad total
  - hace falta definir motor de descuentos y contratos de lectura/edición

### 4.2 Operaciones comerciales
- Vista front: `app/ventas/operaciones/page.tsx`
- Hooks base: `useComprobantes`, `useCobros`, `useListasPrecios`, `usePuntosFacturacion`, `useSucursales`, `useLegacyLocalCollection`
- Legacy asociado: monitores y operación transversal de ventas
- Backend actual: `GAP/PARCIAL`
- Cobertura requerida:
  - tablero consolidado de operaciones de ventas
  - pendientes y alertas por documento, cobranza, precios y fiscalidad
  - seguimiento transversal del circuito
- Gaps backend:
  - falta endpoint agregado específicamente orientado a la pantalla
  - eliminar dependencia local

---

## 5. Analítica y reportes

### 5.1 Reportes de ventas
- Vista front: `app/ventas/reportes/page.tsx`
- Hooks base: `useComprobantes`, `useItems`, `useTerceros`
- Legacy: reportes de ventas, IVA, remitos, cuenta corriente, monitoreo comercial
- Backend actual: `REAL/PARCIAL`
  - `ReportesController` ofrece remitos, informes, parametrizados, dashboards y exportación
- Cobertura requerida:
  - libro IVA ventas
  - ventas por cliente/producto/vendedor/zona/canal
  - margen
  - remitos
  - cobranzas y cuentas corrientes
  - exportación por formato
- Gaps backend:
  - revisar si los tipos parametrizados actuales cubren todos los cortes legacy
  - agregar reportes faltantes antes de exigirle al front cálculos locales

---

## 6. Dependencias transversales obligatorias

### 6.1 Catálogos y combos
Todo combo del front debe resolverse desde backend cuando en legacy provenía de maestro:
- tipos de comprobante
- estados de comprobante
- puntos de facturación
- sucursales
- cajas y formas de pago
- categorías/estados de clientes
- zonas, marcas, jurisdicciones, auxiliares, atributos comerciales
- listas de precios
- monedas y geografía

### 6.2 Relaciones documentales
El backend debe exponer trazabilidad clara de:
- pedido -> remito -> factura
- factura -> nota crédito / nota débito
- devolución -> documento origen
- cobro -> recibo -> comprobantes aplicados
- imputación -> origen / destino / historial

### 6.3 Estados operativos
No mezclar en un único campo si el legado requería diferenciarlos:
- estado documental
- estado fiscal
- estado logístico
- estado financiero
- estado comercial/bloqueo operativo

### 6.4 Impresión y exportación
Cada bloque debe decidir explícitamente:
- qué se imprime
- qué se exporta
- en qué formato
- si requiere endpoint dedicado o comando de exportación

### 6.5 Seguridad y auditoría
Para acciones críticas:
- emisión
- anulación
- vinculación/desvinculación
- imputación/desimputación
- cambios de cheque
- configuración fiscal
- procesos masivos
Debe existir soporte de autorización, trazabilidad y registro auditable.

---

## 7. Backlog backend-first por prioridad

### Ola 1 — Base maestra
1. `clientes`
2. `productos`
3. `listas-precios`
4. `puntos-facturacion`

### Ola 2 — Núcleo documental
5. `facturas`
6. `pedidos`
7. `remitos`
8. `notas-credito`
9. `notas-debito`

### Ola 3 — Financiero ventas
10. `cobros`
11. `cheques`
12. `cuenta-corriente`
13. `imputaciones`

### Ola 4 — Gaps hoy incompletos
14. `descuentos`
15. `devoluciones`
16. `ajustes`
17. `operaciones`

### Ola 5 — Cierre ejecutivo
18. `dashboard ventas`
19. `reportes`

---

## 8. Checklist de aceptación por vista
Antes de cerrar una vista de Ventas, validar:
1. todos los campos visibles del legacy tienen fuente backend real
2. todos los combos salen de API o metadata persistida
3. todas las acciones críticas tienen endpoint real
4. los estados necesarios son distinguibles
5. existe trazabilidad con documentos relacionados
6. se cubren filtros, KPIs y detalle operativo
7. se resolvió impresión/exportación si el legacy la usaba
8. no depende de `useLegacyLocalCollection`
9. smoke backend y prueba manual comparada contra `zuluApp`

## 9. Siguiente bloque recomendado
Iniciar por `Ventas > Clientes`.

Razones:
- ya hay mucho trabajo avanzado en `Terceros`
- impacta facturas, cobros, cuenta corriente, descuentos y reportes
- cualquier faltante aquí contamina casi todo el módulo de ventas

Desglose detallado disponible en:
- `docs\ventas-clientes-backlog-backend.md`
