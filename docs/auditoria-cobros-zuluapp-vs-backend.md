# Auditoría Completa - Vista Cobros zuluApp vs ZuluIA_Back

**Fecha:** 2025-01-20  
**Objetivo:** Identificar todos los gaps funcionales en la vista de ventas/cobros comparando zuluApp legacy con ZuluIA_Back actual

## 1. FORMULARIOS DE COBROS EN zuluApp (Legacy)

### 1.1 Formularios Detectados
- `frmCobro` - Cobro general/administrativo
- `frmCobroVentanillaCP` - Cobro ventanilla Contra Pedido
- `frmCobroVentanillaCT` - Cobro ventanilla Contra Entrega (mostrador)
- `frmAuditoriaCheques` - Auditoría de cheques recibidos
- `frmChequesTerceroVenta` - Gestión cheques de terceros para ventas
- `frmChequesTerceroDeposito` - Depósito de cheques de terceros
- `frmChequesTerceroRechazo` - Registro de rechazo de cheques

### 1.2 Campos Detectados en frmCobro (Cobro General)

#### Cabecera del Cobro
- **Identificación:**
  - Número de recibo (serie + número)
  - Fecha del cobro
  - Sucursal
  - Usuario cajero/cobrador
  - Tipo de cobro: ventanilla, ruta, administrativo

- **Cliente:**
  - Legajo cliente
  - Razón social
  - CUIT/CUIL
  - Condición IVA
  - Domicilio completo (snapshot)
  - Zona comercial
  - Vendedor asignado
  - Cobrador asignado
  - Límite de crédito
  - Saldo actual cuenta corriente
  - Deuda vencida
  - Días de mora promedio

- **Datos del Cobro:**
  - Moneda
  - Cotización (si es moneda extranjera)
  - Total cobrado
  - Observaciones
  - Observaciones internas (no visibles en recibo)

#### Medios de Pago (Múltiples)
Para cada medio:
- Forma de pago: Efectivo, Cheque Propio, Cheque de Tercero, Transferencia, Tarjeta Débito, Tarjeta Crédito, Vale, Anticipo, Nota de Crédito
- Caja destino
- Importe
- Moneda del medio (puede diferir del cobro)
- Cotización del medio

**Datos adicionales por tipo de medio:**

**Si es Cheque Propio:**
- Banco
- Sucursal bancaria
- Número de cheque
- Fecha de emisión
- Fecha de pago/vencimiento
- Plaza
- Estado inicial: "Al Día" o "Diferido"
- CUIT/CUIL titular
- Nombre titular

**Si es Cheque de Tercero:**
- Seleccionar de cheques disponibles previamente ingresados
- Ver datos del cheque: banco, número, titular, fecha pago, importe
- Cambiar estado a "Entregado en Pago"

**Si es Transferencia:**
- Banco origen
- Banco destino
- Número de operación
- Fecha de acreditación

**Si es Tarjeta:**
- Tipo: débito/crédito
- Terminal/POS
- Número de operación/cupón
- Número de lote (batch)
- Plan de cuotas (si aplica)
- Código de autorización
- Fecha de acreditación estimada

**Si es Anticipo:**
- Seleccionar anticipo disponible del cliente
- Ver saldo disponible del anticipo

**Si es Nota de Crédito:**
- Seleccionar NC disponible del cliente
- Ver saldo disponible de la NC

#### Imputación de Comprobantes
Grilla de facturas pendientes:
- Seleccionar facturas a imputar
- Ver por factura:
  - Tipo y número de comprobante
  - Fecha de emisión
  - Fecha de vencimiento
  - Días de mora (si vencida)
  - Importe total original
  - Saldo pendiente
  - Moneda
  - Importe a imputar (editable)
  - Observaciones de la imputación

**Funciones de imputación:**
- Imputar automáticamente: distribuye el cobro en facturas por antigüedad
- Imputar manual: usuario selecciona importes por factura
- Ver historial de cobros previos de cada factura
- Permitir cobro parcial de facturas
- Validar que suma de imputaciones = total cobrado

#### Totales y Resumen
- Total medios de pago
- Total imputado a facturas
- Diferencia (debe ser 0 para confirmar)
- Total efectivo
- Total cheques (propios + terceros)
- Total electrónico (transferencias + tarjetas)
- Anticipo/Saldo a favor generado (si total medios > total imputado)

#### Acciones del Formulario
- **Nuevo:** Iniciar nuevo cobro
- **Buscar:** Buscar cobros anteriores
- **Modificar:** Editar cobro (solo si no está cerrado en caja)
- **Anular:** Anular cobro registrado
- **Imprimir Recibo:** Generar recibo oficial
- **Imprimir Comprobante Interno:** Ticket/comprobante no fiscal
- **Enviar por Email:** Enviar recibo al cliente
- **Ver Cuenta Corriente:** Ver movimientos del cliente
- **Aceptar:** Confirmar y guardar cobro
- **Cancelar:** Descartar cambios

### 1.3 Campos Específicos de frmCobroVentanillaCP y frmCobroVentanillaCT

Estos formularios simplifican el cobro en ventanilla/mostrador:

**Diferencias con frmCobro general:**
- Interfaz simplificada para cobro rápido
- Foco en efectivo y tarjetas (menos medios complejos)
- Imputación automática de facturas del día
- Generación automática de recibo al confirmar
- Validación obligatoria de caja abierta
- Registro de turno/ventanilla del cajero

**Campos adicionales:**
- Ventanilla/Mostrador (número o código)
- Turno del cajero (mañana/tarde/noche)
- Comprobante que origina el cobro (factura del mostrador)
- Vuelto a entregar (si paga con efectivo)
- Ticket/Comprobante fiscal emitido (si usa controlador fiscal)

**Flujo típico:**
1. Cliente se presenta con factura o compra en mostrador
2. Cajero ingresa legajo o busca cliente
3. Sistema muestra facturas pendientes o genera factura del día
4. Cajero registra medio de pago (efectivo o tarjeta)
5. Sistema imputa automáticamente
6. Genera e imprime recibo
7. Registra en caja y actualiza cuenta corriente

### 1.4 Vista de Auditoría de Cheques

**Grilla principal muestra:**
- Todos los cheques recibidos (propios y de terceros)
- Filtros:
  - Por fecha recepción
  - Por banco
  - Por cliente emisor
  - Por estado: Al Día, Diferido, Depositado, Rechazado, Endosado, En Cartera
  - Por plaza
  - Por moneda
  - Por rango de importes
  - Por cobrador que lo recibió

**Acciones disponibles:**
- Depositar cheque(s)
- Endosar a proveedor (cobro con cheque de tercero)
- Registrar rechazo
- Registrar renovación
- Ver historial completo del cheque
- Imprimir listado de cheques
- Exportar a Excel

**Campos de cada cheque:**
- Número de cheque
- Banco, sucursal, plaza
- CUIT/CUIL y nombre del titular
- Fecha de emisión
- Fecha de pago/vencimiento
- Días al cobro (si es diferido)
- Importe y moneda
- Cliente que lo entregó
- Cobro en el que fue recibido
- Estado actual
- Movimientos: recepción, depósito, rechazo, endoso
- Observaciones

---

## 2. ESTADO ACTUAL DEL BACKEND (ZuluIA_Back)

### 2.1 Endpoints Existentes

**CobrosController:**
- ✅ `GET /api/cobros` - Listado paginado con filtros básicos
- ✅ `GET /api/cobros/{id}` - Detalle de cobro
- ✅ `POST /api/cobros` - Registrar cobro
- ✅ `POST /api/cobros/{id}/anular` - Anular cobro

**RecibosController:**
- ✅ `GET /api/recibos` - Listado de recibos
- ✅ `GET /api/recibos/{id}` - Detalle de recibo
- ✅ `POST /api/recibos` - Emitir recibo
- ✅ `POST /api/recibos/{id}/anular` - Anular recibo

**ChequesController:**
- ✅ `GET /api/cheques` - Listado de cheques
- ✅ `GET /api/cheques/{id}` - Detalle de cheque
- ✅ `POST /api/cheques` - Crear cheque
- ✅ `POST /api/cheques/{id}/cambiar-estado` - Cambiar estado de cheque

**CajasController:**
- ✅ `POST /api/cajas/abrir` - Abrir caja
- ✅ `POST /api/cajas/cerrar` - Cerrar caja
- Parcial: Consultas de movimientos de caja

### 2.2 DTOs Actuales

**CobroDto (actual):**
```csharp
public class CobroDto
{
    public long Id { get; set; }
    public long SucursalId { get; set; }
    public long TerceroId { get; set; }
    public string TerceroRazonSocial { get; set; }
    public DateOnly Fecha { get; set; }
    public long MonedaId { get; set; }
    public string MonedaSimbolo { get; set; }
    public decimal Cotizacion { get; set; }
    public decimal Total { get; set; }
    public string? Observacion { get; set; }
    public string Estado { get; set; }
    public int? NroCierre { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public IReadOnlyList<CobroMedioDto> Medios { get; set; }
}
```

**ReciboDto (actual):**
```csharp
public class ReciboDto
{
    public long Id { get; set; }
    public long SucursalId { get; set; }
    public long TerceroId { get; set; }
    public string TerceroRazonSocial { get; set; }
    public DateOnly Fecha { get; set; }
    public string Serie { get; set; }
    public int Numero { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; }
    public long? CobroId { get; set; }
    public string? Observacion { get; set; }
    public IReadOnlyList<ReciboItemDto> Items { get; set; }
}
```

---

## 3. MATRIZ DE GAPS FUNCIONALES

### 3.1 Gaps CRÍTICOS (Bloquean Reemplazo Funcional) 🔴

| # | Gap | Legacy zuluApp | Backend Actual | Prioridad |
|---|-----|----------------|----------------|-----------|
| 1 | **Usuario cajero/cobrador** | Registra quién cobra y en qué ventanilla/turno | No se expone en DTO | CRÍTICA |
| 2 | **Datos fiscales del cliente en recibo** | CUIT, Condición IVA, Domicilio en snapshot | Solo razón social | CRÍTICA |
| 3 | **Vendedor y cobrador asociados** | Se muestra en recibo y estadísticas | No en DTO de cobro | CRÍTICA |
| 4 | **Zona comercial** | Para reportes y segmentación | No disponible | CRÍTICA |
| 5 | **Detalles completos de cheques** | Banco, plaza, titular, fechas, CUIT | Parcial en ChequeDto | CRÍTICA |
| 6 | **Estado avanzado de cheques** | Al Día, Diferido, Depositado, Rechazado, Endosado, En Cartera | Solo estados básicos | CRÍTICA |
| 7 | **Gestión de cheques de terceros para endoso** | Permite usar cheque recibido para pagar a proveedor | No implementado | CRÍTICA |
| 8 | **Tarjetas: plan de cuotas y datos POS** | Terminal, cupón, lote, plan de cuotas | No en MedioPago | CRÍTICA |
| 9 | **Transferencias con datos bancarios** | Banco origen/destino, nro operación, fecha acreditación | No en MedioPago | CRÍTICA |
| 10 | **Imputación automática de facturas** | Distribuye cobro por antigüedad | No existe | CRÍTICA |
| 11 | **Listado de facturas pendientes del cliente** | Para seleccionar qué imputar | No existe query dedicada | CRÍTICA |
| 12 | **Anticipo/Saldo a favor** | Si cobra más de lo debido, genera anticipo | No gestionado | CRÍTICA |
| 13 | **Cobro ventanilla/mostrador simplificado** | Flujo rápido con validaciones automáticas | No diferenciado | ALTA |
| 14 | **Validación de caja abierta** | No permite cobrar sin caja abierta | No validado en command | CRÍTICA |
| 15 | **Cierre de caja con arqueo** | Arqueo físico vs registrado por medio de pago | Parcial | ALTA |

### 3.2 Gaps ALTOS (Reducen Valor Operativo) 🟡

| # | Gap | Legacy zuluApp | Backend Actual | Prioridad |
|---|-----|----------------|----------------|-----------|
| 16 | **Observaciones internas** | Separadas de observaciones en recibo | Solo una observación | ALTA |
| 17 | **Historial de cobros de una factura** | Ver cobros parciales previos | No disponible fácilmente | ALTA |
| 18 | **Totales por forma de pago** | Efectivo total, cheques total, electrónico total | Hay que calcular manualmente | ALTA |
| 19 | **Cuenta corriente en tiempo real** | Ver saldo actual del cliente al cobrar | No se consulta automáticamente | ALTA |
| 20 | **Alertas de mora** | Indica facturas vencidas y días de mora | No expuesto | ALTA |
| 21 | **Límite de crédito** | Validación al vender o cobrar | No validado en flujo | ALTA |
| 22 | **Reportes de cobranza** | Efectividad, gestión, cheques en cartera | No implementado | ALTA |
| 23 | **Exportación de libro de cobros** | PDF/Excel para contabilidad | No implementado | ALTA |
| 24 | **Auditoría completa de cheques** | Vista consolidada con filtros avanzados | Listado básico | ALTA |
| 25 | **Gestión de cobranza** | Registro de llamadas, visitas, promesas | No existe | ALTA |

### 3.3 Gaps MEDIOS (Mejoran UX y Trazabilidad) 🟢

| # | Gap | Legacy zuluApp | Backend Actual | Prioridad |
|---|-----|----------------|----------------|-----------|
| 26 | **Búsqueda avanzada de cobros** | Por múltiples criterios combinados | Filtros básicos | MEDIA |
| 27 | **Impresión personalizada de recibos** | Formato configurable, copias, leyendas | No implementado | MEDIA |
| 28 | **Envío de recibo por email** | Adjunta PDF al cliente | No implementado | MEDIA |
| 29 | **Vuelto en ventas mostrador** | Calcula y muestra vuelto | No en flujo | MEDIA |
| 30 | **Dashboard de cobros del día** | Indicadores en tiempo real | No existe | MEDIA |
| 31 | **Notificaciones de cheques próximos a vencer** | Alerta diferidos | No implementado | MEDIA |
| 32 | **Trazabilidad de endoso de cheques** | Historial completo del cheque | No completo | MEDIA |

---

## 4. PRIORIZACIÓN Y ROADMAP

### 4.1 Sprint 1 - Cobros Básicos Completos (Semana 1-2)
**Objetivo:** Permitir registrar cobros con todos los datos necesarios

- Ampliar `CobroDto` y `ReciboDto` con campos comerciales y fiscales
- Implementar `CobroDetalleDto` enriquecido
- Agregar campos de usuario cajero/cobrador
- Incluir vendedor, cobrador, zona comercial
- Agregar datos fiscales del cliente en recibo
- Ampliar `MedioPagoDto` con detalles de cheques, tarjetas, transferencias
- Crear script SQL para migración de esquema

### 4.2 Sprint 2 - Flujo de Imputación (Semana 3)
**Objetivo:** Gestionar correctamente la aplicación de cobros a facturas

- Implementar `GetComprobantesClientePendientesCobroQuery`
- Incluir cálculo de días de mora
- Implementar `ImputarCobroAutomaticoCommand`
- Validar suma de imputaciones = total cobrado
- Permitir cobros parciales
- Generar anticipos/saldo a favor

### 4.3 Sprint 3 - Gestión de Cheques (Semana 4)
**Objetivo:** Completar ciclo de vida de cheques

- Ampliar `ChequeDto` con todos los campos
- Implementar estados avanzados de cheques
- Crear `DepositarChequeCommand`
- Crear `EndosarChequeCommand`
- Crear `RegistrarRechazoChequeCommand`
- Implementar `GetChequesTerceroDisponiblesQuery`
- Crear auditoría completa de cheques

### 4.4 Sprint 4 - Cobro Ventanilla/Mostrador (Semana 5)
**Objetivo:** Flujo simplificado para cobros en mostrador

- Crear `RegistrarCobroVentanillaCommand`
- Validar caja abierta obligatoriamente
- Imputación automática de facturas
- Generación automática de recibo
- Cálculo de vuelto
- Integración con cierre de caja

### 4.5 Sprint 5 - Reportes y Dashboards (Semana 6)
**Objetivo:** Visibilidad operativa y gerencial

- Implementar `GetResumenCobranzaDelDiaQuery`
- Implementar `GetReporteGestionCobranzaQuery`
- Crear dashboard de ventas/cobros
- Implementar exportación de reportes
- Alertas de mora y gestión

### 4.6 Sprint 6 - Gestión de Cobranza (Semana 7)
**Objetivo:** Herramientas para cobradores

- Implementar `RegistrarGestionCobranzaCommand`
- Programar recordatorios de seguimiento
- Registrar promesas de pago
- Tracking de efectividad por cobrador
- Reportes de gestión

---

## 5. DEFINICIÓN DE "PARIDAD FUNCIONAL 100%"

Se considera alcanzada la paridad funcional total cuando:

✅ **Datos:** Todos los campos visibles en zuluApp están disponibles en DTOs del backend  
✅ **Operaciones:** Todas las acciones del legacy tienen comando/query equivalente  
✅ **Validaciones:** Reglas de negocio críticas están implementadas  
✅ **Flujos:** Cobro ventanilla, administrativo y gestión funcionan completos  
✅ **Reportes:** Reportes operativos y gerenciales están disponibles  
✅ **Auditoría:** Trazabilidad completa de movimientos de efectivo y cheques  
✅ **UX:** Frontend puede construirse sin overlays locales ni campos inventados  

---

## 6. CRITERIOS DE ACEPTACIÓN POR SPRINT

### Sprint 1 - Cobros Básicos
- [ ] DTO `CobroDto` contiene todos los campos del frmCobro
- [ ] DTO `ReciboDto` puede generar recibo idéntico al legacy
- [ ] `MedioPagoDto` soporta todos los tipos de medios con sus datos específicos
- [ ] Tests unitarios cubren validaciones de campos nuevos
- [ ] Migración SQL ejecutada sin errores en entorno de desarrollo

### Sprint 2 - Imputación
- [ ] Query retorna facturas pendientes con mora calculada
- [ ] Command valida suma de imputaciones = total cobrado
- [ ] Se generan anticipos correctamente cuando hay exceso de cobro
- [ ] Se actualizan saldos de comprobantes imputados
- [ ] Tests de integración cobran facturas completa y parcialmente

### Sprint 3 - Cheques
- [ ] Cheques tienen todos los estados del legacy
- [ ] Comando depositar actualiza estado y genera movimiento tesorería
- [ ] Comando endosar permite usar cheque recibido para pagar
- [ ] Query de auditoría replica filtros y agrupaciones del legacy
- [ ] Tests cubren ciclo completo: recepción -> depósito/endoso

### Sprint 4 - Ventanilla
- [ ] Command valida caja abierta antes de registrar cobro
- [ ] Genera recibo automáticamente
- [ ] Imputa facturas del día automáticamente
- [ ] Calcula vuelto correctamente
- [ ] Tests simulan flujo completo de mostrador

### Sprint 5 - Reportes
- [ ] Query de resumen del día totaliza correctamente por forma de pago
- [ ] Dashboard consume datos en tiempo real
- [ ] Exportaciones generan archivos idénticos al legacy
- [ ] Tests validan cálculos de totales y comparativos

### Sprint 6 - Gestión Cobranza
- [ ] Command registra gestión con todos los campos
- [ ] Recordatorios se programan correctamente
- [ ] Reportes muestran efectividad por cobrador/zona
- [ ] Tests cubren flujo: gestión -> promesa -> cobro efectivo

---

## 7. CONCLUSIÓN

El backend actual cubre aproximadamente el **30-40%** de la funcionalidad de cobros del sistema legacy.

**Gaps más críticos a resolver:**
1. Datos completos en DTOs (comerciales, fiscales, usuario)
2. Gestión avanzada de cheques (endoso, depósito, auditoría)
3. Imputación automática e inteligente de facturas
4. Validaciones de caja abierta y límites de crédito
5. Reportes operativos y gerenciales
6. Gestión de cobranza con seguimiento

**Complejidad estimada:** 6 sprints (7 semanas) con 1 desarrollador full-time.

**Riesgo principal:** Sin estos gaps cerrados, el frontend tendrá que implementar lógica de negocio compleja localmente, generando inconsistencias con el backend y pérdida de trazabilidad.
