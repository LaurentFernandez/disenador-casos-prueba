using Casos.Core.Modelo;
using Casos.Core.Tecnicas;

namespace Casos.Core;

/// <summary>
/// Convierte los campos de una historia en casos de prueba con partición de equivalencia y valores límite, sus
/// reglas en casos de tabla de decisión y, si se pide, sus campos de lista en combinaciones por pares. Cada caso
/// cambia el valor de un solo campo, de los dos campos de su regla o de los campos de lista que combina; los demás
/// llevan su valor válido por defecto.
/// </summary>
public static class GeneradorCasos
{
    private const string PrefijoCodigo = "CP-";

    /// <summary>
    /// Devuelve los casos en orden: primero los de partición de equivalencia, empezando por el caso
    /// con todos los valores por defecto, después los de valores límite, campo por campo, y al final
    /// los de tabla de decisión, regla por regla, y si la historia lo pide, las combinaciones por pares de sus
    /// campos de lista.
    /// </summary>
    public static IReadOnlyList<CasoPrueba> Generar(Historia historia)
    {
        if (historia.Campos.Count == 0)
        {
            return [];
        }

        var clases = historia.Campos.ToDictionary(campo => campo, ParticionEquivalencia.Generar);
        // El valor por defecto de cada campo es el ejemplo de su primera clase válida.
        var porDefecto = historia.Campos.ToDictionary(campo => campo, campo => clases[campo].First(clase => clase.EsValida));

        // null representa el caso con todos los valores por defecto, que abre los de partición de equivalencia.
        IEnumerable<Cambio?> cambios =
        [
            null,
            .. historia.Campos.SelectMany(campo => clases[campo]
                .Where(clase => clase != porDefecto[campo])
                .Select(clase => new Cambio(Tecnica.ParticionEquivalencia, campo, clase))),
            .. historia.Campos.SelectMany(LimitesDe),
            .. historia.Reglas.SelectMany(regla => CambiosDeRegla(historia, regla)),
        ];

        var casos = cambios
            .Select((cambio, indice) => new CasoPrueba(
                Codigo(indice + 1),
                cambio?.Tecnica ?? Tecnica.ParticionEquivalencia,
                cambio?.Campo,
                cambio?.Clase.Tipo,
                cambio?.Posicion,
                cambio?.EsValido ?? true,
                Datos(historia.Campos, porDefecto, cambio?.Valores() ?? new Dictionary<Campo, string>()),
                cambio?.Regla))
            .ToList();

        if (historia.PorPares)
        {
            casos.AddRange(CasosPorPares(historia, porDefecto, casos.Count + 1));
        }

        return casos;
    }

    // Solo combinan opciones válidas, así que el sistema debería aceptarlas todas.
    private static IEnumerable<CasoPrueba> CasosPorPares(
        Historia historia, IReadOnlyDictionary<Campo, ClaseEquivalencia> porDefecto, int primerNumero)
    {
        var listas = historia.Campos.OfType<CampoLista>().ToArray();
        return Pares.Generar(listas).Select((combinacion, indice) => new CasoPrueba(
            Codigo(primerNumero + indice),
            Tecnica.Pares,
            CampoProbado: null,
            ClaseProbada: null,
            Posicion: null,
            EsValido: true,
            Datos(historia.Campos, porDefecto, listas.Zip(combinacion).ToDictionary(par => (Campo)par.First, par => par.Second)),
            CamposCombinados: listas));
    }

    private static IEnumerable<Cambio> CambiosDeRegla(Historia historia, ReglaObligatorio regla)
    {
        // AgregarRegla ya comprobó que los dos campos existen y que el de la condición es una lista.
        var condicion = (CampoLista)historia.CampoLlamado(regla.CampoCondicion)!;
        var obligatorio = historia.CampoLlamado(regla.CampoObligatorio)!;
        return TablaDecision.Generar(condicion, regla.Opcion, obligatorio)
            .Select(combinacion => new Cambio(Tecnica.TablaDecision, obligatorio, combinacion.ClaseObligatorio)
            {
                EsValido = combinacion.EsValida,
                Regla = regla,
                CampoCondicion = condicion,
                ValorCondicion = combinacion.ValorCondicion,
            });
    }

    private static IEnumerable<Cambio> LimitesDe(Campo campo) => campo switch
    {
        CampoEntero entero => ValoresLimite.Generar(entero.Minimo, entero.Maximo)
            .Select(valor => new Cambio(
                Tecnica.ValoresLimite,
                entero,
                ClaseDelLimite(valor, entero.Minimo, entero.Maximo, Ejemplos.Numero(valor)),
                PosicionDe(valor, entero.Minimo, entero.Maximo))),
        CampoTexto texto => ValoresLimite.GenerarLongitudes(texto)
            .Select(longitud => new Cambio(
                Tecnica.ValoresLimite,
                texto,
                ClaseDeLongitud(texto, longitud),
                PosicionDe(longitud, texto.LongitudMinima, texto.LongitudMaxima))),
        // Una lista no tiene rango, así que no tiene valores límite.
        _ => [],
    };

    // Un texto de longitud 0 es el campo vacío: válido solo si el campo es opcional.
    private static ClaseEquivalencia ClaseDeLongitud(CampoTexto campo, long longitud) =>
        longitud == 0
            ? new(TipoClase.Vacio, !campo.Obligatorio, "")
            : ClaseDelLimite(longitud, campo.LongitudMinima, campo.LongitudMaxima, Ejemplos.TextoDe(longitud));

    // Un valor límite pertenece a la clase del lado del rango en que cae.
    private static ClaseEquivalencia ClaseDelLimite(long valor, long minimo, long maximo, string ejemplo) =>
        valor < minimo ? new(TipoClase.MenorQueMinimo, false, ejemplo)
        : valor > maximo ? new(TipoClase.MayorQueMaximo, false, ejemplo)
        : new(TipoClase.DentroDelRango, true, ejemplo);

    // En un rango corto un valor puede ser a la vez mínimo, máximo o vecino de ambos: se informa el primero que aplique.
    private static PosicionLimite PosicionDe(long valor, long minimo, long maximo) =>
        valor < minimo ? PosicionLimite.DebajoDelMinimo
        : valor == minimo ? PosicionLimite.EnElMinimo
        : valor > maximo ? PosicionLimite.EncimaDelMaximo
        : valor == maximo ? PosicionLimite.EnElMaximo
        : valor == minimo + 1 ? PosicionLimite.EncimaDelMinimo
        : PosicionLimite.DebajoDelMaximo;

    // Cada campo lleva el valor que el caso le cambia o, si no, su valor por defecto.
    private static IReadOnlyDictionary<string, string> Datos(
        IReadOnlyList<Campo> campos,
        IReadOnlyDictionary<Campo, ClaseEquivalencia> porDefecto,
        IReadOnlyDictionary<Campo, string> cambios)
    {
        // OrderedDictionary garantiza que los datos salgan en el orden de los campos de la historia.
        var datos = new OrderedDictionary<string, string>();
        foreach (var campo in campos)
        {
            datos.Add(campo.Nombre, cambios.TryGetValue(campo, out var valor) ? valor : porDefecto[campo].ValorEjemplo);
        }

        return datos;
    }

    private static string Codigo(int numero) => $"{PrefijoCodigo}{numero:000}";

    /// <summary>
    /// El valor que un caso cambia respecto a los valores por defecto. En una tabla de decisión también cambia
    /// el valor del campo de la condición, y la validez la decide la regla en lugar de la clase.
    /// </summary>
    private sealed record Cambio(Tecnica Tecnica, Campo Campo, ClaseEquivalencia Clase, PosicionLimite? Posicion = null)
    {
        public bool EsValido { get; init; } = Clase.EsValida;

        public ReglaObligatorio? Regla { get; init; }

        public Campo? CampoCondicion { get; init; }

        public string? ValorCondicion { get; init; }

        public IReadOnlyDictionary<Campo, string> Valores()
        {
            var valores = new Dictionary<Campo, string> { [Campo] = Clase.ValorEjemplo };
            if (CampoCondicion is not null && ValorCondicion is not null)
            {
                valores[CampoCondicion] = ValorCondicion;
            }

            return valores;
        }
    }
}
