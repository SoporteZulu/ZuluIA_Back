# Ventas > Clientes — backlog técnico backend-first

## Objetivo
Cerrar al 100% el soporte backend necesario para que `C:\Zulu\ZuluIA_Front\app\ventas\clientes\page.tsx` reemplace funcionalmente a `frmCliente` y formularios relacionados de `C:\Zulu\zuluApp`, sin overlays locales ni faltantes de datos visibles, filtros, reglas, acciones o trazabilidad.

## Alcance fuente
### Frontend nuevo relevado
Vista principal:
- `app/ventas/clientes/page.tsx`

Bloques visibles detectados en la UX actual:
- identificación básica
- contacto principal
- domicilio
- documento y condición fiscal
- facturación y claves fiscales
- moneda, categoría, cobrador, vendedor, comisiones y observaciones
- roles del registro
- resumen operativo/comercial
- perfil comercial
- contactos múltiples
- sucursales y puntos de entrega
- transportes asociados
- ventanas de cobranza
- resumen comercial rápido
- cobertura de contacto
- listado/búsqueda/alta/edición/desactivación

### Legacy `zuluApp`
Formularios equivalentes:
- `frmCliente`
- `frmClienteResumido`
- `frmImportadorClientes`

Campos/capas visibles detectados en el legado:
- identificación: legajo, nro interno, razón social, nombre comercial, personería, estado, fecha alta, fecha registro
- fiscal/documental: tipo y nro documento, condición fiscal, clave fiscal, valor de clave fiscal, nro ingresos brutos, nro municipalidad, facturable, mínimo facturas MiPymes
- geografía: país, sucursal, categoría, domicilio/s, medios de contacto
- comercial: rubro, sector, zona, moneda, facturador por defecto, días y horarios de cobranzas, plazo de cobro, condición de venta
- cuenta corriente: límite de saldo, límite de crédito total, vigencia desde/hasta de ambos
- vendedor/cobrador: legajo, nombre, aplica comisión, porcentaje de comisión
- persona física: apellido, nombre, sexo, nacionalidad, estado civil, profesión, tratamiento, fecha nacimiento, entidad gubernamental
- seguridad/usuario: usuario, nombre de usuario, grupo, contraseña y confirmación
- operación: buscar, nuevo, modificar, eliminar, aceptar, cancelar, vista resumida
- extras detectados: porcentaje máximo de descuento, importación de clientes

### Backend actual relevado
Endpoints reales ya disponibles:
- `GET /api/terceros`
- `GET /api/terceros/{id}`
- `GET /api/terceros/legajo/{legajo}`
- `GET /api/terceros/catalogos`
- `GET /api/terceros/perfil-comercial` indirecto por detalle y endpoint propio por id
- `GET/PUT /api/terceros/{id}/perfil-comercial`
- `GET/PUT /api/terceros/{id}/contactos`
- `GET/PUT /api/terceros/{id}/sucursales-entrega`
- `GET/PUT /api/terceros/{id}/transportes`
- `GET/PUT /api/terceros/{id}/ventanas-cobranza`
- `POST /api/terceros`
- `PUT /api/terceros/{id}`
- `DELETE /api/terceros/{id}`
- `PATCH /api/terceros/{id}/activar`
- catálogos auxiliares de clientes/proveedores

DTOs/contratos existentes relevantes:
- `CreateTerceroCommand`
- `UpdateTerceroCommand`
- `TerceroDto`
- `TerceroListDto`
- `TerceroPerfilComercialDto`
- `TerceroContactoDto`
- `TerceroSucursalEntregaDto`
- `TerceroTransporteDto`
- `TerceroVentanaCobranzaDto`

---

## 1. Matriz de cobertura funcional de Clientes

| Bloque | Legacy `zuluApp` | Front nuevo | Backend actual | Estado | Gap backend real |
|---|---|---|---|---|---|
| Identificación base | Legajo, Nro interno, Razón social, Nombre comercial, Personería, Estado, Fechas | Sí | Legajo, razón social, nombre fantasía, personería, activo, createdAt | PARCIAL | Falta `nro interno` y distinguir si `estado` legacy es más que `activo/inactivo` |
| Documento y fiscal | Tipo doc, nro doc, condición fiscal, clave fiscal, valor clave fiscal, IB, municipalidad, facturable | Sí | Sí | REAL/PARCIAL | Revisar si faltan catálogos/validaciones equivalentes del legacy |
| Geografía | País, sucursal, categoría, domicilio | Parcial | Sucursal, localidad, barrio, categoría general | PARCIAL | Falta `pais` en DTO/command de terceros; revisar múltiples domicilios legacy |
| Comercial base | Moneda, rubro, sector, zona, condición venta, plazo cobro, facturador por defecto | Sí | Sí vía perfil comercial | REAL/PARCIAL | Falta verificar catálogos cerrados para rubro/sector/condición si en legacy no eran libres |
| Cobranza | Días y horarios de cobranzas | Sí | Ventanas de cobranza | REAL/PARCIAL | Falta verificar equivalencia total con el legado |
| Cuenta corriente | Límite saldo, límite crédito total, vigencias desde/hasta | Parcial | `LimiteCredito`, `SaldoMaximoVigente`, `VigenciaSaldo` | PARCIAL | Falta separar claramente `límite saldo` vs `límite crédito total` y sus vigencias |
| Vendedor/Cobrador | Legajo, nombre, aplica comisión, porcentaje | Sí parcial | `VendedorId`, `CobradorId`, porcentajes | PARCIAL | Faltan flags explícitos `aplica comisión`; el front hoy usa IDs directos |
| Contactos múltiples | Medios de contacto | Sí | Sí | REAL | Revisar si falta tipificación de medio o prioridad más rica |
| Sucursales/entrega | Domicilios / puntos de entrega | Sí | Sí | REAL/PARCIAL | Revisar si el legado tenía múltiples domicilios además de sucursales de entrega |
| Transportes | Transportista / logística | Sí | Sí | REAL/PARCIAL | Revisar datos auxiliares de chofer/ruta si eran visibles en cliente |
| Persona física extendida | Apellido, nombre, sexo, nacionalidad, estado civil, profesión, tratamiento, fecha nacimiento | No completo | Solo nombre, apellido, personería y entidad gubernamental | GAP | Faltan campos y reglas para persona física avanzada |
| Operación / seguridad | Usuario, grupo, contraseña, nombre usuario | No | No en Terceros | GAP | Definir si este bloque sigue perteneciendo a Cliente o pasó a Usuarios/Seguridad |
| Descuento comercial | % máximo de descuento | No | No en tercero | GAP | Falta exponerlo o definir modelo formal de descuentos por cliente |
| Importación | Importador de clientes | No | No validado | GAP | Falta decidir si requiere comando/proceso masivo |

---

## 2. Backlog técnico detallado

### B1. Consolidar contrato funcional de `Tercero` para Ventas
Objetivo: convertir `Tercero` en contrato maestro definitivo para `Ventas > Clientes`.

Tareas:
1. auditar `TerceroDto`, `TerceroListDto`, `CreateTerceroCommand` y `UpdateTerceroCommand` contra la ficha completa de `frmCliente`
2. identificar faltantes concretos de propiedades
3. separar faltantes en:
   - campos nuevos persistidos
   - campos derivados
   - catálogos relacionados
   - bloques que en realidad pertenecen a otro módulo
4. definir contrato final de `cliente ventas` sin ambigüedad entre `tercero` general y `cliente` operativo

Criterio de salida:
- lista cerrada de propiedades soportadas y pendientes, con decisión por cada campo del legacy

### B2. Completar identificación y estado operativo
Problema actual:
- el legacy muestra `nro interno`, estado y fechas con más granularidad aparente que el contrato actual

Tareas:
1. confirmar si `nro interno` es campo propio o alias histórico de otro dato
2. confirmar si `estado` legacy es solo `activo` o combinación con estado cliente bloqueante
3. definir si `TerceroListDto` y `TerceroDto` deben exponer:
   - `EstadoOperativo`
   - `EstadoOperativoDescripcion`
   - `FechaAlta`
   - `FechaRegistro`
4. agregar filtros backend si el legacy filtraba por estado operativo real

Dependencias:
- estados de cliente
- regla operativa de bloqueo ya existente

### B3. Completar geografía y domicilio
Problema actual:
- la vista actual soporta localidad/barrio, pero el legacy expone también `país` y probablemente multiplica domicilios

Tareas:
1. confirmar si el domicilio principal del tercero debe persistir `PaisId`
2. extender entidad/DTO/commands si falta `PaisId`
3. revisar si `domicilios` legacy implica:
   - varios domicilios generales del tercero
   - o solo puntos de entrega separados
4. si eran varios domicilios reales, diseñar agregado `TerceroDomicilio` sin duplicar `SucursalesEntrega`
5. exponer descripciones geográficas completas en detalle/listado

Prioridad: alta
Impacto: clientes, facturas, remitos, reportes

### B4. Cerrar cuenta corriente del cliente dentro de la ficha
Problema actual:
- el legacy distingue `límite de saldo`, `límite de crédito total` y vigencias desde/hasta
- hoy el backend solo muestra parte de eso mediante `LimiteCredito`, `SaldoMaximoVigente` y `VigenciaSaldo`

Tareas:
1. decidir modelo final de crédito cliente
2. mapear exactamente:
   - límite de saldo
   - vigencia límite saldo desde/hasta
   - límite crédito total
   - vigencia límite crédito total desde/hasta
3. extender persistencia si falta
4. exponer estos datos en DTO de detalle
5. validar consistencia con circuitos de venta y cuenta corriente

Riesgo:
- si no se modela ahora, luego se parchan reglas comerciales sobre campos ambiguos

### B5. Completar vendedor/cobrador y comisiones
Problema actual:
- el legacy tenía `aplica comisión` separado del porcentaje
- hoy solo existen ids y porcentajes

Tareas:
1. verificar si `aplica comisión` puede inferirse correctamente desde `id != null && porcentaje > 0`
2. si no, agregar flags explícitos a persistencia/DTO/commands:
   - `AplicaComisionVendedor`
   - `AplicaComisionCobrador`
3. exponer descripción/nombre/legajo del vendedor y cobrador en detalle y listado según necesidad de la UX
4. agregar validaciones de consistencia:
   - no permitir porcentaje sin flag o responsable asignado
   - no permitir flag activo sin responsable según regla elegida

### B6. Completar persona física extendida
Problema actual:
- `frmCliente` maneja más datos de persona física que el backend no soporta aún

Campos detectados faltantes:
- `Tratamiento`
- `Profesion`
- `EstadoCivil`
- `Nacionalidad`
- `Sexo`
- `FechaNacimiento`

Tareas:
1. decidir cuáles deben persistirse en `Tercero`
2. decidir cuáles salen de catálogos y cuáles son texto libre
3. agregar campos a entidad, configuración EF, scripts SQL y DTOs
4. ampliar reglas de `TerceroFiscalRules` y validadores para persona física
5. exponerlos en create/update/detail

Prioridad: media/alta
Condición: obligatoria si el front pretende reemplazar al legacy en ficha completa

### B7. Definir alcance de usuario/seguridad dentro de Clientes
Problema actual:
- el legacy muestra bloque de usuario, grupo y contraseña dentro de la ficha
- el backend actual separa `UsuariosController` y seguridad en otro módulo

Tareas:
1. decidir funcionalmente si eso sigue perteneciendo a `Ventas > Clientes`
2. si no pertenece, documentar explícitamente la separación
3. si sí pertenece, definir integración controlada entre `Tercero` y `Usuario`
4. evitar mezclar responsabilidad comercial con autenticación salvo evidencia clara del legado de negocio actual

Resultado esperado:
- decisión formal para que el front no reintroduzca campos sin backend o acoplamiento indebido

### B8. Formalizar descuentos por cliente
Problema actual:
- el legacy muestra `% máximo de descuento`
- la sección de descuentos en ventas sigue incompleta

Tareas:
1. decidir si `% máximo de descuento` es propiedad del cliente o regla comercial externa
2. si es propia del cliente, agregarla a `Tercero`
3. si depende de motor de descuentos, exponerla como dato calculado/derivado
4. enlazar esta decisión con backlog de `Ventas > Descuentos`

Decisión operativa actual:
- `% máximo de descuento` queda como propiedad persistida de `Tercero`
- actúa como tope comercial para descuentos manuales aplicados en ventas
- `DescuentoComercial` queda separado como regla explícita por `cliente + item + vigencia`
- mientras no exista motor integral, el backend debe validar el tope del cliente al crear o emitir comprobantes de venta

### B9. Revisar catálogos auxiliares de cliente
Catálogos que deben quedar claros para la ficha:
- categoría general de tercero
- categoría cliente
- estado cliente
- moneda
- localidad
- barrio
- país
- zona comercial
- cobrador
- vendedor
- condición IVA
- tipo documento
- cualquier catálogo de persona física que se incorpore

Tareas:
1. revisar qué combos del front ya tienen endpoint real
2. agregar endpoints faltantes o ampliar contratos actuales
3. priorizar endpoint agregado de configuración si la vista hoy hace demasiadas llamadas dispersas

Sugerencia:
- evaluar un endpoint `GET /api/terceros/configuracion-clientes` con catálogos específicos de la ficha

### B10. Mejorar listado operativo de clientes
Problema actual:
- el grid nuevo puede necesitar más columnas/flags para cubrir al legacy

Tareas:
1. revisar columnas realmente visibles/esperadas en la vista nueva y en `frmCliente`
2. ampliar `TerceroListDto` si faltan:
   - estado operativo consolidado
   - país/sucursal/categorías si la vista los usa
   - vendedor/cobrador visible
   - alertas comerciales simples
3. revisar filtros backend adicionales:
   - por vendedor
   - por cobrador
   - por zona
   - por estado operativo consolidado
   - por cliente bloqueado/no bloqueado

### B11. Importación de clientes
Problema actual:
- existe referencia legacy a importador de clientes
- no está decidido si será necesario en reemplazo real

Tareas:
1. validar si el negocio actual aún requiere importación masiva
2. si sí, definir comando/servicio de importación
3. diseñar validación, auditoría y reportes de errores
4. decidir formato de entrada y endpoint asociado

### B12. Auditoría funcional de cliente
Objetivo:
- que la ficha de clientes sea auditable y trazable

Tareas:
1. identificar acciones críticas:
   - alta
   - edición
   - baja/reactivación
   - cambio de estado cliente
   - cambio de datos fiscales
   - cambio de límites/comisiones
2. validar que `created_at`, `updated_at`, `created_by`, `updated_by` alcancen
3. si no alcanza, preparar historial explícito por eventos de cliente

---

## 3. Orden de implementación recomendado

### Ola C1 — Cierre del contrato base
1. auditar `TerceroDto` / `TerceroListDto`
2. definir campos faltantes obligatorios
3. resolver geografía/pais
4. resolver estado operativo consolidado

### Ola C2 — Comercial y cuenta corriente
5. modelar límites y vigencias de cuenta corriente
6. cerrar vendedor/cobrador/comisiones
7. formalizar `% máximo descuento` o derivación equivalente

### Ola C3 — Persona física extendida
8. agregar tratamiento/profesión/estado civil/nacionalidad/sexo/fecha nacimiento
9. ampliar validaciones de persona física

### Ola C4 — Integración y configuración de ficha
10. decidir alcance usuario/seguridad
11. consolidar catálogos de ficha cliente
12. evaluar endpoint agregado de configuración clientes

### Ola C5 — Operación avanzada
13. ampliar filtros/listado
14. decidir importación masiva
15. definir auditoría ampliada si hace falta

---

## 4. Checklist técnico de aceptación
Antes de cerrar `Ventas > Clientes`, validar:
- el backend cubre todos los campos visibles en `frmCliente`
- el front no necesita IDs manuales ni textos provisorios para completar la ficha
- todos los combos tienen fuente backend real
- create/update/detail/listado representan la misma realidad funcional
- estado cliente bloqueante y activo general quedan claramente diferenciados
- reglas de persona física/jurídica son consistentes con el legado requerido
- límites, comisiones, contactos, transportes, sucursales y ventanas funcionan desde backend real
- la vista no depende de overlays locales
- smoke API cubre alta, edición, lectura, catálogos y bloques agregados

## 5. Siguiente acción recomendada
Comenzar por `B1 + B2 + B3`.

Motivo:
- permiten cerrar rápidamente el contrato maestro de cliente
- destraban casi todas las demás tareas de Ventas
- evitan que el frontend siga modelando faltantes de forma implícita
