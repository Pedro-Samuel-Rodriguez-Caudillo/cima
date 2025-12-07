# Catalogo de documentacion de CIMA

Este catalogo da una vista rapida de las partes activas de la documentacion y de donde buscar notas historicas o especificas.

## Entrada principal
- `README.md` - punto de entrada general que ahora se enfoca en explicar la vision global y referencia directamente a `docs/INDEX.md`.
- `docs/INDEX.md` - indice estructurado que agrupa las guias por carpeta.

## Guias esenciales
- `docs/01-getting-started/README.md`, `quick-reference.md`, `setup-inicial.md` - configuracion inicial, comandos frecuentes y enlaces de referencia rapida.
- `docs/02-architecture/technical-specs.md` - especificaciones tecnicas y decisiones arquitectonicas clave.
- `docs/03-development/guides/` - guias sobre migraciones, tailwind, logging y clases CSS.
- `docs/04-deployment/` - branching strategy, variables de Railway y documentacion operativa relacionada con despliegues.
- `docs/05-troubleshooting/` - registros de errores resolvidos y pasos para reproducir/solventar los problemas mas criticos.

## Historia y sprints
- `docs/03-development/sprints/sprint-01/`, `sprint-02/` y `sprint-03/` - reportes diarios organizados por sprint. Cada archivo detalla lo avanzado y los proximos pasos.
- `docs/archive/sprints/` - archivos extras consolidados por sprint/dia; cada archivo suplementario se genera solo una vez por dia cuando se removieron los duplicados.

## Registros de dias especificos
- `docs/03-development/sprints/sprint-03/` - resumenes finales de DIA_9 a DIA_12 listos para consulta rapida sin explorar multiples archivos.
- `docs/archive/sprints/` - archivos extra por sprint y dia (un archivo por dia) que reemplazan los registros anteriores de day-logs y evitan multiples archivos por dia.

## Notas operativas y de reestructuracion
- `docs/archive/root-notes/` - aqui se movieron los informes de refactorizaciones, resumenes ejecutivos, checklists y similares que antes vivian en la raiz del repo. Se conservan por si se necesita contexto historico, pero ya no pululan el directorio principal.
- `docs/archive/*.md` (e.g., `INICIO_OLD.md`, `ARCHITECTURA_TECNICA_OLD.md`) - referencias legacy que tambien estan archivadas.

## Otros recursos
- `agents/README.md` y `agents/AGENTS_*.md` - guias para agentes IA (Copilot, Gemini, Codex) y convenciones de commits especificas.
- `.github/BRANCHING_STRATEGY.md` y `.github/WORKFLOW_VISUAL.md` - politicas de GitHub que ayudan a entender el flujo de trabajo.

## Como usar este catalogo
1. Usa la seccion de entrada principal para encontrar el punto de partida (`README.md` / `docs/INDEX.md`).
2. Si necesitas contexto historico, revisa `docs/archive/root-notes/` o `docs/archive/`.
3. Para cualquier guia tecnica, escoge la carpeta correspondiente (01-getting-started, 03-development, etc.).

Manten este catalogo actualizado si agregas, eliminas o mueves documentacion activa.
