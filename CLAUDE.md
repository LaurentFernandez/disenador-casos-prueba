# Diseñador de casos de prueba: contexto del proyecto

## Qué es

Aplicación web (Blazor WebAssembly, instalable como PWA) que genera casos de prueba a partir de historias de usuario aplicando técnicas de diseño de pruebas de caja negra (ISTQB): partición de equivalencia, valores límite, tablas de decisión y combinatoria por pares (pairwise). Muestra los casos en una tabla con su técnica de origen, los convierte a Gherkin y los exporta a CSV, Markdown y `.feature`.

Es un proyecto de portafolio de Laurent Andrés Fernandez (ingeniero de software en NyGsoft, con experiencia en desarrollo .NET y QA). Su objetivo es mostrar análisis, QA y desarrollo .NET en un solo proyecto, con demo en vivo. El plan completo está en `docs/plan.md`.

## Decisiones tomadas (no reabrir sin preguntar a Laurent)

- **Costo cero:** solo herramientas gratuitas u open source. Nada de APIs de pago. Sin IA en el núcleo; la Fase 4 opcional usaría IA local con Ollama, solo en modo local.
- **Aplicación web** en Blazor WebAssembly con PWA, publicada gratis en GitHub Pages, sin servidor.
- **.NET 10 (LTS).** Pruebas: xUnit para el motor; bUnit para componentes y Playwright para .NET (E2E) desde la Fase 3.
- **Interfaz en español e inglés**, con `IStringLocalizer` y archivos `.resx` (Fase 2).
- **Código en español:** clases, métodos, variables y nombres de pruebas, igual que los demás proyectos de Laurent.
- **El motor (`Casos.Core`) no depende de la interfaz.** Toda la lógica vive y se prueba ahí; `Casos.Web` solo presenta.
- **El motor no devuelve textos para mostrar:** devuelve tipos (por ejemplo, el enum `TipoClase`) y la interfaz arma la descripción en español o inglés.
- **Repositorio:** `github.com/LaurentFernandez/disenador-casos-prueba`, rama `main`. Público.
- La interfaz actual es la plantilla de Blazor con Bootstrap; el diseño visual se decide en la Fase 2.

## Estructura

```
DisenadorCasosPrueba.slnx
src/Casos.Core/          motor (modelo en Modelo/, técnicas en Tecnicas/)
src/Casos.Web/           interfaz Blazor WebAssembly + PWA
tests/Casos.Core.Tests/  pruebas xUnit del motor
docs/plan.md             objetivo, técnicas, arquitectura y fases
docs/historias-de-usuario.md   requerimientos (los escribe Laurent)
.github/workflows/ci.yml compila y prueba en cada push a main y en cada PR
```

## Comandos

```bash
dotnet build
dotnet test
dotnet run --project src/Casos.Web
```

## Cómo trabajamos

- **Laurent es el analista.** Escribe las historias de usuario en `docs/historias-de-usuario.md`, y cada funcionalidad nace de una historia. Si una tarea no tiene historia, pídesela o propón un borrador marcado como tal para que él lo revise.
- **Laurent programa las partes que quiera aprender.** Claude prepara la base, explica las decisiones y revisa su código.
- Un issue por historia, una rama por funcionalidad y pull requests hacia `main`.
- Las reglas globales de Laurent (`~/.claude/CLAUDE.md`) aplican aquí: respuestas en español, confirmación explícita antes de cada commit, push o publicación, código limpio y verificación antes de decir "listo".

## Estado actual

- **Fases 0 y 1 terminadas.** El motor tiene el modelo de campos con sus validaciones (`Modelo/`), la partición de equivalencia (`Tecnicas/ParticionEquivalencia`) y los valores límite de enteros y de longitud de texto (`Tecnicas/ValoresLimite`), con 24 pruebas. HU-01 a HU-04 están hechas en el motor y les falta la interfaz; por eso los issues de HU-02 a HU-04 (#1, #2 y #5) siguen abiertos.
- **Siguiente:** Fase 2, la interfaz. Primero hay que decidir el diseño visual. La tabla de casos (HU-05) necesita además, en el motor, la generación de casos: código, datos y resultado esperado, con un solo valor inválido por caso.

## Fases

| Fase | Contenido |
|---|---|
| 0. Arranque | Esqueleto, CI y primeras historias |
| 1. Motor | Modelo de campos, partición de equivalencia y valores límite |
| 2. Interfaz | Formulario, tabla de resultados, Gherkin, exportaciones, español e inglés, GitHub Pages |
| 3. Técnicas avanzadas | Tablas de decisión y pairwise; pruebas bUnit y Playwright |
| 4. Opcional | Modo IA local con Ollama |
