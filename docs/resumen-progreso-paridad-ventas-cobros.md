# Resumen de Progreso - Paridad Funcional Ventas/Cobros

**Fecha:** 2025-01-20  
**Objetivo:** Lograr paridad funcional 100% de la vista de ventas/cobros comparando ZuluIA_Back con zuluApp legacy

---

## Estado Actual del Plan

**Pasos Totales:** 17  
**Completados:** 5 (29%)  
**En Progreso:** 0  
**Pendientes:** 12 (71%)

---

## ✅ Pasos Completados (5/17)

### ✅ Paso 1: Auditar vista de cobros zuluApp
**Completado:** 2025-01-20

**Entregables:**
- `docs/auditoria-cobros-zuluapp-vs-backend.md` - Auditoría completa con 32 gaps identificados
  - 15 CRÍTICOS: Bloquean reemplazo funcional
  - 10 ALTOS: Reducen valor operativo  
  - 7 MEDIOS: Mejoran UX y trazabilidad

**Formularios Legacy Relevados:**
- frmCobro (cobro general)
- frmCobroVentanillaCP (contra pedido)
- frmCobroVentanillaCT (mostrador)
- frmAuditoriaCheques
- frmChequesTerceroVenta/Deposito/Rechazo

**Roadmap Definido:**
- 6 sprints de trabajo (7 semanas)
- Priorización clara por criticidad

---

### ✅ Paso 2: Ampliar CobroDto con campos faltantes
**Completado:** 2025-01-20

**Cambios en Entidades:**

**Cobro.cs:**
- ✅ VendedorId, CobradorId, ZonaComercialId
- ✅ UsuarioCajeroId, VentanillaTurno
- ✅ TipoCobro (enum: Administrativo, VentanillaContraEntrega, VentanillaContraPedido, CobranzaEnRuta, Bancario, Electronico)
- ✅ ObservacionInterna
- ✅ TerceroCuit, TerceroCondicionIva, TerceroDomicilioSnapshot

**CobroMedio.cs:**
- ✅ BancoOrigen, BancoDestino, NumeroOperacion, FechaAcreditacion (para transferencias)
- ✅ TerminalPOS, NumeroCupon, NumeroLote, CodigoAutorizacion, CantidadCuotas, PlanCuotas, FechaAcreditacionEstimada (para tarjetas)

**DTOs Actualizados:**

**CobroDto:**
- ✅ 25+ campos nuevos comerciales, operativos y fiscales
- ✅ TotalEfectivo, TotalCheques, TotalElectronico
- ✅ CreatedBy, UpdatedBy con nombres de usuario

**CobroMedioDto:**
- ✅ 12+ campos específicos para tarjetas/transferencias
- ✅ Datos de cheque vinculado (número, banco)

**Archivos Creados/Modificados:**
- src/ZuluIA_Back.Domain/Entities/Finanzas/Cobro.cs ✏️
- src/ZuluIA_Back.Domain/Entities/Finanzas/CobroMedio.cs ✏️
- src/ZuluIA_Back.Domain/Enums/TipoCobro.cs ✨ NEW
- src/ZuluIA_Back.Application/Features/Finanzas/DTOs/CobroDto.cs ✏️
- database/zuluia_back_cobros_recibos_upgrade.md ✨ NEW (script SQL)

---

### ✅ Paso 3: Ampliar ReciboDto con campos comerciales
**Completado:** 2025-01-20 (integrado con Paso 2)

**Cambios en Entidades:**

**Recibo.cs:**
- ✅ VendedorId, CobradorId, ZonaComercialId, UsuarioCajeroId
- ✅ TerceroCuit, TerceroCondicionIva, TerceroDomicilio (snapshot)
- ✅ LeyendaFiscal
- ✅ FormatoImpresion, CopiasImpresas, FechaImpresion
- ✅ Método RegistrarImpresion()

**ReciboItem.cs:**
- ✅ ComprobanteImputadoId (vinculación a factura)

**DTOs Actualizados:**

**ReciboDto:**
- ✅ Datos comerciales (vendedor, cobrador, zona)
- ✅ Snapshot de datos fiscales del tercero
- ✅ Leyendas fiscales
- ✅ Metadatos de impresión
- ✅ Auditoría de usuario creador

**ReciboItemDto:**
- ✅ ComprobanteImputadoId, Numero, Tipo

**Archivos Creados/Modificados:**
- src/ZuluIA_Back.Domain/Entities/Finanzas/Recibo.cs ✏️
- src/ZuluIA_Back.Domain/Entities/Finanzas/ReciboItem.cs ✏️
- src/ZuluIA_Back.Application/Features/Recibos/DTOs/ReciboDto.cs ✏️

---

### ✅ Paso 4: Implementar query GetComprobantesClientePendientesCobroQuery
**Completado:** 2025-01-20

**Funcionalidad Implementada:**
- Query que retorna facturas con saldo pendiente de un cliente
- Cálculo automático de días de mora
- Filtros por sucursal, moneda, solo vencidos, fecha hasta
- Ordenamiento por antigüedad (fecha vencimiento, luego fecha emisión)

**DTO Creado:**

**ComprobantePendienteCobroDto:**
- ✅ Datos completos del comprobante (tipo, número, fechas)
- ✅ DiasMora calculados
- ✅ EstaVencido (flag)
- ✅ ImporteTotal, ImporteCobrado, SaldoPendiente
- ✅ ImporteAImputar (campo editable en cliente)

**Endpoint Agregado:**
```
GET /api/cobros/clientes/{terceroId}/pendientes
    ?sucursalId=
    &monedaId=
    &soloVencidos=false
    &fechaHasta=
```

**Archivos Creados:**
- src/ZuluIA_Back.Application/Features/Comprobantes/Queries/GetComprobantesClientePendientesCobroQuery.cs ✨ NEW
- src/ZuluIA_Back.Application/Features/Comprobantes/Queries/GetComprobantesClientePendientesCobroQueryHandler.cs ✨ NEW
- src/ZuluIA_Back.Application/Features/Comprobantes/DTOs/ComprobantePendienteCobroDto.cs ✨ NEW
- src/ZuluIA_Back.Api/Controllers/CobrosController.cs ✏️

---

### ✅ Paso 5: Implementar query GetChequesTerceroDisponiblesQuery
**Completado:** 2025-01-20

**Funcionalidad Implementada:**
- Query que retorna cheques de terceros disponibles para endosar/usar en pagos
- Filtros por banco, moneda, rango de importes, fechas de pago, plaza
- Solo cheques en estado "Cartera" (disponibles)
- Cálculo de días al cobro

**Cambios en Entidad Cheque:**
- ✅ Plaza (campo nuevo)
- ✅ Cuit (campo nuevo)
- ✅ CobroOrigenId (vinculación)
- ✅ PagoDestinoId (vinculación)
- ✅ Método Endosar() para gestionar endoso a proveedores

**DTO Creado:**

**ChequeDisponibleDto:**
- ✅ Datos completos del cheque (número, banco, titular, CUIT)
- ✅ Fechas (emisión, pago) y días al cobro
- ✅ EsDiferido (flag)
- ✅ Origen del cheque (cliente, cobro)
- ✅ Importe y moneda

**Archivos Creados/Modificados:**
- src/ZuluIA_Back.Domain/Entities/Finanzas/Cheque.cs ✏️
- src/ZuluIA_Back.Application/Features/Cheques/Queries/GetChequesTerceroDisponiblesQuery.cs ✨ NEW
- src/ZuluIA_Back.Application/Features/Cheques/Queries/GetChequesTerceroDisponiblesQueryHandler.cs ✨ NEW
- src/ZuluIA_Back.Application/Features/Cheques/DTOs/ChequeDisponibleDto.cs ✨ NEW

---

## 📋 Pasos Pendientes (12/17)

### Paso 6: Ampliar GetComprobanteDetalleQueryHandler con datos de cobro
**Estado:** Pendiente  
**Prioridad:** ALTA

**Tareas:**
- Incluir cobros aplicados al comprobante
- Mostrar anticipos utilizados
- Calcular saldo actualizado con cobros parciales
- Incluir historial de gestión de cobranza

---

### Paso 7: Implementar command RegistrarCobroVentanillaCommand
**Estado:** Pendiente  
**Prioridad:** CRÍTICA

**Tareas:**
- Crear command específico para cobro rápido en mostrador
- Validar apertura de caja obligatoria
- Generar recibo automático
- Imputar automáticamente facturas pendientes
- Calcular vuelto si paga con efectivo

---

### Paso 8: Implementar command RegistrarCobroConChequesTercerosCommand
**Estado:** Pendiente  
**Prioridad:** CRÍTICA

**Tareas:**
- Permitir usar cheques de terceros previamente recibidos
- Validar disponibilidad y estado de cheques
- Actualizar estado a "Endosado" o "Entregado"
- Generar movimientos de tesorería

---

### Paso 9: Implementar query GetResumenCobranzaDelDiaQuery
**Estado:** Pendiente  
**Prioridad:** ALTA

**Tareas:**
- Totalizar cobros del día por forma de pago
- Agrupar por cajero/cobrador
- Comparativo con días anteriores
- Incluir cheques recibidos pendientes de depósito

---

### Paso 10: Implementar query GetReporteGestionCobranzaQuery
**Estado:** Pendiente  
**Prioridad:** ALTA

**Tareas:**
- Listado de clientes con deuda vencida
- Gestiones de cobranza realizadas
- Promesas de pago y seguimiento
- Efectividad de cobranza por cobrador/zona

---

### Paso 11: Implementar command RegistrarGestionCobranzaCommand
**Estado:** Pendiente  
**Prioridad:** MEDIA

**Tareas:**
- Registrar llamada, visita o contacto de cobranza
- Asociar a cliente y facturas gestionadas
- Programar recordatorio de seguimiento
- Registrar resultado (promesa, reclamo, sin contacto)

---

### Paso 12: Ampliar flujo de imputaciones con anticipos
**Estado:** Pendiente  
**Prioridad:** CRÍTICA

**Tareas:**
- Permitir aplicar anticipos a facturas
- Listar anticipos disponibles del cliente
- Registrar imputación automática
- Actualizar saldo de comprobantes

---

### Paso 13: Implementar consulta de cuenta corriente de cliente enriquecida
**Estado:** Pendiente  
**Prioridad:** ALTA

**Tareas:**
- Mostrar movimientos completos (factura, cobro, NC, ND, anticipo)
- Calcular saldo acumulado por fecha
- Indicadores de mora y días de atraso
- Filtrar por sucursal, vendedor, estado

---

### Paso 14: Implementar DTOs de dashboard de ventas/cobros
**Estado:** Pendiente  
**Prioridad:** MEDIA

**Tareas:**
- Ventas del día/mes con comparativo
- Cobros del día/mes con efectividad
- Top clientes por facturación y mora
- Alertas de cobranza y objetivos

---

### Paso 15: Crear endpoints de exportación de reportes
**Estado:** Pendiente  
**Prioridad:** MEDIA

**Tareas:**
- Exportar cuenta corriente a PDF/Excel
- Exportar libro de cobros diario
- Exportar resumen de gestión de cobranza
- Exportar reporte de cheques de terceros

---

### Paso 16: Implementar validaciones de negocio críticas
**Estado:** Pendiente  
**Prioridad:** CRÍTICA

**Tareas:**
- No permitir cobrar sin caja abierta
- Validar límite de crédito antes de vender
- Alertar facturas vencidas al vender
- Bloquear clientes con mora crítica configurable

---

### Paso 17: Crear tests de integración de flujos completos
**Estado:** Pendiente  
**Prioridad:** ALTA

**Tareas:**
- Test: venta → factura → cobro → recibo → cierre caja
- Test: venta → factura → anticipo → aplicación anticipo
- Test: cobro con cheque tercero → depósito → compensación
- Test: gestión de cobranza → promesa → cobro efectivo

---

## 📊 Métricas de Progreso

### Cobertura Funcional Actual
- **Antes:** ~30-40% de funcionalidad de cobros
- **Ahora:** ~45-50% de funcionalidad de cobros
- **Objetivo Final:** 100%

### Gaps Críticos Resueltos
- ✅ **Gap 1:** Usuario cajero/cobrador ✅ RESUELTO
- ✅ **Gap 2:** Datos fiscales del cliente en recibo ✅ RESUELTO
- ✅ **Gap 3:** Vendedor y cobrador asociados ✅ RESUELTO
- ✅ **Gap 4:** Zona comercial ✅ RESUELTO
- ✅ **Gap 5:** Detalles completos de cheques ✅ RESUELTO
- ⚠️ **Gap 6:** Estado avanzado de cheques ✅ PARCIAL (falta enum EstadoCheque completo)
- ✅ **Gap 7:** Gestión de cheques de terceros ✅ RESUELTO (método Endosar)
- ✅ **Gap 8:** Tarjetas con plan de cuotas y POS ✅ RESUELTO
- ✅ **Gap 9:** Transferencias con datos bancarios ✅ RESUELTO
- ❌ **Gap 10:** Imputación automática de facturas ❌ PENDIENTE
- ✅ **Gap 11:** Listado de facturas pendientes del cliente ✅ RESUELTO
- ❌ **Gap 12:** Anticipo/Saldo a favor ❌ PENDIENTE
- ❌ **Gap 13:** Cobro ventanilla simplificado ❌ PENDIENTE
- ❌ **Gap 14:** Validación de caja abierta ❌ PENDIENTE
- ❌ **Gap 15:** Cierre de caja con arqueo ❌ PENDIENTE (requiere módulo específico)

**Gaps Críticos Resueltos:** 9/15 (60%)  
**Gaps Altos Resueltos:** 0/10 (0%)  
**Gaps Medios Resueltos:** 0/7 (0%)

---

## 🗂️ Archivos Generados/Modificados

### Documentación (3 archivos)
- ✨ docs/auditoria-cobros-zuluapp-vs-backend.md
- ✨ database/zuluia_back_cobros_recibos_upgrade.md
- ✨ docs/resumen-progreso-paridad-ventas-cobros.md (este archivo)

### Dominio (4 archivos)
- ✏️ src/ZuluIA_Back.Domain/Entities/Finanzas/Cobro.cs
- ✏️ src/ZuluIA_Back.Domain/Entities/Finanzas/CobroMedio.cs
- ✏️ src/ZuluIA_Back.Domain/Entities/Finanzas/Recibo.cs
- ✏️ src/ZuluIA_Back.Domain/Entities/Finanzas/ReciboItem.cs
- ✏️ src/ZuluIA_Back.Domain/Entities/Finanzas/Cheque.cs
- ✨ src/ZuluIA_Back.Domain/Enums/TipoCobro.cs

### Application DTOs (5 archivos)
- ✏️ src/ZuluIA_Back.Application/Features/Finanzas/DTOs/CobroDto.cs
- ✏️ src/ZuluIA_Back.Application/Features/Recibos/DTOs/ReciboDto.cs
- ✨ src/ZuluIA_Back.Application/Features/Comprobantes/DTOs/ComprobantePendienteCobroDto.cs
- ✨ src/ZuluIA_Back.Application/Features/Cheques/DTOs/ChequeDisponibleDto.cs

### Application Queries (4 archivos)
- ✨ src/ZuluIA_Back.Application/Features/Comprobantes/Queries/GetComprobantesClientePendientesCobroQuery.cs
- ✨ src/ZuluIA_Back.Application/Features/Comprobantes/Queries/GetComprobantesClientePendientesCobroQueryHandler.cs
- ✨ src/ZuluIA_Back.Application/Features/Cheques/Queries/GetChequesTerceroDisponiblesQuery.cs
- ✨ src/ZuluIA_Back.Application/Features/Cheques/Queries/GetChequesTerceroDisponiblesQueryHandler.cs

### API Controllers (1 archivo)
- ✏️ src/ZuluIA_Back.Api/Controllers/CobrosController.cs

---

## 🎯 Próximos Pasos Recomendados

### Sprint Inmediato (Semana 1-2)
**Prioridad:** Resolver gaps críticos restantes

1. **Paso 7:** RegistrarCobroVentanillaCommand
   - Crítico para mostrador
   - Valida caja abierta
   - Genera recibo automático

2. **Paso 12:** Ampliar flujo de imputaciones con anticipos
   - Crítico para gestión de cobros adelantados
   - Aplicación a facturas futuras

3. **Paso 16:** Implementar validaciones de negocio críticas
   - No cobrar sin caja abierta
   - Validar límite de crédito
   - Alertar facturas vencidas

### Sprint Siguiente (Semana 3)
**Prioridad:** Completar funcionalidad operativa

4. **Paso 8:** RegistrarCobroConChequesTercerosCommand
5. **Paso 6:** Ampliar GetComprobanteDetalleQueryHandler
6. **Paso 13:** Cuenta corriente enriquecida

### Sprints Posteriores
**Prioridad:** Reportes, dashboards y tests

7-11. Gestión de cobranza, reportes y dashboards
17. Tests de integración

---

## 📝 Notas Importantes

### Antes de Continuar
1. **Ejecutar migraciones SQL:** Aplicar script en `database/zuluia_back_cobros_recibos_upgrade.md`
2. **Actualizar EF Configurations:** Revisar `CobroConfiguration`, `ReciboConfiguration`, `ChequeConfiguration`
3. **Regenerar migraciones EF Core** si es necesario
4. **Tests unitarios:** Validar que entidades modificadas no rompan tests existentes

### Consideraciones de Diseño
- Los campos agregados son **nullable** para no afectar datos existentes
- Todos los DTOs incluyen **auditoría de usuario** (CreatedBy, UpdatedBy)
- Los **snapshots de datos del tercero** preservan información histórica
- Las **validaciones de negocio** deben implementarse en Commands, no en Queries

### Riesgos Identificados
1. **Migraciones en producción:** Backup obligatorio antes de aplicar cambios
2. **Performance:** Queries complejas pueden requerir índices adicionales
3. **Concurrencia:** Validaciones de caja abierta deben manejar escenarios multi-usuario
4. **Legacy data:** Datos existentes sin campos nuevos deben manejarse con defaults

---

## 🏆 Conclusión

Se ha avanzado significativamente en la paridad funcional de cobros:
- ✅ **5/17 pasos completados (29%)**
- ✅ **9/15 gaps críticos resueltos (60%)**
- ✅ **18+ archivos creados/modificados**
- ✅ **Script SQL de migración documentado**

**Próximo hito:** Completar gaps críticos restantes (Pasos 7, 12, 16) para alcanzar el 75% de funcionalidad crítica.

**Estimación para 100%:** 4-5 sprints adicionales (~6 semanas) con 1 desarrollador full-time.
