# Prompts para Copilot - Cierre de paridad de `Clientes` y `Productos` en 2 días

**Objetivo:** dejar listos prompts copy/paste para trabajar con `2 máquinas`, `2 equipos` y `múltiples requests en paralelo` sin pisarse.

**Scope recomendado:** backend de ventas en `ZuluIA_Back`, con `C:\Zulu\zuluApp` como referencia funcional principal.

---

## 1. Cómo usar este archivo

## Distribución recomendada por máquina
- **Máquina 1** → `Equipo Clientes`
- **Máquina 2** → `Equipo Productos`

## Regla de oro
En cada máquina:
- podés abrir **3 requests en paralelo de análisis**
- luego **1 request integrador**
- luego **1 request final de validación**

No mandes en paralelo dos requests que intenten editar los mismos archivos.

## Secuencia recomendada
### Ola 1 - Paralela
- análisis de gaps
- comparación contra legado
- propuesta de tests

### Ola 2 - Integración
- un único request que implemente cambios y tests

### Ola 3 - Cierre
- build, validación, residual y checklist

## Regla extra por historial no compartido
Como las distintas requests no comparten historial entre sí, antes de mandar `C4` o `P4` hay que pegar un **resumen consolidado manual** con lo que devolvieron las 3 requests de análisis.

Ese resumen tiene que incluir solo:
- hallazgos confirmados
- gaps prioritarios
- tests críticos a cubrir
- archivos probables a tocar
- decisiones de scope

No hay que pegar toda la conversación anterior. Solo una síntesis corta y estructurada.

---

## 2. Qué máquina usa cada prompt

| Máquina | Equipo | Ola 1 | Ola 2 | Ola 3 |
|---|---|---|---|---|
| `Máquina 1` | Clientes | `C1`, `C2`, `C3` | `C4` | `C5` |
| `Máquina 2` | Productos | `P1`, `P2`, `P3` | `P4` | `P5` |

---

## 3. Reglas para todos los prompts

Usá estos prompts tal cual o con mínimos ajustes. Todos ya vienen orientados a:
- usar `C:\Zulu\zuluApp` como referencia funcional
- cerrar primero backend
- validar contra PostgreSQL local si hace falta (`localhost:5432`)
- no abrir scope administrativo innecesario
- terminar `OK ventas`, no `paridad total`

## Plantilla de resumen consolidado para pegar antes de `C4` o `P4`

Usá esta plantilla como bloque previo en un chat nuevo antes del prompt integrador.

```text
Resumen consolidado de análisis previos:

1. Ya cubierto
- ...
- ...

2. Gaps confirmados
- ...
- ...

3. Prioridad de esta ventana
- ...
- ...

4. Tests críticos a cubrir
- ...
- ...

5. Archivos candidatos a tocar
- ...
- ...

6. Scope explícito
- Cerrar solo OK ventas
- No tocar frontend
- No abrir scope administrativo

Tomá este resumen como fuente de verdad para implementar.
```

## Plantilla corta para `C4`

```text
Resumen consolidado de C1/C2/C3:

Ya cubierto:
- ...

Gaps confirmados:
- ...

Tests críticos:
- ...

Archivos a tocar:
- ...

Scope:
- cerrar solo clientes OK ventas
- no tocar frontend
- cambios mínimos
```

## Plantilla corta para `P4`

```text
Resumen consolidado de P1/P2/P3:

Ya cubierto:
- ...

Gaps confirmados:
- ...

Tests críticos:
- ...

Archivos a tocar:
- ...

Scope:
- cerrar solo productos OK ventas
- no tocar frontend
- cambios mínimos
```

---

# MÁQUINA 1 - EQUIPO CLIENTES

## Objetivo del equipo
Cerrar la paridad **comercial mínima usable por ventas** de clientes para que `Pedidos`, `Remitos` y `Facturas` puedan:
- buscar cliente
- validar si puede operar
- traer datos comerciales mínimos
- resolver domicilios/sucursales de entrega

---

## C1 - Análisis de gaps de backend de clientes
**Cuándo mandarlo:** primero, en paralelo con `C2` y `C3`.

```text
Necesito que analices el backend de clientes/terceros para ventas en este workspace y me digas exactamente qué falta para dejarlo OK ventas en 2 días, sin ir a frontend.

Contexto obligatorio:
- `C:\Zulu\zuluApp` es la referencia funcional principal.
- Hay que priorizar backend-first.
- El objetivo NO es paridad administrativa total, sino paridad comercial mínima usable por ventas.
- Validar contra PostgreSQL local en `localhost:5432` solo si fuera necesario.

Quiero que revises específicamente estos archivos:
- `src/ZuluIA_Back.Api/Controllers/TercerosController.cs`
- `src/ZuluIA_Back.Application/Features/Terceros/Queries/GetTercerosPagedQuery.cs`
- `src/ZuluIA_Back.Application/Features/Terceros/Queries/GetTercerosPagedQueryHandler.cs`
- `src/ZuluIA_Back.Application/Features/Terceros/DTOs/TerceroListDto.cs`
- `src/ZuluIA_Back.Domain/Interfaces/ITerceroRepository.cs`
- `src/ZuluIA_Back.Infrastructure/Persistence/Repositories/TerceroRepository.cs`

Y los contrastes contra estos elementos del legado:
- `C:\Zulu\zuluApp\ASP\CLIENTES_Listado.asp`
- `C:\Zulu\zuluApp\ASP\ConsultarClientes_Listado.asp`
- `C:\Zulu\zuluApp\ASP\FichaDeCliente.asp`

Necesito como salida:
1. tabla de lo ya cubierto para ventas
2. tabla de gaps reales para ventas
3. priorización Alta/Media/Baja
4. recomendación concreta de cambios mínimos para cerrar en 2 días
5. lista de archivos exactos a tocar si hubiera que implementar

No implementes todavía. Solo análisis ejecutable y concreto.
```

---

## C2 - Diseño de tests críticos de clientes
**Cuándo mandarlo:** primero, en paralelo con `C1` y `C3`.

```text
Necesito que me diseñes los tests críticos para cerrar clientes de ventas en 2 días, sin implementar todavía.

Contexto:
- `C:\Zulu\zuluApp` es la referencia funcional.
- Scope: backend de ventas, no frontend.
- Objetivo: dejar clientes OK ventas, no ABM total.

Quiero que revises:
- `src/ZuluIA_Back.Api/Controllers/TercerosController.cs`
- `src/ZuluIA_Back.Application/Features/Terceros/Queries/GetTercerosPagedQueryHandler.cs`
- `src/ZuluIA_Back.Application/Features/Terceros/Queries/GetTerceroByIdQuery.cs`
- tests ya existentes relacionados con terceros si los encontrás

Necesito que me devuelvas:
1. lista de escenarios críticos de negocio para ventas
2. lista de tests unitarios recomendados
3. lista de tests de controller/API recomendados
4. qué casos edge cubrir sí o sí:
   - cliente inactivo
   - cliente bloqueado
   - cliente no facturable
   - cliente sin sucursal de entrega
   - cliente encontrado por legajo / razón social / documento
5. propuesta de nombres de tests y archivos de test

No implementes. Solo diseño de pruebas priorizado y corto.
```

---

## C3 - Comparación funcional legacy vs backend de clientes
**Cuándo mandarlo:** primero, en paralelo con `C1` y `C2`.

```text
Necesito una comparación puntual entre el legado y el backend actual para clientes orientado a ventas.

Referencia principal:
- `C:\Zulu\zuluApp`

Revisá especialmente:
- `C:\Zulu\zuluApp\ASP\CLIENTES_Listado.asp`
- `C:\Zulu\zuluApp\ASP\ConsultarClientes_Listado.asp`
- `C:\Zulu\zuluApp\ASP\FichaDeCliente.asp`

Y contrastalo con:
- `src/ZuluIA_Back.Api/Controllers/TercerosController.cs`
- `src/ZuluIA_Back.Application/Features/Terceros/DTOs/TerceroListDto.cs`
- `src/ZuluIA_Back.Application/Features/Terceros/Queries/GetTercerosPagedQuery.cs`

Quiero una salida enfocada en ventas:
1. qué campos visibles del legado ya están expuestos por backend
2. qué filtros del legado ya están cubiertos
3. qué datos comerciales usa ventas y faltan o están dudosos
4. qué endpoint debería consumir un frontend de ventas para selector de cliente
5. qué faltaría para considerar clientes en estado OK ventas

No implementes. Solo comparación concreta y accionable.
```

---

## C4 - Request integrador de clientes
**Cuándo mandarlo:** después de leer respuestas de `C1`, `C2` y `C3`.

**Importante:** mandarlo en un **chat nuevo** y pegar antes el `Resumen consolidado de C1/C2/C3`.

```text
Ahora quiero que implementes los cambios mínimos necesarios para dejar clientes OK ventas en este backend.

Objetivo:
Cerrar la paridad comercial mínima usable por ventas para clientes, sin ampliar scope administrativo.

Reglas obligatorias:
- `C:\Zulu\zuluApp` es la referencia funcional principal.
- Backend-first.
- Hacer cambios mínimos.
- Seguir el estilo actual del proyecto.
- Agregar o ajustar tests relevantes.
- Validar compilación al final.
- No tocar frontend.

Revisá e implementá sobre los archivos que correspondan, priorizando:
- `src/ZuluIA_Back.Api/Controllers/TercerosController.cs`
- `src/ZuluIA_Back.Application/Features/Terceros/Queries/GetTercerosPagedQuery.cs`
- `src/ZuluIA_Back.Application/Features/Terceros/Queries/GetTercerosPagedQueryHandler.cs`
- `src/ZuluIA_Back.Application/Features/Terceros/DTOs/TerceroListDto.cs`
- `src/ZuluIA_Back.Domain/Interfaces/ITerceroRepository.cs`
- `src/ZuluIA_Back.Infrastructure/Persistence/Repositories/TerceroRepository.cs`
- tests relacionados de terceros/API si corresponde

Priorizá estas capacidades:
1. selector comercial de clientes usable para ventas
2. búsqueda rápida por legajo / razón social / documento si falta
3. validación de cliente inactivo / bloqueado / no facturable
4. exponer datos comerciales mínimos necesarios para ventas
5. dejar pruebas mínimas cubriendo escenarios críticos

Al final:
- corré build
- listá qué quedó resuelto
- listá residual fuera de esta ventana
```

---

## C5 - Validación final de clientes
**Cuándo mandarlo:** último request de la máquina.

```text
Necesito que hagas una validación final del cierre de clientes para ventas en este backend.

Objetivo:
verificar si clientes ya quedó OK ventas o si todavía falta algo crítico.

Quiero que:
1. revises los cambios aplicados
2. identifiques riesgos funcionales remanentes
3. revises cobertura mínima de tests
4. corrobores que el build esté limpio
5. me dejes una checklist final tipo GO / NO GO para clientes ventas

La salida debe ser corta y ejecutiva:
- OK resuelto
- falta crítica
- falta no crítica
- recomendación final
```

---

# MÁQUINA 2 - EQUIPO PRODUCTOS

## Objetivo del equipo
Cerrar la paridad **comercial mínima usable por ventas** de productos para que `Pedidos`, `Remitos` y `Facturas` puedan:
- buscar producto
- resolver precio
- consultar stock
- validar si el ítem puede venderse

---

## P1 - Análisis de gaps de backend de productos
**Cuándo mandarlo:** primero, en paralelo con `P2` y `P3`.

```text
Necesito que analices el backend de productos/items para ventas en este workspace y me digas exactamente qué falta para dejarlo OK ventas en 2 días, sin tocar frontend.

Contexto obligatorio:
- `C:\Zulu\zuluApp` es la referencia funcional principal.
- Hay que priorizar backend-first.
- Objetivo: paridad comercial mínima usable por ventas, no paridad administrativa total.
- Validar contra PostgreSQL local en `localhost:5432` solo si fuera necesario.

Quiero que revises específicamente:
- `src/ZuluIA_Back.Api/Controllers/ItemsController.cs`
- `src/ZuluIA_Back.Application/Features/Items/Queries/GetItemsPagedQuery.cs`
- `src/ZuluIA_Back.Application/Features/Items/Queries/GetItemsPagedQueryHandler.cs`
- `src/ZuluIA_Back.Application/Features/Items/DTOs/ItemListDto.cs`
- `src/ZuluIA_Back.Domain/Interfaces/IItemRepository.cs`
- `src/ZuluIA_Back.Infrastructure/Persistence/Repositories/ItemRepository.cs`
- `src/ZuluIA_Back.Application/Features/Items/Queries/GetItemPrecioQuery.cs`
- `src/ZuluIA_Back.Application/Features/Items/Queries/GetItemPrecioQueryHandler.cs`

Contrastalo con el legado:
- `C:\Zulu\zuluApp\ASP\ITEMSNOFINANCIEROS_Listado.asp`
- `C:\Zulu\zuluApp\ASP\ITEMSNUEVOSTOCK_Listado.asp`
- `C:\Zulu\zuluApp\ASP\ITEMS_EditarForm.asp`

Necesito como salida:
1. tabla de lo ya cubierto para ventas
2. tabla de gaps reales para ventas
3. priorización Alta/Media/Baja
4. recomendación de cambios mínimos para cerrar en 2 días
5. lista de archivos exactos a tocar si hubiera que implementar

No implementes todavía. Solo análisis concreto.
```

---

## P2 - Diseño de tests críticos de productos
**Cuándo mandarlo:** primero, en paralelo con `P1` y `P3`.

```text
Necesito que me diseñes los tests críticos para cerrar productos/items de ventas en 2 días, sin implementar todavía.

Contexto:
- `C:\Zulu\zuluApp` es la referencia funcional.
- Scope: backend de ventas, no frontend.
- Objetivo: dejar productos OK ventas, no ABM total.

Quiero que revises:
- `src/ZuluIA_Back.Api/Controllers/ItemsController.cs`
- `src/ZuluIA_Back.Application/Features/Items/Queries/GetItemsPagedQueryHandler.cs`
- `src/ZuluIA_Back.Application/Features/Items/Queries/GetItemPrecioQueryHandler.cs`
- tests existentes de items si los encontrás

Necesito que me devuelvas:
1. lista de escenarios críticos de negocio para ventas
2. lista de tests unitarios recomendados
3. lista de tests de controller/API recomendados
4. qué casos edge cubrir sí o sí:
   - item inactivo
   - item no vendible
   - item sin stock
   - item con stock
   - item por código
   - item por código de barras
   - item con lista de precio
5. propuesta de nombres de tests y archivos de test

No implementes. Solo diseño de pruebas priorizado y corto.
```

---

## P3 - Comparación funcional legacy vs backend de productos
**Cuándo mandarlo:** primero, en paralelo con `P1` y `P2`.

```text
Necesito una comparación puntual entre el legado y el backend actual para productos orientado a ventas.

Referencia principal:
- `C:\Zulu\zuluApp`

Revisá especialmente:
- `C:\Zulu\zuluApp\ASP\ITEMSNOFINANCIEROS_Listado.asp`
- `C:\Zulu\zuluApp\ASP\ITEMSNUEVOSTOCK_Listado.asp`
- `C:\Zulu\zuluApp\ASP\ITEMS_EditarForm.asp`

Y contrastalo con:
- `src/ZuluIA_Back.Api/Controllers/ItemsController.cs`
- `src/ZuluIA_Back.Application/Features/Items/DTOs/ItemListDto.cs`
- `src/ZuluIA_Back.Application/Features/Items/Queries/GetItemsPagedQuery.cs`
- `src/ZuluIA_Back.Application/Features/Items/Queries/GetItemPrecioQueryHandler.cs`

Quiero una salida enfocada en ventas:
1. qué campos visibles del legado ya están expuestos por backend
2. qué filtros del legado ya están cubiertos
3. qué datos comerciales usa ventas y faltan o están dudosos
4. qué endpoint debería consumir un frontend de ventas para selector de producto
5. qué faltaría para considerar productos en estado OK ventas

No implementes. Solo comparación concreta y accionable.
```

---

## P4 - Request integrador de productos
**Cuándo mandarlo:** después de leer respuestas de `P1`, `P2` y `P3`.

**Importante:** mandarlo en un **chat nuevo** y pegar antes el `Resumen consolidado de P1/P2/P3`.

```text
Ahora quiero que implementes los cambios mínimos necesarios para dejar productos OK ventas en este backend.

Objetivo:
Cerrar la paridad comercial mínima usable por ventas para productos, sin ampliar scope administrativo.

Reglas obligatorias:
- `C:\Zulu\zuluApp` es la referencia funcional principal.
- Backend-first.
- Hacer cambios mínimos.
- Seguir el estilo actual del proyecto.
- Agregar o ajustar tests relevantes.
- Validar compilación al final.
- No tocar frontend.

Revisá e implementá sobre los archivos que correspondan, priorizando:
- `src/ZuluIA_Back.Api/Controllers/ItemsController.cs`
- `src/ZuluIA_Back.Application/Features/Items/Queries/GetItemsPagedQuery.cs`
- `src/ZuluIA_Back.Application/Features/Items/Queries/GetItemsPagedQueryHandler.cs`
- `src/ZuluIA_Back.Application/Features/Items/DTOs/ItemListDto.cs`
- `src/ZuluIA_Back.Domain/Interfaces/IItemRepository.cs`
- `src/ZuluIA_Back.Infrastructure/Persistence/Repositories/ItemRepository.cs`
- `src/ZuluIA_Back.Application/Features/Items/Queries/GetItemPrecioQueryHandler.cs`
- tests relacionados de items/API si corresponde

Priorizá estas capacidades:
1. selector comercial de productos usable para ventas
2. búsqueda rápida por código / código de barras / descripción
3. validación de item inactivo / no vendible
4. consistencia de precio y stock para renglones de venta
5. dejar pruebas mínimas cubriendo escenarios críticos

Al final:
- corré build
- listá qué quedó resuelto
- listá residual fuera de esta ventana
```

---

## P5 - Validación final de productos
**Cuándo mandarlo:** último request de la máquina.

```text
Necesito que hagas una validación final del cierre de productos para ventas en este backend.

Objetivo:
verificar si productos ya quedó OK ventas o si todavía falta algo crítico.

Quiero que:
1. revises los cambios aplicados
2. identifiques riesgos funcionales remanentes
3. revises cobertura mínima de tests
4. corrobores que el build esté limpio
5. me dejes una checklist final tipo GO / NO GO para productos ventas

La salida debe ser corta y ejecutiva:
- OK resuelto
- falta crítica
- falta no crítica
- recomendación final
```

---

# 4. Orden exacto de envío por máquina

## Máquina 1 - Clientes
### Ola 1 - mandar juntos
- `C1`
- `C2`
- `C3`

### Cuando respondan esos 3
- armar `Resumen consolidado de C1/C2/C3`
- revisar coincidencias
- mandar `C4`

### Al final
- mandar `C5`

---

## Máquina 2 - Productos
### Ola 1 - mandar juntos
- `P1`
- `P2`
- `P3`

### Cuando respondan esos 3
- armar `Resumen consolidado de P1/P2/P3`
- revisar coincidencias
- mandar `P4`

### Al final
- mandar `P5`

---

# 5. Recomendación práctica de ventanas de Copilot

## Máquina 1
- Ventana A → `C1`
- Ventana B → `C2`
- Ventana C → `C3`
- Ventana D → `Resumen consolidado C1/C2/C3` + `C4`
- Ventana E → `C5`

## Máquina 2
- Ventana A → `P1`
- Ventana B → `P2`
- Ventana C → `P3`
- Ventana D → `Resumen consolidado P1/P2/P3` + `P4`
- Ventana E → `P5`

**Importante:** `D` y `E` no se mandan junto con `A/B/C`; se mandan después.

---

# 6. Qué resultado deberías esperar al final

## Clientes
- selector usable
- contrato claro para ventas
- validación de cliente utilizable
- tests mínimos críticos
- build limpio

## Productos
- selector usable
- precio y stock confiables
- validación de ítem vendible
- tests mínimos críticos
- build limpio

---

# 7. Si querés acelerar aún más

Si una de las máquinas termina antes:
- usarla para un request adicional de auditoría final cruzada.

## Prompt extra opcional - auditoría cruzada
```text
Necesito una auditoría cruzada final del cierre de clientes y productos para ventas en este backend.

Quiero que revises los cambios ya aplicados en ambos frentes y me digas:
1. si hay inconsistencias de contrato entre clientes y productos para consumo desde ventas
2. si falta algún caso crítico de validación
3. si los tests mínimos cubren realmente el flujo de ventas
4. si esto ya puede considerarse GO para pedidos/remitos/facturas
5. backlog residual separado en crítico y no crítico

Salida corta y ejecutiva.
```

---

**Última actualización:** 2026-03-31  
**Versión:** 1.0
