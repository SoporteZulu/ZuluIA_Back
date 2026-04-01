# Matriz de Paridad Ventas: Clientes y Productos en 2 días

**Fecha:** 2026-03-31  
**Objetivo:** cerrar la paridad operativa mínima de `Clientes` y `Productos` para la sección de ventas en `ZuluIA_Back` en una ventana de **2 días** con **2 equipos**.

---

## 1. Diagnóstico realista

## Estado actual backend

### Clientes (`Terceros`)
El backend ya tiene una base fuerte para ventas:
- `GET /api/terceros` con filtros combinables
- `GET /api/terceros/{id}`
- `GET /api/terceros/legajo/{legajo}`
- `GET /api/terceros/clientes-activos`
- `GET /api/terceros/configuracion-clientes`
- catálogos de estados/categorías
- `CreateTercero`, `UpdateTercero`, `DeleteTercero`
- detalle ampliado: domicilios, contactos, perfil comercial, sucursales de entrega, transportes, ventanas de cobranza

### Productos (`Items`)
El backend también está avanzado:
- `GET /api/items`
- `GET /api/items/{id}`
- `GET /api/items/por-codigo/{codigo}`
- `GET /api/items/por-codigo-barras/{codigoBarras}`
- `GET /api/items/{id}/precio`
- `GET /api/items/{id}/stock`
- `POST /api/items`
- `PUT /api/items/{id}`
- `PATCH /api/items/{id}/precios`
- `DELETE /api/items/{id}`

## Conclusión
En **2 días** no conviene plantear una paridad total administrativa. Sí conviene cerrar la **paridad comercial mínima usable por ventas**:
- selección de cliente
- validación comercial del cliente
- selección de producto
- validación comercial y stock del producto
- filtros y campos que usa ventas para operar

---

## 2. Qué significa “cerrar paridad” en esta ventana

## Alcance recomendado de 2 días
Se considera **cerrado para ventas** si quedan cubiertos estos 2 frentes:

### A. Clientes para ventas
1. Búsqueda rápida por:
   - `Id`
   - `Legajo`
   - `Razón social`
   - `CUIT / documento`
2. Selector de clientes activos usable desde pedidos, remitos y facturas
3. Exposición de datos comerciales mínimos:
   - condición IVA
   - estado cliente
   - si bloquea o no
   - límite de crédito / saldo si aplica
   - vendedor / cobrador
   - domicilios / sucursales de entrega
4. Validación para no usar clientes inactivos o bloqueados en ventas
5. Configuración/catálogos necesarios para formularios de cliente

### B. Productos para ventas
1. Búsqueda rápida por:
   - `Id`
   - `Código`
   - `Código de barras`
   - `Descripción`
2. Selector de productos usable desde pedidos, remitos y facturas
3. Exposición de datos comerciales mínimos:
   - unidad
   - IVA venta
   - precio venta
   - porcentaje máximo descuento
   - stock físico / disponible / comprometido / reservado
   - si aplica ventas
   - si maneja stock
4. Validación para no usar productos inactivos o no vendibles
5. Endpoint de stock y precio consistente para renglones de ventas

---

## 3. Gaps prioritarios detectados para foco ventas

## Clientes
### Ya cubierto
- listado paginado fuerte
- detalle fuerte
- catálogos fuertes
- combos de clientes activos
- datos comerciales amplios en `TerceroListDto`

### Gap de 2 días
- verificar que `GET /api/terceros` cubra exactamente lo que ventas necesita como grilla/selectores
- validar si falta búsqueda exacta por documento desde endpoint dedicado
- validar si falta endpoint resumido para selector comercial liviano
- endurecer validación de uso en ventas: cliente bloqueado / inactivo / no facturable
- dejar testeado el contrato que consumirá frontend ventas

## Productos
### Ya cubierto
- listado paginado
- detalle
- búsqueda por código y código de barras
- stock por depósito
- precio resuelto por lista
- CRUD base

### Gap de 2 días
- verificar filtros de ventas sobre `items`:
  - solo activos
  - solo productos
  - solo vendibles
  - solo con stock cuando aplique
- validar que el listado exponga bien banderas comerciales (`AplicaVentas`, `ManejaStock`, `EsProducto`)
- endurecer validación de uso en ventas: item inactivo / no vendible
- dejar testeado el contrato de selector comercial y consulta rápida de stock/precio

---

## 4. Matriz operativa de 2 días con 2 equipos

## Regla de WIP
- máximo `2 frentes activos grandes` al mismo tiempo
- cada equipo puede abrir **microtareas paralelas** solo si no rompe foco
- objetivo: terminar **ventas usable**, no ABM perfecto

## Equipo 1: Clientes ventas
**Objetivo:** dejar cerrada la paridad comercial mínima de clientes para pedidos/remitos/facturas.

### Día 1 AM
- relevar contrato actual de `TercerosController`
- validar listado `GET /api/terceros` contra necesidades de ventas
- identificar faltantes exactos de filtros/campos
- revisar `GetClientesActivosQuery` y `GetConfiguracionClientesTercerosQuery`

### Día 1 PM
- implementar ajustes mínimos de backend necesarios
- priorizar:
  1. selector liviano de clientes
  2. validación comercial de cliente usable por ventas
  3. búsqueda rápida faltante si aplica
- agregar/ajustar tests unitarios y/o controller tests

### Día 2 AM
- probar escenarios de ventas:
  - cliente activo
  - cliente inactivo
  - cliente bloqueado
  - cliente no facturable
  - cliente con sucursales de entrega
- documentar contrato final para frontend ventas

### Día 2 PM
- cerrar tests
- validar build
- dejar checklist de consumo frontend
- dejar issues residuales fuera de ventana

### Entregables de Equipo 1
- endpoints de clientes consolidados para ventas
- tests verdes
- documento de contrato de consumo
- backlog residual no crítico separado

---

## Equipo 2: Productos ventas
**Objetivo:** dejar cerrada la paridad comercial mínima de productos para pedidos/remitos/facturas.

### Día 1 AM
- relevar contrato actual de `ItemsController`
- validar `GET /api/items`, `GET /api/items/{id}/precio`, `GET /api/items/{id}/stock`
- identificar faltantes exactos para selector de renglones de venta

### Día 1 PM
- implementar ajustes mínimos de backend necesarios
- priorizar:
  1. filtros de ventas
  2. validación de item vendible
  3. consistencia precio/stock
  4. selector rápido por código/barra
- agregar/ajustar tests unitarios y/o controller tests

### Día 2 AM
- probar escenarios de ventas:
  - item activo
  - item inactivo
  - item no vendible
  - item con stock
  - item sin stock
  - item con lista de precio
  - item por código de barras

### Día 2 PM
- cerrar tests
- validar build
- dejar checklist de consumo frontend
- dejar issues residuales fuera de ventana

### Entregables de Equipo 2
- endpoints de productos consolidados para ventas
- tests verdes
- contrato de precio/stock/selector validado
- backlog residual no crítico separado

---

## 5. Matriz de tareas detallada

| Frente | Task | Prioridad | Tipo | Resultado esperado |
|---|---|---:|---|---|
| Clientes | validar filtros reales de ventas en `GET /api/terceros` | Alta | análisis | saber si ya cubre grilla comercial |
| Clientes | consolidar selector liviano de clientes activos | Alta | backend | contrato simple para combos/autocomplete |
| Clientes | endurecer validación de cliente usable en ventas | Alta | backend | no operar con bloqueados/inactivos |
| Clientes | testear casos comerciales críticos | Alta | test | cobertura de uso real |
| Clientes | documentar contrato para frontend ventas | Media | doc | consumo sin ambigüedad |
| Productos | validar filtros reales de ventas en `GET /api/items` | Alta | análisis | saber si ya cubre selector comercial |
| Productos | consolidar consulta rápida código/barra | Alta | backend | alta velocidad para renglones |
| Productos | endurecer validación de item usable en ventas | Alta | backend | no operar con no vendibles/inactivos |
| Productos | validar precio/stock de uso comercial | Alta | backend | contrato consistente por ítem |
| Productos | testear escenarios críticos de venta | Alta | test | cobertura de uso real |
| Productos | documentar contrato para frontend ventas | Media | doc | consumo sin ambigüedad |

---

## 6. Qué NO entra en 2 días

No meter en esta ventana:
- ABM completo de mantenimiento fino de clientes
- ABM completo de mantenimiento fino de productos
- mejoras cosméticas de DTOs sin impacto en ventas
- catálogos administrativos secundarios
- refactors amplios
- front nuevo completo
- paridad total de reportes o fichas extendidas no usadas por ventas

---

## 7. Criterio de éxito al final del día 2

## Clientes queda “OK ventas” si:
- existe búsqueda confiable de cliente para comprobantes
- el backend devuelve estado comercial suficiente
- el backend permite decidir si el cliente puede operar
- están cubiertos tests de clientes bloqueados/inactivos/facturables

## Productos queda “OK ventas” si:
- existe búsqueda confiable por código/barra/descripcion
- el backend devuelve precio y stock utilizable
- el backend permite decidir si el ítem puede venderse
- están cubiertos tests de productos activos/no vendibles/sin stock

---

## 8. Cómo usar múltiples requests a Copilot por máquina

## Sí, cuenta mucho
**Sí**, hay que contemplarlo. Tener múltiples requests a Copilot por máquina **sí aumenta el throughput**, pero no multiplica linealmente la capacidad del equipo.

## Cómo aprovecharlo bien
Cada equipo puede trabajar como **1 carril humano + 3 o 4 carriles asistidos**:

### Carril humano principal
- decide prioridad
- revisa diffs
- integra cambios
- ejecuta tests
- valida funcionalidad

### Carriles asistidos por Copilot
- request 1: análisis de gaps sobre archivo/controlador puntual
- request 2: propuesta de tests de un handler o controller
- request 3: documentación de contrato / matriz / checklist
- request 4: revisión puntual de validaciones o DTOs

## Ejemplo por equipo
### Equipo 1 - Clientes
- request A: “revisá `TercerosController` y decime qué endpoints sirven para selector de ventas”
- request B: “compará `GetTercerosPagedQuery` con los filtros legacy y listá gaps”
- request C: “proponé tests para cliente bloqueado/no facturable”
- request D: “armá checklist funcional para pedidos/remitos/facturas”

### Equipo 2 - Productos
- request A: “revisá `ItemsController` y decime contrato mínimo para selector comercial”
- request B: “revisá `GetItemsPagedQueryHandler` y detectá campos faltantes para ventas”
- request C: “proponé tests para item inactivo/no vendible/sin stock”
- request D: “armá checklist funcional de precio/stock por ítem”

## Regla importante
No usar múltiples requests para que cada uno haga cambios en paralelo sobre los **mismos archivos** sin coordinación. Eso genera:
- solapamiento
- diffs incompatibles
- ruido de merge
- pérdida de tiempo

## Mejor práctica
Usar múltiples requests en paralelo para:
- análisis
- pruebas
- documentación
- validaciones
- pequeños cambios aislados

No usarlos para:
- editar simultáneamente el mismo controller/handler/configuration

---

## 9. Modelo recomendado de ejecución con 2 equipos y Copilot

## Equipo 1
- humano: integra `clientes`
- request paralelo 1: análisis de controller/query
- request paralelo 2: tests
- request paralelo 3: checklist/documentación

## Equipo 2
- humano: integra `productos`
- request paralelo 1: análisis de controller/query
- request paralelo 2: tests
- request paralelo 3: checklist/documentación

## Resultado práctico
No tienes `7 matrices` en paralelo real, pero sí:
- `2 matrices activas reales`
- `6 a 8 microfrentes asistidos` por requests simultáneos

Eso sí es realista en 2 días.

---

## 10. Plan horario sugerido

| Día | Equipo 1 | Equipo 2 |
|---|---|---|
| Día 1 AM | relevamiento clientes ventas | relevamiento productos ventas |
| Día 1 PM | ajustes backend + tests clientes | ajustes backend + tests productos |
| Día 2 AM | validación funcional clientes | validación funcional productos |
| Día 2 PM | cierre + build + documentación | cierre + build + documentación |

---

## 11. Recomendación final

**Sí lo veo posible**, pero solo si redefinimos el objetivo a:

> **“paridad comercial mínima usable por ventas”**

No a:

> **“paridad total administrativa de clientes y productos”**

Con 2 días y 2 equipos, la apuesta correcta es:
1. cerrar backend de clientes para selección y validación comercial
2. cerrar backend de productos para selección, precio y stock
3. dejar documentado el contrato para que el frontend consuma sin ambigüedad
4. capturar residual en backlog separado

---

## 12. Backlog residual recomendado

Dejar fuera de la ventana y pasar a siguiente iteración:
- mejoras avanzadas de ficha de cliente
- atributos secundarios no usados por ventas
- variantes/composición avanzada de productos si no impacta venta inmediata
- optimizaciones no críticas
- enriquecimientos de reportes y dashboards

---

## 13. Decisión ejecutiva sugerida

### Si el objetivo es llegar sí o sí en 2 días
Aprobar esta estrategia:
- `Equipo 1 = Clientes ventas`
- `Equipo 2 = Productos ventas`
- `Copilot = múltiples requests por equipo para análisis/tests/docs`
- `Scope = solo backend comercial mínimo usable`

### Si el objetivo es paridad total
No entra en 2 días. Ahí conviene:
- 2 días para `OK ventas`
- 3 a 5 días más para cierre administrativo fino

---

**Última actualización:** 2026-03-31  
**Versión:** 1.0
