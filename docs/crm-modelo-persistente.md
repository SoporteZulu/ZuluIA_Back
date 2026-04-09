# Modelo persistente CRM

## Alcance

Este documento resume el estado actual del modelo persistente del módulo CRM en `ZuluIA_Back`, tomando como fuente el código de:

- `src/ZuluIA_Back.Domain/Entities/CRM`
- `src/ZuluIA_Back.Infrastructure/Persistence/Configurations`
- `src/ZuluIA_Back.Infrastructure/Persistence/AppDbContext.cs`

El objetivo es dejar trazabilidad del diseño backend actual antes de una validación real contra PostgreSQL local y antes de introducir migraciones explícitas.

## Estado general

- El módulo CRM ya tiene `DbSet` explícitos en `IApplicationDbContext` y `AppDbContext`.
- Las entidades CRM principales tienen configuración EF Core dedicada.
- El modelo usa `AuditableEntity` como base común para gran parte del módulo.
- No se encontraron migraciones CRM dentro del workspace actual.
- La validación de persistencia debe hacerse contra PostgreSQL local en `localhost:5432`.

## Entidades principales

| Entidad | Tabla | Propósito |
|---|---|---|
| `CrmCliente` | `CRMCLIENTES` | Perfil CRM de un tercero/cliente |
| `CrmContacto` | `CRMCONTACTOS` | Contactos asociados al cliente CRM |
| `CrmOportunidad` | `CRMOPORTUNIDADES` | Pipeline comercial por cliente |
| `CrmInteraccion` | `CRMINTERACCIONES` | Bitácora de interacciones CRM |
| `CrmTarea` | `CRMTAREAS` | Tareas operativas comerciales |
| `CrmSegmento` | `CRMSEGMENTOS` | Segmentos estáticos o dinámicos |
| `CrmSegmentoMiembro` | `CRMSEGMENTOS_MIEMBROS` | Membresía manual para segmentos estáticos |
| `CrmCampana` | `CRMCAMPANAS` | Campañas de marketing CRM |
| `CrmSeguimiento` | `CRMSEGUIMIENTOS` | Seguimientos comerciales |
| `CrmComunicado` | `CRMCOMUNICADOS` | Comunicados asociados a clientes/campañas |
| `CrmUsuarioPerfil` | `CRMUSUARIOS` | Perfil CRM del usuario |
| `TipoRelacionContactoCatalogo` | `tipos_relaciones_contacto` | Catálogo semántico para `TipoRelacionId` en contactos legacy |
| `Contacto` | `CONTACTOS` | Relación legacy entre dos personas expuesta por `api/crm/relaciones-contacto` |
| `CrmMotivo` | `CRMMOTIVOS` | Catálogo de motivos |
| `CrmInteres` | `CRMINTERESES` | Catálogo de intereses |
| `CrmTipoComunicado` | `CRMTIPOCOMUNICADOS` | Catálogo de tipos de comunicado |

## Relaciones lógicas del módulo

> Nota: la mayoría de estas relaciones hoy se resuelven por ids escalares e invariantes de dominio. No todas están modeladas como navegación EF.

- `CrmCliente.TerceroId` -> `Tercero.Id`
- `CrmCliente.ResponsableId` -> `Usuario.Id`
- `CrmContacto.ClienteId` -> `CrmCliente.TerceroId`
- `CrmOportunidad.ClienteId` -> `CrmCliente.TerceroId`
- `CrmOportunidad.ContactoPrincipalId` -> `CrmContacto.Id`
- `CrmOportunidad.ResponsableId` -> `Usuario.Id`
- `CrmInteraccion.ClienteId` -> `CrmCliente.TerceroId`
- `CrmInteraccion.ContactoId` -> `CrmContacto.Id`
- `CrmInteraccion.OportunidadId` -> `CrmOportunidad.Id`
- `CrmInteraccion.UsuarioResponsableId` -> `Usuario.Id`
- `CrmTarea.ClienteId` -> `CrmCliente.TerceroId`
- `CrmTarea.OportunidadId` -> `CrmOportunidad.Id`
- `CrmTarea.AsignadoAId` -> `Usuario.Id`
- `CrmSegmentoMiembro.SegmentoId` -> `CrmSegmento.Id`
- `CrmSegmentoMiembro.ClienteId` -> `CrmCliente.TerceroId`
- `CrmCampana.SucursalId` -> `Sucursal.Id`
- `CrmCampana.SegmentoObjetivoId` -> `CrmSegmento.Id`
- `CrmCampana.ResponsableId` -> `Usuario.Id`
- `CrmSeguimiento.SucursalId` -> `Sucursal.Id`
- `CrmSeguimiento.TerceroId` -> `Tercero.Id`
- `CrmSeguimiento.MotivoId` -> `CrmMotivo.Id`
- `CrmSeguimiento.InteresId` -> `CrmInteres.Id`
- `CrmSeguimiento.CampanaId` -> `CrmCampana.Id`
- `CrmSeguimiento.UsuarioId` -> `Usuario.Id`
- `CrmComunicado.SucursalId` -> `Sucursal.Id`
- `CrmComunicado.TerceroId` -> `Tercero.Id`
- `CrmComunicado.CampanaId` -> `CrmCampana.Id`
- `CrmComunicado.TipoId` -> `CrmTipoComunicado.Id`
- `CrmComunicado.UsuarioId` -> `Usuario.Id`
- `CrmUsuarioPerfil.UsuarioId` -> `Usuario.Id`
- `Contacto.TipoRelacionId` -> `TipoRelacionContactoCatalogo.Id`
- `Contacto.PersonaId` -> `Tercero.Id`
- `Contacto.PersonaContactoId` -> `Tercero.Id`
- `VinculacionPersona.TipoRelacionId` -> `TipoRelacionContactoCatalogo.Id`

## Auditoría persistida

Las siguientes entidades tienen mapeadas columnas de auditoría:

- `CrmCliente`
- `CrmContacto`
- `CrmOportunidad`
- `CrmInteraccion`
- `CrmTarea`
- `CrmSegmento`
- `CrmSegmentoMiembro`
- `CrmCampana`
- `CrmUsuarioPerfil`
- `CrmComunicado`
- `CrmSeguimiento`
- `CrmMotivo`
- `CrmInteres`
- `CrmTipoComunicado`
- `TipoRelacionContactoCatalogo`

Columnas estandarizadas actualmente:

- `created_at`
- `updated_at`
- `deleted_at`
- `created_by`
- `updated_by`

## Índices operativos actuales

### Clientes y contactos

- `CRMCLIENTES`
  - único por `tercero_id`
  - índice por `responsable_id, estado_relacion`
- `CRMCONTACTOS`
  - índice por `cliente_id, activo`

### Pipeline y actividad

- `CRMOPORTUNIDADES`
  - índice por `cliente_id, activa`
  - índice por `responsable_id, etapa`
- `CRMINTERACCIONES`
  - índice por `cliente_id, fecha_hora`
  - índice por `oportunidad_id`
- `CRMTAREAS`
  - índice por `asignado_a_id, estado, fecha_vencimiento`
  - índice por `cliente_id`
  - índice por `oportunidad_id`

### Segmentación

- `CRMSEGMENTOS`
  - único por `nombre`
- `CRMSEGMENTOS_MIEMBROS`
  - único por `segmento_id, cliente_id`

### Campañas y seguimiento

- `CRMCAMPANAS`
  - índice por `sucursal_id, fecha_inicio`
  - índice por `segmento_objetivo_id`
- `CRMSEGUIMIENTOS`
  - índice por `sucursal_id, fecha`
  - índice por `tercero_id`
  - índice por `campana_id`
- `CRMCOMUNICADOS`
  - índice por `sucursal_id, fecha`
  - índice por `tercero_id`
  - índice por `campana_id`

### Catálogos

- `CRMMOTIVOS`
  - único por `codigo`
  - índice por `activo`
- `CRMINTERESES`
  - único por `codigo`
  - índice por `activo`
- `CRMTIPOCOMUNICADOS`
  - único por `codigo`
  - índice por `activo`
- `tipos_relaciones_contacto`
  - único por `codigo`
  - índice por `activo`
- `CONTACTOS`
  - índice por `id_persona`
  - índice por `id_persona, id_contacto`
- `CRMUSUARIOS`
  - único por `usuario_id`
  - índice por `rol, activo`

## Tipos de columnas relevantes

### JSONB

Se usan columnas `jsonb` en PostgreSQL para:

- `CRMSEGMENTOS.criterios_json`
- `CRMINTERACCIONES.adjuntos_json`

### Decimales

Montos y presupuestos se modelan con precisión:

- `CRMOPORTUNIDADES.monto_estimado` -> `numeric(18,2)`
- `CRMCAMPANAS.presupuesto` -> `numeric(18,2)`
- `CRMCAMPANAS.presupuesto_gastado` -> `numeric(18,2)`

## Reglas operativas que impactan persistencia

### Oportunidades

- una oportunidad cerrada queda terminal (`cerrado_ganado` o `cerrado_perdido`)
- `cerrado_ganado` fuerza probabilidad `100`
- `cerrado_perdido` fuerza probabilidad `0` y conserva `motivo_perdida`
- no se permite reasignar responsables sobre oportunidades ya cerradas

### Tareas

- las tareas soportan transición controlada entre `pendiente`, `en_curso`, `vencida` y `completada`
- `fecha_completado` solo queda persistida cuando el estado es `completada`
- reabrir una tarea limpia `fecha_completado`

### Segmentos

- `estatico` usa membresía manual persistida en `CRMSEGMENTOS_MIEMBROS`
- `dinamico` se resuelve por criterios JSON y no admite membresía manual

### Cliente CRM

- la baja de cliente aplica desactivación/cascada lógica sobre contactos, oportunidades y tareas relacionadas
- algunas colecciones relacionadas se resuelven por ids y lógica de aplicación, no por cascada declarativa de FK EF

## Observaciones de naming a revisar más adelante

Hay observaciones de naming heredadas que siguen requiriendo cuidado operativo:

- `CRMUSUARIOS` representa perfiles CRM, no necesariamente la tabla maestra de usuarios del sistema
- varias relaciones CRM usan `TerceroId` o `ClienteId` según el agregado, lo cual requiere cuidado al documentar joins
- `CRMTIPOCOMUNICADOS` mantiene un nombre histórico que no sigue una pluralización castellana perfecta, pero hoy se considera estable dentro del módulo

## Estado de migraciones

- no se encontraron migraciones EF Core específicas del módulo CRM dentro del workspace
- cualquier validación de esquema debe hacerse primero contra PostgreSQL local en `localhost:5432`
- antes de generar migraciones reales conviene cerrar:
  - naming definitivo de tablas CRM
  - FKs explícitas que se quieran materializar en base
  - estrategia de soft delete por entidad

## Recomendación operativa

Antes de emitir migraciones o scripts de despliegue del módulo CRM:

1. validar este modelo contra PostgreSQL local
2. validar en PostgreSQL local el impacto de las tablas CRM normalizadas (`CRMCAMPANAS`, `CRMINTERESES`, `CRMOPORTUNIDADES`)
3. decidir qué relaciones seguirán solo como integridad de aplicación y cuáles pasarán a FK explícita
4. recién después generar documentación de delta de esquema o migraciones controladas
