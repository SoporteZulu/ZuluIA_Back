# Plan de Implementación - Paridad Funcional Ventas/Facturación vs zuluApp

## Estado Actual del Plan

**Objetivo:** Alcanzar paridad funcional del 100% del módulo de ventas/facturación de ZuluIA_Back con el sistema legacy zuluApp.

**Pasos Totales:** 17
**Completados:** 2
**Pendientes:** 15
**Progreso:** 12%

---

## Pasos Completados ✅

### Paso 1: Auditoría de Contratos ✅
**Entregable:** `docs\auditoria-ventas-facturacion-gaps.md`

**Resultado:** Identificados 24 gaps funcionales clasificados en:
- **7 CRÍTICOS** (bloquean reemplazo funcional)
- **8 ALTOS** (reducen valor operativo)
- **9 MEDIOS** (mejoran UX)

**Gaps Críticos Identificados:**
1. Vendedor/Cobrador/Zona en comprobantes no se exponen
2. Logística en remitos sin transporte, chofer, patentes
3. Pedidos sin lógica propia (sin reserva stock, aprobación, cumplimiento)
4. Listas precios sin motor resolución de precio aplicable
5. Descuentos comerciales inexistentes
6. Devoluciones sin backend dedicado
7. Ajustes de ventas inexistentes

### Paso 2: Ampliar DTOs de Comprobantes ✅
**Entregables:**
- Entidad `Comprobante` ampliada con 30+ campos nuevos
- `ComprobanteConfiguration` EF Core actualizada
- `ComprobanteDto` y `ComprobanteDetalleDto` enriquecidos
- Script SQL: `database\zuluia_back_comprobantes_comercial_logistico_upgrade.sql`

**Campos Agregados:**
- **Comerciales:** VendedorId, CobradorId, ZonaComercialId, ListaPreciosId, CondicionPagoId, PlazoDias, CanalVentaId, PorcentajeComisionVendedor, PorcentajeComisionCobrador
- **Logísticos:** TransporteId, ChoferNombre, ChoferDni, PatVehiculo, PatAcoplado, DomicilioEntrega, ObservacionesLogisticas, FechaEstimadaEntrega, FechaRealEntrega, FirmaConformidad, NombreQuienRecibe
- **Adicionales:** ObservacionInterna, ObservacionFiscal, RecargoPorcentaje, RecargoImporte, DescuentoPorcentaje, TerceroDomicilioSnapshot

**Impacto:** Resuelve gap crítico #1 y sienta base para remitos valorizados completos.

---

## Pasos Pendientes (Prioridad CRÍTICA)

### Paso 3: Facturación Masiva y Automática 🔴 CRÍTICO
**Objetivo:** Implementar endpoints para facturación masiva por lote y facturación automática programada.

**Tareas:**
1. Crear `FacturarDocumentosMasivoCommand` con gestión de errores por documento
2. Crear `FacturarAutomaticamenteCommand` con programación
3. Implementar auditoría de procesos masivos
4. Exponer progreso y errores en tiempo real

**Entregables:**
- `POST /api/ventas/facturas/emitir-masivo`
- `POST /api/ventas/facturas/facturacion-automatica`
- Entidad `ProcesoFacturacionMasiva` con auditoría
- Query `GetProgresoFacturacionMasivaQuery`

**Dependencias:** Paso 2 completado ✅

**Criticidad:** Alta - legacy lo usa intensivamente

---

### Paso 4: Pedidos con Reserva de Stock 🔴 CRÍTICO
**Objetivo:** Crear agregado `Pedido` con lógica propia, reserva stock, aprobación, y seguimiento cumplimiento.

**Tareas:**
1. Diseñar entidad `Pedido` separada de `Comprobante` genérico
2. Implementar estados propios: Pendiente, Aprobado, Rechazado, EnPreparacion, CumplidoParcial, CumplidoTotal, Anulado
3. Crear `PedidoItem` con `CantidadSolicitada`, `CantidadReservada`, `CantidadEntregada`, `CantidadPendiente`
4. Implementar reserva y liberación de stock
5. Workflow de aprobación/rechazo
6. Conversión a remito/factura con actualización de cumplimiento

**Entregables:**
- Entidad `Pedido` y `PedidoItem`
- `CrearPedidoCommand`, `AprobarPedidoCommand`, `RechazarPedidoCommand`
- `ConvertirPedidoARemitoCommand` con trazabilidad cantidades
- Script SQL: `zuluia_back_pedidos_reserva_stock_upgrade.sql`

**Dependencias:** Requiere módulo de stock con reservas

**Criticidad:** **MUY ALTA** - Sin esto, no hay trazabilidad real pedido->remito->factura

---

### Paso 9: Motor Resolución de Precio 🔴 CRÍTICO
**Objetivo:** Implementar endpoint que dado cliente+item+fecha+cantidad retorna precio aplicable.

**Tareas:**
1. Diseñar reglas de prioridad: precio especial cliente > precio lista vigente > precio base
2. Implementar herencia entre listas de precios
3. Crear `ListaPrecioEspecialCliente`, `ListaPrecioEspecialCanal`, `ListaPrecioEspecialVendedor`
4. Implementar promociones con vigencia
5. Endpoint de resolución: `GET /api/listas-precios/resolver?clienteId=X&itemId=Y&fecha=Z&cantidad=N`

**Entregables:**
- Servicio `PrecioResolutionService`
- Entidades `ListaPrecioEspecial`, `Promocion`
- Query `ResolverPrecioAplicableQuery`
- Reglas de prioridad documentadas

**Dependencias:** Ninguna

**Criticidad:** **MUY ALTA** - Sin esto, no hay coherencia de precios en ventas

---

### Paso 10: Descuentos Comerciales 🔴 CRÍTICO
**Objetivo:** Implementar gestión de descuentos estructurados con validación.

**Tareas:**
1. Crear entidad `DescuentoComercial` con vigencias
2. Tipos: por cliente+item, por categoríaCliente+rubroItem
3. Exponer `PorcentajeMaximoDescuento` en `TerceroDto`
4. Validar en `EmitirDocumentoVentaCommand` que descuento aplicado ≤ máximo permitido
5. CRUD completo de descuentos

**Entregables:**
- Entidad `DescuentoComercial`
- `CreateDescuentoCommand`, `UpdateDescuentoCommand`
- Validación en emisión de comprobantes
- Script SQL: `zuluia_back_descuentos_comerciales_upgrade.sql`

**Dependencias:** Paso 2 completado ✅

**Criticidad:** **MUY ALTA** - Requerido para emisión correcta de facturas con descuentos

---

### Paso 7: Devoluciones Backend Dedicado 🔴 CRÍTICO
**Objetivo:** Crear módulo completo de devoluciones con workflow.

**Tareas:**
1. Entidad `DevolucionVenta` separada o vista materializada
2. Estados: Pendiente, Autorizada, Rechazada, Procesada (NC emitida)
3. Query `GetDevolucionesPagedQuery` con filtros
4. Workflow de autorización previa a emisión NC
5. Diferencias por renglón con motivo

**Entregables:**
- Entidad/vista `DevolucionVenta`
- `RegistrarDevolucionCommand` (ya existe, completar)
- `AutorizarDevolucionCommand`, `RechazarDevolucionCommand`
- `GetDevolucionesPagedQuery`

**Dependencias:** Notas de crédito formalizadas (Paso 6)

**Criticidad:** Alta - legacy lo gestiona separado

---

### Paso 8: Ajustes de Ventas 🔴 CRÍTICO
**Objetivo:** Crear módulo completo de ajustes comerciales/fiscales.

**Tareas:**
1. Entidad `AjusteVenta` con tipo (Crédito/Débito) y motivo tipificado
2. Catálogo de motivos: diferencia precio, error carga, bonificación posterior, etc.
3. Impacto selectivo en stock, cuenta corriente, IVA
4. Workflow de autorización por monto/tipo
5. Auditoría completa

**Entregables:**
- Entidad `AjusteVenta`, catálogo `MotivoAjusteVenta`
- `RegistrarAjusteCreditoCommand`, `RegistrarAjusteDebitoCommand`
- `AutorizarAjusteCommand`
- Script SQL: `zuluia_back_ajustes_ventas_upgrade.sql`

**Dependencias:** Ninguna

**Criticidad:** Alta - funcionalidad usada frecuentemente en legacy

---

## Pasos Pendientes (Prioridad ALTA)

### Paso 5: Remitos Completos con Logística
**Objetivo:** Completar remitos con atributos logísticos y estados de entrega.

### Paso 6: Notas de Crédito/Débito con Vínculos Formales
**Objetivo:** Formalizar vínculo renglón-a-renglón con factura origen.

### Paso 11: Dashboard y KPIs Ventas
**Objetivo:** Endpoint agregado de dashboard con métricas en tiempo real.

### Paso 12: Cuenta Corriente y Aging
**Objetivo:** Aging por tramos, exportación, impresión.

### Paso 13: Imputaciones Avanzadas
**Objetivo:** Endpoint agregado para pantalla completa sin estado local.

---

## Pasos Pendientes (Prioridad MEDIA)

### Paso 14: Scripts SQL Datos Maestros
**Objetivo:** Semillas de tipos comprobante, catálogos motivos NC/ND, estados.

### Paso 15: Endpoints Impresión/Exportación
**Objetivo:** Reimpresión facturas, exportación remitos, reportes.

### Paso 16: Tests Integración Circuito Completo
**Objetivo:** Test pedido->remito->factura, facturación masiva, NC con devolución.

### Paso 17: Documentación APIs y Contratos
**Objetivo:** Swagger completo, ejemplos requests, checklist paridad.

---

## Roadmap Recomendado

### Sprint 1 (Crítico - Fundacional)
✅ Paso 1: Auditoría (COMPLETADO)
✅ Paso 2: DTOs Comprobantes (COMPLETADO)
🔴 Paso 4: Pedidos con Reserva Stock
🔴 Paso 9: Motor Resolución Precio
🔴 Paso 10: Descuentos Comerciales

### Sprint 2 (Crítico - Operativo)
🔴 Paso 3: Facturación Masiva/Automática
🔴 Paso 7: Devoluciones Backend Dedicado
🔴 Paso 8: Ajustes de Ventas
🟡 Paso 5: Remitos Completos

### Sprint 3 (Alto - Cuenta Corriente)
🟡 Paso 6: NC/ND con Vínculos Formales
🟡 Paso 12: Cuenta Corriente y Aging
🟡 Paso 13: Imputaciones Avanzadas
🟡 Paso 11: Dashboard KPIs

### Sprint 4 (Cierre - Soporte y Docs)
⚪ Paso 14: Scripts SQL Maestros
⚪ Paso 15: Impresión/Exportación
⚪ Paso 16: Tests Integración
⚪ Paso 17: Documentación Final

---

## Métricas de Éxito

### Funcionales
- [ ] Todos los campos visibles en frmFacturaVenta cubiertos
- [ ] Pedidos con reserva stock operativos
- [ ] Motor precio resuelve correctamente con prioridades
- [ ] Descuentos validados al emitir facturas
- [ ] Devoluciones con workflow completo
- [ ] Ajustes con autorización funcionando
- [ ] Dashboard muestra KPIs en tiempo real
- [ ] Cuenta corriente con aging exportable
- [ ] Imputaciones sin dependencia local en frontend

### Técnicas
- [ ] Migración SQL ejecutada sin errores
- [ ] DTOs poblados completamente desde queries
- [ ] Tests de integración circuito completo pasan
- [ ] Swagger documentado al 100%
- [ ] Smoke tests cubren endpoints críticos

### Negocio
- [ ] Frontend puede reemplazar frmFacturaVenta sin overlays locales
- [ ] Frontend puede reemplazar frmNotaPedido completamente
- [ ] Frontend puede reemplazar frmRemitos completamente
- [ ] Operación diaria de ventas funciona sin dependencia de zuluApp

---

## Riesgos y Mitigaciones

### Riesgo 1: Complejidad Motor Precios
**Mitigación:** Implementar en fases: primero lista simple, luego especiales, luego herencia

### Riesgo 2: Reserva Stock en Pedidos
**Mitigación:** Coordinar con equipo de stock, validar concurrencia

### Riesgo 3: Migración Datos Legacy
**Mitigación:** Scripts idempotentes, rollback plan, validación post-migración

### Riesgo 4: Cambio de Contratos en Frontend
**Mitigación:** Versionado APIs, deprecation gradual, comunicación con equipo front

---

## Próxima Acción Recomendada

**Iniciar Paso 4: Pedidos con Reserva de Stock**

Motivo: Es el gap crítico más bloqueante para el circuito completo pedido->remito->factura, que es el flujo core de ventas. Sin esto, no hay trazabilidad real ni reserva de stock, lo cual impide reemplazar frmNotaPedido.

**Alternativa:** Si el equipo de stock no está disponible, iniciar **Paso 9: Motor Resolución Precio** en paralelo, ya que no tiene dependencias externas.

---

## Notas Finales

Este plan representa el trabajo necesario para alcanzar paridad funcional total con zuluApp en el módulo de ventas/facturación. Se estima entre 6-8 semanas de desarrollo con equipo dedicado.

**Progreso actual:** 12% (2 de 17 pasos)
**Criticidad alta pendiente:** 5 pasos
**Dependencias externas:** Módulo de stock (para pedidos con reserva)

**Fecha documento:** $(Get-Date)
**Responsable:** Equipo ZuluIA Backend
**Revisión:** Mensual o al completar cada sprint
