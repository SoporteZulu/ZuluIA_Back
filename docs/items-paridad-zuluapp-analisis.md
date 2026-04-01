# Análisis de Paridad Funcional: Items/Productos ZuluApp vs ZuluIA_Back

## Fecha de Análisis
2025-01-XX

## Objetivo
Lograr la paridad funcional total entre la gestión de items/productos de `C:\Zulu\zuluApp` (VB6 legacy) y `C:\Zulu\ZuluIA_Back` (.NET 8 moderno).

---

## 1. CAMPOS ACTUALES EN ZULUIA_BACK

### Entidad Item (Domain)
```csharp
public class Item : AuditableEntity
{
    public string Codigo { get; private set; }              // ✅
    public string? CodigoBarras { get; private set; }       // ✅
    public string Descripcion { get; private set; }         // ✅
    public string? DescripcionAdicional { get; private set; } // ✅
    public long? CategoriaId { get; private set; }          // ✅
    public long? MarcaId { get; private set; }              // ✅
    public long UnidadMedidaId { get; private set; }        // ✅
    public long AlicuotaIvaId { get; private set; }         // ✅
    public long MonedaId { get; private set; }              // ✅
    public bool EsProducto { get; private set; }            // ✅
    public bool EsServicio { get; private set; }            // ✅
    public bool EsFinanciero { get; private set; }          // ✅
    public bool ManejaStock { get; private set; }           // ✅ (StockSINO en zuluApp)
    public decimal PrecioCosto { get; private set; }        // ✅ (Costo en zuluApp)
    public decimal PrecioVenta { get; private set; }        // ✅
    public decimal StockMinimo { get; private set; }        // ✅
    public decimal? StockMaximo { get; private set; }       // ✅
    public string? CodigoAfip { get; private set; }         // ✅
    public long? SucursalId { get; private set; }           // ✅
    public bool Activo { get; private set; }                // ✅
    
    // Herencia de AuditableEntity:
    public DateTimeOffset CreatedAt { get; }                // ✅
    public DateTimeOffset UpdatedAt { get; }                // ✅
    public DateTimeOffset? DeletedAt { get; }               // ✅
    public long? CreatedBy { get; }                         // ✅
    public long? UpdatedBy { get; }                         // ✅
}
```

---

## 2. CAMPOS EN ZULUAPP (LEGACY) - TABLA ITEMS

### Campos identificados en clsItems.cls y frmItems.frm

#### Campos Básicos
| Campo ZuluApp          | Campo ZuluIA_Back      | Estado | Observaciones                          |
|------------------------|------------------------|--------|----------------------------------------|
| `Codigo`               | `Codigo`               | ✅     | Mapeo directo                          |
| `Codigo2`              | **FALTA**              | ❌     | Código alternativo / código interno    |
| `Descripcion`          | `Descripcion`          | ✅     | Mapeo directo                          |
| `Fecha_alta`           | `CreatedAt`            | ✅     | Funcionalidad cubierta por auditoría   |
| `Unidad`               | `UnidadMedidaId`       | ✅     | Migración de texto a FK                |
| `Observacion`          | `DescripcionAdicional` | ⚠️     | Semantica diferente, evaluar          |

#### Características Físicas y Logísticas
| Campo ZuluApp          | Campo ZuluIA_Back      | Estado | Observaciones                          |
|------------------------|------------------------|--------|----------------------------------------|
| `Peso`                 | **FALTA**              | ❌     | Peso del producto (decimal)            |
| `Volumen`              | **FALTA**              | ❌     | Volumen del producto (decimal)         |
| `PuntoReposicion`      | **FALTA**              | ❌     | Punto de reposición de stock           |
| `StockSeguridad`       | **FALTA**              | ❌     | Stock de seguridad                     |
| `StockMinimo`          | `StockMinimo`          | ✅     | Mapeo directo                          |
| `StockMaximo`          | `StockMaximo`          | ✅     | Mapeo directo                          |

#### Precios e Impuestos
| Campo ZuluApp          | Campo ZuluIA_Back      | Estado | Observaciones                          |
|------------------------|------------------------|--------|----------------------------------------|
| `Costo`                | `PrecioCosto`          | ✅     | Mapeo directo                          |
| `PorcentajeIVA`        | `AlicuotaIvaId`        | ✅     | Migración de % a FK                    |
| `PorcentajeIVACompra`  | **FALTA**              | ❌     | IVA específico para compras            |
| `Impuesto`             | **FALTA**              | ❌     | Impuesto interno adicional             |

#### Flags y Características
| Campo ZuluApp          | Campo ZuluIA_Back      | Estado | Observaciones                          |
|------------------------|------------------------|--------|----------------------------------------|
| `Sistema`              | **FALTA**              | ❌     | Item del sistema (no modificable)      |
| `Financiero`           | `EsFinanciero`         | ✅     | Mapeo directo                          |
| `StockSINO`            | `ManejaStock`          | ✅     | Mapeo directo                          |
| `Compra`               | **FALTA**              | ❌     | Aplica a compras (bool)                |
| `Venta`                | **FALTA**              | ❌     | Aplica a ventas (bool)                 |
| `Foto`                 | **FALTA**              | ❌     | Ruta de imagen del producto            |
| `RecibeMovimiento`     | **FALTA**              | ❌     | ¿Permite movimientos de stock?         |
| `Esrubro`              | **FALTA**              | ❌     | Es categoría/rubro (no producto)       |
| `esRPT`                | **FALTA**              | ❌     | Régimen de Percepción y Retención      |

#### Integración y Contabilidad
| Campo ZuluApp          | Campo ZuluIA_Back      | Estado | Observaciones                          |
|------------------------|------------------------|--------|----------------------------------------|
| `Id_integradora`       | **FALTA**              | ❌     | ID en sistema integrador externo       |
| `Id_plancuentas`       | **FALTA**              | ❌     | Cuenta contable para ventas            |
| `Id_plancuentasCompra` | **FALTA**              | ❌     | Cuenta contable para compras           |

---

## 3. FUNCIONALIDADES ADICIONALES EN ZULUAPP

### 3.1 Pestañas del Formulario frmItems.frm

#### Pestaña "Ventas/Ingresos"
- `chkAplicaVentas`: Aplica a ventas (bool)
- `txtGanancia`: Porcentaje de ganancia sobre costo
- `txtPorcentajeMaxDescuento`: % máximo de descuento permitido
- `cmbIVAPAIS_Venta`: IVA país para ventas
- `chkRPT`: Régimen de Percepción y Retención
- **Estado**: ❌ No implementado

#### Pestaña "Compras/Egresos"
- `chkAplicaCompras`: Aplica a compras (bool)
- `txtCosto`: Costo del producto
- `cmbImpuesto`: Impuesto interno
- `cmbIvaPais_Compra`: IVA país para compras
- **Estado**: ⚠️ Parcialmente implementado (solo costo)

#### Pestaña "Stock"
- `chkAplicaStock`: Maneja stock (bool)
- `chkTrazable`: Producto trazable (lotes/series)
- `txtStockMinimo`: Stock mínimo
- `txtStockMaximo`: Stock máximo
- `txtPuntoReposicion`: Punto de reposición
- `txtStockSeguridad`: Stock de seguridad
- `txtLapsoVencimientoLimite`: Días límite de vencimiento
- `cmbDepositos`: Depósito por defecto
- **Estado**: ⚠️ Parcialmente implementado

#### Pestaña "Unidades" (Características Físicas)
- `txtPeso`: Peso
- `txtVolumen`: Volumen
- `txtMedida`: Medida (ancho/alto/profundidad)
- `cmbTipoMedida`: Tipo de medida
- `cmbUnidadMedida`: Unidad de medida
- `chkEstructuraValor`: Usa estructura de valor
- `chkFraccionario`: Permite fraccionamiento
- **Estado**: ❌ No implementado

#### Pestaña "Listas de Precios"
- Gestión de precios por lista de precios
- Precios de compra y venta
- **Estado**: ⚠️ Existe módulo separado (verificar integración)

#### Pestaña "Unidades" (Conversión)
- Unidad de compra
- Unidad de venta
- Multiplicador compra-venta
- **Estado**: ❌ No implementado

#### Pestaña "Componentes" (Pack)
- `chkEsPack`: Es un pack/combo
- `chkControlStockPack`: Controla stock del pack
- Grilla de componentes del pack
- **Estado**: ❌ No implementado

#### Pestaña "Atributos"
- Asignación de atributos comerciales al item
- **Estado**: ⚠️ Existe modelo de atributos, verificar integración

#### Pestaña "Impuestos"
- `chkAplicaImpuestos`: Aplica impuestos
- Configuración de impuestos específicos
- **Estado**: ⚠️ Parcialmente implementado (solo IVA)

---

## 4. ENTIDADES RELACIONADAS EN ZULUIA_BACK

### Existentes y Relacionadas
- ✅ `CategoriaItem` - Categorías de items
- ✅ `MarcaComercial` - Marcas
- ✅ `UnidadMedida` - Unidades de medida
- ✅ `AlicuotaIva` - Alícuotas de IVA
- ✅ `Moneda` - Monedas
- ✅ `AtributoComercial` - Atributos comerciales
- ✅ `ComprobanteItemAtributoComercial` - Relación item-atributo
- ⚠️ `ListaPrecio` - Listas de precios (verificar integración con items)

### Faltantes o Por Verificar
- ❌ `ItemUnidadConversion` - Conversiones de unidades
- ❌ `ItemComponente` - Componentes de pack
- ❌ `ItemAtributo` - Atributos asignados al item (verificar)
- ❌ `ItemCuentaContable` - Cuentas contables por item
- ❌ `ItemImpuesto` - Impuestos adicionales por item
- ❌ `ItemImagen` - Imágenes del producto
- ❌ `ItemStock` - Stock por depósito

---

## 5. CAMPOS PRIORITARIOS A AGREGAR

### Prioridad ALTA (Esenciales para operación de ventas)
1. **`AplicaVentas`** (bool) - Indica si el item se usa en ventas
2. **`AplicaCompras`** (bool) - Indica si el item se usa en compras
3. **`PorcentajeGanancia`** (decimal?) - % de ganancia sobre costo
4. **`PorcentajeMaximoDescuento`** (decimal?) - % máximo de descuento permitido
5. **`EsRpt`** (bool) - Sujeto a Régimen de Percepción/Retención
6. **`EsSistema`** (bool) - Item del sistema (no modificable por usuario)

### Prioridad MEDIA (Gestión de stock y logística)
7. **`PuntoReposicion`** (decimal?) - Punto de reposición de stock
8. **`StockSeguridad`** (decimal?) - Stock de seguridad
9. **`Peso`** (decimal?) - Peso del producto
10. **`Volumen`** (decimal?) - Volumen del producto
11. **`EsTrazable`** (bool) - Requiere trazabilidad (lotes/series)
12. **`PermiteFraccionamiento`** (bool) - Permite venta fraccionada
13. **`DiasVencimientoLimite`** (int?) - Días límite para vencimiento

### Prioridad MEDIA-BAJA (Contabilidad e integración)
14. **`CuentaContableVentaId`** (long?) - Cuenta contable para ventas
15. **`CuentaContableCompraId`** (long?) - Cuenta contable para compras
16. **`CodigoIntegradora`** (string?) - Código en sistema externo
17. **`AlicuotaIvaCompraId`** (long?) - IVA específico para compras
18. **`ImpuestoInternoId`** (long?) - Impuesto interno adicional

### Prioridad BAJA (Características adicionales)
19. **`RutaImagen`** (string?) - Ruta de la imagen del producto
20. **`CodigoAlternativo`** (string?) - Código alternativo (Codigo2)
21. **`DepositoPorDefectoId`** (long?) - Depósito predeterminado
22. **`EsRubro`** (bool) - Es categoría/no es producto vendible
23. **`PermiteMovimientos`** (bool) - Permite movimientos de stock

---

## 6. FUNCIONALIDADES MODULARES A IMPLEMENTAR

### 6.1 Gestión de Pack/Componentes
```csharp
public class ItemComponente : AuditableEntity
{
    public long ItemPadreId { get; set; }    // Item pack
    public long ItemHijoId { get; set; }     // Item componente
    public decimal Cantidad { get; set; }     // Cantidad del componente
    public bool ControlaStock { get; set; }   // Descuenta stock del componente
}
```

### 6.2 Conversión de Unidades
```csharp
public class ItemUnidadConversion : AuditableEntity
{
    public long ItemId { get; set; }
    public long UnidadOrigenId { get; set; }
    public long UnidadDestinoId { get; set; }
    public decimal Factor { get; set; }         // Factor de conversión
    public bool EsUnidadCompra { get; set; }
    public bool EsUnidadVenta { get; set; }
}
```

### 6.3 Atributos Asignados
```csharp
public class ItemAtributo : AuditableEntity
{
    public long ItemId { get; set; }
    public long AtributoId { get; set; }
    public string? ValorTexto { get; set; }
    public decimal? ValorNumerico { get; set; }
    public bool? ValorBooleano { get; set; }
}
```

### 6.4 Cuentas Contables
```csharp
public class ItemCuentaContable : AuditableEntity
{
    public long ItemId { get; set; }
    public long CuentaVentaId { get; set; }
    public long CuentaCompraId { get; set; }
    public long CuentaCostoId { get; set; }
}
```

### 6.5 Stock por Depósito
```csharp
public class ItemStock : AuditableEntity
{
    public long ItemId { get; set; }
    public long DepositoId { get; set; }
    public long SucursalId { get; set; }
    public decimal StockActual { get; set; }
    public decimal StockComprometido { get; set; }
    public decimal StockDisponible { get; set; }
    public decimal StockMinimo { get; set; }
    public decimal StockMaximo { get; set; }
}
```

---

## 7. ESTRATEGIA DE IMPLEMENTACIÓN

### Fase 1: Campos Esenciales de Ventas (SPRINT 1)
**Objetivo**: Habilitar todas las operaciones de venta de zuluApp

1. Agregar campos a entidad `Item`:
   - `AplicaVentas`, `AplicaCompras`
   - `PorcentajeGanancia`, `PorcentajeMaximoDescuento`
   - `EsRpt`, `EsSistema`

2. Actualizar `ItemConfiguration` (EF Core)
3. Crear migración de base de datos
4. Actualizar `ItemDto` y mappings
5. Extender validators y commands
6. Actualizar endpoints del controller
7. Tests unitarios

### Fase 2: Stock y Logística (SPRINT 2)
**Objetivo**: Gestión avanzada de stock

1. Agregar campos a entidad `Item`:
   - `PuntoReposicion`, `StockSeguridad`
   - `Peso`, `Volumen`
   - `EsTrazable`, `PermiteFraccionamiento`
   - `DiasVencimientoLimite`

2. Crear entidad `ItemStock` (stock por depósito)
3. Implementar lógica de punto de reposición
4. Implementar alertas de stock
5. Tests unitarios

### Fase 3: Contabilidad e Integración (SPRINT 3)
**Objetivo**: Integración contable completa

1. Agregar campos a entidad `Item`:
   - `CuentaContableVentaId`, `CuentaContableCompraId`
   - `CodigoIntegradora`
   - `AlicuotaIvaCompraId`, `ImpuestoInternoId`

2. Crear relaciones con plan de cuentas
3. Implementar lógica de asientos automáticos
4. Tests de integración

### Fase 4: Funcionalidades Avanzadas (SPRINT 4)
**Objetivo**: Completar paridad total

1. Implementar gestión de packs (`ItemComponente`)
2. Implementar conversiones de unidades (`ItemUnidadConversion`)
3. Implementar gestión de atributos asignados (`ItemAtributo`)
4. Implementar gestión de imágenes
5. Tests end-to-end

---

## 8. SCRIPTS DE MIGRACIÓN

### 8.1 Agregar Campos a Tabla `items`
```sql
-- Fase 1: Ventas
ALTER TABLE items ADD COLUMN aplica_ventas boolean NOT NULL DEFAULT TRUE;
ALTER TABLE items ADD COLUMN aplica_compras boolean NOT NULL DEFAULT TRUE;
ALTER TABLE items ADD COLUMN porcentaje_ganancia numeric(18,4);
ALTER TABLE items ADD COLUMN porcentaje_maximo_descuento numeric(18,4);
ALTER TABLE items ADD COLUMN es_rpt boolean NOT NULL DEFAULT FALSE;
ALTER TABLE items ADD COLUMN es_sistema boolean NOT NULL DEFAULT FALSE;

-- Fase 2: Stock y Logística
ALTER TABLE items ADD COLUMN punto_reposicion numeric(18,4);
ALTER TABLE items ADD COLUMN stock_seguridad numeric(18,4);
ALTER TABLE items ADD COLUMN peso numeric(18,4);
ALTER TABLE items ADD COLUMN volumen numeric(18,4);
ALTER TABLE items ADD COLUMN es_trazable boolean NOT NULL DEFAULT FALSE;
ALTER TABLE items ADD COLUMN permite_fraccionamiento boolean NOT NULL DEFAULT FALSE;
ALTER TABLE items ADD COLUMN dias_vencimiento_limite integer;

-- Fase 3: Contabilidad
ALTER TABLE items ADD COLUMN cuenta_contable_venta_id bigint;
ALTER TABLE items ADD COLUMN cuenta_contable_compra_id bigint;
ALTER TABLE items ADD COLUMN codigo_integradora character varying(50);
ALTER TABLE items ADD COLUMN alicuota_iva_compra_id bigint;
ALTER TABLE items ADD COLUMN impuesto_interno_id bigint;

-- Fase 4: Adicionales
ALTER TABLE items ADD COLUMN ruta_imagen character varying(500);
ALTER TABLE items ADD COLUMN codigo_alternativo character varying(50);
ALTER TABLE items ADD COLUMN deposito_por_defecto_id bigint;
ALTER TABLE items ADD COLUMN es_rubro boolean NOT NULL DEFAULT FALSE;
ALTER TABLE items ADD COLUMN permite_movimientos boolean NOT NULL DEFAULT TRUE;
```

### 8.2 Crear Tablas Relacionadas
```sql
-- ItemComponente (Packs)
CREATE TABLE items_componentes (
    id bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    item_padre_id bigint NOT NULL REFERENCES items(id),
    item_hijo_id bigint NOT NULL REFERENCES items(id),
    cantidad numeric(18,4) NOT NULL,
    controla_stock boolean NOT NULL DEFAULT TRUE,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    deleted_at timestamp with time zone,
    created_by bigint,
    updated_by bigint
);

-- ItemUnidadConversion
CREATE TABLE items_unidades_conversion (
    id bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    item_id bigint NOT NULL REFERENCES items(id),
    unidad_origen_id bigint NOT NULL,
    unidad_destino_id bigint NOT NULL,
    factor numeric(18,6) NOT NULL,
    es_unidad_compra boolean NOT NULL DEFAULT FALSE,
    es_unidad_venta boolean NOT NULL DEFAULT FALSE,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    deleted_at timestamp with time zone,
    created_by bigint,
    updated_by bigint
);

-- ItemStock (Stock por depósito)
CREATE TABLE items_stock (
    id bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    item_id bigint NOT NULL REFERENCES items(id),
    deposito_id bigint NOT NULL,
    sucursal_id bigint NOT NULL,
    stock_actual numeric(18,4) NOT NULL DEFAULT 0,
    stock_comprometido numeric(18,4) NOT NULL DEFAULT 0,
    stock_disponible numeric(18,4) NOT NULL DEFAULT 0,
    stock_minimo numeric(18,4) NOT NULL DEFAULT 0,
    stock_maximo numeric(18,4),
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    deleted_at timestamp with time zone,
    created_by bigint,
    updated_by bigint,
    UNIQUE(item_id, deposito_id, sucursal_id)
);
```

---

## 9. VALIDACIÓN Y TESTING

### 9.1 Casos de Prueba Críticos
1. **Crear item con todos los campos nuevos**
2. **Validar porcentaje de ganancia automático**
3. **Validar descuento máximo en ventas**
4. **Gestión de packs con componentes**
5. **Conversión de unidades compra/venta**
6. **Cálculo de stock disponible por depósito**
7. **Trazabilidad de lotes y series**
8. **Integración contable automática**
9. **Alertas de punto de reposición**
10. **Items de sistema no modificables**

### 9.2 Smoke Test con zuluApp
- Migrar 50 items reales de zuluApp
- Validar todos los campos migrados
- Probar operación de venta completa
- Validar cálculo de precios y descuentos
- Validar movimiento de stock

---

## 10. DOCUMENTACIÓN ADICIONAL

### Referencias
- ✅ Formulario principal: `C:\Zulu\zuluApp\FORMULARIOS\frmItems.frm`
- ✅ Clase de negocio: `C:\Zulu\zuluApp\CLASES\clsItems.cls`
- ✅ Entidad actual: `src\ZuluIA_Back.Domain\Entities\Items\Item.cs`
- ✅ Configuración EF: `src\ZuluIA_Back.Infrastructure\Persistence\Configurations\ItemConfiguration.cs`
- ✅ Controller: `src\ZuluIA_Back.Api\Controllers\ItemsController.cs`

### Dependencias
- Módulo de Listas de Precios (verificar integración)
- Módulo de Atributos Comerciales (verificar integración)
- Módulo de Plan de Cuentas (crear/verificar)
- Módulo de Depósitos (crear/verificar)
- Módulo de Impuestos Internos (crear/verificar)

---

## 11. RIESGOS Y CONSIDERACIONES

### Riesgos Técnicos
1. **Migración de datos existentes**: Items actuales no tienen los nuevos campos
2. **Compatibilidad con módulos existentes**: Ventas, compras, stock
3. **Performance**: Agregar muchos campos puede afectar queries
4. **Complejidad de packs**: Lógica recursiva de stock

### Mitigaciones
1. Migración incremental por fases
2. Tests de regresión exhaustivos
3. Índices adecuados en BD
4. Documentación clara de reglas de negocio

---

## 12. CONCLUSIONES Y PRÓXIMOS PASOS

### Resumen de Gaps Identificados
- **23 campos faltantes** en entidad Item
- **5 tablas relacionadas** por crear
- **9 pestañas funcionales** con lógica asociada
- **Funcionalidades críticas**: Pack, conversiones, trazabilidad

### Recomendación
Implementar en **4 sprints** priorizando:
1. Campos esenciales de ventas (SPRINT 1) ← **INICIO INMEDIATO**
2. Stock y logística (SPRINT 2)
3. Contabilidad e integración (SPRINT 3)
4. Funcionalidades avanzadas (SPRINT 4)

### Estimación de Esfuerzo
- Sprint 1: 5-7 días
- Sprint 2: 5-7 días
- Sprint 3: 4-6 días
- Sprint 4: 7-10 días
- **Total**: 21-30 días de desarrollo

---

**Documento Generado**: 2025-01-XX  
**Autor**: GitHub Copilot + Análisis de Código  
**Versión**: 1.0  
**Estado**: En Revisión
