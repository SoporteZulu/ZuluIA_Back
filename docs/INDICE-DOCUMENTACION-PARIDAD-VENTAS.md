# 📚 ÍNDICE DE DOCUMENTACIÓN: Paridad Ventas ZuluIA

**Fecha de generación:** 2026-03-31  
**Propósito:** Índice maestro para navegar toda la documentación de paridad funcional de ventas

---

## 🎯 INICIO RÁPIDO

### ¿Eres nuevo en el proyecto?
👉 **Empieza aquí:** [RESUMEN-EJECUTIVO-PARIDAD-VENTAS.md](./RESUMEN-EJECUTIVO-PARIDAD-VENTAS.md)

### ¿Eres Tech Lead o PM?
👉 **Lee esto:** [matrices-paridad-ventas-paralelo.md](./matrices-paridad-ventas-paralelo.md)

### ¿Eres Developer listo para implementar?
👉 **Guía práctica:** [guia-practica-trabajo-paralelo.md](./guia-practica-trabajo-paralelo.md)

### ¿Necesitas validar paridad técnica?
👉 **Referencia:** [comparativa-tripartita-ventas.md](./comparativa-tripartita-ventas.md)

---

## 📁 DOCUMENTOS PRINCIPALES (Nuevos - 2026-03-31)

### 1. 📊 RESUMEN-EJECUTIVO-PARIDAD-VENTAS.md
**Qué es:** Resumen ejecutivo de todo el análisis  
**Audiencia:** Tech Leads, Product Owner, Management  
**Contenido:**
- Estado actual paridad (70% backend, 30% frontend)
- Tabla resumen por módulo
- Equipos y asignaciones sugeridas
- Cronograma Gantt (8 semanas)
- KPIs y métricas de éxito
- Riesgos y mitigaciones
- Contactos y próximos pasos

**🔗 Link:** [RESUMEN-EJECUTIVO-PARIDAD-VENTAS.md](./RESUMEN-EJECUTIVO-PARIDAD-VENTAS.md)

---

### 2. 🎯 matrices-paridad-ventas-paralelo.md
**Qué es:** 9 Matrices de trabajo para equipos paralelos  
**Audiencia:** Tech Leads, Developers, Scrum Masters  
**Contenido:**
- **Matriz 1:** Pedidos (Backend + Frontend)
- **Matriz 2:** Remitos (Backend + Frontend)
- **Matriz 3:** Facturas (Backend + Frontend)
- **Matriz 4:** Notas de Crédito (Backend + Frontend)
- **Matriz 5:** Notas de Débito (Backend + Frontend)
- **Matriz 6:** Cobros y Cuenta Corriente (Backend + Frontend)
- **Matriz 7:** Cheques (Backend + Frontend)
- **Matriz 8:** Listas de Precios (Backend + Frontend)
- **Matriz 9:** Reportes y Consultas (Backend + Frontend)

**Por cada matriz:**
- ✅ Entidades/Enums (status actual)
- ✅ Comandos (implementados vs faltantes)
- ✅ Queries (implementadas vs faltantes)
- ✅ Endpoints API (existentes vs gaps)
- ✅ Validaciones y reglas de negocio
- ✅ Páginas frontend
- ✅ Componentes frontend
- ✅ Servicios API frontend
- ✅ Referencia zuluApp (formularios ASP)

**Cómo trabajar en paralelo:**
- Estrategia de equipos independientes
- Cronograma Gantt sugerido
- Stubs para dependencias
- Reglas de sincronización
- Branching strategy
- Definition of Done (DoD) por matriz

**🔗 Link:** [matrices-paridad-ventas-paralelo.md](./matrices-paridad-ventas-paralelo.md)

---

### 3. 🔍 comparativa-tripartita-ventas.md
**Qué es:** Análisis técnico detallado de 3 proyectos  
**Audiencia:** Developers (implementación)  
**Contenido:**

**Comparativa por módulo:**
1. **zuluApp (legado):**
   - Formularios VB6/ASP identificados
   - Campos de BD exactos (SQL schemas con tipos)
   - Operaciones disponibles
   - Filtros y búsquedas
   - Validaciones específicas (código VBScript original)

2. **ZuluIA_Back (backend actual):**
   - Entidades/Enums creados (con líneas de código)
   - Comandos implementados (con firmas)
   - Queries implementadas (con parámetros)
   - Endpoints API existentes
   - ✅ Lo que ya funciona
   - ❌ Gaps identificados

3. **ZuluIA_Front (frontend actual):**
   - Estructura de archivos existente
   - Páginas implementadas
   - Componentes implementados
   - ❌ Gaps identificados

**Módulos analizados:**
- Pedidos (detallado)
- Remitos (detallado)
- Facturas (resumido)
- Notas Crédito/Débito (resumido)
- Cobros (resumido)
- Cheques (resumido)

**Dependencias entre módulos** (diagrama)

**Estimación de esfuerzo** (Story Points por módulo)

**🔗 Link:** [comparativa-tripartita-ventas.md](./comparativa-tripartita-ventas.md)

---

### 4. 🛠️ guia-practica-trabajo-paralelo.md
**Qué es:** Manual operativo step-by-step  
**Audiencia:** Todos los developers, Scrum Master  
**Contenido:**

**1. Setup Inicial (Día 0):**
- Crear branches por matriz
- Crear issues en GitHub
- Setup projects/boards

**2. Organización de Equipos:**
- Tabla de asignaciones
- Slack/Teams channels

**3. Workflow Diario:**
- Rutina diaria del equipo
- Daily standup (template)
- Commits y push strategy
- Sync cross-team

**4. Dependencias y Stubs:**
- ¿Cuándo usar stubs?
- Código ejemplo de stub
- Coordinar entrega de dependencias
- Calendario de dependencias

**5. Code Review y Merge:**
- Pull Request template
- Code review checklist
- Merge strategy (orden crítico)
- Antes de merge (checklist)

**6. Testing Strategy:**
- Backend: xUnit + FluentAssertions (código ejemplo)
- Frontend: Jest + Playwright (código ejemplo)
- Correr tests (comandos)

**7. CI/CD Pipeline:**
- GitHub Actions Backend (YAML completo)
- GitHub Actions Frontend (YAML completo)

**8. Daily Tracking:**
- Burndown chart (template Excel)
- Tablero Kanban

**9. Comunicación:**
- Daily standup template (Slack)
- Blocker escalation (template)

**10. Retrospectiva Semanal:**
- Template retrospectiva

**11. Validación Final:**
- Acceptance testing vs zuluApp
- Checklist final por matriz
- Validación ejecutiva específica de productos: [validacion-final-productos-ventas.md](./validacion-final-productos-ventas.md)

**🔗 Link:** [guia-practica-trabajo-paralelo.md](./guia-practica-trabajo-paralelo.md)

---

## 📚 DOCUMENTOS EXISTENTES (Pre-existentes)

### 5. analisis-completo-paridad-ventas-zuluapp.md
**Qué es:** Análisis inicial consolidado (pre-2026-03-31)  
**Contenido:**
- Estructura de datos COMPROBANTES (campos zuluApp)
- Tabla PEDIDOS específica
- Tabla CMP_COT (COT para remitos Paraguay)
- Tabla VTA_CMP_ITEMS (detalle items)
- Análisis de formularios zuluApp

**🔗 Link:** [analisis-completo-paridad-ventas-zuluapp.md](./analisis-completo-paridad-ventas-zuluapp.md)

---

### 6. ventas-pedidos-paridad-zuluapp.md
**Qué es:** Análisis específico de Pedidos  
**Contenido:**
- Formularios zuluApp pedidos
- Campos identificados del legado
- Estados operativos
- Operaciones identificadas
- Filtros operativos
- Análisis backend actual
- Gaps identificados backend
- Enums faltantes

**🔗 Link:** [ventas-pedidos-paridad-zuluapp.md](./ventas-pedidos-paridad-zuluapp.md)

---

### 7. paridad-remitos-zuluapp-backend.md
**Qué es:** Análisis específico de Remitos  
**Contenido:**
- Campos existentes en backend
- COT (Carta Oficial Transporte) - CRÍTICO
- Atributos de Remito
- Estado operativo/logístico
- Queries y filtros zuluApp
- Gaps en queries backend
- Comandos faltantes

**🔗 Link:** [paridad-remitos-zuluapp-backend.md](./paridad-remitos-zuluapp-backend.md)

---

### 8. auditoria-backend-actual-vs-zuluapp.md
**Qué es:** Auditoría campo por campo  
**Contenido:**
- Tabla comparativa COMPROBANTE (campo zuluApp ↔ backend actual)
- Status por campo (✅ OK, ⚠️ PARCIAL, ❌ FALTA)
- Líneas de código donde están implementados
- Campos base ✅
- Campos comerciales ✅
- Campos logísticos (remitos) ✅ (parcial)
- Campos pedido ⚠️
- Campos monetarios ✅
- Fiscal AFIP ✅
- Fiscal SIFEN ✅
- COT ✅
- Notas Débito/Crédito ✅

**🔗 Link:** [auditoria-backend-actual-vs-zuluapp.md](./auditoria-backend-actual-vs-zuluapp.md)

---

## 🗺️ MAPA DE NAVEGACIÓN

### Por Rol

#### Tech Lead / Product Owner
1. Leer: [RESUMEN-EJECUTIVO-PARIDAD-VENTAS.md](./RESUMEN-EJECUTIVO-PARIDAD-VENTAS.md)
2. Revisar: [matrices-paridad-ventas-paralelo.md](./matrices-paridad-ventas-paralelo.md)
3. Asignar equipos a matrices
4. Trackear progreso con KPIs del resumen ejecutivo

#### Developer Backend (nuevo en el proyecto)
1. Leer: [RESUMEN-EJECUTIVO-PARIDAD-VENTAS.md](./RESUMEN-EJECUTIVO-PARIDAD-VENTAS.md) (sección "Estado Actual")
2. Leer: Matriz asignada en [matrices-paridad-ventas-paralelo.md](./matrices-paridad-ventas-paralelo.md)
3. Consultar: [comparativa-tripartita-ventas.md](./comparativa-tripartita-ventas.md) (zuluApp + gaps backend)
4. Seguir: [guia-practica-trabajo-paralelo.md](./guia-practica-trabajo-paralelo.md) (sección workflow + testing backend)
5. Validar: [auditoria-backend-actual-vs-zuluapp.md](./auditoria-backend-actual-vs-zuluapp.md) (estado actual)

#### Developer Frontend (nuevo en el proyecto)
1. Leer: [RESUMEN-EJECUTIVO-PARIDAD-VENTAS.md](./RESUMEN-EJECUTIVO-PARIDAD-VENTAS.md) (sección "Estado Actual")
2. Leer: Matriz asignada en [matrices-paridad-ventas-paralelo.md](./matrices-paridad-ventas-paralelo.md)
3. Consultar: [comparativa-tripartita-ventas.md](./comparativa-tripartita-ventas.md) (zuluApp + gaps frontend)
4. Seguir: [guia-practica-trabajo-paralelo.md](./guia-practica-trabajo-paralelo.md) (sección workflow + testing frontend)
5. Usar: Stubs temporales (sección 4 de guía práctica)

#### QA / Tester
1. Leer: [RESUMEN-EJECUTIVO-PARIDAD-VENTAS.md](./RESUMEN-EJECUTIVO-PARIDAD-VENTAS.md)
2. Consultar: [comparativa-tripartita-ventas.md](./comparativa-tripartita-ventas.md) (funcionalidad zuluApp exacta)
3. Ejecutar: [guia-practica-trabajo-paralelo.md](./guia-practica-trabajo-paralelo.md) (sección 11: Validación Final)
4. Validar: Acceptance testing vs zuluApp (checklist final por matriz)

#### Scrum Master
1. Leer: [RESUMEN-EJECUTIVO-PARIDAD-VENTAS.md](./RESUMEN-EJECUTIVO-PARIDAD-VENTAS.md) (cronograma + equipos)
2. Facilitar: [guia-practica-trabajo-paralelo.md](./guia-practica-trabajo-paralelo.md) (daily standups, retrospectivas)
3. Trackear: Burndown chart (sección 8 guía práctica)
4. Escalar: Blockers (sección 9 guía práctica)

### Por Módulo Funcional

#### Pedidos
1. **Análisis:** [ventas-pedidos-paridad-zuluapp.md](./ventas-pedidos-paridad-zuluapp.md)
2. **Matriz de trabajo:** [matrices-paridad-ventas-paralelo.md](./matrices-paridad-ventas-paralelo.md) → Matriz 1
3. **Referencia técnica:** [comparativa-tripartita-ventas.md](./comparativa-tripartita-ventas.md) → Sección Pedidos

#### Remitos
1. **Análisis:** [paridad-remitos-zuluapp-backend.md](./paridad-remitos-zuluapp-backend.md)
2. **Matriz de trabajo:** [matrices-paridad-ventas-paralelo.md](./matrices-paridad-ventas-paralelo.md) → Matriz 2
3. **Referencia técnica:** [comparativa-tripartita-ventas.md](./comparativa-tripartita-ventas.md) → Sección Remitos

#### Facturas
1. **Matriz de trabajo:** [matrices-paridad-ventas-paralelo.md](./matrices-paridad-ventas-paralelo.md) → Matriz 3
2. **Referencia técnica:** [comparativa-tripartita-ventas.md](./comparativa-tripartita-ventas.md) → Sección Facturas

#### Notas Crédito
1. **Matriz de trabajo:** [matrices-paridad-ventas-paralelo.md](./matrices-paridad-ventas-paralelo.md) → Matriz 4

#### Notas Débito
1. **Matriz de trabajo:** [matrices-paridad-ventas-paralelo.md](./matrices-paridad-ventas-paralelo.md) → Matriz 5

#### Cobros
1. **Matriz de trabajo:** [matrices-paridad-ventas-paralelo.md](./matrices-paridad-ventas-paralelo.md) → Matriz 6

#### Cheques
1. **Matriz de trabajo:** [matrices-paridad-ventas-paralelo.md](./matrices-paridad-ventas-paralelo.md) → Matriz 7

#### Listas Precios
1. **Matriz de trabajo:** [matrices-paridad-ventas-paralelo.md](./matrices-paridad-ventas-paralelo.md) → Matriz 8

#### Reportes
1. **Matriz de trabajo:** [matrices-paridad-ventas-paralelo.md](./matrices-paridad-ventas-paralelo.md) → Matriz 9

---

## 🔍 BÚSQUEDA RÁPIDA

### ¿Necesitas saber...?

**¿Qué campos tiene la tabla COMPROBANTES en zuluApp?**  
→ [analisis-completo-paridad-ventas-zuluapp.md](./analisis-completo-paridad-ventas-zuluapp.md) (sección 1.1)  
→ [auditoria-backend-actual-vs-zuluapp.md](./auditoria-backend-actual-vs-zuluapp.md) (tabla comparativa)

**¿Qué formularios ASP existen en zuluApp para Pedidos?**  
→ [ventas-pedidos-paridad-zuluapp.md](./ventas-pedidos-paridad-zuluapp.md) (sección 1.1)  
→ [comparativa-tripartita-ventas.md](./comparativa-tripartita-ventas.md) (sección Pedidos → zuluApp)

**¿Qué es COT y por qué es obligatorio?**  
→ [paridad-remitos-zuluapp-backend.md](./paridad-remitos-zuluapp-backend.md) (sección 1.2)  
→ [comparativa-tripartita-ventas.md](./comparativa-tripartita-ventas.md) (sección Remitos → COT)

**¿Qué comandos/queries faltan en el backend de Pedidos?**  
→ [matrices-paridad-ventas-paralelo.md](./matrices-paridad-ventas-paralelo.md) (Matriz 1 → Backend → Gaps)  
→ [comparativa-tripartita-ventas.md](./comparativa-tripartita-ventas.md) (Pedidos → Gaps Backend)

**¿Cómo crear un stub temporal para una API que otro equipo aún no entregó?**  
→ [guia-practica-trabajo-paralelo.md](./guia-practica-trabajo-paralelo.md) (sección 4: Dependencias y Stubs)

**¿Cómo validar que mi implementación tiene paridad con zuluApp?**  
→ [guia-practica-trabajo-paralelo.md](./guia-practica-trabajo-paralelo.md) (sección 11: Validación Final)

**¿Cuál es el DoD (Definition of Done) de mi matriz?**  
→ [matrices-paridad-ventas-paralelo.md](./matrices-paridad-ventas-paralelo.md) (cada matriz tiene su DoD)  
→ [guia-practica-trabajo-paralelo.md](./guia-practica-trabajo-paralelo.md) (sección 11: Checklist Final)

**¿Qué filtros tiene la grilla de Remitos en zuluApp?**  
→ [paridad-remitos-zuluapp-backend.md](./paridad-remitos-zuluapp-backend.md) (sección 2.1)  
→ [comparativa-tripartita-ventas.md](./comparativa-tripartita-ventas.md) (Remitos → zuluApp → Filtros)

**¿Cómo es el workflow diario de mi equipo?**  
→ [guia-practica-trabajo-paralelo.md](./guia-practica-trabajo-paralelo.md) (sección 3: Workflow Diario)

**¿Cuándo mergear mi branch a develop?**  
→ [guia-practica-trabajo-paralelo.md](./guia-practica-trabajo-paralelo.md) (sección 5: Merge Strategy)  
→ [matrices-paridad-ventas-paralelo.md](./matrices-paridad-ventas-paralelo.md) (Cronograma Gantt → orden de merges)

---

## 📊 ESTRUCTURA DE ARCHIVOS GENERADOS

```
docs/
├── INDICE-DOCUMENTACION-PARIDAD-VENTAS.md ⭐ ESTE ARCHIVO
├── RESUMEN-EJECUTIVO-PARIDAD-VENTAS.md (nuevo 2026-03-31)
├── matrices-paridad-ventas-paralelo.md (nuevo 2026-03-31)
├── comparativa-tripartita-ventas.md (nuevo 2026-03-31)
├── guia-practica-trabajo-paralelo.md (nuevo 2026-03-31)
├── analisis-completo-paridad-ventas-zuluapp.md (existente)
├── ventas-pedidos-paridad-zuluapp.md (existente)
├── paridad-remitos-zuluapp-backend.md (existente)
└── auditoria-backend-actual-vs-zuluapp.md (existente)
```

---

## 📈 MÉTRICAS DE DOCUMENTACIÓN

| Métrica | Valor |
|---------|-------|
| **Documentos totales** | 9 |
| **Documentos nuevos (2026-03-31)** | 4 |
| **Páginas totales** | ~180 páginas |
| **Módulos cubiertos** | 9 (Pedidos, Remitos, Facturas, NC, ND, Cobros, Cheques, Listas Precios, Reportes) |
| **Formularios zuluApp analizados** | 40+ |
| **Campos BD documentados** | 150+ |
| **Story Points estimados** | 337 SP |
| **Equipos paralelos soportados** | 7 equipos |

---

## 🎯 CÓMO USAR ESTE ÍNDICE

### Para navegar offline
Este índice está diseñado para funcionar con links relativos. Clona el repositorio y abre este archivo en cualquier editor Markdown (VS Code, Obsidian, etc.).

### Para búsqueda de texto
Usa `Ctrl+F` (Windows) o `Cmd+F` (Mac) para buscar palabras clave:
- "COT" → Te lleva a documentos que hablan de Carta Oficial Transporte
- "Pedidos gaps" → Te lleva a gaps de pedidos
- "zuluApp formularios" → Te lleva a análisis de formularios legados

### Para imprimir/PDF
Cada documento se puede exportar a PDF individualmente. El resumen ejecutivo es ideal para presentaciones a management.

---

## ✅ CHECKLIST DE LECTURA RECOMENDADA

### Onboarding Nuevo Developer
- [ ] Leer RESUMEN-EJECUTIVO-PARIDAD-VENTAS.md (15 min)
- [ ] Leer matriz asignada en matrices-paridad-ventas-paralelo.md (30 min)
- [ ] Revisar comparativa-tripartita-ventas.md sección de tu módulo (20 min)
- [ ] Leer guia-practica-trabajo-paralelo.md secciones 1-5 (40 min)
- [ ] **Total:** ~2 horas lectura

### Sprint Planning
- [ ] Revisar matriz asignada (DoD + tasks)
- [ ] Estimar Story Points por task
- [ ] Identificar dependencias con otros equipos
- [ ] Crear issues en GitHub
- [ ] **Total:** ~1 hora

### Daily Standup
- [ ] Actualizar estado en GitHub Project
- [ ] Reportar blockers (si hay)
- [ ] Coordinar con otros equipos (si dependencias)
- [ ] **Total:** 15 min

### Code Review
- [ ] Verificar DoD de matriz
- [ ] Validar vs zuluApp (comparativa-tripartita-ventas.md)
- [ ] Verificar tests (guia-practica-trabajo-paralelo.md sección 6)
- [ ] **Total:** 30-60 min por PR

---

## 🆘 SOPORTE

### ¿Encontraste un error en la documentación?
Crea un issue en GitHub con label `documentation`.

### ¿Falta algo importante?
Consulta con Tech Lead o Product Owner.

### ¿Necesitas aclaración de zuluApp?
Consulta a usuarios legacy o revisa el código fuente en `C:\Zulu\zuluApp`.

---

**Última actualización:** 2026-03-31  
**Versión:** 1.0  
**Mantenido por:** GitHub Copilot + Equipo ZuluIA

---

**📌 FAVORITO ESTE ARCHIVO** - Es tu punto de entrada a toda la documentación de paridad ventas.
