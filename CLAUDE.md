# Diseñador de casos de prueba: contexto del proyecto

## Qué es

Aplicación web (Blazor WebAssembly, instalable como PWA) que genera casos de prueba a partir de historias de usuario aplicando técnicas de diseño de pruebas de caja negra (ISTQB): partición de equivalencia, valores límite, tablas de decisión y combinatoria por pares (pairwise). Muestra los casos en una tabla con su técnica de origen, los convierte a Gherkin y los exporta a CSV, Markdown y `.feature`.

Es un proyecto de portafolio de Laurent Andrés Fernandez (ingeniero de software en NyGsoft, con experiencia en desarrollo .NET y QA). Su objetivo es mostrar análisis, QA y desarrollo .NET en un solo proyecto, con demo en vivo. El plan completo está en `docs/plan.md`.

## Decisiones tomadas (no reabrir sin preguntar a Laurent)

- **Costo cero:** solo herramientas gratuitas u open source. Nada de APIs de pago. Sin IA en el núcleo; la Fase 4 opcional usaría IA local con Ollama, solo en modo local.
- **Aplicación web** en Blazor WebAssembly con PWA, publicada gratis en GitHub Pages, sin servidor: `https://laurentfernandez.github.io/disenador-casos-prueba/`. La publica el trabajo `publicar` de `ci.yml` en cada push a `main` con las pruebas en verde. La base es relativa (`<base href="./" />`) para que la misma compilación sirva en local y en la subcarpeta; no se modifica el `index.html` publicado porque rompería la verificación de integridad del service worker. Si el despliegue falla con "No artifacts named github-pages", es un desfase de la API de GitHub: basta con volver a ejecutar el trabajo.
- **.NET 10 (LTS).** Pruebas: xUnit para el motor y para la lógica de la interfaz; bUnit para componentes y Playwright para .NET (E2E) desde la Fase 3.
- **Interfaz en español e inglés**, con `IStringLocalizer` y archivos `.resx`: los textos están en `Recursos/Textos.resx` (español) y `Recursos/Textos.en.resx` (inglés), y se leen con la clase estática `Textos`. El idioma se fija al arrancar (el elegido antes o, la primera vez, inglés si el navegador está en inglés y español en cualquier otro caso); cambiarlo guarda la elección y recarga la página. Los nombres que escribe el usuario no se traducen y el CSV usa ";" en los dos idiomas. Las pruebas de textos fijan su idioma heredando de `PruebaEnIdioma`.
- **Código en español:** clases, métodos, variables y nombres de pruebas, igual que los demás proyectos de Laurent.
- **El motor (`Casos.Core`) no depende de la interfaz.** Toda la lógica vive y se prueba ahí; `Casos.Web` solo presenta.
- **El motor no devuelve textos para mostrar:** devuelve tipos (por ejemplo, los enums `TipoClase` y `ErrorCampo`) y la interfaz arma la descripción o el aviso en español o inglés.
- **Textos neutros:** los nombres de los campos los escribe el usuario, así que los textos de los casos evitan el género gramatical ("Edad sin valor", "Rechaza el valor de edad") y un texto largo se describe por su longitud ("un texto de 26 caracteres").
- **Casos en vivo:** los casos se generan cada vez que cambia la hoja de campos, sin botón. Si alguna fila tiene un aviso, no se muestran casos hasta corregirla. La longitud máxima de un texto es 10 000 caracteres (`CampoTexto.LongitudMaximaPermitida`).
- **Repositorio:** `github.com/LaurentFernandez/disenador-casos-prueba`, rama `main`. Público.
- **Diseño visual:** está en `docs/diseno/maqueta.html`. Los campos se editan en una hoja tipo cuadrícula. Los casos se ven en tres vistas a elección del usuario: Matriz (una columna por campo, con el valor probado resaltado), Recta (el diagrama de partición de cada campo) y Gherkin. El estilo es el mismo en todas: títulos en Barlow (incluida en el proyecto, licencia OFL), azul tinta, lo válido en azul verdoso y lo inválido rayado en naranja. Colores, tamaños y espaciados son variables CSS en `wwwroot/css/app.css`; no se usa Bootstrap.

## Estructura

```
DisenadorCasosPrueba.slnx
src/Casos.Core/          motor (modelo en Modelo/, técnicas en Tecnicas/)
src/Casos.Web/           interfaz Blazor WebAssembly + PWA (componentes en Componentes/, textos en Recursos/)
tests/Casos.Core.Tests/  pruebas xUnit del motor
tests/Casos.Web.Tests/   pruebas xUnit de la lógica de la interfaz
docs/plan.md             objetivo, técnicas, arquitectura y fases
docs/historias-de-usuario.md   requerimientos (los escribe Laurent)
docs/diseno/maqueta.html       maqueta de la interfaz (se abre en el navegador)
.github/workflows/ci.yml compila y prueba en cada push a main y en cada PR; publica en GitHub Pages lo que llega a main
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

- **Fases 0 y 1 terminadas.** El motor tiene el modelo de campos con sus validaciones (`Modelo/`), la partición de equivalencia (`Tecnicas/ParticionEquivalencia`) y los valores límite de enteros y de longitud de texto (`Tecnicas/ValoresLimite`).
- **Fase 2 en curso.** Ya están la generación de casos en el motor (`GeneradorCasos`), el estilo base de la maqueta, la hoja de campos (`Componentes/HojaCampos.razor`), el nombre de la historia y las vistas Matriz, Recta y Gherkin (`Componentes/CasosGenerados.razor`; el Gherkin lo arma `GherkinCasos`), la exportación a CSV, Markdown y `.feature` (`ExportacionCasos` y `wwwroot/js/descargas.js`) y el cambio de idioma (`Componentes/SelectorIdioma.razor` y `wwwroot/js/idioma.js`). La demo está publicada en GitHub Pages. Hay 36 pruebas del motor y 38 de la lógica de la interfaz.
- **Historias:** HU-01 a HU-08 están hechas, en el motor y en la interfaz, y sus issues están cerrados. HU-01 no tuvo issue porque se hizo antes de crear el repositorio.
- **Siguiente:** HU-09 (cargar ejemplos listos) y HU-10 (instalar la aplicación y usarla sin conexión), que completan la Fase 2. Para HU-10 ya hay un service worker que guarda la aplicación en caché; falta comprobar sus escenarios. Después sigue la Fase 3.
- **Por decidir:** el motor usa el valor de prueba "Opción no válida" para el caso fuera de la lista (`ParticionEquivalencia`), y en la interfaz en inglés aparece tal cual porque es un dato, no un texto de la interfaz.

## Fases

| Fase | Contenido |
|---|---|
| 0. Arranque | Esqueleto, CI y primeras historias |
| 1. Motor | Modelo de campos, partición de equivalencia y valores límite |
| 2. Interfaz | Formulario, tabla de resultados, Gherkin, exportaciones, español e inglés, GitHub Pages |
| 3. Técnicas avanzadas | Tablas de decisión y pairwise; pruebas bUnit y Playwright |
| 4. Opcional | Modo IA local con Ollama |
