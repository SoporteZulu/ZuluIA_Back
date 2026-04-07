# Matrices de Paridad Funcional Ventas - Trabajo Paralelo
**Fecha:** 2026-03-31  
**Objetivo:** Matrices de trabajo en paralelo para cerrar gaps funcionales entre ZuluApp → ZuluIA_Back → ZuluIA_Front

---

## RESUMEN EJECUTIVO

### Estado Actual Paridad Backend
- ✅ **80%** - Infraestructura de datos (entidades, configs EF)
- ✅ **75%** - Comandos básicos (crear, emitir, vincular)
- ⚠️ **60%** - Queries específicas por módulo
- ⚠️ **50%** - Lógica de negocio específica (validaciones, workflows)
- ❌ **30%** - Endpoints API completos con todos los filtros

### Estado Actual Paridad Frontend
- ✅ **70%** - Estructura de rutas y páginas
- ⚠️ **40%** - Componentes de listados con filtros completos
- ⚠️ **35%** - Formularios de alta/edición
- ❌ **20%** - Acciones operativas específicas

### Estrategia de Cierre
**PRIORIDAD:** Cerrar backend al 100% antes de avanzar frontend
**MÉTODO:** Matrices paralelas independientes por módulo funcional

---

## MATRIZ 1: PEDIDOS (NOTAS DE PEDIDO)
**Prioridad:** CRÍTICA  
**Dependencias:** Ninguna  
**Responsable sugerido:** Dev Backend Senior + Dev Frontend Junior

### Backend - Pedidos

#### 1.1 Entidades y Enums ✅ COMPLETADO
- [x] `EstadoPedido` enum
- [x] `EstadoEntregaItem` enum
- [x] `PrioridadPedido` enum
- [x] Campos pedido en `Comprobante`
- [x] Campos entrega en `ComprobanteItem`

#### 1.2 Comandos ✅ COMPLETADO
- [x] `CrearBorradorVentaCommand` (ya soporta pedidos)
- [x] `CerrarPedidoCommand`
- [x] `CerrarPedidosMasivoCommand`
- [x] `ActualizarCumplimientoPedidoCommand`

#### 1.3 Queries ⚠️ PARCIAL
- [x] `GetPedidosConEstadoQuery`
- [x] `GetPedidoVinculacionesQuery`
- [ ] **GAP:** `GetPedidosConAtrasosQuery` - Filtrar pedidos atrasados
- [ ] **GAP:** `GetPedidosExpedicionQuery` - Vista desde logística
- [ ] **GAP:** `GetPedidosParaCierreQuery` - Pedidos elegibles para cierre masivo

#### 1.4 Endpoints API ⚠️ PARCIAL
- [ ] `POST /api/ventas/pedidos` - Crear pedido
- [ ] `GET /api/ventas/pedidos` - Listar con filtros completos (estado, atraso, cliente, fecha entrega, producto)
- [ ] `GET /api/ventas/pedidos/{id}` - Detalle completo
- [ ] `GET /api/ventas/pedidos/{id}/vinculaciones` - Documentos generados (remitos, facturas)
- [ ] `PUT /api/ventas/pedidos/{id}/cerrar` - Cerrar individual
- [ ] `POST /api/ventas/pedidos/cerrar-masivo` - Cerrar múltiples
- [ ] `GET /api/ventas/pedidos/estados` - Contadores por estado
- [ ] `GET /api/ventas/pedidos/atrasos` - Dashboard de atrasos

#### 1.5 Validaciones y Reglas de Negocio ⚠️ PARCIAL
- [x] No cerrar pedido con entregas pendientes sin autorización
- [x] No editar pedido con entregas parciales
- [ ] **GAP:** Validar stock antes de confirmar pedido
- [ ] **GAP:** Calcular fecha estimada entrega según zona/producto
- [ ] **GAP:** Alertar si fecha entrega < plazo mínimo del producto

### Frontend - Pedidos

#### 1.6 Páginas ⚠️ PARCIAL
- [x] `/app/ventas/pedidos/page.tsx` - Listado básico
- [ ] **GAP:** Filtros completos (estado entrega, atraso, fecha compromiso, producto)
- [ ] **GAP:** `/app/ventas/pedidos/nuevo` - Formulario creación
- [ ] **GAP:** `/app/ventas/pedidos/[id]` - Detalle con acciones
- [ ] **GAP:** `/app/ventas/pedidos/[id]/editar` - Edición (si aplica)

#### 1.7 Componentes
- [ ] `PedidosDataTable` - Grilla con todos los filtros zuluApp
- [ ] `PedidoForm` - Formulario alta/edición
- [ ] `PedidoDetalle` - Vista completa con timeline de cumplimiento
- [ ] `PedidoItemsTable` - Detalle de items con estados de entrega
- [ ] `CerrarPedidoDialog` - Modal cierre individual
- [ ] `CerrarPedidosMasivoDialog` - Selección múltiple + confirmar
- [ ] `PedidoVinculacionesTimeline` - Timeline visual de documentos generados
- [ ] `PedidoAtrasosAlert` - Indicador de atrasos

#### 1.8 Servicios API
- [ ] `pedidosApi.create()`
- [ ] `pedidosApi.getAll()` con filtros completos
- [ ] `pedidosApi.getById()`
- [ ] `pedidosApi.getVinculaciones()`
- [ ] `pedidosApi.cerrar()`
- [ ] `pedidosApi.cerrarMasivo()`

### Referencia zuluApp - Pedidos
**Formularios:**
- `frmNotaPedido.frm` - Alta/edición
- `VTAESTADONOTASPEDIDO_Listado.asp` - Grilla principal
- `VTACERRARNOTASPEDIDO_*.asp` - Flujos de cierre
- `VTAConsultaNotaPedidoVinculadas_Listado.asp` - Vinculaciones
- `VTAESTADONOTASPEDIDOEXPEDICION_Listado.asp` - Vista expedición

**Filtros críticos:**
- Razón social, Sucursal, Fecha entrega, Número pedido, Estado item, Código producto, Atraso, Fecha comprobante, Estado pedido

---

## MATRIZ 2: REMITOS
**Prioridad:** ALTA  
**Dependencias:** Pedidos (para vinculación)  
**Responsable sugerido:** Dev Backend Mid + Dev Frontend Mid

### Backend - Remitos

#### 2.1 Entidades y Enums ✅ MAYORMENTE COMPLETADO
- [x] `ComprobanteCot` entity
- [x] `ComprobanteAtributo` entity
- [x] `EstadoLogisticoRemito` enum
- [x] Campos logísticos en `Comprobante`
- [ ] **GAP:** `RemitoCabAtributo` (atributos específicos de cabecera)

#### 2.2 Comandos ⚠️ PARCIAL
- [x] `EmitirDocumentoVentaCommand` (soporta remitos)
- [x] `EmitirRemitosVentaMasivosCommand`
- [x] `UpsertRemitoCotCommand`
- [x] `ReplaceRemitoAtributosCommand`
- [ ] **GAP:** `ActualizarEstadoLogisticoRemitoCommand`
- [ ] **GAP:** `RegistrarConformidadEntregaCommand` (firma + quien recibe)
- [ ] **GAP:** `VincularRemitosAFacturaCommand` (múltiple → 1)

#### 2.3 Queries ❌ FALTA
- [x] `GetRemitosPagedQuery` (existe pero verificar filtros)
- [ ] **GAP:** Filtro por COT (número y rango fecha vigencia)
- [ ] **GAP:** Filtro por depósito origen
- [ ] **GAP:** Filtro por estado logístico
- [ ] **GAP:** Filtro por transporte
- [ ] **GAP:** `GetRemitoDetalleQuery` - Detalle completo con COT, atributos, stock
- [ ] **GAP:** `GetRemitosParaFacturarQuery` - Remitos sin factura asociada
- [ ] **GAP:** `GetRemitosPorEstadoLogisticoQuery`

#### 2.4 Endpoints API ❌ FALTA MAYORÍA
- [ ] `POST /api/ventas/remitos` - Crear remito
- [ ] `POST /api/ventas/remitos/masivo` - Crear múltiples desde pedidos
- [ ] `GET /api/ventas/remitos` - Listar con TODOS los filtros zuluApp (sucursal, fecha, COT, depósito, tercero, transporte, estado logístico)
- [ ] `GET /api/ventas/remitos/{id}` - Detalle completo
- [ ] `PUT /api/ventas/remitos/{id}/cot` - Actualizar COT
- [ ] `PUT /api/ventas/remitos/{id}/atributos` - Gestionar atributos
- [ ] `PUT /api/ventas/remitos/{id}/estado-logistico` - Cambiar estado (en tránsito, entregado, etc.)
- [ ] `PUT /api/ventas/remitos/{id}/conformidad` - Registrar firma y datos de quien recibe
- [ ] `POST /api/ventas/remitos/vincular-factura` - Vincular múltiples remitos a factura
- [ ] `GET /api/ventas/remitos/para-facturar` - Remitos disponibles para facturar

#### 2.5 Validaciones y Reglas de Negocio ⚠️ PARCIAL
- [x] COT obligatorio (longitud mínima 6 caracteres)
- [x] Fecha vigencia COT >= fecha emisión remito
- [ ] **GAP:** Validar stock disponible en depósito origen antes de emitir
- [ ] **GAP:** Validar que remito no valorizado no permita ítems con precio
- [ ] **GAP:** Validar que al facturar, remitos sean del mismo cliente
- [ ] **GAP:** Actualizar automáticamente cumplimiento de pedido al emitir remito

### Frontend - Remitos

#### 2.6 Páginas ❌ FALTA MAYORÍA
- [x] `/app/ventas/remitos/page.tsx` - Listado básico
- [ ] **GAP:** Filtros completos (COT, depósito, estado logístico, transporte)
- [ ] **GAP:** `/app/ventas/remitos/nuevo` - Formulario creación
- [ ] **GAP:** `/app/ventas/remitos/[id]` - Detalle completo
- [ ] **GAP:** `/app/ventas/remitos/[id]/cot` - Gestión COT
- [ ] **GAP:** `/app/ventas/remitos/[id]/atributos` - Gestión atributos
- [ ] **GAP:** `/app/ventas/remitos/[id]/tracking` - Seguimiento logístico
- [ ] **GAP:** `/app/ventas/remitos/masivo` - Emisión masiva desde pedidos

#### 2.7 Componentes
- [ ] `RemitosDataTable` - Grilla con todos los filtros
- [ ] `RemitoForm` - Formulario alta con validación COT
- [ ] `RemitoDetalle` - Vista completa
- [ ] `RemitoCotForm` - Gestión de COT
- [ ] `RemitoAtributosManager` - Gestión dinámica de atributos
- [ ] `RemitoTrackingTimeline` - Timeline logístico
- [ ] `RemitoConformidadDialog` - Captura firma y datos de recepción
- [ ] `RemitosParaFacturarSelector` - Selección múltiple para facturar
- [ ] `RemitoMasivoWizard` - Wizard emisión masiva desde pedidos

#### 2.8 Servicios API
- [ ] `remitosApi.create()`
- [ ] `remitosApi.createMasivo()`
- [ ] `remitosApi.getAll()` con filtros completos
- [ ] `remitosApi.getById()`
- [ ] `remitosApi.updateCot()`
- [ ] `remitosApi.updateAtributos()`
- [ ] `remitosApi.updateEstadoLogistico()`
- [ ] `remitosApi.registrarConformidad()`
- [ ] `remitosApi.vincularFactura()`
- [ ] `remitosApi.getParaFacturar()`

### Referencia zuluApp - Remitos
**Formularios:**
- `VTACOMPROBANTESREMITOS_*.asp` - CRUD remitos
- `frmRemitos_Atributos.frm` - Gestión atributos
- `frmRemitos_COT.frm` - Gestión COT
- `EditarDB.asp` - Update COT

**Filtros críticos:**
- Sucursal, Fecha emisión, Prefijo/Número, Legajo/Razón social, COT (número + rango fecha vigencia), Depósito

---

## MATRIZ 3: FACTURAS
**Prioridad:** ALTA  
**Dependencias:** Remitos (para facturación desde remitos), Pedidos (para facturación directa)  
**Responsable sugerido:** Dev Backend Senior + Dev Frontend Senior

### Backend - Facturas

#### 3.1 Entidades y Enums ✅ COMPLETADO
- [x] Todos los campos base de `Comprobante` soportan facturas
- [x] Integración fiscal AFIP (Argentina)
- [x] Integración fiscal SIFEN (Paraguay)

#### 3.2 Comandos ⚠️ PARCIAL
- [x] `CrearBorradorVentaCommand` (soporta pre-facturas)
- [x] `EmitirDocumentoVentaCommand` (emite facturas)
- [x] `VincularComprobanteVentaCommand`
- [ ] **GAP:** `FacturarDesdeRemitosCommand` - Crear factura vinculando múltiples remitos
- [ ] **GAP:** `FacturarDesdePedidoCommand` - Facturación directa sin remito
- [ ] **GAP:** `AnularFacturaCommand` - Anulación con validaciones específicas (no debe tener pagos aplicados)
- [ ] **GAP:** `RegenerarFiscalizacionFacturaCommand` - Re-enviar a AFIP/SIFEN

#### 3.3 Queries ⚠️ PARCIAL
- [x] `GetComprobantesPagedQuery` (genérico, soporta facturas)
- [ ] **GAP:** `GetFacturasPagedQuery` - Especializada con filtros específicos de facturación
- [ ] **GAP:** `GetFacturasVencidasQuery` - Facturas con vencimiento pasado
- [ ] **GAP:** `GetFacturasPendientesCobroQuery` - Facturas con saldo > 0
- [ ] **GAP:** `GetFacturasPorVencimientosQuery` - Agrupadas por rango de vencimiento
- [ ] **GAP:** `GetFacturasAnuladasQuery` - Histórico de anulaciones
- [ ] **GAP:** `GetEstadoFiscalFacturaQuery` - Estado AFIP/SIFEN específico

#### 3.4 Endpoints API ❌ FALTA MAYORÍA
- [ ] `POST /api/ventas/facturas` - Crear factura (borrador o directa)
- [ ] `POST /api/ventas/facturas/desde-remitos` - Facturar desde remitos
- [ ] `POST /api/ventas/facturas/desde-pedido` - Facturación directa
- [ ] `GET /api/ventas/facturas` - Listar con filtros (sucursal, fecha, cliente, estado fiscal, vencimiento, saldo)
- [ ] `GET /api/ventas/facturas/{id}` - Detalle completo
- [ ] `PUT /api/ventas/facturas/{id}/anular` - Anular factura
- [ ] `POST /api/ventas/facturas/{id}/refiscalizar` - Re-enviar a AFIP/SIFEN
- [ ] `GET /api/ventas/facturas/vencidas` - Dashboard vencimientos
- [ ] `GET /api/ventas/facturas/pendientes-cobro` - Dashboard cobranzas
- [ ] `GET /api/ventas/facturas/{id}/pdf` - Generar PDF imprimible
- [ ] `GET /api/ventas/facturas/{id}/afip-status` - Consultar estado AFIP en tiempo real
- [ ] `GET /api/ventas/facturas/{id}/sifen-status` - Consultar estado SIFEN

#### 3.5 Validaciones y Reglas de Negocio ⚠️ PARCIAL
- [x] Validar que cliente tiene condición IVA compatible
- [x] Validar que punto de facturación esté activo
- [x] Validar CAE/CAEA antes de emitir (Argentina)
- [x] Validar timbrado vigente (Paraguay)
- [ ] **GAP:** No permitir anular factura con pagos aplicados
- [ ] **GAP:** No permitir anular factura después de N días de emitida
- [ ] **GAP:** Validar que remitos a facturar sean del mismo cliente
- [ ] **GAP:** Alertar si factura supera límite de crédito del cliente

### Frontend - Facturas

#### 3.6 Páginas ⚠️ PARCIAL
- [x] `/app/ventas/facturas/page.tsx` - Listado básico
- [ ] **GAP:** Filtros completos (estado fiscal, vencimiento, saldo pendiente, AFIP/SIFEN status)
- [ ] **GAP:** `/app/ventas/facturas/nueva` - Formulario creación directa
- [ ] **GAP:** `/app/ventas/facturas/desde-remitos` - Wizard facturación desde remitos
- [ ] **GAP:** `/app/ventas/facturas/desde-pedido` - Facturación directa desde pedido
- [ ] **GAP:** `/app/ventas/facturas/[id]` - Detalle completo
- [ ] **GAP:** `/app/ventas/facturas/[id]/pdf` - Vista previa PDF
- [ ] **GAP:** `/app/ventas/facturas/vencimientos` - Dashboard de vencimientos
- [ ] **GAP:** `/app/ventas/facturas/cobranzas` - Dashboard de cobranzas

#### 3.7 Componentes
- [ ] `FacturasDataTable` - Grilla con todos los filtros
- [ ] `FacturaForm` - Formulario alta directa
- [ ] `FacturaDetalle` - Vista completa con estado fiscal
- [ ] `FacturarDesdeRemitosWizard` - Wizard 3 pasos (seleccionar remitos → preview → confirmar)
- [ ] `FacturarDesdePedidoDialog` - Facturación express
- [ ] `FacturaEstadoFiscalBadge` - Indicador estado AFIP/SIFEN
- [ ] `FacturaAnularDialog` - Anulación con motivo y validaciones
- [ ] `FacturaVencimientosCalendar` - Calendario de vencimientos
- [ ] `FacturaSaldoBadge` - Indicador de saldo pendiente
- [ ] `FacturaPdfViewer` - Visor PDF embebido

#### 3.8 Servicios API
- [ ] `facturasApi.create()`
- [ ] `facturasApi.createDesdeRemitos()`
- [ ] `facturasApi.createDesdePedido()`
- [ ] `facturasApi.getAll()` con filtros completos
- [ ] `facturasApi.getById()`
- [ ] `facturasApi.anular()`
- [ ] `facturasApi.refiscalizar()`
- [ ] `facturasApi.getVencidas()`
- [ ] `facturasApi.getPendientesCobro()`
- [ ] `facturasApi.getPdf()`
- [ ] `facturasApi.getAfipStatus()`
- [ ] `facturasApi.getSifenStatus()`

### Referencia zuluApp - Facturas
**Formularios:**
- `frmFactura.frm` - Alta/edición
- `VTACOMPROBANTES_Listado.asp` - Grilla principal
- `VTACOMPROBANTESPORVENCIMIENTOS_Listado.asp` - Por vencimientos
- `VTACOMPROBANTEAFECHA_Listado.asp` - Rango fechas
- `VTACOMPROBANTESPDF_Mostrar.asp` - Visualización PDF
- `VTACOMPROBANTESANULADOS_Listado.asp` - Anuladas
- `VTACOMPROBANTESVINCULADOS_Listado.asp` - Vinculaciones

**Filtros críticos:**
- Sucursal, Tipo, Fecha emisión/vencimiento, Cliente, Estado fiscal, Estado AFIP, Saldo, Vendedor, Zona

---

## MATRIZ 4: NOTAS DE CRÉDITO (DEVOLUCIONES)
**Prioridad:** MEDIA  
**Dependencias:** Facturas (origen de NC), Stock (reingreso)  
**Responsable sugerido:** Dev Backend Mid + Dev Frontend Mid

### Backend - Notas Crédito

#### 4.1 Entidades y Enums ✅ COMPLETADO
- [x] `MotivoDevolucion` enum
- [x] `TipoDevolucion` enum
- [x] Campos devolución en `Comprobante`

#### 4.2 Comandos ✅ MAYORMENTE COMPLETADO
- [x] `RegistrarDevolucionVentaCommand`
- [x] `RegistrarDevolucionVentaInternaCommand`
- [ ] **GAP:** `AnularNotaCreditoCommand` - Anulación específica NC (reversa stock si reingresó)

#### 4.3 Queries ⚠️ PARCIAL
- [ ] **GAP:** `GetNotasCreditoPagedQuery` - Especializada con filtros específicos NC
- [ ] **GAP:** `GetNotasCreditoPorMotivoQuery` - Agrupadas por motivo
- [ ] **GAP:** `GetDevolucionesPendientesStockQuery` - NC que aún no reingresaron stock

#### 4.4 Endpoints API ❌ FALTA MAYORÍA
- [ ] `POST /api/ventas/notas-credito` - Crear NC (borrador o directa)
- [ ] `GET /api/ventas/notas-credito` - Listar con filtros (motivo, tipo, cliente, fecha, estado stock)
- [ ] `GET /api/ventas/notas-credito/{id}` - Detalle completo
- [ ] `PUT /api/ventas/notas-credito/{id}/anular` - Anular NC
- [ ] `GET /api/ventas/notas-credito/por-motivo` - Estadísticas por motivo
- [ ] `GET /api/ventas/notas-credito/pendientes-stock` - Pendientes reingreso

#### 4.5 Validaciones y Reglas de Negocio ⚠️ PARCIAL
- [x] Validar que comprobante origen sea factura/ND
- [x] Validar que totales no superen origen
- [x] Validar autorización según monto
- [ ] **GAP:** Validar que al anular NC, se revierta movimiento de stock si aplica
- [ ] **GAP:** Validar que al reintegrar dinero, factura origen reduzca saldo

### Frontend - Notas Crédito

#### 4.6 Páginas ⚠️ PARCIAL
- [x] `/app/ventas/notas-credito/page.tsx` - Listado básico
- [ ] **GAP:** Filtros completos (motivo, tipo, estado stock, autorización)
- [ ] **GAP:** `/app/ventas/notas-credito/nueva` - Formulario creación
- [ ] **GAP:** `/app/ventas/notas-credito/[id]` - Detalle completo

#### 4.7 Componentes
- [ ] `NotasCreditoDataTable` - Grilla con todos los filtros
- [ ] `NotaCreditoForm` - Formulario alta con selección factura origen
- [ ] `NotaCreditoDetalle` - Vista completa
- [ ] `NotaCreditoAutorizacionDialog` - Flujo autorización
- [ ] `NotaCreditoEstadoStockBadge` - Indicador estado stock

#### 4.8 Servicios API
- [ ] `notasCreditoApi.create()`
- [ ] `notasCreditoApi.getAll()` con filtros completos
- [ ] `notasCreditoApi.getById()`
- [ ] `notasCreditoApi.anular()`

### Referencia zuluApp - Notas Crédito
**Formularios:**
- `frmNotaCredito.frm` - Alta/edición
- Similar a facturas pero con campos específicos de devolución

---

## MATRIZ 5: NOTAS DE DÉBITO
**Prioridad:** MEDIA  
**Dependencias:** Facturas (origen de ND)  
**Responsable sugerido:** Dev Backend Junior + Dev Frontend Junior

### Backend - Notas Débito

#### 5.1 Entidades y Enums ✅ COMPLETADO
- [x] `MotivoDebito` entity
- [x] Campos débito en `Comprobante`

#### 5.2 Comandos ✅ COMPLETADO
- [x] `RegistrarNotaDebitoVentaCommand`

#### 5.3 Queries ✅ COMPLETADO
- [x] `GetNotasDebitoPagedQuery`
- [x] `GetNotasDebitoByOrigenQuery`
- [x] `GetMotivosDebitoQuery`

#### 5.4 Endpoints API ✅ MAYORMENTE COMPLETADO
- [x] `POST /api/ventas/notas-debito` - Crear ND
- [x] `GET /api/ventas/notas-debito` - Listar con filtros
- [ ] **GAP:** `GET /api/ventas/notas-debito/{id}` - Detalle completo
- [ ] **GAP:** `PUT /api/ventas/notas-debito/{id}/anular` - Anular ND

#### 5.5 Validaciones y Reglas de Negocio ✅ COMPLETADO
- [x] Validar comprobante origen
- [x] Validar motivo débito
- [x] Validar totales

### Frontend - Notas Débito

#### 5.6 Páginas ⚠️ PARCIAL
- [x] `/app/ventas/notas-debito/page.tsx` - Listado básico
- [ ] **GAP:** `/app/ventas/notas-debito/nueva` - Formulario creación
- [ ] **GAP:** `/app/ventas/notas-debito/[id]` - Detalle completo

#### 5.7 Componentes
- [ ] `NotasDebitoDataTable` - Grilla con filtros
- [ ] `NotaDebitoForm` - Formulario alta
- [ ] `NotaDebitoDetalle` - Vista completa

#### 5.8 Servicios API
- [ ] `notasDebitoApi.create()`
- [ ] `notasDebitoApi.getAll()`
- [ ] `notasDebitoApi.getById()`
- [ ] `notasDebitoApi.getMotivos()`

### Referencia zuluApp - Notas Débito
**Formularios:**
- `frmNotaDebito.frm` - Alta/edición
- Similar a facturas pero con motivos de débito

---

## MATRIZ 6: COBROS Y CUENTA CORRIENTE
**Prioridad:** ALTA  
**Dependencias:** Facturas (aplicar cobros), Recibos (emitir)  
**Responsable sugerido:** Dev Backend Senior + Dev Frontend Senior

### Backend - Cobros

#### 6.1 Entidades y Enums ✅ MAYORMENTE COMPLETADO
- [x] `Cobro` entity
- [x] `Recibo` entity
- [x] `Cheque` entity (para cobros con cheques)
- [ ] **GAP:** `ImputacionCobro` entity (detalle de qué facturas se pagan con cada cobro)

#### 6.2 Comandos ⚠️ PARCIAL
- [x] `RegistrarCobroCommand`
- [x] `EmitirReciboCommand`
- [ ] **GAP:** `AnularCobroCommand` - Anular cobro (reversa aplicaciones)
- [ ] **GAP:** `AplicarCobroAFacturasCommand` - Aplicar cobro a múltiples facturas
- [ ] **GAP:** `RegistrarCobroConChequeCommand` - Cobro con cheque específico

#### 6.3 Queries ❌ FALTA MAYORÍA
- [ ] **GAP:** `GetCobrosPagedQuery` - Listar cobros con filtros
- [ ] **GAP:** `GetCobrosClienteQuery` - Cobros de un cliente
- [ ] **GAP:** `GetSaldoCuentaCorrienteQuery` - Saldo actual del cliente
- [ ] **GAP:** `GetMovimientosCuentaCorrienteQuery` - Movimientos de cuenta corriente
- [ ] **GAP:** `GetFacturasPendientesCobroQuery` - Facturas del cliente con saldo > 0
- [ ] **GAP:** `GetComprobantesClientePendientesCobroQuery` - Ya existe, verificar

#### 6.4 Endpoints API ❌ FALTA MAYORÍA
- [ ] `POST /api/ventas/cobros` - Registrar cobro
- [ ] `POST /api/ventas/cobros/con-cheque` - Cobro con cheque
- [ ] `POST /api/ventas/cobros/{id}/aplicar` - Aplicar a facturas
- [ ] `PUT /api/ventas/cobros/{id}/anular` - Anular cobro
- [ ] `GET /api/ventas/cobros` - Listar cobros
- [ ] `GET /api/ventas/cobros/{id}` - Detalle cobro
- [ ] `GET /api/ventas/cuenta-corriente/{terceroId}` - Cuenta corriente cliente
- [ ] `GET /api/ventas/cuenta-corriente/{terceroId}/saldo` - Saldo actual
- [ ] `GET /api/ventas/cuenta-corriente/{terceroId}/movimientos` - Movimientos
- [ ] `GET /api/ventas/cuenta-corriente/{terceroId}/facturas-pendientes` - Facturas con saldo
- [ ] `POST /api/ventas/recibos` - Emitir recibo
- [ ] `GET /api/ventas/recibos/{id}/pdf` - PDF recibo

#### 6.5 Validaciones y Reglas de Negocio ⚠️ PARCIAL
- [x] Validar que cobro no supere saldo de factura
- [ ] **GAP:** Validar que cheque esté disponible (no usado en otro cobro)
- [ ] **GAP:** Validar que al anular cobro, saldo de facturas se restaure
- [ ] **GAP:** Validar que recibo tenga al menos una aplicación

### Frontend - Cobros y Cuenta Corriente

#### 6.6 Páginas ❌ FALTA MAYORÍA
- [x] `/app/ventas/cobros/page.tsx` - Listado básico
- [ ] **GAP:** `/app/ventas/cobros/nuevo` - Formulario registrar cobro
- [ ] **GAP:** `/app/ventas/cobros/[id]` - Detalle cobro
- [x] `/app/ventas/cuenta-corriente/page.tsx` - Listado básico
- [ ] **GAP:** `/app/ventas/cuenta-corriente/[terceroId]` - Cuenta corriente específica

#### 6.7 Componentes
- [ ] `CobrosDataTable` - Grilla cobros
- [ ] `CobroForm` - Formulario registrar cobro
- [ ] `CobroDetalle` - Vista completa
- [ ] `AplicarCobroDialog` - Aplicar a facturas
- [ ] `CuentaCorrienteTable` - Movimientos cuenta corriente
- [ ] `SaldoCuentaCorrienteBadge` - Indicador saldo
- [ ] `FacturasPendientesCobroTable` - Facturas para cobrar

#### 6.8 Servicios API
- [ ] `cobrosApi.create()`
- [ ] `cobrosApi.createConCheque()`
- [ ] `cobrosApi.aplicar()`
- [ ] `cobrosApi.anular()`
- [ ] `cobrosApi.getAll()`
- [ ] `cobrosApi.getById()`
- [ ] `cuentaCorrienteApi.getSaldo()`
- [ ] `cuentaCorrienteApi.getMovimientos()`
- [ ] `cuentaCorrienteApi.getFacturasPendientes()`

### Referencia zuluApp - Cobros
**Formularios:**
- `frmCobro.frm` - Registrar cobro
- `frmRecibo.frm` - Emitir recibo
- `VTACUENTACORRIENTE_Listado.asp` - Cuenta corriente
- `VTACOBRANZASREALIZADAS_Listado.asp` - Cobros realizados

---

## MATRIZ 7: CHEQUES (TERCER CHEQUES DE CLIENTES)
**Prioridad:** MEDIA  
**Dependencias:** Cobros (cheques recibidos)  
**Responsable sugerido:** Dev Backend Mid + Dev Frontend Junior

### Backend - Cheques

#### 7.1 Entidades y Enums ✅ COMPLETADO
- [x] `Cheque` entity
- [x] `EstadoCheque` enum
- [x] `MotivoChequeRechazado` enum

#### 7.2 Comandos ✅ MAYORMENTE COMPLETADO
- [x] `CreateChequeCommand`
- [x] `CambiarEstadoChequeCommand`
- [ ] **GAP:** `DepositarChequesCommand` - Depositar múltiples
- [ ] **GAP:** `EndosarChequeCommand` - Endosar a proveedor

#### 7.3 Queries ✅ MAYORMENTE COMPLETADO
- [x] `GetChequesTerceroDisponiblesQuery`
- [ ] **GAP:** `GetChequesPagedQuery` - Listar todos con filtros (estado, emisor, fecha)
- [ ] **GAP:** `GetChequesCarteraQuery` - Cheques en cartera

#### 7.4 Endpoints API ⚠️ PARCIAL
- [x] `POST /api/cheques` - Crear cheque
- [x] `PUT /api/cheques/{id}/estado` - Cambiar estado
- [x] `GET /api/cheques/tercero/{terceroId}/disponibles` - Disponibles de tercero
- [ ] **GAP:** `GET /api/cheques` - Listar con filtros
- [ ] **GAP:** `GET /api/cheques/cartera` - En cartera
- [ ] **GAP:** `POST /api/cheques/depositar` - Depositar múltiples
- [ ] **GAP:** `PUT /api/cheques/{id}/endosar` - Endosar

#### 7.5 Validaciones y Reglas de Negocio ✅ MAYORMENTE COMPLETADO
- [x] Validar cambios de estado permitidos
- [x] Validar motivo rechazo al rechazar
- [ ] **GAP:** Validar que cheque endosado no se pueda depositar

### Frontend - Cheques

#### 7.6 Páginas ⚠️ PARCIAL
- [x] `/app/ventas/cheques/page.tsx` - Listado básico
- [ ] **GAP:** Filtros completos (estado, emisor, banco, fecha pago)
- [ ] **GAP:** `/app/ventas/cheques/cartera` - Dashboard de cartera

#### 7.7 Componentes
- [ ] `ChequesDataTable` - Grilla con filtros
- [ ] `ChequeDetalle` - Vista completa
- [ ] `ChequeEstadoBadge` - Indicador estado
- [ ] `DepositarChequesDialog` - Depositar múltiples
- [ ] `EndosarChequeDialog` - Endosar

#### 7.8 Servicios API
- [ ] `chequesApi.getAll()`
- [ ] `chequesApi.getCartera()`
- [ ] `chequesApi.depositar()`
- [ ] `chequesApi.endosar()`

### Referencia zuluApp - Cheques
**Formularios:**
- `frmCheque.frm` - Alta/edición
- `VTACHEQUESCARTERA_Listado.asp` - Cartera
- `VTACONCEPTOSCHEQUESRECHAZADOS_Listado.asp` - Rechazados

---

## MATRIZ 8: LISTAS DE PRECIOS
**Prioridad:** MEDIA  
**Dependencias:** Items (productos)  
**Responsable sugerido:** Dev Backend Junior + Dev Frontend Junior

### Backend - Listas Precios

#### 8.1 Entidades y Enums ✅ COMPLETADO
- [x] `ListaPrecios` entity
- [x] `ListaPreciosItem` entity
- [x] `PrecioEspecialCliente` entity
- [x] `PrecioEspecialVendedor` entity
- [x] `PrecioEspecialCanal` entity
- [x] `ListaPreciosPromocion` entity

#### 8.2 Comandos ✅ COMPLETADO
- [x] `CreateListaPreciosCommand`
- [x] `UpdateListaPreciosCommand`

#### 8.3 Queries ✅ COMPLETADO
- [x] `GetListasPreciosPagedQuery`
- [x] `ResolvePrecioListaQuery`
- [x] `GetItemPrecioQuery`

#### 8.4 Endpoints API ✅ MAYORMENTE COMPLETADO
- [x] `POST /api/listas-precios` - Crear
- [x] `PUT /api/listas-precios/{id}` - Actualizar
- [x] `GET /api/listas-precios` - Listar
- [x] `GET /api/items/{id}/precio` - Resolver precio
- [ ] **GAP:** `DELETE /api/listas-precios/{id}` - Eliminar
- [ ] **GAP:** `POST /api/listas-precios/{id}/clonar` - Clonar

#### 8.5 Validaciones y Reglas de Negocio ✅ COMPLETADO
- [x] Validar vigencia de lista
- [x] Resolver precedencia (especial cliente > especial vendedor > especial canal > lista)

### Frontend - Listas Precios

#### 8.6 Páginas ✅ MAYORMENTE COMPLETADO
- [x] `/app/ventas/listas-precios/page.tsx` - Listado
- [ ] **GAP:** `/app/ventas/listas-precios/nueva` - Formulario creación
- [ ] **GAP:** `/app/ventas/listas-precios/[id]` - Edición

#### 8.7 Componentes
- [ ] `ListasPreciosDataTable` - Grilla
- [ ] `ListaPreciosForm` - Formulario
- [ ] `PreciosEspecialesManager` - Gestión precios especiales

#### 8.8 Servicios API
- [ ] `listasPreciosApi.create()`
- [ ] `listasPreciosApi.update()`
- [ ] `listasPreciosApi.getAll()`
- [ ] `listasPreciosApi.delete()`
- [ ] `listasPreciosApi.clonar()`

---

## MATRIZ 9: REPORTES Y CONSULTAS
**Prioridad:** BAJA  
**Dependencias:** Todos los módulos anteriores  
**Responsable sugerido:** Dev Full Stack Mid

### Backend - Reportes

#### 9.1 Queries ❌ FALTA MAYORÍA
- [ ] **GAP:** `GetAnalisisFacturadoMensualQuery` - Facturación mensual
- [ ] **GAP:** `GetCobranzasPorVendedorQuery` - Cobranzas por vendedor
- [ ] **GAP:** `GetMargenesVentasQuery` - Análisis de márgenes
- [ ] **GAP:** `GetComprobantesPorVencimientosQuery` - Agrupado por vencimiento
- [ ] **GAP:** `GetComprobantesAnuladosQuery` - Histórico anulaciones

#### 9.2 Endpoints API ❌ FALTA MAYORÍA
- [ ] `GET /api/ventas/reportes/facturacion-mensual` - Facturación mensual
- [ ] `GET /api/ventas/reportes/cobranzas-vendedor` - Por vendedor
- [ ] `GET /api/ventas/reportes/margenes` - Márgenes
- [ ] `GET /api/ventas/reportes/vencimientos` - Por vencimiento
- [ ] `GET /api/ventas/reportes/anulaciones` - Anulaciones

### Frontend - Reportes

#### 9.3 Páginas ❌ FALTA MAYORÍA
- [x] `/app/ventas/reportes/page.tsx` - Índice reportes
- [ ] **GAP:** `/app/ventas/reportes/facturacion-mensual`
- [ ] **GAP:** `/app/ventas/reportes/cobranzas-vendedor`
- [ ] **GAP:** `/app/ventas/reportes/margenes`
- [ ] **GAP:** `/app/ventas/reportes/vencimientos`

#### 9.4 Componentes
- [ ] `FacturacionMensualChart` - Gráfico facturación
- [ ] `CobranzasVendedorTable` - Tabla cobranzas
- [ ] `MargenesVentasChart` - Gráfico márgenes
- [ ] `VencimientosTimeline` - Timeline vencimientos

### Referencia zuluApp - Reportes
**Formularios:**
- `VTAANALISISFACTURADOMENSUAL_Listado.asp` - Facturación mensual
- `VTACOBRANZASPORVENDEDOR_Listado.asp` - Cobranzas vendedor
- `VTAConsultarMargenesVentas_Listado.asp` - Márgenes
- `VTACOMPROBANTESPORVENCIMIENTOS_Listado.asp` - Vencimientos
- `VTACOMPROBANTESANULADOS_Listado.asp` - Anulados

---

## CÓMO TRABAJAR EN PARALELO

### Estrategia de Paralelización

#### 1. Equipos Independientes por Matriz

**Equipo A: Pedidos (Matriz 1)**
- Dev Backend Senior
- Dev Frontend Junior
- Duración estimada: 2-3 semanas

**Equipo B: Remitos (Matriz 2)**
- Dev Backend Mid
- Dev Frontend Mid
- Duración estimada: 2-3 semanas
- **Depende de:** Equipo A (solo para vinculaciones, puede arrancar en paralelo con stub)

**Equipo C: Facturas (Matriz 3)**
- Dev Backend Senior
- Dev Frontend Senior
- Duración estimada: 3-4 semanas
- **Depende de:** Equipo B (solo para facturación desde remitos, puede arrancar en paralelo)

**Equipo D: Notas Crédito/Débito (Matrices 4 y 5)**
- Dev Backend Mid
- Dev Frontend Mid
- Duración estimada: 2 semanas
- **Depende de:** Equipo C (leve, puede usar stubs)

**Equipo E: Cobros (Matriz 6)**
- Dev Backend Senior
- Dev Frontend Senior
- Duración estimada: 2-3 semanas
- **Depende de:** Equipo C (necesita facturas, puede arrancar semana 2)

**Equipo F: Cheques + Listas Precios (Matrices 7 y 8)**
- Dev Backend Junior
- Dev Frontend Junior
- Duración estimada: 1-2 semanas
- **Depende de:** Equipo E (cheques depende de cobros)

**Equipo G: Reportes (Matriz 9)**
- Dev Full Stack Mid
- Duración estimada: 1-2 semanas
- **Depende de:** Todos (arrancar al final)

#### 2. Cronograma Gantt Sugerido

```
Semana 1-2:  [Equipo A]  [Equipo B]  [Equipo C]
Semana 3-4:  [Equipo A]  [Equipo B]  [Equipo C]  [Equipo E]
Semana 5-6:                          [Equipo D]  [Equipo E]  [Equipo F]
Semana 7-8:                                      [Equipo G]
```

#### 3. Reglas de Sincronización

**Daily Sync:** 15 min diarios - cada equipo reporta:
- ¿Qué completaste ayer?
- ¿Qué harás hoy?
- ¿Bloqueantes?

**Integration Points (2x semana):**
- Lunes: Code review cruzado
- Jueves: Integración de ramas + validación E2E

**Branching Strategy:**
```
main
├─ feature/pedidos (Equipo A)
├─ feature/remitos (Equipo B)
├─ feature/facturas (Equipo C)
├─ feature/notas-credito (Equipo D)
├─ feature/cobros (Equipo E)
├─ feature/cheques (Equipo F)
└─ feature/reportes (Equipo G)
```

**Merge Order:**
1. Pedidos → main (week 2)
2. Remitos → main (week 3)
3. Facturas → main (week 4)
4. Notas Crédito/Débito → main (week 5)
5. Cobros → main (week 6)
6. Cheques → main (week 6)
7. Reportes → main (week 8)

#### 4. Stubs para Dependencias No Listas

**Ejemplo:** Equipo C (Facturas) necesita remitos pero Equipo B aún no termina.

**Stub remitosApi.getParaFacturar():**
```typescript
// lib/api/stubs/remitos-stub.ts
export const remitosStub = {
  getParaFacturar: async (terceroId: number) => ({
    data: [
      { id: 1, numero: '0001-00000123', fecha: '2026-03-15', total: 15000 },
      { id: 2, numero: '0001-00000124', fecha: '2026-03-16', total: 8500 }
    ]
  })
}

// Usar stub hasta que Equipo B entregue
import { remitosStub as remitosApi } from '@/lib/api/stubs/remitos-stub'
// Luego reemplazar con:
// import { remitosApi } from '@/lib/api/remitos'
```

#### 5. Comunicación Async

**Slack/Teams:**
- Canal `#ventas-backend` - Equipo backend coordina
- Canal `#ventas-frontend` - Equipo frontend coordina
- Canal `#ventas-sync` - Cross-team blockers

**Notion/Confluence:**
- Documentar decisiones de diseño
- Actualizar estado de cada matriz daily

**GitHub:**
- Issues por task de matriz
- Labels: `matriz-1-pedidos`, `backend`, `frontend`, `blocker`
- PRs pequeños (max 500 LOC) - review < 24hs

#### 6. Definition of Done (DoD) por Matriz

**Backend DoD:**
- [ ] Entidades/Enums creados
- [ ] Comandos con validaciones completas
- [ ] Queries con filtros completos
- [ ] Endpoints API documentados (Swagger)
- [ ] Unit tests > 80% coverage
- [ ] Integration tests de endpoints críticos
- [ ] Validado contra BD local PostgreSQL

**Frontend DoD:**
- [ ] Páginas creadas con routing correcto
- [ ] Componentes con props typados (TypeScript)
- [ ] Servicios API implementados
- [ ] Manejo de errores y loading states
- [ ] Validaciones de formularios (React Hook Form + Zod)
- [ ] Tests E2E de flujos críticos (Playwright)
- [ ] Responsive mobile/tablet/desktop

**Matriz DoD (completa):**
- [ ] Backend DoD ✅
- [ ] Frontend DoD ✅
- [ ] Integración E2E validada
- [ ] Paridad funcional con zuluApp validada por PO
- [ ] Documentación actualizada

#### 7. Validación de Paridad con ZuluApp

**Checklist por matriz:**

Para cada funcionalidad de zuluApp:
1. ✅ Identificar en formulario/vista ASP
2. ✅ Listar campos visibles
3. ✅ Listar acciones disponibles
4. ✅ Listar filtros/búsquedas
5. ✅ Implementar en backend
6. ✅ Implementar en frontend
7. ✅ Validar funcionalmente (mismos inputs → mismos outputs)

**Acceptance Test:**
- PO prueba en zuluApp
- PO repite misma operación en ZuluIA_Front
- Comparar resultados

#### 8. Herramientas Recomendadas

**Backend:**
- **Tests:** xUnit + FluentAssertions
- **Mocks:** Moq
- **Coverage:** Coverlet
- **DB:** EF Core migrations + PostgreSQL local

**Frontend:**
- **Tests E2E:** Playwright
- **Storybook:** Documentar componentes aislados
- **React Query:** Manejo de estado server
- **Zod:** Validaciones compartidas con backend (si usan TypeScript)

**DevOps:**
- **CI/CD:** GitHub Actions
  - Pipeline backend: build + test + migrations check
  - Pipeline frontend: build + lint + test E2E
- **Environments:**
  - Local (cada dev)
  - Dev (integración continua)
  - Staging (validación PO)
  - Prod (solo después de 100% paridad)

---

## MÉTRICAS DE PROGRESO

### KPIs por Semana

**Semana 1-2:**
- [ ] 50% backend pedidos
- [ ] 30% backend remitos
- [ ] 20% backend facturas

**Semana 3-4:**
- [ ] 100% backend pedidos ✅
- [ ] 80% backend remitos
- [ ] 60% backend facturas
- [ ] 30% frontend pedidos

**Semana 5-6:**
- [ ] 100% backend remitos ✅
- [ ] 100% backend facturas ✅
- [ ] 100% backend notas crédito/débito ✅
- [ ] 80% frontend pedidos
- [ ] 50% frontend remitos

**Semana 7-8:**
- [ ] 100% backend cobros ✅
- [ ] 100% backend cheques ✅
- [ ] 100% frontend facturas ✅
- [ ] 80% frontend cobros
- [ ] 100% reportes ✅

### Burndown Chart Ideal

```
100% │
     │╲
 80% │ ╲___
     │     ╲___
 60% │         ╲___
     │             ╲___
 40% │                 ╲___
     │                     ╲___
 20% │                         ╲___
     │                             ╲___
  0% └────────────────────────────────╲
     W1  W2  W3  W4  W5  W6  W7  W8
```

---

## CONTACTOS Y RESPONSABILIDADES

### Tech Leads
- **Backend Lead:** [Nombre] - Coordina matrices 1-9 backend
- **Frontend Lead:** [Nombre] - Coordina matrices 1-9 frontend
- **QA Lead:** [Nombre] - Valida paridad funcional vs zuluApp

### Product Owner
- **PO:** [Nombre] - Valida acceptance de cada matriz

### Daily Scrum
- **Hora:** 9:30 AM
- **Duración:** 15 min
- **Platform:** Google Meet / Zoom

### Integration Sync
- **Día:** Lunes y Jueves
- **Hora:** 16:00 PM
- **Duración:** 30 min

---

## CONCLUSIÓN

Esta estructura matricial permite:
1. ✅ **Trabajo paralelo** de hasta 7 equipos simultáneos
2. ✅ **Independencia** entre matrices (dependencias manejadas con stubs)
3. ✅ **Trazabilidad** del progreso (cada matriz = milestone)
4. ✅ **Paridad garantizada** (DoD exige validación funcional vs zuluApp)
5. ✅ **Entrega incremental** (merges semanales a main)

**Próximos pasos:**
1. Asignar equipos a matrices
2. Crear issues en GitHub por cada tarea
3. Setup de ramas por feature
4. Kickoff meeting con todos los equipos
5. ¡Arrancar desarrollo!

---

**Última actualización:** 2026-03-31  
**Versión:** 1.0
