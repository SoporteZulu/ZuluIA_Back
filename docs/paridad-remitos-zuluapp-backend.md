# Paridad Funcional: Remitos - ZuluApp vs Backend Actual

## Objetivo
Documentar los gaps funcionales entre la vista de remitos de `C:\Zulu\zuluApp` (referencia principal) y el backend actual de `ZuluIA_Back`, para alcanzar paridad 100% antes de completar el frontend nuevo.

## Fecha de análisis
2026-03-20

## 1. Estructura de datos - Entidad Comprobante (Remitos)

### 1.1 Campos existentes en backend

**Entidad:** `Comprobante.cs`

#### Campos base (✅ OK):
- `SucursalId`
- `PuntoFacturacionId`
- `TipoComprobanteId`
- `Numero` (NroComprobante: prefijo + número)
- `Fecha`
- `TerceroId`
- `MonedaId`
- `Cotizacion`
- `Subtotal`, `Total`, `Saldo`
- `Estado`
- `Observacion`

#### Campos logísticos parciales (⚠️ PARCIAL):
- `TransporteId` ✅
- `ChoferNombre` ✅
- `ChoferDni` ✅
- `PatVehiculo` ✅
- `PatAcoplado` ✅
- `DomicilioEntrega` ✅
- `ObservacionesLogisticas` ✅
- `FechaEstimadaEntrega` ✅
- `FechaRealEntrega` ✅
- `FirmaConformidad` ✅
- `NombreQuienRecibe` ✅

### 1.2 Campos faltantes (❌ GAP)

#### COT (Carta Oficial de Transporte) - **CRÍTICO**
ZuluApp requiere tabla relacionada `CMP_COT`:
- `Cot_Valor` (string, mínimo 6 caracteres) - Número de COT
- `Cot_fecha` (date) - Fecha vigencia COT (debe ser >= fecha emisión del remito)
- `Cot_Descripcion` (string, opcional) - Descripción del COT

**Validaciones críticas zuluApp:**
```vbscript
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
```

**Estructura SQL zuluApp:**
```sql
CREATE TABLE CMP_COT (
    Cot_id bigint identity,
    cmp_id bigint NOT NULL,  -- FK a comprobante
    Cot_fecha date NOT NULL,
    Cot_Valor nvarchar(50) NOT NULL,
    Cot_Descripcion nvarchar(500),
    PRIMARY KEY (Cot_id),
    FOREIGN KEY (cmp_id) REFERENCES COMPROBANTES(Id_Comprobante)
)
```

#### Atributos de Remito
ZuluApp tiene formulario dedicado `frmRemitos_Atributos.frm` que permite agregar metadatos adicionales por remito.

Debe existir:
- Entidad `RemitoCabAtributo` o similar
- Relación 1:N con `Comprobante`
- Estructura flexible key-value o catálogo de atributos predefinidos

#### Depósito origen del movimiento
ZuluApp hace `JOIN` explícito con:
```sql
DEPOSITOS INNER JOIN MOVIMIENTOSTOCK ON DEPOSITOS.id = MOVIMIENTOSTOCK.id_deposito
INNER JOIN VTA_CMP_COMPROBANTES_STOCK ON MOVIMIENTOSTOCK.id_comprobante = VTA_CMP_COMPROBANTES_STOCK.Id_Comprobante
```

Backend actual:
- ⚠️ Verificar si `MovimientoStock` ya relaciona depósito correctamente
- ❌ DTO de remito debe exponer `DepositoId` y `DepositoDescripcion`

#### Estado operativo/logístico específico de remitos
ZuluApp diferencia:
- Remitos valorizados vs no valorizados
- Estado de entrega: Pendiente / En tránsito / Entregado / Rechazado
- Vinculación con factura (puede haber remitos sin factura o múltiples remitos → 1 factura)

Backend actual:
- ❌ Falta enum `EstadoLogisticoRemito`
- ❌ Falta flag `EsValorizado` en Comprobante o metadata
- ⚠️ Vinculación remito-factura existe como `ComprobanteOrigenId` pero no es bidireccional ni expone lista de remitos vinculados a una factura

---

## 2. Queries y Filtros

### 2.1 Filtros existentes en zuluApp

Vista `VTACOMPROBANTESREMITOS_Listado.asp`:

#### Filtros implementados (11 filtros):
1. **Sucursal Emisión** (`VTA_CMP_COMPROBANTES_STOCK.Id_Sucursal`)
2. **Fecha Emisión Desde** (`FechaEmision >= ?`)
3. **Fecha Emisión Hasta** (`FechaEmision <= ?`)
4. **Prefijo Comprobante** (`PrefijoComprobante = ?`)
5. **Número Comprobante** (`NumeroComprobante = ?`)
6. **Legajo Persona** (`LegajoPersona = ?`)
7. **Legajo Sucursal Persona** (`LegajoSucursalPersona = ?`)
8. **Denominación Social** (`DenominacionSocialPersona LIKE ?`)
9. **Número COT** (`CMP_COT.Cot_Valor LIKE ?`)
10. **Fecha Vigencia COT Desde** (`Cot_fecha >= ?`)
11. **Fecha Vigencia COT Hasta** (`Cot_fecha <= ?`)
12. **Depósito** (`DEPOSITOS.id = ?`)

#### Tipos de comprobante remito:
```vbscript
WHERE VTA_CMP_COMPROBANTES_STOCK.Id_TipoComprobante IN (68,83,85)
```
Verificar en backend actual qué IDs corresponden a remitos.

### 2.2 Campos mostrados en grilla (zuluApp)

Columnas principales:
1. Número Sucursal
2. Fecha Emisión
3. Tipo Comprobante
4. Prefijo Comprobante
5. Número Comprobante
6. Legajo Persona
7. Legajo Sucursal Persona
8. Denominación Social Persona
9. Número COT (`CMP_COT.Cot_Valor`)
10. Fecha Vigencia COT (`CMP_COT.Cot_fecha`)
11. Depósito (`DEPOSITOS.Descripcion`)
12. Moneda
13. Total Bruto Moneda Origen
14. Total Bruto Moneda Corriente

### 2.3 Gaps en queries backend

#### Query GetComprobantes paginada
❌ **Falta:**
- Filtro por rango de fecha COT
- Filtro por número COT
- Filtro por depósito
- Exposición de datos COT en DTO de listado
- Exposición de depósito origen en DTO de listado

#### Query GetComprobanteDetalle
⚠️ **Verificar:**
- Si incluye todos los campos logísticos
- Si carga relación COT
- Si carga atributos de remito
- Si carga detalle de movimiento de stock con depósito

---

## 3. Comandos y Operaciones

### 3.1 Operaciones existentes en backend

**Controlador:** `VentasController.cs`

#### Comandos disponibles:
- ✅ `CrearBorradorVentaCommand`
- ✅ `EmitirDocumentoVentaCommand`
- ✅ `EmitirRemitosVentaMasivosCommand`
- ✅ `RegistrarDevolucionVentaCommand`
- ✅ `VincularComprobanteVentaCommand`
- ✅ `ConvertirDocumentoVentaCommand`

### 3.2 Operaciones faltantes (❌ GAP)

#### Remitos específicos:
1. **UpsertRemitoConCOTCommand** - Crear/actualizar remito con datos COT obligatorios
2. **SetRemitoCOTCommand** - Asignar/actualizar COT a remito existente (EditarDB.asp)
3. **SetRemitoAtributosCommand** - Gestionar atributos personalizados por remito
4. **GetRemitosPagedQuery** - Query especializada con filtros COT y depósito
5. **GetRemitoDetalleQuery** - Detalle completo incluyendo COT, atributos, logística y stock
6. **VincularRemitosAFacturaCommand** - Vinculación múltiple remitos → 1 factura
7. **ImprimirRemitoCommand** - Genera PDF/impresión con layout específico de remito

#### Validaciones específicas de remitos:
- Validar que COT sea obligatorio para tipos de comprobante remito
- Validar que `Cot_Valor` tenga al menos 6 caracteres
- Validar que `Cot_fecha >= Fecha` del remito
- Validar que depósito tenga stock disponible antes de emitir remito
- Validar restricciones de cliente sobre entrega (sucursal entrega, transporte, etc.)

---

## 4. DTOs

### 4.1 DTO actual: ComprobanteDto

#### Campos logísticos presentes (✅):
```csharp
public long? TransporteId { get; set; }
public string? TransporteRazonSocial { get; set; }
public string? ChoferNombre { get; set; }
public string? ChoferDni { get; set; }
public string? PatVehiculo { get; set; }
public string? PatAcoplado { get; set; }
public string? DomicilioEntrega { get; set; }
public string? ObservacionesLogisticas { get; set; }
public DateOnly? FechaEstimadaEntrega { get; set; }
public DateOnly? FechaRealEntrega { get; set; }
public string? FirmaConformidad { get; set; }
public string? NombreQuienRecibe { get; set; }
```

### 4.2 DTOs faltantes (❌ GAP)

#### RemitoDto especializado
Debería extender o especializar `ComprobanteDto`:

```csharp
public class RemitoDto : ComprobanteDto
{
    // Datos COT
    public string? CotNumero { get; set; }
    public DateOnly? CotFechaVigencia { get; set; }
    public string? CotDescripcion { get; set; }
    
    // Depósito origen
    public long? DepositoId { get; set; }
    public string? DepositoDescripcion { get; set; }
    
    // Estado logístico
    public EstadoLogisticoRemito? EstadoLogistico { get; set; }
    public bool EsValorizado { get; set; }
    
    // Vinculación
    public List<long> RemitosVinculadosIds { get; set; } = new();
    public long? FacturaVinculadaId { get; set; }
    public string? FacturaVinculadaNumero { get; set; }
    
    // Atributos adicionales
    public List<RemitoAtributoDto> Atributos { get; set; } = new();
}

public class RemitoAtributoDto
{
    public long Id { get; set; }
    public string Clave { get; set; } = string.Empty;
    public string? Valor { get; set; }
    public string? TipoDato { get; set; }
}

public class RemitoCOTDto
{
    public long Id { get; set; }
    public long ComprobanteId { get; set; }
    public string CotNumero { get; set; } = string.Empty;
    public DateOnly CotFechaVigencia { get; set; }
    public string? CotDescripcion { get; set; }
}
```

#### RemitoListDto (para grilla)
```csharp
public class RemitoListDto
{
    public long Id { get; set; }
    public string SucursalCodigo { get; set; } = string.Empty;
    public DateOnly FechaEmision { get; set; }
    public string TipoComprobante { get; set; } = string.Empty;
    public short Prefijo { get; set; }
    public long Numero { get; set; }
    public string NumeroFormateado { get; set; } = string.Empty;
    public string TerceroLegajo { get; set; } = string.Empty;
    public string TerceroLegajoSucursal { get; set; } = string.Empty;
    public string TerceroDenominacionSocial { get; set; } = string.Empty;
    public string? CotNumero { get; set; }
    public DateOnly? CotFechaVigencia { get; set; }
    public string? DepositoDescripcion { get; set; }
    public string Moneda { get; set; } = string.Empty;
    public decimal TotalMonedaOrigen { get; set; }
    public decimal TotalMonedaCorriente { get; set; }
    public EstadoComprobante Estado { get; set; }
    public EstadoLogisticoRemito? EstadoLogistico { get; set; }
}
```

---

## 5. Configuraciones de Entity Framework

### 5.1 Entidades nuevas requeridas

#### ComprobanteCOT
```csharp
public class ComprobanteCOT : AuditableEntity
{
    public long ComprobanteId { get; private set; }
    public string CotNumero { get; private set; } = string.Empty;
    public DateOnly CotFechaVigencia { get; private set; }
    public string? CotDescripcion { get; private set; }
    
    // Navigation
    public Comprobante Comprobante { get; private set; } = null!;
    
    private ComprobanteCOT() { }
    
    public static ComprobanteCOT Crear(
        long comprobanteId,
        string cotNumero,
        DateOnly cotFechaVigencia,
        DateOnly fechaEmisionComprobante,
        string? cotDescripcion,
        long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cotNumero);
        
        if (cotNumero.Length < 6)
            throw new ArgumentException("El número de COT debe tener al menos 6 caracteres.", nameof(cotNumero));
        
        if (cotFechaVigencia < fechaEmisionComprobante)
            throw new ArgumentException("La fecha de vigencia del COT no puede ser anterior a la fecha de emisión del remito.", nameof(cotFechaVigencia));
        
        var cot = new ComprobanteCOT
        {
            ComprobanteId = comprobanteId,
            CotNumero = cotNumero.Trim(),
            CotFechaVigencia = cotFechaVigencia,
            CotDescripcion = cotDescripcion?.Trim()
        };
        
        cot.SetCreated(userId);
        return cot;
    }
    
    public void Actualizar(string cotNumero, DateOnly cotFechaVigencia, DateOnly fechaEmisionComprobante, string? cotDescripcion, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cotNumero);
        
        if (cotNumero.Length < 6)
            throw new ArgumentException("El número de COT debe tener al menos 6 caracteres.", nameof(cotNumero));
        
        if (cotFechaVigencia < fechaEmisionComprobante)
            throw new ArgumentException("La fecha de vigencia del COT no puede ser anterior a la fecha de emisión del remito.", nameof(cotFechaVigencia));
        
        CotNumero = cotNumero.Trim();
        CotFechaVigencia = cotFechaVigencia;
        CotDescripcion = cotDescripcion?.Trim();
        
        SetUpdated(userId);
    }
}
```

#### ComprobanteAtributo
```csharp
public class ComprobanteAtributo : AuditableEntity
{
    public long ComprobanteId { get; private set; }
    public string Clave { get; private set; } = string.Empty;
    public string? Valor { get; private set; }
    public string? TipoDato { get; private set; }
    
    // Navigation
    public Comprobante Comprobante { get; private set; } = null!;
    
    private ComprobanteAtributo() { }
    
    public static ComprobanteAtributo Crear(long comprobanteId, string clave, string? valor, string? tipoDato, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clave);
        
        var atributo = new ComprobanteAtributo
        {
            ComprobanteId = comprobanteId,
            Clave = clave.Trim(),
            Valor = valor?.Trim(),
            TipoDato = tipoDato?.Trim()
        };
        
        atributo.SetCreated(userId);
        return atributo;
    }
    
    public void ActualizarValor(string? valor, long? userId)
    {
        Valor = valor?.Trim();
        SetUpdated(userId);
    }
}
```

### 5.2 Enum nuevo

#### EstadoLogisticoRemito
```csharp
namespace ZuluIA_Back.Domain.Enums;

public enum EstadoLogisticoRemito
{
    Pendiente = 1,
    EnPreparacion = 2,
    Preparado = 3,
    Despachado = 4,
    EnTransito = 5,
    Entregado = 6,
    Rechazado = 7,
    Devuelto = 8
}
```

### 5.3 Modificación en Comprobante

Agregar:
```csharp
public EstadoLogisticoRemito? EstadoLogistico { get; private set; }
public bool EsValorizado { get; private set; } = true;
public long? DepositoOrigenId { get; private set; }

// Navegación
public ComprobanteCOT? COT { get; private set; }
private readonly List<ComprobanteAtributo> _atributos = [];
public IReadOnlyCollection<ComprobanteAtributo> Atributos => _atributos.AsReadOnly();

public void AsignarCOT(ComprobanteCOT cot)
{
    COT = cot ?? throw new ArgumentNullException(nameof(cot));
}

public void CambiarEstadoLogistico(EstadoLogisticoRemito nuevoEstado, long? userId)
{
    EstadoLogistico = nuevoEstado;
    SetUpdated(userId);
}

public void AgregarAtributo(ComprobanteAtributo atributo)
{
    if (_atributos.Any(a => a.Clave == atributo.Clave))
        throw new InvalidOperationException($"Ya existe un atributo con la clave '{atributo.Clave}'.");
    
    _atributos.Add(atributo);
}

public void RemoverAtributo(string clave)
{
    var atributo = _atributos.FirstOrDefault(a => a.Clave == clave);
    if (atributo is not null)
        _atributos.Remove(atributo);
}
```

---

## 6. Configuraciones EF Core

### 6.1 ComprobanteCOTConfiguration
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Comprobantes;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class ComprobanteCOTConfiguration : IEntityTypeConfiguration<ComprobanteCOT>
{
    public void Configure(EntityTypeBuilder<ComprobanteCOT> builder)
    {
        builder.ToTable("cmp_cot");
        
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");
        
        builder.Property(c => c.ComprobanteId).HasColumnName("cmp_id").IsRequired();
        builder.Property(c => c.CotNumero).HasColumnName("cot_valor").HasMaxLength(50).IsRequired();
        builder.Property(c => c.CotFechaVigencia).HasColumnName("cot_fecha").IsRequired();
        builder.Property(c => c.CotDescripcion).HasColumnName("cot_descripcion").HasMaxLength(500);
        
        builder.HasOne(c => c.Comprobante)
               .WithOne(c => c.COT)
               .HasForeignKey<ComprobanteCOT>(c => c.ComprobanteId)
               .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasIndex(c => c.ComprobanteId).IsUnique();
        builder.HasIndex(c => c.CotNumero);
        builder.HasIndex(c => c.CotFechaVigencia);
    }
}
```

### 6.2 ComprobanteAtributoConfiguration
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Comprobantes;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class ComprobanteAtributoConfiguration : IEntityTypeConfiguration<ComprobanteAtributo>
{
    public void Configure(EntityTypeBuilder<ComprobanteAtributo> builder)
    {
        builder.ToTable("comprobante_atributo");
        
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");
        
        builder.Property(c => c.ComprobanteId).HasColumnName("comprobante_id").IsRequired();
        builder.Property(c => c.Clave).HasColumnName("clave").HasMaxLength(100).IsRequired();
        builder.Property(c => c.Valor).HasColumnName("valor").HasMaxLength(500);
        builder.Property(c => c.TipoDato).HasColumnName("tipo_dato").HasMaxLength(50);
        
        builder.HasOne(c => c.Comprobante)
               .WithMany(c => c.Atributos)
               .HasForeignKey(c => c.ComprobanteId)
               .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(c => new { c.ComprobanteId, c.Clave }).IsUnique();
    }
}
```

### 6.3 Actualización de ComprobanteConfiguration

Agregar mappings:
```csharp
// En Configure method
builder.Property(c => c.EstadoLogistico).HasColumnName("estado_logistico");
builder.Property(c => c.EsValorizado).HasColumnName("es_valorizado").HasDefaultValue(true);
builder.Property(c => c.DepositoOrigenId).HasColumnName("deposito_origen_id");

builder.HasOne(c => c.COT)
       .WithOne(c => c.Comprobante)
       .HasForeignKey<ComprobanteCOT>(c => c.ComprobanteId);

builder.HasMany(c => c.Atributos)
       .WithOne(c => c.Comprobante)
       .HasForeignKey(c => c.ComprobanteId);
```

---

## 7. Scripts de migración de base de datos

### 7.1 Script de creación de tabla CMP_COT

```sql
-- Script: add_cmp_cot_table.sql
-- Descripción: Crea tabla para almacenar datos COT (Carta Oficial de Transporte) de remitos

BEGIN;

CREATE TABLE IF NOT EXISTS cmp_cot (
    id bigint GENERATED BY DEFAULT AS IDENTITY,
    cmp_id bigint NOT NULL,
    cot_valor character varying(50) NOT NULL,
    cot_fecha date NOT NULL,
    cot_descripcion character varying(500),
    created_at timestamptz NOT NULL DEFAULT now(),
    created_by bigint,
    updated_at timestamptz,
    updated_by bigint,
    CONSTRAINT pk_cmp_cot PRIMARY KEY (id),
    CONSTRAINT fk_cmp_cot_comprobante FOREIGN KEY (cmp_id)
        REFERENCES comprobantes (id) ON DELETE RESTRICT
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_cmp_cot_cmp_id ON cmp_cot (cmp_id);
CREATE INDEX IF NOT EXISTS ix_cmp_cot_cot_valor ON cmp_cot (cot_valor);
CREATE INDEX IF NOT EXISTS ix_cmp_cot_cot_fecha ON cmp_cot (cot_fecha);

COMMENT ON TABLE cmp_cot IS 'Carta Oficial de Transporte asociada a remitos';
COMMENT ON COLUMN cmp_cot.cmp_id IS 'FK al comprobante (remito)';
COMMENT ON COLUMN cmp_cot.cot_valor IS 'Número de COT (mínimo 6 caracteres)';
COMMENT ON COLUMN cmp_cot.cot_fecha IS 'Fecha de vigencia del COT (debe ser >= fecha emisión remito)';
COMMENT ON COLUMN cmp_cot.cot_descripcion IS 'Descripción adicional del COT';

COMMIT;
```

### 7.2 Script de creación de tabla COMPROBANTE_ATRIBUTO

```sql
-- Script: add_comprobante_atributo_table.sql
-- Descripción: Crea tabla para almacenar atributos personalizados por comprobante (remitos)

BEGIN;

CREATE TABLE IF NOT EXISTS comprobante_atributo (
    id bigint GENERATED BY DEFAULT AS IDENTITY,
    comprobante_id bigint NOT NULL,
    clave character varying(100) NOT NULL,
    valor character varying(500),
    tipo_dato character varying(50),
    created_at timestamptz NOT NULL DEFAULT now(),
    created_by bigint,
    updated_at timestamptz,
    updated_by bigint,
    CONSTRAINT pk_comprobante_atributo PRIMARY KEY (id),
    CONSTRAINT fk_comprobante_atributo_comprobante FOREIGN KEY (comprobante_id)
        REFERENCES comprobantes (id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_comprobante_atributo_comprobante_clave
    ON comprobante_atributo (comprobante_id, clave);

CREATE INDEX IF NOT EXISTS ix_comprobante_atributo_comprobante_id
    ON comprobante_atributo (comprobante_id);

COMMENT ON TABLE comprobante_atributo IS 'Atributos personalizados por comprobante (ej. atributos de remitos)';
COMMENT ON COLUMN comprobante_atributo.clave IS 'Clave del atributo (única por comprobante)';
COMMENT ON COLUMN comprobante_atributo.valor IS 'Valor del atributo';
COMMENT ON COLUMN comprobante_atributo.tipo_dato IS 'Tipo de dato del valor (texto, número, fecha, etc.)';

COMMIT;
```

### 7.3 Script de actualización de tabla COMPROBANTES

```sql
-- Script: add_remito_fields_to_comprobantes.sql
-- Descripción: Agrega campos específicos de remitos a tabla comprobantes

BEGIN;

ALTER TABLE comprobantes
    ADD COLUMN IF NOT EXISTS estado_logistico smallint,
    ADD COLUMN IF NOT EXISTS es_valorizado boolean NOT NULL DEFAULT true,
    ADD COLUMN IF NOT EXISTS deposito_origen_id bigint;

CREATE INDEX IF NOT EXISTS ix_comprobantes_estado_logistico
    ON comprobantes (estado_logistico) WHERE estado_logistico IS NOT NULL;

CREATE INDEX IF NOT EXISTS ix_comprobantes_deposito_origen_id
    ON comprobantes (deposito_origen_id) WHERE deposito_origen_id IS NOT NULL;

COMMENT ON COLUMN comprobantes.estado_logistico IS 'Estado logístico del remito (1=Pendiente, 2=EnPreparacion, 3=Preparado, 4=Despachado, 5=EnTransito, 6=Entregado, 7=Rechazado, 8=Devuelto)';
COMMENT ON COLUMN comprobantes.es_valorizado IS 'Indica si el remito es valorizado (true) o no valorizado (false)';
COMMENT ON COLUMN comprobantes.deposito_origen_id IS 'FK al depósito origen del movimiento de stock del remito';

-- Opcional: agregar FK a depositos si existe la tabla
-- ALTER TABLE comprobantes
--     ADD CONSTRAINT fk_comprobantes_deposito_origen
--     FOREIGN KEY (deposito_origen_id) REFERENCES depositos (id) ON DELETE RESTRICT;

COMMIT;
```

---

## 8. Resumen de Gaps Críticos

### Prioridad ALTA (bloqueante para paridad funcional)

1. ✅ **Tabla CMP_COT**: Almacenar número COT, fecha vigencia y descripción
2. ✅ **Validaciones COT**: Longitud mínima 6 chars, fecha vigencia >= fecha emisión
3. ✅ **Enum EstadoLogisticoRemito**: Estados propios del ciclo logístico
4. ❌ **Query GetRemitosPagedQuery**: Con filtros por COT, depósito y campos logísticos
5. ❌ **Command UpsertRemitoConCOTCommand**: Crear/editar remito con COT obligatorio
6. ❌ **DTO RemitoDto/RemitoListDto**: Exponer todos los campos de zuluApp
7. ❌ **Exposición de depósito origen**: En listado y detalle de remitos

### Prioridad MEDIA (mejora UX pero no bloquea operación básica)

8. ❌ **Tabla COMPROBANTE_ATRIBUTO**: Atributos personalizados por remito
9. ❌ **Command SetRemitoAtributosCommand**: Gestionar atributos
10. ❌ **Vinculación múltiple remitos→factura**: Lista de remitos vinculados a una factura
11. ❌ **Flag EsValorizado**: Diferenciar remitos valorizados/no valorizados
12. ❌ **ImprimirRemitoCommand**: Layout específico con datos COT y logística

### Prioridad BAJA (nice-to-have, auditoría y reportes)

13. ❌ **Auditoría de cambios de estado logístico**: Historial de transiciones
14. ❌ **Reportes específicos de remitos**: Agrupados por transporte, depósito, COT, etc.
15. ❌ **Dashboard logístico**: KPIs de remitos en tránsito, entregados, rechazados

---

## 9. Plan de Implementación Sugerido

### Fase 1: Fundamentos (Entidades y Configuraciones)
- [ ] Crear entidad `ComprobanteCOT`
- [ ] Crear entidad `ComprobanteAtributo`
- [ ] Agregar enum `EstadoLogisticoRemito`
- [ ] Actualizar entidad `Comprobante` con campos remito
- [ ] Crear configuraciones EF Core
- [ ] Ejecutar migraciones de base de datos

### Fase 2: DTOs y Mapeos
- [ ] Crear `RemitoDto`
- [ ] Crear `RemitoListDto`
- [ ] Crear `RemitoCOTDto`
- [ ] Crear `RemitoAtributoDto`
- [ ] Actualizar `MappingProfile` con mapeos automáticos

### Fase 3: Commands (Escritura)
- [ ] Implementar `UpsertRemitoConCOTCommand` y `CommandHandler`
- [ ] Implementar `UpsertRemitoConCOTCommandValidator`
- [ ] Implementar `SetRemitoCOTCommand` y `CommandHandler`
- [ ] Implementar `SetRemitoAtributosCommand` y `CommandHandler`
- [ ] Implementar `CambiarEstadoLogisticoRemitoCommand` y `CommandHandler`

### Fase 4: Queries (Lectura)
- [ ] Implementar `GetRemitosPagedQuery` y `QueryHandler`
- [ ] Implementar `GetRemitoDetalleQuery` y `QueryHandler`
- [ ] Agregar filtros COT y depósito a queries existentes
- [ ] Agregar proyecciones de datos COT en DTOs de listado

### Fase 5: Repositorio y UnitOfWork
- [ ] Agregar métodos específicos de remitos en `IComprobanteRepository`
- [ ] Implementar métodos en `ComprobanteRepository`
- [ ] Agregar queries de vinculación remito-factura

### Fase 6: API Controller
- [ ] Actualizar `VentasController` o crear `RemitosController` dedicado
- [ ] Exponer endpoints de creación/edición con COT
- [ ] Exponer endpoints de atributos
- [ ] Exponer endpoints de cambio de estado logístico
- [ ] Exponer endpoint de listado con filtros extendidos

### Fase 7: Tests
- [ ] Tests unitarios de entidades `ComprobanteCOT` y `ComprobanteAtributo`
- [ ] Tests de validadores de commands
- [ ] Tests de handlers de commands
- [ ] Tests de handlers de queries
- [ ] Tests de integración de repositorio
- [ ] Tests de API controller

### Fase 8: Validación y Smoke Tests
- [ ] Ejecutar smoke tests contra PostgreSQL local
- [ ] Validar que todos los campos de zuluApp estén cubiertos
- [ ] Validar reglas de negocio (validaciones COT, fechas, etc.)
- [ ] Comparar outputs con zuluApp para confirmar paridad

---

## 10. Criterios de Aceptación (Checklist de Paridad)

- [ ] Backend expone TODOS los campos visibles en grilla de zuluApp
- [ ] Backend expone TODOS los campos visibles en detalle de remito de zuluApp
- [ ] Backend soporta TODOS los filtros de búsqueda de zuluApp (11 filtros)
- [ ] Backend valida número COT (obligatorio, mínimo 6 chars)
- [ ] Backend valida fecha vigencia COT (>= fecha emisión remito)
- [ ] Backend permite crear/editar COT independientemente del remito (EditarDB.asp)
- [ ] Backend permite gestionar atributos personalizados por remito
- [ ] Backend expone depósito origen del movimiento de stock
- [ ] Backend diferencia estados logísticos propios de remitos
- [ ] Backend permite vinculación múltiple remitos → factura
- [ ] Frontend puede dejar de usar `useLegacyLocalCollection` para remitos
- [ ] Tests E2E validan flujo completo: crear remito con COT → emitir → cambiar estado → vincular a factura

---

## Notas Finales

**Referencia principal:** `C:\Zulu\zuluApp`
- Vista ASP: `VTACOMPROBANTESREMITOS_Listado.asp`
- Edición COT: `VTACOMPROBANTESREMITOS_EditarForm.asp`, `VTACOMPROBANTESREMITOS_EditarDB.asp`
- Atributos: `frmRemitos_Atributos.frm`

**Regla de trabajo:**
> "El comportamiento funcional de `C:\Zulu\zuluApp` es la referencia principal ('la ley') para validar y completar la lógica de reemplazo en el backend y frontend modernos."

Una vez implementados todos los gaps documentados, el backend estará al 100% de paridad funcional con zuluApp para remitos, permitiendo que el frontend nuevo pueda operar sin dependencias locales ni faltantes de datos.
