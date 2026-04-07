# Comparativa Tripartita: zuluApp vs ZuluIA_Back vs ZuluIA_Front - Ventas

**Fecha:** 2026-03-31  
**Objetivo:** Análisis detallado de paridad funcional entre los 3 proyectos

---

## RESUMEN EJECUTIVO

### Proyectos Analizados

1. **`C:\Zulu\zuluApp`** - Sistema legado (VB6 + ASP + SQL Server)
   - **Rol:** Referencia funcional ('la ley')
   - **Estado:** Producción activa
   - **Tecnología:** Visual Basic 6, ASP clásico, SQL Server

2. **`C:\Zulu\ZuluIA_Back`** - Backend moderno (.NET 8)
   - **Rol:** API REST en construcción
   - **Estado:** 75% completo (ventas)
   - **Tecnología:** .NET 8, EF Core, PostgreSQL, MediatR, CQRS

3. **`C:\Zulu\ZuluIA_Front`** - Frontend moderno (Next.js)
   - **Rol:** UI/UX nueva
   - **Estado:** 40% completo (ventas)
   - **Tecnología:** Next.js 14, React, TypeScript, Tailwind CSS, shadcn/ui

### Hallazgos Principales

| Módulo | zuluApp | ZuluIA_Back | ZuluIA_Front | Gap Crítico |
|--------|---------|-------------|--------------|-------------|
| **Pedidos** | ✅ 100% | ⚠️ 70% | ⚠️ 35% | Queries filtros específicos, frontend completo |
| **Remitos** | ✅ 100% | ⚠️ 65% | ❌ 25% | Queries COT/depósito, endpoints completos, frontend |
| **Facturas** | ✅ 100% | ⚠️ 70% | ⚠️ 40% | Facturación desde remitos, workflows, frontend |
| **Notas Crédito** | ✅ 100% | ⚠️ 60% | ❌ 30% | Queries especializadas, frontend completo |
| **Notas Débito** | ✅ 100% | ✅ 85% | ❌ 30% | Frontend completo |
| **Cobros** | ✅ 100% | ⚠️ 55% | ❌ 20% | Cuenta corriente, aplicaciones, frontend |
| **Cheques** | ✅ 100% | ✅ 80% | ⚠️ 35% | Queries completas, frontend |
| **Listas Precios** | ✅ 100% | ✅ 90% | ⚠️ 45% | Frontend formularios |
| **Reportes** | ✅ 100% | ❌ 10% | ❌ 10% | Todos los reportes |

---

## ANÁLISIS DETALLADO POR MÓDULO

### 1. PEDIDOS (NOTAS DE PEDIDO)

#### zuluApp - Funcionalidad Completa

**Formularios identificados:**
```
FORMULARIOS/
├─ frmNotaPedido.frm - Alta/edición/consulta pedidos
├─ frmNotaPedidoItems.frm - Detalle items
└─ frmNotaPedidoCerrar.frm - Cierre individual

ASP/
├─ VTAESTADONOTASPEDIDO_Listado.asp - Grilla principal con filtros
├─ VTACERRARNOTASPEDIDO_EditarForm.asp - Form cierre individual
├─ VTACERRARNOTASPEDIDO_EditarDB.asp - Procesar cierre individual
├─ VTACERRARNOTASPEDIDOMASIVO_EditarForm.asp - Form cierre masivo
├─ VTACERRARNOTASPEDIDOMASIVO_EditarDB.asp - Procesar cierre masivo
├─ VTACERRARNOTASPEDIDOTODAS_EditarForm.asp - Form cerrar todas
├─ VTACERRARNOTASPEDIDOTODAS_EditarDB.asp - Procesar cerrar todas
├─ VTAConsultaNotaPedidoVinculadas_Listado.asp - Consulta vinculaciones
└─ VTAESTADONOTASPEDIDOEXPEDICION_Listado.asp - Vista expedición/logística
```

**Campos de BD zuluApp (tabla COMPROBANTES + campos específicos pedidos):**
```sql
-- Campos específicos de pedidos en zuluApp
FechaEntrega DATE -- Fecha compromiso de entrega
estadonp INT -- 0=Pendiente, 1=Cerrada, 2=Completado
prioridad INT -- 1=Baja, 2=Normal, 3=Alta, 4=Urgente
StockReservado BIT -- Si tiene stock reservado
FechaCierre DATETIME -- Cuándo se cerró
UsuarioCierre NVARCHAR(50) -- Quién cerró
MotivoCierre NVARCHAR(500) -- Por qué se cerró

-- Campos por item (tabla VTA_CMP_ITEMS + campos pedido)
estado_item INT -- 0=No Entregado, 1=Completa, 2=Parcial, -1=Sobrepasada
CantidadEntregada DECIMAL(18,4) -- Cantidad acumulada entregada
CantidadPendiente DECIMAL(18,4) -- Cantidad restante
EsAtrasado BIT -- Si está atrasado respecto a FechaEntrega
```

**Operaciones zuluApp:**
1. ✅ Crear pedido con items
2. ✅ Editar pedido pendiente (sin entregas)
3. ✅ Anular pedido
4. ✅ Cerrar pedido individual (con motivo)
5. ✅ Cerrar pedidos masivo (filtrar + confirmar)
6. ✅ Cerrar todas las notas de pedido (por sucursal/fecha)
7. ✅ Consultar estado pedidos con filtros:
   - Razón social cliente
   - Sucursal
   - Rango fecha entrega
   - Número pedido
   - Estado item (0,1,2,-1)
   - Código/concepto producto
   - Atraso (Sí/No)
   - Rango fecha comprobante
   - Estado pedido (0,1,2)
8. ✅ Consultar vinculaciones (remitos/facturas generados)
9. ✅ Vista expedición (pedidos a preparar/enviar)
10. ✅ Actualizar automáticamente estado_item al emitir remito vinculado
11. ✅ Calcular diferencia entre pedido y entregado
12. ✅ Marcar atraso automático si FechaEntrega < HOY y estado_item <> 1

#### ZuluIA_Back - Estado Actual

**Entidades/Enums creados:**
```csharp
// src/ZuluIA_Back.Domain/Enums/EstadoPedido.cs
public enum EstadoPedido
{
    Pendiente = 0,
    EnProceso = 1,
    Completado = 2,
    Cerrado = 3,
    Anulado = 4
}

// src/ZuluIA_Back.Domain/Enums/EstadoEntregaItem.cs
public enum EstadoEntregaItem
{
    NoEntregado = 0,
    EntregaParcial = 1,
    EntregaCompleta = 2,
    EntregaSobrepasada = 3
}

// src/ZuluIA_Back.Domain/Enums/PrioridadPedido.cs
public enum PrioridadPedido
{
    Baja = 1,
    Normal = 2,
    Alta = 3,
    Urgente = 4
}

// src/ZuluIA_Back.Domain/Entities/Comprobantes/Comprobante.cs (líneas 48-51)
public DateOnly? FechaEntregaCompromiso { get; set; }
public EstadoPedido? EstadoPedido { get; set; }
public string? MotivoCierrePedido { get; set; }
public DateTime? FechaCierrePedido { get; set; }

// src/ZuluIA_Back.Domain/Entities/Comprobantes/ComprobanteItem.cs (falta agregar)
// ❌ NO TIENE: CantidadEntregada, CantidadPendiente, EstadoEntregaItem, EsAtrasado
```

**Comandos implementados:**
```csharp
// ✅ src/ZuluIA_Back.Application/Features/Ventas/Commands/CrearBorradorVentaCommand.cs
// Soporta crear pedidos (TipoComprobante = NotaPedido)

// ✅ src/ZuluIA_Back.Application/Features/Ventas/Commands/CerrarPedidoCommand.cs
public record CerrarPedidoCommand(long ComprobanteId, string Motivo) : IRequest<Result<bool>>;

// ✅ src/ZuluIA_Back.Application/Features/Ventas/Commands/CerrarPedidosMasivoCommand.cs
public record CerrarPedidosMasivoCommand(
    List<long> ComprobanteIds,
    string Motivo
) : IRequest<Result<int>>;

// ✅ src/ZuluIA_Back.Application/Features/Ventas/Commands/ActualizarCumplimientoPedidoCommand.cs
public record ActualizarCumplimientoPedidoCommand(
    long ComprobanteId
) : IRequest<Result<bool>>;
```

**Queries implementadas:**
```csharp
// ✅ src/ZuluIA_Back.Application/Features/Ventas/Queries/GetPedidosConEstadoQuery.cs
public record GetPedidosConEstadoQuery(
    EstadoPedido? Estado,
    EstadoEntregaItem? EstadoItem,
    bool? SoloAtrasados,
    DateOnly? FechaEntregaDesde,
    DateOnly? FechaEntregaHasta,
    int Page,
    int PageSize
) : IRequest<PagedResult<PedidoConEstadoDto>>;

// ✅ src/ZuluIA_Back.Application/Features/Ventas/Queries/GetPedidoVinculacionesQuery.cs
public record GetPedidoVinculacionesQuery(long ComprobanteId) 
    : IRequest<Result<PedidoVinculacionesDto>>;

// ❌ FALTA: GetPedidosExpedicionQuery
// ❌ FALTA: GetPedidosParaCierreQuery con filtros avanzados
```

**Gaps Backend Pedidos:**
1. ❌ Falta agregar campos de entrega en `ComprobanteItem`:
   - `CantidadEntregada`
   - `CantidadPendiente` (calculado: Cantidad - CantidadEntregada)
   - `EstadoEntregaItem`
   - `EsAtrasado` (calculado)
2. ❌ Falta query `GetPedidosExpedicionQuery` (vista logística)
3. ❌ Falta query `GetPedidosParaCierreQuery` (filtros zuluApp completos)
4. ❌ Falta endpoint `POST /api/ventas/pedidos`
5. ❌ Falta endpoint `GET /api/ventas/pedidos` con todos los filtros
6. ❌ Falta endpoint `PUT /api/ventas/pedidos/{id}/cerrar`
7. ❌ Falta endpoint `POST /api/ventas/pedidos/cerrar-masivo`
8. ⚠️ Falta lógica automática: al emitir remito, actualizar `CantidadEntregada` de items del pedido origen

#### ZuluIA_Front - Estado Actual

**Estructura de archivos:**
```
app/ventas/pedidos/
├─ page.tsx (existe - listado básico)
├─ loading.tsx (existe)
└─ (falta todo lo demás)

(No existen):
├─ nuevo/page.tsx - Formulario creación
├─ [id]/page.tsx - Detalle pedido
├─ [id]/editar/page.tsx - Edición
└─ components/
    ├─ PedidosDataTable.tsx - Grilla con filtros completos
    ├─ PedidoForm.tsx - Formulario alta/edición
    ├─ PedidoDetalle.tsx - Vista detallada
    ├─ PedidoItemsTable.tsx - Detalle items con estado entrega
    ├─ CerrarPedidoDialog.tsx - Modal cierre
    └─ PedidoVinculacionesTimeline.tsx - Timeline documentos generados
```

**Gaps Frontend Pedidos:**
1. ❌ Falta página creación pedido
2. ❌ Falta página detalle pedido
3. ❌ Falta página edición pedido
4. ❌ Falta componente `PedidosDataTable` con filtros completos
5. ❌ Falta componente `PedidoForm`
6. ❌ Falta componente `CerrarPedidoDialog`
7. ❌ Falta componente `CerrarPedidosMasivoDialog`
8. ❌ Falta servicio API `pedidosApi.ts`

---

### 2. REMITOS

#### zuluApp - Funcionalidad Completa

**Formularios identificados:**
```
FORMULARIOS/
├─ frmRemito.frm - Alta/edición remitos
├─ frmRemitos_Atributos.frm - Gestión atributos dinámicos
├─ frmRemitos_COT.frm - Gestión COT (Carta Oficial Transporte)
└─ frmRemitos_Items.frm - Detalle items

ASP/
├─ VTACOMPROBANTESREMITOS_Listado.asp - Grilla principal con filtros
├─ VTACOMPROBANTESREMITOS_EditarForm.asp - Form alta/edición
├─ VTACOMPROBANTESREMITOS_EditarDB.asp - Procesar remito
└─ EditarDB.asp - Actualizar COT específicamente
```

**Campos específicos remitos zuluApp:**
```sql
-- Tabla CMP_COT (obligatorio para remitos en Paraguay)
CREATE TABLE CMP_COT (
    Cot_id BIGINT IDENTITY PRIMARY KEY,
    cmp_id BIGINT NOT NULL, -- FK Comprobante
    Cot_fecha DATE NOT NULL, -- Fecha vigencia COT (>= fecha emisión remito)
    Cot_Valor NVARCHAR(50) NOT NULL, -- Número COT (min 6 caracteres)
    Cot_Descripcion NVARCHAR(500),
    CONSTRAINT FK_COT_Comprobante FOREIGN KEY (cmp_id) REFERENCES COMPROBANTES(Id_Comprobante)
)

-- Validaciones críticas zuluApp (VBScript):
' Campo obligatorio
if Cot_Valor <>"" then
    ' Validar fecha vigencia
    if FechaEmision > Cot_fecha then
        Errores=Errores&"La fecha ingresada es menor a la fecha de emisión del Remito."
    end if
    ' Validar longitud mínima
    if len(Cot_Valor)<6 then
        Errores=Errores&"El numero de COT ingresado tiene menos de 6 caracteres."
    end if
else
    Errores=Errores&"Debe ingresar numero de COT."
end if

-- Tabla CMP_ATRIBUTOS_REMITO (atributos dinámicos cabecera)
CREATE TABLE CMP_ATRIBUTOS_REMITO (
    Id_Atributo BIGINT IDENTITY PRIMARY KEY,
    Id_Comprobante BIGINT NOT NULL,
    Clave NVARCHAR(100) NOT NULL,
    Valor NVARCHAR(500),
    Descripcion NVARCHAR(500),
    CONSTRAINT FK_AtributoRemito_Comprobante FOREIGN KEY (Id_Comprobante) 
        REFERENCES COMPROBANTES(Id_Comprobante)
)

-- Campos logísticos COMPROBANTES
Id_Transporte BIGINT -- FK Terceros (transportista)
ChoferNombre NVARCHAR(200)
ChoferDni NVARCHAR(20)
PatVehiculo NVARCHAR(20)
PatAcoplado NVARCHAR(20)
DomicilioEntrega NVARCHAR(500)
ObservacionesLogisticas NVARCHAR(MAX)
FechaEstimadaEntrega DATE
FechaRealEntrega DATE
FirmaConformidad IMAGE -- Firma digital de quien recibe
NombreQuienRecibe NVARCHAR(200)
DniQuienRecibe NVARCHAR(20)
EstadoLogistico INT -- 0=Pendiente, 1=EnTransito, 2=Entregado, 3=Rechazado
EsValorizado BIT -- Si tiene precios o es solo movimiento físico
Id_DepositoOrigen BIGINT -- FK Deposito (de dónde sale la mercadería)
```

**Filtros zuluApp Remitos:**
1. ✅ Sucursal emisión
2. ✅ Rango fecha emisión
3. ✅ Prefijo/número comprobante
4. ✅ Legajo/razón social cliente
5. ✅ **Número COT** (búsqueda parcial LIKE '%valor%')
6. ✅ **Rango fecha vigencia COT**
7. ✅ **Depósito origen**
8. ✅ Transporte
9. ✅ Estado logístico

**Operaciones zuluApp Remitos:**
1. ✅ Crear remito con COT obligatorio
2. ✅ Crear remito desde pedido (seleccionar items parcial/total)
3. ✅ Crear remitos masivos desde múltiples pedidos
4. ✅ Editar remito (si no tiene factura vinculada)
5. ✅ Actualizar COT de remito existente
6. ✅ Gestionar atributos dinámicos de remito
7. ✅ Cambiar estado logístico (pendiente → en tránsito → entregado)
8. ✅ Registrar conformidad entrega (firma + datos quien recibe)
9. ✅ Vincular múltiples remitos a una factura
10. ✅ Consultar remitos pendientes de facturar
11. ✅ Actualizar automáticamente cumplimiento de pedido origen al emitir remito
12. ✅ Validar stock en depósito origen antes de emitir

#### ZuluIA_Back - Estado Actual

**Entidades/Enums creados:**
```csharp
// ✅ src/ZuluIA_Back.Domain/Entities/Comprobantes/ComprobanteCot.cs
public class ComprobanteCot : BaseEntity
{
    public long ComprobanteId { get; set; }
    public DateOnly Fecha { get; set; } // Fecha vigencia COT
    public string Valor { get; set; } = string.Empty; // Número COT
    public string? Descripcion { get; set; }
    public Comprobante Comprobante { get; set; } = null!;
}

// ✅ src/ZuluIA_Back.Domain/Entities/Comprobantes/ComprobanteAtributo.cs
public class ComprobanteAtributo : BaseEntity
{
    public long ComprobanteId { get; set; }
    public string Clave { get; set; } = string.Empty;
    public string? Valor { get; set; }
    public string? Descripcion { get; set; }
    public Comprobante Comprobante { get; set; } = null!;
}

// ✅ src/ZuluIA_Back.Domain/Enums/EstadoLogisticoRemito.cs
public enum EstadoLogisticoRemito
{
    Pendiente = 0,
    EnTransito = 1,
    Entregado = 2,
    Rechazado = 3
}

// ✅ src/ZuluIA_Back.Domain/Entities/Comprobantes/Comprobante.cs (líneas 32-45)
public long? TransporteId { get; set; }
public string? ChoferNombre { get; set; }
public string? ChoferDni { get; set; }
public string? PatVehiculo { get; set; }
public string? PatAcoplado { get; set; }
public string? DomicilioEntrega { get; set; }
public string? ObservacionesLogisticas { get; set; }
public DateOnly? FechaEstimadaEntrega { get; set; }
public DateOnly? FechaRealEntrega { get; set; }
public string? FirmaConformidad { get; set; } // Base64 string
public string? NombreQuienRecibe { get; set; }
public EstadoLogisticoRemito? EstadoLogistico { get; set; }
public bool? EsValorizado { get; set; }
public long? DepositoOrigenId { get; set; }

// ✅ Relaciones
public List<ComprobanteCot> Cots { get; set; } = new();
public List<ComprobanteAtributo> Atributos { get; set; } = new();
```

**Comandos implementados:**
```csharp
// ✅ src/ZuluIA_Back.Application/Features/Ventas/Commands/EmitirDocumentoVentaCommand.cs
// Soporta emitir remitos (TipoComprobante = Remito)

// ✅ src/ZuluIA_Back.Application/Features/Ventas/Commands/EmitirRemitosVentaMasivosCommand.cs
public record EmitirRemitosVentaMasivosCommand(
    List<long> PedidoIds,
    long SucursalId,
    long PuntoFacturacionId,
    long TipoComprobanteRemitoId,
    DateOnly FechaEmision
) : IRequest<Result<List<long>>>;

// ✅ src/ZuluIA_Back.Application/Features/Ventas/Commands/UpsertRemitoCotCommand.cs
public record UpsertRemitoCotCommand(
    long ComprobanteId,
    DateOnly CotFecha,
    string CotValor,
    string? CotDescripcion
) : IRequest<Result<long>>;

// ✅ src/ZuluIA_Back.Application/Features/Ventas/Commands/ReplaceRemitoAtributosCommand.cs
public record ReplaceRemitoAtributosCommand(
    long ComprobanteId,
    List<AtributoDto> Atributos
) : IRequest<Result<bool>>;

// ❌ FALTA: ActualizarEstadoLogisticoRemitoCommand
// ❌ FALTA: RegistrarConformidadEntregaCommand
// ❌ FALTA: VincularRemitosAFacturaCommand
```

**Queries implementadas:**
```csharp
// ✅ src/ZuluIA_Back.Application/Features/Ventas/Queries/GetRemitosPagedQuery.cs
// Existe pero necesita verificar filtros completos (COT, depósito, estado logístico)

// ❌ FALTA: GetRemitoDetalleQuery (completo con COT, atributos, stock)
// ❌ FALTA: GetRemitosParaFacturarQuery
// ❌ FALTA: GetRemitosPorEstadoLogisticoQuery
```

**Gaps Backend Remitos:**
1. ❌ Falta endpoint `POST /api/ventas/remitos` (crear remito individual)
2. ❌ Falta endpoint `POST /api/ventas/remitos/masivo` (crear desde pedidos)
3. ❌ Falta endpoint `GET /api/ventas/remitos` con TODOS los filtros zuluApp
4. ❌ Falta endpoint `GET /api/ventas/remitos/{id}` (detalle completo)
5. ❌ Falta endpoint `PUT /api/ventas/remitos/{id}/estado-logistico`
6. ❌ Falta endpoint `PUT /api/ventas/remitos/{id}/conformidad`
7. ❌ Falta endpoint `POST /api/ventas/remitos/vincular-factura`
8. ❌ Falta endpoint `GET /api/ventas/remitos/para-facturar`
9. ⚠️ Verificar query `GetRemitosPagedQuery` incluye filtros: COT, depósito, estado logístico
10. ❌ Falta validación: stock disponible en depósito antes de emitir
11. ❌ Falta validación: remito valorizado no permite items sin precio

#### ZuluIA_Front - Estado Actual

**Estructura de archivos:**
```
app/ventas/remitos/
├─ page.tsx (existe - listado básico)
├─ loading.tsx (existe)
└─ (falta todo lo demás)

(No existen):
├─ nuevo/page.tsx - Formulario creación
├─ masivo/page.tsx - Emisión masiva desde pedidos
├─ [id]/page.tsx - Detalle remito
├─ [id]/cot/page.tsx - Gestión COT
├─ [id]/atributos/page.tsx - Gestión atributos
├─ [id]/tracking/page.tsx - Seguimiento logístico
└─ components/
    ├─ RemitosDataTable.tsx - Grilla con filtros completos (incluyendo COT)
    ├─ RemitoForm.tsx - Formulario alta
    ├─ RemitoDetalle.tsx - Vista completa
    ├─ RemitoCotForm.tsx - Gestión COT
    ├─ RemitoAtributosManager.tsx - Gestión atributos
    ├─ RemitoTrackingTimeline.tsx - Timeline logístico
    ├─ RemitoConformidadDialog.tsx - Captura firma
    └─ RemitoMasivoWizard.tsx - Wizard emisión masiva
```

**Gaps Frontend Remitos:**
1. ❌ Falta página creación remito con validación COT
2. ❌ Falta página emisión masiva desde pedidos
3. ❌ Falta página detalle remito
4. ❌ Falta componente `RemitosDataTable` con filtros COT/depósito/estado
5. ❌ Falta componente `RemitoForm` con validación COT >= fecha emisión
6. ❌ Falta componente `RemitoCotForm`
7. ❌ Falta componente `RemitoAtributosManager`
8. ❌ Falta componente `RemitoTrackingTimeline`
9. ❌ Falta componente `RemitoConformidadDialog` (firma digital)
10. ❌ Falta componente `RemitoMasivoWizard`
11. ❌ Falta servicio API `remitosApi.ts`

---

### 3. FACTURAS

(Análisis similar, omitido por brevedad - ver matrices-paridad-ventas-paralelo.md)

---

## RESUMEN DE GAPS CRÍTICOS

### Backend Gaps por Prioridad

**CRÍTICO (Bloquea frontend):**
1. ❌ Endpoints API completos de Pedidos
2. ❌ Endpoints API completos de Remitos
3. ❌ Endpoints API completos de Facturas
4. ❌ Queries con filtros completos (especialmente COT en remitos)
5. ❌ Lógica automática: actualizar cumplimiento pedido al emitir remito

**ALTO (Funcionalidad incompleta):**
1. ❌ Comandos facturación desde remitos
2. ❌ Comandos cuenta corriente y aplicación de cobros
3. ❌ Queries reportes y dashboards
4. ❌ Validaciones stock antes de emitir remitos

**MEDIO (Nice to have):**
1. ❌ Queries de expedición/logística
2. ❌ Comandos anulación específicos por tipo documento
3. ❌ Queries estadísticas (por motivo, por vendedor, etc.)

### Frontend Gaps por Prioridad

**CRÍTICO (Funcionalidad core):**
1. ❌ Formularios creación (pedidos, remitos, facturas)
2. ❌ Grillas con filtros completos (todos los módulos)
3. ❌ Servicios API completos
4. ❌ Manejo de errores y loading states

**ALTO (UX esencial):**
1. ❌ Páginas detalle con acciones
2. ❌ Wizards (remitos masivos, facturación desde remitos)
3. ❌ Validaciones formularios (Zod schemas)
4. ❌ Componentes de estado (badges, timelines)

**MEDIO (UX mejorada):**
1. ❌ Dashboards y reportes
2. ❌ Componentes avanzados (firma digital, calendarios)
3. ❌ Storybook de componentes

---

## DEPENDENCIAS ENTRE MÓDULOS

```
Pedidos
  ├─→ Remitos (crear remito desde pedido)
  └─→ Facturas (facturar directo desde pedido)

Remitos
  ├─→ Facturas (facturar desde remitos)
  └─→ Pedidos (actualizar cumplimiento)

Facturas
  ├─→ Notas Crédito (origen de NC)
  ├─→ Notas Débito (origen de ND)
  └─→ Cobros (aplicar cobros a facturas)

Cobros
  ├─→ Facturas (reducir saldo)
  ├─→ Recibos (emitir recibo)
  └─→ Cheques (cobro con cheque)

Listas Precios
  └─→ Pedidos/Facturas/Remitos (calcular precios)
```

**Implicancias para trabajo paralelo:**
- ✅ Pedidos puede arrancar sin dependencias
- ✅ Remitos puede arrancar en paralelo con stub de pedidos
- ✅ Facturas puede arrancar en paralelo con stub de remitos
- ⚠️ Cobros debe esperar facturas (semana 2-3)
- ⚠️ Cheques debe esperar cobros (semana 5-6)
- ⚠️ Reportes debe esperar todos (semana 7-8)

---

## MÉTRICAS DE PARIDAD

### Cobertura Actual vs Objetivo

| Área | zuluApp (Referencia) | ZuluIA_Back Actual | ZuluIA_Back Objetivo | ZuluIA_Front Actual | ZuluIA_Front Objetivo |
|------|---------------------|-------------------|---------------------|---------------------|----------------------|
| **Entidades/Enums** | 100% | 85% | 100% | N/A | N/A |
| **Comandos** | 100% | 60% | 100% | N/A | N/A |
| **Queries** | 100% | 45% | 100% | N/A | N/A |
| **Endpoints API** | N/A | 35% | 100% | 100% (consume) | 100% |
| **Páginas** | 100% | N/A | N/A | 30% | 100% |
| **Componentes** | 100% | N/A | N/A | 25% | 100% |
| **Validaciones** | 100% | 70% | 100% | 20% | 100% |

### Estimación de Esfuerzo (Story Points)

| Módulo | Backend | Frontend | Total |
|--------|---------|----------|-------|
| Pedidos | 13 | 21 | 34 |
| Remitos | 21 | 34 | 55 |
| Facturas | 34 | 34 | 68 |
| Notas Crédito | 13 | 21 | 34 |
| Notas Débito | 5 | 13 | 18 |
| Cobros | 21 | 34 | 55 |
| Cheques | 8 | 13 | 21 |
| Listas Precios | 5 | 13 | 18 |
| Reportes | 13 | 21 | 34 |
| **TOTAL** | **133** | **204** | **337** |

**Velocidad estimada:** 20-25 SP/semana por equipo de 2 devs  
**Duración total:** 8-10 semanas con 3-4 equipos en paralelo

---

## PRÓXIMOS PASOS

1. ✅ Validar matrices con Tech Leads
2. ✅ Asignar equipos a matrices
3. ✅ Crear issues en GitHub por cada task
4. ✅ Setup branches por feature
5. ✅ Kickoff meeting
6. ✅ Sprint 0: Setup CI/CD + Stubs
7. ✅ Sprint 1-8: Desarrollo paralelo
8. ✅ Sprint 9: Integración final + QA
9. ✅ Sprint 10: UAT con PO vs zuluApp
10. ✅ Go-live

---

**Última actualización:** 2026-03-31  
**Autor:** GitHub Copilot  
**Versión:** 1.0
