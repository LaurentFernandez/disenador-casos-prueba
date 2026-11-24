# Historias de usuario

Este documento lo escribe Laurent como analista del proyecto y es la fuente de requerimientos: cada funcionalidad nace de una historia de aquí.

## Formato

Cada historia lleva:

- **Código:** HU-01, HU-02…
- **Historia:** Como [rol] quiero [acción] para [beneficio].
- **Criterios de aceptación:** escritos en Gherkin (Dado / Cuando / Entonces).
- **Prioridad:** Alta, Media o Baja.
- **Estado:** Pendiente, En desarrollo o Hecha.

## Fase 1. Motor

### HU-01. Generar los valores límite de un campo numérico

**Historia:** Como analista de QA quiero que la herramienta calcule los valores límite de un campo numérico con rango para no hacerlo a mano ni olvidar ningún borde.

**Criterios de aceptación:**

```gherkin
Escenario: Rango normal
  Dado un campo entero con mínimo 18 y máximo 65
  Cuando genero los valores límite
  Entonces obtengo 17, 18, 19, 64, 65 y 66

Escenario: Rango de un solo valor
  Dado un campo entero con mínimo 5 y máximo 5
  Cuando genero los valores límite
  Entonces obtengo 4, 5 y 6, sin valores repetidos

Escenario: Rango inválido
  Dado un campo entero con mínimo 10 y máximo 1
  Cuando intento generar los valores límite
  Entonces la herramienta avisa que el mínimo no puede ser mayor que el máximo
```

**Prioridad:** Alta
**Estado:** Hecha

### HU-02. Definir los campos de una historia

**Historia:** Como analista de QA quiero registrar los campos de una historia de usuario con su tipo y sus reglas para que la herramienta sepa qué datos debe probar.

**Criterios de aceptación:**

```gherkin
Escenario: Campo entero con rango
  Dada la historia "Registro de usuario"
  Cuando agrego el campo "edad" de tipo entero, obligatorio, con mínimo 18 y máximo 65
  Entonces el campo "edad" queda en la lista de campos con esas reglas

Escenario: Campo de texto con longitud
  Dada la historia "Registro de usuario"
  Cuando agrego el campo "nombre" de tipo texto, obligatorio, con longitud entre 3 y 50 caracteres
  Entonces el campo "nombre" queda en la lista de campos con esas reglas

Escenario: Campo de lista de opciones
  Dada la historia "Registro de usuario"
  Cuando agrego el campo "tipo de cliente" de tipo lista con las opciones "Persona" y "Empresa"
  Entonces el campo "tipo de cliente" queda en la lista de campos con esas opciones

Escenario: Nombre repetido
  Dado que la historia ya tiene el campo "edad"
  Cuando intento agregar otro campo llamado "edad"
  Entonces la herramienta avisa que ya existe un campo con ese nombre
  Y no lo agrega

Escenario: Lista sin opciones
  Cuando intento agregar un campo de tipo lista sin opciones
  Entonces la herramienta avisa que la lista necesita al menos una opción

Escenario: Longitud inválida
  Cuando intento agregar un campo de texto con longitud mínima 10 y máxima 5
  Entonces la herramienta avisa que la longitud mínima no puede ser mayor que la máxima

Escenario: Longitud negativa
  Cuando intento agregar un campo de texto con longitud mínima -1 y máxima 5
  Entonces la herramienta avisa que la longitud no puede ser negativa

Escenario: Longitud mayor que la permitida
  Cuando intento agregar un campo de texto con longitud máxima 10001
  Entonces la herramienta avisa que la longitud máxima no puede pasar de 10000 caracteres
```

**Fuera de alcance:** campos decimales y de fecha; se evaluarán en una historia aparte.
**Prioridad:** Alta
**Estado:** Hecha

### HU-03. Generar las clases de equivalencia de cada campo

**Historia:** Como analista de QA quiero que la herramienta identifique las clases de equivalencia válidas e inválidas de cada campo para cubrir cada tipo de dato con el mínimo de pruebas.

**Criterios de aceptación:**

```gherkin
Escenario: Campo entero obligatorio con rango
  Dado el campo "edad" de tipo entero, obligatorio, con mínimo 18 y máximo 65
  Cuando genero las clases de equivalencia
  Entonces obtengo estas clases, cada una con un valor de ejemplo:
    | clase              | tipo     | valor de ejemplo |
    | entre 18 y 65      | válida   | 41               |
    | menor que 18       | inválida | 17               |
    | mayor que 65       | inválida | 66               |
    | número con decimal | inválida | 18.5             |
    | no es un número    | inválida | abc              |
    | vacío              | inválida | (vacío)          |

Escenario: Campo opcional
  Dado el campo "edad" de tipo entero, opcional, con mínimo 18 y máximo 65
  Cuando genero las clases de equivalencia
  Entonces la clase "vacío" es válida

Escenario: Campo de texto con longitud
  Dado el campo "nombre" de tipo texto, obligatorio, con longitud entre 3 y 50 caracteres
  Cuando genero las clases de equivalencia
  Entonces obtengo estas clases, cada una con un valor de ejemplo:
    | clase                   | tipo     | valor de ejemplo       |
    | entre 3 y 50 caracteres | válida   | texto de 26 caracteres |
    | de 1 a 2 caracteres     | inválida | texto de 2 caracteres  |
    | más de 50 caracteres    | inválida | texto de 51 caracteres |
    | vacío                   | inválida | (vacío)                |

Escenario: Campo de lista de opciones
  Dado el campo "tipo de cliente" de tipo lista, obligatorio, con las opciones "Persona" y "Empresa"
  Cuando genero las clases de equivalencia
  Entonces obtengo estas clases, cada una con un valor de ejemplo:
    | clase               | tipo     | valor de ejemplo |
    | Persona             | válida   | Persona          |
    | Empresa             | válida   | Empresa          |
    | no está en la lista | inválida | Opción no válida |
    | vacío               | inválida | (vacío)          |
```

**Prioridad:** Alta
**Estado:** Hecha

### HU-04. Generar los valores límite de la longitud de un texto

**Historia:** Como analista de QA quiero que la herramienta calcule los valores límite de la longitud de un campo de texto para probar los bordes de caracteres permitidos sin contarlos a mano.

**Criterios de aceptación:**

```gherkin
Escenario: Longitud con mínimo y máximo
  Dado el campo "nombre" de tipo texto con longitud entre 3 y 50 caracteres
  Cuando genero los valores límite
  Entonces obtengo textos de 2, 3, 4, 49, 50 y 51 caracteres

Escenario: Longitud mínima de cero
  Dado el campo "comentario" de tipo texto con longitud entre 0 y 10 caracteres
  Cuando genero los valores límite
  Entonces obtengo textos de 0, 1, 9, 10 y 11 caracteres
  Y no obtengo longitudes negativas
```

**Prioridad:** Media
**Estado:** Hecha

## Fase 2. Interfaz

### HU-05. Ver los casos generados en una tabla

**Historia:** Como analista de QA quiero ver los casos generados en una tabla con la técnica que originó cada uno para revisarlos rápido y justificar por qué existe cada prueba.

**Criterios de aceptación:**

```gherkin
Escenario: Columnas y códigos
  Dada la historia "Registro de usuario" con el campo "edad" de tipo entero, obligatorio, entre 18 y 65
  Cuando genero los casos de prueba
  Entonces veo una tabla con las columnas código, descripción, técnica, datos y resultado esperado
  Y los códigos son consecutivos: CP-001, CP-002, CP-003…

Escenario: Técnica de origen
  Dada la historia "Registro de usuario" con el campo "edad" de tipo entero, obligatorio, entre 18 y 65
  Cuando genero los casos de prueba
  Entonces el caso con edad 41 indica la técnica "Partición de equivalencia"
  Y el caso con edad 64 indica la técnica "Valores límite"

Escenario: Mismo valor generado por dos técnicas
  Dada la historia "Registro de usuario" con el campo "edad" de tipo entero, obligatorio, entre 18 y 65
  Cuando genero los casos de prueba
  Entonces hay dos casos con edad 17: uno con la técnica "Partición de equivalencia" y otro con la técnica "Valores límite"

Escenario: Un solo valor inválido por caso
  Dados los campos "edad" (entero, 18 a 65) y "nombre" (texto, 3 a 50 caracteres), ambos obligatorios
  Cuando genero los casos de prueba
  Entonces el caso con edad 17 usa un nombre válido
  Y su resultado esperado es que el sistema rechace la edad

Escenario: Caso con todos los valores válidos
  Dados los campos "edad" (entero, 18 a 65) y "nombre" (texto, 3 a 50 caracteres), ambos obligatorios
  Cuando genero los casos de prueba
  Entonces el caso con edad 41 y un nombre válido tiene como resultado esperado que el sistema acepte los datos
```

**Prioridad:** Alta
**Estado:** Hecha

### HU-06. Ver cada caso como escenario Gherkin

**Historia:** Como analista de QA quiero ver cada caso de prueba como un escenario Gherkin para usarlo directamente en herramientas de BDD o compartirlo con el equipo.

**Criterios de aceptación:**

```gherkin
Escenario: Caso inválido en Gherkin
  Dado el caso CP-002 de la historia "Registro de usuario" con edad 17, un nombre de 26 caracteres y el tipo de cliente "Persona"
  Cuando veo el caso en Gherkin
  Entonces obtengo:
    """
    Escenario: CP-002 Edad menor que 18
      Dado el formulario "Registro de usuario"
      Cuando ingreso "17" en edad
      Y ingreso un texto de 26 caracteres en nombre
      Y elijo "Persona" en tipo de cliente
      Entonces el sistema rechaza el valor de edad
    """

Escenario: Valor vacío en Gherkin
  Dado un caso con el nombre sin valor
  Cuando veo el caso en Gherkin
  Entonces el paso del nombre es "Y dejo nombre sin valor"

Escenario: Caso válido en Gherkin
  Dado el caso CP-001 de la historia "Registro de usuario"
  Cuando veo el caso en Gherkin
  Entonces el escenario termina con "Entonces el sistema acepta los datos"
```

Los pasos evitan el género gramatical porque los nombres de los campos los escribe el usuario, y un texto se describe por su longitud para que el escenario se pueda leer.

**Prioridad:** Alta
**Estado:** Hecha

### HU-07. Exportar los casos

**Historia:** Como analista de QA quiero exportar los casos generados a CSV, Markdown y `.feature` para llevarlos a Excel, Jira, TestRail, a la documentación o al proyecto de automatización.

**Criterios de aceptación:**

```gherkin
Escenario: Exportar a CSV
  Dado que generé 12 casos para la historia "Registro de usuario"
  Cuando exporto a CSV
  Entonces descargo el archivo "registro-de-usuario.csv" con una fila de encabezados y 12 filas de casos
  Y las tildes y la ñ se ven bien al abrirlo en Excel

Escenario: Exportar a Markdown
  Dado que generé 12 casos para la historia "Registro de usuario"
  Cuando exporto a Markdown
  Entonces descargo el archivo "registro-de-usuario.md" con la tabla de los 12 casos

Escenario: Exportar a .feature
  Dado que generé 12 casos para la historia "Registro de usuario"
  Cuando exporto a .feature
  Entonces descargo el archivo "registro-de-usuario.feature"
  Y el archivo empieza con "# language: es"
  Y contiene una Característica "Registro de usuario" con 12 escenarios

Escenario: Sin casos generados
  Dado que no he generado casos
  Entonces las opciones de exportar no están disponibles
```

**Prioridad:** Media
**Estado:** Hecha

### HU-08. Cambiar el idioma entre español e inglés

**Historia:** Como usuario de la herramienta quiero usarla en español o en inglés para trabajar en el idioma de mi equipo.

**Criterios de aceptación:**

```gherkin
Escenario: Cambiar a inglés
  Dado que la interfaz está en español
  Cuando elijo el idioma inglés
  Entonces los textos de la interfaz se muestran en inglés
  Y los escenarios Gherkin usan Given, When y Then

Escenario: Recordar el idioma elegido
  Dado que elegí el idioma inglés
  Cuando cierro y vuelvo a abrir la aplicación
  Entonces la interfaz sigue en inglés

Escenario: Idioma inicial según el navegador
  Dado que abro la aplicación por primera vez
  Y mi navegador está en inglés
  Entonces la interfaz se muestra en inglés

Escenario: Idioma inicial por defecto
  Dado que abro la aplicación por primera vez
  Y mi navegador no está en español ni en inglés
  Entonces la interfaz se muestra en español

Escenario: Valor fuera de la lista en inglés
  Dado que la interfaz está en inglés
  Y un campo de lista "customer type" con las opciones Individual y Company
  Cuando veo el caso de una opción fuera de la lista
  Entonces su valor es "Invalid option" en la matriz, en la recta, en Gherkin y en las exportaciones
```

**Prioridad:** Media
**Estado:** Hecha

### HU-09. Cargar ejemplos listos

**Historia:** Como visitante de la demo quiero cargar un ejemplo ya preparado para ver qué hace la herramienta sin tener que definir campos desde cero.

**Criterios de aceptación:**

```gherkin
Escenario: Ejemplos disponibles
  Cuando abro la lista de ejemplos
  Entonces veo "Registro de usuario", "Recuperar contraseña", "Carrito de compras" y "Compatibilidad"

Escenario: Cargar un ejemplo
  Dado que no he definido campos
  Cuando elijo el ejemplo "Registro de usuario"
  Entonces se cargan la historia y sus campos con sus reglas
  Y puedo generar los casos sin escribir nada más

Escenario: Proteger lo que ya escribí
  Dado que ya definí campos propios
  Cuando elijo un ejemplo
  Entonces la herramienta pide confirmación antes de reemplazarlos

Escenario: Ejemplo para la combinación por pares
  Cuando elijo el ejemplo "Compatibilidad"
  Entonces se cargan los campos "navegador" (Chrome, Firefox, Edge), "sistema" (Windows, macOS) e "idioma" (español, inglés)
  Y la combinación por pares queda marcada
  Y veo 6 combinaciones por pares de 12 posibles
```

**Prioridad:** Media
**Estado:** Hecha

### HU-10. Instalar la aplicación y usarla sin conexión

**Historia:** Como usuario de la herramienta quiero instalarla en mi equipo o celular y usarla sin internet para diseñar casos donde no tenga conexión.

**Criterios de aceptación:**

```gherkin
Escenario: Instalar la aplicación
  Dado que abro la aplicación publicada en un navegador compatible
  Cuando elijo instalarla
  Entonces queda disponible como aplicación en mi equipo

Escenario: Usar sin conexión
  Dado que ya abrí la aplicación al menos una vez con conexión
  Y ahora no tengo conexión a internet
  Cuando abro la aplicación
  Entonces puedo definir campos, generar casos y exportarlos
```

**Prioridad:** Baja
**Estado:** Hecha

## Fase 3. Técnicas avanzadas

### HU-11. Definir reglas de negocio y generar su tabla de decisión

**Historia:** Como analista de QA quiero definir reglas de negocio que combinan condiciones para obtener una prueba por cada combinación relevante sin dejar ninguna por fuera.

**Criterios de aceptación:**

```gherkin
Escenario: Regla que hace obligatorio un campo
  Dado el campo "tipo de cliente" con las opciones "Persona" y "Empresa"
  Y el campo "NIT" de tipo texto, opcional
  Y la regla "si el tipo de cliente es Empresa, el NIT es obligatorio"
  Cuando genero la tabla de decisión
  Entonces obtengo:
    | tipo de cliente | NIT       | resultado esperado      |
    | Persona         | vacío     | se aceptan los datos    |
    | Persona         | con valor | se aceptan los datos    |
    | Empresa         | vacío     | se rechaza: falta el NIT |
    | Empresa         | con valor | se aceptan los datos    |

Escenario: Regla sobre un campo que no existe
  Dado que la historia no tiene el campo "NIT"
  Cuando intento crear la regla "si el tipo de cliente es Empresa, el NIT es obligatorio"
  Entonces la herramienta avisa que el campo "NIT" no existe
```

**Prioridad:** Media
**Estado:** Hecha

### HU-12. Reducir combinaciones con la técnica por pares (pairwise)

**Historia:** Como analista de QA quiero generar el mínimo de casos que cubra todas las parejas de valores entre campos para no tener que probar todas las combinaciones posibles.

**Criterios de aceptación:**

```gherkin
Escenario: Tres campos de lista
  Dados los campos "navegador" (Chrome, Firefox, Edge), "sistema" (Windows, macOS) e "idioma" (español, inglés)
  Cuando genero las combinaciones por pares
  Entonces obtengo 6 casos en lugar de las 12 combinaciones completas
  Y cada pareja de valores entre dos campos aparece en al menos un caso

Escenario: Menos de dos campos
  Dado que la historia tiene un solo campo de lista
  Cuando intento generar las combinaciones por pares
  Entonces la herramienta avisa que se necesitan al menos dos campos
```

**Prioridad:** Media
**Estado:** Hecha

### HU-13. Comprobar los componentes y los flujos completos de la aplicación

> Borrador de Claude, pendiente de que Laurent lo revise.

**Historia:** Como mantenedor del proyecto quiero pruebas automáticas de los componentes de la interfaz y de los flujos completos en el navegador para que un cambio no rompa lo que ya funciona sin que me entere.

**Criterios de aceptación:**

```gherkin
Escenario: Resaltar los valores probados en la Matriz
  Dado un caso que cambia un solo campo
  Cuando la Matriz lo muestra
  Entonces solo la celda de ese campo aparece resaltada

Escenario: Mostrar en la leyenda solo las técnicas que tienen casos
  Dados casos sin reglas ni combinación por pares
  Cuando se muestra la leyenda
  Entonces no incluye la tabla de decisión ni la combinación por pares

Escenario: Conservar el campo elegido en una regla
  Dada una regla que hace obligatorio el campo "NIT"
  Cuando renombro "NIT" a "RUT" en la hoja de campos
  Entonces la regla sigue mostrando "NIT" como campo obligatorio
  Y "RUT" aparece entre los campos que se pueden elegir

Escenario: Cargar un ejemplo y ver sus casos en el navegador
  Dado que abro la aplicación
  Cuando cargo el ejemplo "Registro de usuario"
  Entonces veo 24 casos en la Matriz
  Y en la vista Gherkin veo un escenario por cada caso

Escenario: Corregir un aviso en el navegador
  Dado que escribo un campo entero sin mínimo ni máximo
  Entonces veo el aviso de la fila y no se muestran casos
  Cuando completo el mínimo y el máximo
  Entonces aparecen los casos

Escenario: Exportar desde el navegador
  Dado que cargué un ejemplo
  Cuando descargo el CSV
  Entonces el archivo tiene el encabezado y una fila por caso

Escenario: Cambiar el idioma en el navegador
  Cuando elijo inglés
  Entonces la interfaz se muestra en inglés
  Y la elección se mantiene al recargar la página

Escenario: Ejecutar las pruebas antes de publicar
  Cuando un cambio llega a main o a un pull request
  Entonces la integración continua ejecuta las pruebas de componentes y las de extremo a extremo
  Y solo publica si todas pasan
```

**Prioridad:** Media
**Estado:** Hecha

## Fase 4. Opcional

### HU-14. Proponer los campos a partir del texto de una historia (modo IA local)

> Borrador de Claude, pendiente de que Laurent lo revise.

**Historia:** Como analista de QA quiero que la herramienta proponga los campos a partir del texto de una historia para no tener que escribirlos uno por uno.

**Criterios de aceptación:**

```gherkin
Escenario: Proponer campos a partir de una historia
  Dado que la aplicación corre en mi equipo y Ollama está en marcha
  Cuando escribo "Como visitante quiero registrarme con mi edad, entre 18 y 65 años, y mi nombre, de 3 a 50 caracteres"
  Y pido proponer los campos
  Entonces veo una propuesta con "edad" (entero, de 18 a 65) y "nombre" (texto, de 3 a 50)
  Y la hoja no cambia hasta que acepto la propuesta

Escenario: Aceptar la propuesta
  Dada una propuesta de campos
  Cuando la acepto
  Entonces los campos pasan a la hoja y se generan sus casos como los de cualquier campo escrito a mano

Escenario: Proteger lo que ya escribí
  Dado que ya definí campos propios
  Cuando acepto una propuesta
  Entonces la herramienta pide confirmación antes de reemplazarlos

Escenario: Propuesta con un error
  Dado que la propuesta trae un texto con mínimo mayor que el máximo
  Cuando la acepto
  Entonces la fila muestra el aviso de siempre y no se generan casos hasta corregirla

Escenario: Ollama no está disponible
  Dado que Ollama no está en marcha en mi equipo
  Cuando pido proponer los campos
  Entonces la herramienta avisa que no lo encuentra y explica cómo iniciarlo
  Y la hoja no cambia

Escenario: La demo publicada no tiene el modo IA
  Cuando abro la aplicación publicada en GitHub Pages
  Entonces no veo la opción de proponer campos
```

**Prioridad:** Baja
**Estado:** Hecha

## Mejoras después del plan

### HU-15. Conservar la historia al recargar o cambiar de idioma

> Borrador de Claude, pendiente de que Laurent lo revise.

**Historia:** Como analista de QA quiero que la herramienta conserve la historia que estoy definiendo para no perder mi trabajo si recargo la página, cambio el idioma o la cierro por error.

**Criterios de aceptación:**

```gherkin
Escenario: Recargar la página
  Dado que definí la historia "Registro de cliente" con sus campos, una regla y la combinación por pares
  Cuando recargo la página
  Entonces veo la misma historia, con sus campos, su regla y la casilla como los dejé

Escenario: Cambiar el idioma sin perder el trabajo
  Dado que definí campos propios
  Cuando cambio el idioma a inglés
  Entonces la interfaz se muestra en inglés
  Y los campos siguen ahí, con los nombres que escribí

Escenario: Abrir sin conexión lo que dejé
  Dado que definí una historia y cerré la aplicación
  Cuando la abro sin conexión a internet
  Entonces veo la historia como la dejé

Escenario: Empezar de nuevo
  Dado que tengo una historia guardada
  Cuando elijo empezar de nuevo y lo confirmo
  Entonces la hoja queda vacía y lo guardado se borra
```

**Prioridad:** Media
**Estado:** Pendiente
