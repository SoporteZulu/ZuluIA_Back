# Análisis del Módulo de Compras - Estado Actual

## Fecha de Análisis
$(Get-Date -Format "yyyy-MM-dd HH:mm")

## Resumen Ejecutivo

El backend de **Compras** tiene una implementación **parcialmente funcional** pero con **inconsistencias críticas** entre el código y la base de datos real.

---

## 1. Frontend Nuevo (ZuluIA_Front)

### Secciones Identificadas (14)
El frontend tiene las siguientes subsecciones en `/app/compras`:

1. **ajustes** - Ajustes de compras (notas de crédito/débito)
2. **cedulones** - Cedulones de proveedores
3. **cotizaciones** - Cotizaciones de compra
4. **devoluciones** - Devoluciones a proveedores
5. **facturas** - Facturas de compra
6. **imputaciones** - Imputaciones de compras
7. **notas-credito** - Notas de crédito de compras
8. **ordenes** - Órdenes de compra
9. **proveedores** - Gestión de proveedores
10. **recepciones** - Recepciones de mercadería
11. **remitos** - Remitos de compra
12. **reportes** - Reportes de compras
13. **requisiciones** - Requisiciones/Solicitudes de compra
14. **solicitudes** - Solicitudes de compra

### APIs que Consume

El frontend consume principalmente:
- `/api/ordenes-compra` (GET, POST)
- `/api/ordenes-compra/{id}` (GET)
- `/api/ordenes-compra/{id}/recibir` (POST)
- `/api/ordenes-compra/{id}/cancelar` (POST)
- `/api/comprobantes` (para facturas, remitos, notas, etc.)

**Hook principal**: `useOrdenesCompra.ts`, `useComprobantes.ts`

---

## 2. Proyecto Viejo (zuluApp)

### Formularios Identificados (24)

En `C:\Zulu\zuluApp\FORMULARIOS`:

1. `frmAjustesCompraCredito.frm`
2. `frmAjustesCompraDebito.frm`
3. `frmAnulacionComprobantesCompras.frm`
4. `frmCompraCotizacionCompra.frm`
5. `frmCompraCotizacionesMasivas.frm`
6. `frmCompraPedidosCompra.frm`
7. `frmCompraProcesarCotizacion.frm`
8. `frmCompraProcesarRequisicion.frm`
9. `frmCompraRequisicionCompra.frm`
10. `frmCompraRequisicionObra.frm`
11. `frmComprobantesCompras.frm`
12. `frmCotizacionCompra.frm`
13. `frmDesimputarComprobantesCompras.frm`
14. `frmDevolucionCompraNoValorizada.frm`
15. `frmDevolucionCompraStock.frm`
16. `frmGenerarIVACompras.frm`
17. `frmImputacionesCompras.frm`
18. `frmImputacionesComprasImportacion.frm`
19. `frmImputacionesComprasMasivas.frm`
20. `frmOrdenCompra.frm`
21. `frmReimpresionComprobantesCompras.frm`
22. `frmReimpresionIVACompras.frm`
23. `frmRemitosComprasNoValorizados.frm`
24. `frmRemitosComprasValorizados.frm`

---

## 3. Base de Datos Real (DBLocal.sql)

### Tablas Relacionadas a Compras

#### ✅ Existe en BD:
```sql
CREATE TABLE public.ordenes_compra_meta (
    id bigint NOT NULL,
    comprobante_id bigint NOT NULL,
    proveedor_id bigint NOT NULL,
    fecha_entrega_req date,
    condiciones_entrega text,
    cantidad_total numeric(18,4) DEFAULT 0.0 NOT NULL,
    cantidad_recibida numeric(18,4) DEFAULT 0.0 NOT NULL,
    fecha_ultima_recepcion date,
    estado_oc character varying(20) NOT NULL,
    habilitada boolean DEFAULT true NOT NULL,
    created_at timestamp with time zone NOT NULL
);
```

#### ❌ NO Existen en BD:
- `cotizaciones_compra`
- `requisiciones_compra`

**Nota crítica**: El módulo de compras funciona reutilizando la tabla `comprobantes` (que es compartida con ventas) + la tabla `ordenes_compra_meta` para metadatos específicos de órdenes de compra.

---

## 4. Backend Actual (ZuluIA_Back)

### Entidades del Dominio

#### ✅ Entidades Existentes:

1. **`OrdenCompraMeta`** (en `Domain\Entities\Comprobantes`)
   - Ubicación: `src\ZuluIA_Back.Domain\Entities\Comprobantes\OrdenCompraMeta.cs`
   - Estado: ✅ **Completo** - Tiene lógica de dominio (Crear, Recibir, Cancelar, etc.)
   - Configuración EF: ✅ Existe en `OrdenCompraMetaConfiguration.cs`
   - DbContext: ✅ Registrado en `IApplicationDbContext` y `AppDbContext`
   - Tabla BD: ✅ Existe (`ordenes_compra_meta`)

2. **`CotizacionCompra`** (en `Domain\Entities\Compras`)
   - Ubicación: `src\ZuluIA_Back.Domain\Entities\Compras\CotizacionCompra.cs`
   - Estado: ⚠️ **INCOMPLETO** - Existe pero sin tabla en BD
   - Configuración EF: ✅ Existe en `CotizacionCompraConfiguration.cs`
   - DbContext: ❌ **NO registrado** en `IApplicationDbContext` ni `AppDbContext`
   - Tabla BD: ❌ **NO existe**

3. **`RequisicionCompra`** (en `Domain\Entities\Compras`)
   - Ubicación: `src\ZuluIA_Back.Domain\Entities\Compras\RequisicionCompra.cs`
   - Estado: ⚠️ **INCOMPLETO** - Existe pero sin tabla en BD
   - Configuración EF: ✅ Existe en `RequisicionCompraConfiguration.cs`
   - DbContext: ❌ **NO registrado** en `IApplicationDbContext` ni `AppDbContext`
   - Tabla BD: ❌ **NO existe**

### Commands y Handlers

#### ✅ Implementados:

**Órdenes de Compra:**
- `CrearOrdenCompraCommand` / `CrearOrdenCompraCommandHandler`
- `RegistrarRecepcionOrdenCompraCommand` / Handler
- `OrdenCompraMetaStateCommands` (Cancelar, etc.)

**Cotizaciones:**
- `CotizacionCompraCommands` / `CotizacionCompraCommandHandlers`
- `CotizacionCompraStateCommandHandlers`

**Requisiciones:**
- `CrearRequisicionCompraCommand` / Handler
- `RequisicionCompraStateCommands` / Handlers

**Documentos de Compra:**
- `CrearBorradorCompraCommand` / Handler
- `EmitirDocumentoCompraCommand` / Handler
- `RegistrarDevolucionCompraCommand` / Handler

### Controller

**`ComprasController.cs`** - ✅ **Funcional parcialmente**

Endpoints implementados:
- `GET /compras/documentos/{id}` - Obtener documento por ID
- `GET /compras/ordenes` - Listar órdenes con filtros
- `GET /compras/ordenes/{id}` - Obtener orden por ID
- `POST /compras/cotizaciones` - Crear cotización
- `POST /compras/requisiciones` - Crear requisición
- `POST /compras/remitos` - Crear remito
- `POST /compras/documentos/{id}/emitir` - Emitir documento
- `POST /compras/ordenes` - Crear orden de compra
- `POST /compras/ordenes/{id}/recibir` - Recibir orden
- `POST /compras/devoluciones` - Registrar devolución

### Enums

✅ Completos:
- `EstadoOrdenCompra`: Pendiente, ParcialmenteRecibida, Recibida, Cancelada
- `EstadoCotizacionCompra`: Pendiente, Aceptada, Rechazada, Procesada
- `EstadoRequisicion`: Borrador, Enviada, Aprobada, Rechazada, Cancelada, Procesada

---

## 5. Análisis de Gaps (Funcionalidad Faltante)

### Crítico - Inconsistencias a Resolver

1. **❌ CotizacionCompra y RequisicionCompra no están en DbContext**
   - Existen entidades, handlers, validators
   - NO están registradas en `IApplicationDbContext`
   - NO están registradas en `AppDbContext`
   - NO tienen tablas en la BD real
   - **Acción**: Decidir si eliminarlas o crear migraciones

2. **❌ Mapeos de AutoMapper faltantes**
   - No existe `ComprasMappingProfile.cs`
   - Falta mapeo de DTOs a entidades

3. **⚠️ Validación de reglas de negocio incompleta**
   - Faltan validaciones vs. proyecto viejo (zuluApp)
   - Falta documentación de flujos de negocio

### Funcionalidad del Frontend que Falta en Backend

Según las 14 subsecciones del frontend:

| Subsección | Estado Backend | Prioridad |
|------------|----------------|-----------|
| **facturas** | ✅ Funcional (usa `comprobantes`) | Baja |
| **ordenes** | ✅ Funcional (`OrdenCompraMeta` + API) | Baja |
| **remitos** | ✅ Funcional (usa `comprobantes`) | Baja |
| **devoluciones** | ✅ Funcional (comando existe) | Baja |
| **ajustes** | ✅ Funcional (usa `comprobantes`) | Baja |
| **notas-credito** | ✅ Funcional (usa `comprobantes`) | Baja |
| **imputaciones** | ⚠️ Parcial (existe tabla, falta API) | **Alta** |
| **cotizaciones** | ❌ Sin tabla BD | **Media** |
| **requisiciones** | ❌ Sin tabla BD | **Media** |
| **recepciones** | ⚠️ Parcial (lógica existe, falta endpoint dedicado) | Media |
| **proveedores** | ✅ Funcional (usa `terceros`) | Baja |
| **cedulones** | ❌ Sin implementar | Baja |
| **solicitudes** | ❌ Sin implementar (¿=requisiciones?) | Media |
| **reportes** | ❌ Sin implementar | Baja |

---

## 6. Comparación vs. Módulo de Ventas

El módulo de **Ventas** está más completo:
- Usa `NotaPedido` (equivalente a cotización/pedido)
- Tiene entidades registradas en DbContext
- Tiene tablas en BD
- Flujo: Nota de Pedido → Comprobante (factura/remito)

**Patrón a seguir para Compras:**
- Requisición → Cotización → Orden de Compra → Comprobante (factura/remito)

---

## 7. Recomendaciones

### Decisión Crítica

**Opción A: Eliminar CotizacionCompra y RequisicionCompra**
- Simplificar usando solo `Comprobante` + `OrdenCompraMeta`
- Alinear con la BD real existente
- Menos complejidad

**Opción B: Completar CotizacionCompra y RequisicionCompra**
- Crear migraciones de BD
- Registrar en DbContext
- Implementar flujo completo
- Mayor paridad con proyecto viejo

### Próximos Pasos Sugeridos

1. **Validar con usuario**: ¿Necesita cotizaciones y requisiciones como entidades separadas o puede usar comprobantes?

2. **Si Opción A** (simplificar):
   - Eliminar `CotizacionCompra.cs`, `RequisicionCompra.cs` y sus handlers
   - Usar tipos de comprobante especiales para cotizaciones/requisiciones
   - Completar APIs faltantes (imputaciones, recepciones)

3. **Si Opción B** (completar):
   - Crear migraciones de BD para `cotizaciones_compra` y `requisiciones_compra`
   - Registrar en `IApplicationDbContext` y `AppDbContext`
   - Completar mapping profiles
   - Implementar queries faltantes

4. **Independiente de la opción**:
   - Crear `ComprasMappingProfile.cs`
   - Completar validadores
   - Implementar endpoints faltantes (imputaciones, cedulones)
   - Agregar tests unitarios
   - Documentar flujos de negocio vs. zuluApp

---

## 8. Archivos Clave para Revisar

### Dominio
- `src\ZuluIA_Back.Domain\Entities\Compras\*.cs`
- `src\ZuluIA_Back.Domain\Entities\Comprobantes\OrdenCompraMeta.cs`
- `src\ZuluIA_Back.Domain\Enums\EstadoOrdenCompra.cs`

### Application
- `src\ZuluIA_Back.Application\Features\Compras\Commands\*.cs`
- `src\ZuluIA_Back.Application\Features\Compras\Queries\*.cs`
- `src\ZuluIA_Back.Application\Features\Compras\DTOs\*.cs`

### Infrastructure
- `src\ZuluIA_Back.Infrastructure\Persistence\Configurations\*Compra*.cs`
- `src\ZuluIA_Back.Infrastructure\Persistence\AppDbContext.cs`

### API
- `src\ZuluIA_Back.Api\Controllers\ComprasController.cs`

### Frontend
- `C:\Zulu\ZuluIA_Front\app\compras\**\*.tsx`
- `C:\Zulu\ZuluIA_Front\lib\hooks\useOrdenesCompra.ts`

### Proyecto Viejo (Referencia)
- `C:\Zulu\zuluApp\FORMULARIOS\frm*Compra*.frm`

---

## Conclusión

El módulo de Compras tiene una **base funcional sólida** pero requiere:

1. **Decisión arquitectónica** sobre cotizaciones/requisiciones
2. **Limpieza de código** (eliminar o completar entidades sin tablas)
3. **Completar funcionalidad faltante** (imputaciones, cedulones, reportes)
4. **Validación funcional** vs. zuluApp para asegurar paridad 100%

**Próximo paso recomendado**: Consultarle al usuario qué enfoque prefiere (Opción A o B) antes de continuar con la implementación.
