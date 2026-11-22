using Casos.Core.Modelo;
using Casos.Core.Tecnicas;

namespace Casos.Core;

/// <summary>
/// Convierte los campos de una historia en casos de prueba con partición de equivalencia y valores límite.
/// Cada caso cambia el valor de un solo campo; los demás llevan su valor válido por defecto.
/// </summary>
public static class GeneradorCasos
{
    private const string PrefijoCodigo = "CP-";

    /// <summary>
    /// Devuelve los casos en orden: primero los de partición de equivalencia, empezando por el caso
    /// con todos los valores por defecto, y después los de valores límite, campo por campo.
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
        ];

        return cambios
            .Select((cambio, indice) => new CasoPrueba(
                Codigo(indice + 1),
                cambio?.Tecnica ?? Tecnica.ParticionEquivalencia,
                cambio?.Campo,
                cambio?.Clase.Tipo,
                cambio?.Posicion,
                cambio?.Clase.EsValida ?? true,
                Datos(historia.Campos, porDefecto, cambio)))
            .ToArray();
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

    private static IReadOnlyDictionary<string, string> Datos(
        IReadOnlyList<Campo> campos, IReadOnlyDictionary<Campo, ClaseEquivalencia> porDefecto, Cambio? cambio)
    {
        // OrderedDictionary garantiza que los datos salgan en el orden de los campos de la historia.
        var datos = new OrderedDictionary<string, string>();
        foreach (var campo in campos)
        {
            var clase = cambio is not null && cambio.Campo == campo ? cambio.Clase : porDefecto[campo];
            datos.Add(campo.Nombre, clase.ValorEjemplo);
        }

        return datos;
    }

    private static string Codigo(int numero) => $"{PrefijoCodigo}{numero:000}";

    /// <summary>
    /// El valor que un caso cambia respecto a los valores por defecto.
    /// </summary>
    private sealed record Cambio(Tecnica Tecnica, Campo Campo, ClaseEquivalencia Clase, PosicionLimite? Posicion = null);
}
