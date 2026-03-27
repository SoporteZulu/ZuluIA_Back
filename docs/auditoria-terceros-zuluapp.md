# Auditoría funcional de `Terceros` contra `zuluApp`

## Referencia principal
El comportamiento de `C:\Zulu\zuluApp` es la base funcional para el reemplazo con `ZuluIA_Back` + `ZuluIA_Front`.

## Hallazgos confirmados

### 1. Alta de terceros
En `zuluApp` el alta es más flexible que en el backend nuevo original:
- `Legajo` puede autogenerarse.
- El documento se valida por duplicado solo si fue informado.
- La carga de cliente/proveedor no depende de que el frontend envíe todos los campos históricos.

### 2. Datos visibles en `frmCliente`
En `zuluApp` la pantalla de clientes muestra o trabaja con estos grupos de datos:
- Identificación: `Legajo`, `Nro Interno`, `Personería`, `Razón Social`, `Nombre Comercial`, `Apellido`, `Nombre`, `Tipo/Nro Doc`.
- Fiscal: `Condición Fiscal`, `Clave Fiscal`, `Valor Clave Fiscal`, `Nro Ingresos Brutos`, `Nro Municipalidad`, facturable, mínimo MiPyme.
- Ubicación/contacto: domicilio, CP, barrio, teléfono, mail, sucursal, país, categoría.
- Comercial: moneda, rubro, sector, zona, condición de venta, plazo de cobro, vendedor, cobrador, comisiones, límite de crédito, período de vigencia.
- Extras: observación, foto, usuario, grupo, contraseña, datos resumidos y avanzados.

### 3. Frontend actual (`ventas/clientes`)
`ZuluIA_Front` hoy mezcla dos cosas:
- datos reales del backend (`/api/terceros`)
- un bloque `legacy` local en `localStorage`

Ese bloque local contiene campos como:
- `rubro`
- `subrubro`
- `condicionCobranza`
- `transportes`
- `ventanasCobranza`
- `sucursales`
- `contactos`
- `observacionLegacy`

Eso **no reemplaza** la funcionalidad real de `zuluApp`; solo simula parte de la UX.

## Ajustes aplicados en backend

### Perfil comercial ampliado
Se implementó la primera capa de migración funcional para terceros:
- tabla `terceros_perfiles_comerciales`
- `GET /api/terceros/{id}/perfil-comercial`
- `PUT /api/terceros/{id}/perfil-comercial`
- inclusión del `PerfilComercial` dentro de `GET /api/terceros/{id}`

Campos soportados en esta etapa:
- `zonaComercialId`
- `rubro`
- `subrubro`
- `sector`
- `condicionCobranza`
- `riesgoCrediticio`
- `saldoMaximoVigente`
- `vigenciaSaldo`
- `condicionVenta`
- `plazoCobro`
- `facturadorPorDefecto`
- `minimoFacturaMipymes`
- `observacionComercial`

### Contactos múltiples
Se implementó la segunda capa de migración funcional para terceros:
- tabla `terceros_contactos`
- `GET /api/terceros/{id}/contactos`
- `PUT /api/terceros/{id}/contactos`
- inclusión de `Contactos` dentro de `GET /api/terceros/{id}`

Campos soportados en esta etapa:
- `nombre`
- `cargo`
- `email`
- `telefono`
- `sector`
- `principal`
- `orden`

### Sucursales / puntos de entrega
Se implementó la tercera capa de migración funcional para terceros:
- tabla `terceros_sucursales_entrega`
- `GET /api/terceros/{id}/sucursales-entrega`
- `PUT /api/terceros/{id}/sucursales-entrega`
- inclusión de `SucursalesEntrega` dentro de `GET /api/terceros/{id}`

Campos soportados en esta etapa:
- `descripcion`
- `direccion`
- `localidad`
- `responsable`
- `telefono`
- `horario`
- `principal`
- `orden`

### Transportes
Se implementó la cuarta capa de migración funcional para terceros:
- tabla `terceros_transportes`
- `GET /api/terceros/{id}/transportes`
- `PUT /api/terceros/{id}/transportes`
- inclusión de `Transportes` dentro de `GET /api/terceros/{id}`

Campos soportados en esta etapa:
- `transportistaId`
- `transportistaNombre`
- `nombre`
- `servicio`
- `zona`
- `frecuencia`
- `observacion`
- `activo`
- `principal`
- `orden`

### Ventanas de cobranza
Se implementó la quinta capa de migración funcional para terceros:
- tabla `terceros_ventanas_cobranza`
- `GET /api/terceros/{id}/ventanas-cobranza`
- `PUT /api/terceros/{id}/ventanas-cobranza`
- inclusión de `VentanasCobranza` dentro de `GET /api/terceros/{id}`

Campos soportados en esta etapa:
- `dia`
- `franja`
- `canal`
- `responsable`
- `principal`
- `orden`

### Reglas fiscales / personería
Se implementó una primera capa de paridad funcional para terceros:
- columnas en `terceros` para `tipo_personeria`, `nombre`, `apellido`, `es_entidad_gubernamental`, `clave_fiscal`, `valor_clave_fiscal`
- validación de `JURIDICA` / `FISICA`
- exigencia de `nombre` y `apellido` para persona física
- bloqueo de `entidad gubernamental` para persona física
- consistencia obligatoria entre `claveFiscal` y `valorClaveFiscal`

Esto no reemplaza todavía toda la lógica fiscal legacy de `zuluApp`, pero deja una base persistida y validada para continuar.

Se agregó una segunda capa básica de reglas fiscales:
- si la condición IVA es `RESPONSABLE INSCRIPTO`, `IVA EXENTO` o `MONOTRIBUTO`, se exige clave fiscal + valor de clave fiscal
- para esas condiciones, el documento debe tener 11 dígitos y ser CUIT/CUIL
- para `CONSUMIDOR FINAL` sin documento, el tipo de documento debe ser `CONSUMIDOR FINAL`
- se bloquea DNI de 11 dígitos
- se bloquea persona jurídica con DNI informado

### Compatibilidad de alta
Se implementó:
- `Legajo` opcional y autogenerado si no viene.
- `TipoDocumentoId` opcional e inferido en backend.
- `NroDocumento` opcional.
- `NombreFantasia` con fallback a `RazonSocial` cuando llega vacío.

### Compatibilidad de listado
Se amplió el DTO paginado de terceros para incluir campos que el frontend ya consume en detalle/overlay:
- `CondicionIvaId`
- `Web`
- domicilio aplanado (`Calle`, `Nro`, `Piso`, `Dpto`, `CodigoPostal`, `LocalidadId`, `BarrioId`)
- `NroIngresosBrutos`, `NroMunicipal`
- `MonedaId`, `CategoriaId`
- `CobradorId`, `PctComisionCobrador`
- `VendedorId`, `PctComisionVendedor`
- `Observacion`
- `CreatedAt`
- `Legajo`

## Faltantes reales en backend respecto de `zuluApp`

### Alta / mantenimiento
Todavía faltan reglas legacy de mayor fidelidad:
- distinguir formalmente persona física / jurídica
- reglas de obligatoriedad distintas según personería
- validaciones fiscales más completas
- validación/gestión de clave fiscal
- validación de domicilio mínimo obligatorio
- soporte de múltiples domicilios/contactos/sucursales relacionadas al tercero
- configuración comercial más rica (rubro, sector, zona, condición venta, plazo cobro, vigencia)

### Modelo de datos
Faltan estructuras persistidas para soportar datos que el frontend hoy guarda localmente:
- contactos múltiples
- sucursales o puntos de entrega del tercero
- transportes asociados
- ventanas de cobranza
- perfil comercial ampliado (rubro/subrubro/zona/condición de cobranza/riesgo)

## Qué debe pedirle el backend al frontend

### Cambios mínimos inmediatos
1. En `ventas/clientes`, al abrir detalle o edición, dejar de depender solo de la fila de grilla y consultar también:
   - `GET /api/terceros/{id}`
2. Extender `lib/types/terceros.ts` para incluir al menos:
   - `legajo`
   - `tipoDocumentoId`
   - `tipoDocumentoDescripcion`
   - `barrioDescripcion`
   - `categoriaDescripcion`
   - `monedaDescripcion`
   - `cobradorNombre`
   - `vendedorNombre`
   - `sucursalId`
   - `sucursalDescripcion`
   - `createdAt`
3. Mostrar `Legajo` en la ficha/listado, porque en `zuluApp` es clave funcional.
4. Reemplazar el texto `legacy/legado` cuando en realidad son datos funcionales que deberían persistirse en backend.

### Cambios funcionales de reemplazo real
El frontend debería migrar el bloque hoy llamado `legacy` a módulos persistidos o a nuevos endpoints backend para:
- contactos múltiples
- sucursales del cliente
- transportes asociados
- ventanas de cobranza
- perfil comercial ampliado

## Sobre el uso del término `legacy` en `ZuluIA_Front`
Hoy está usado como **overlay local** o simulación UX.
Eso sirve para prototipar, pero no representa reemplazo real de `zuluApp`.

### Recomendación
- Si el dato existe de verdad en negocio: no llamarlo `legacy`, llamarlo por dominio real.
- Reservar `legacy` solo para compatibilidad temporal o lectura de sistemas viejos.

## Prioridad recomendada
1. Persistir y exponer en backend el perfil comercial ampliado del tercero.
2. Persistir contactos múltiples.
3. Persistir sucursales/puntos de entrega.
4. Persistir transportes y ventanas de cobranza.
5. Alinear validaciones de personería/fiscalidad con `zuluApp`.
