using System.Globalization;
using Casos.Core.Modelo;

namespace Casos.Core.Tecnicas;

/// <summary>
/// Partición de equivalencia (ISTQB): divide los valores posibles de un campo en clases
/// que el sistema debería tratar igual y toma un valor de ejemplo de cada una.
/// </summary>
public static class ParticionEquivalencia
{
    private const string ValorNoNumerico = "abc";
    private const decimal ParteDecimal = 0.5m;
    private const char CaracterDeRelleno = 'a';

    // shortcut: si una opción de la lista se llama igual, este ejemplo no sería inválido; generar uno distinto si llega a pasar.
    private const string ValorFueraDeLista = "Opción no válida";

    /// <summary>
    /// Devuelve las clases válidas e inválidas del campo, terminando siempre con la clase vacío.
    /// </summary>
    public static IReadOnlyList<ClaseEquivalencia> Generar(Campo campo)
    {
        var clases = campo switch
        {
            CampoEntero entero => ClasesDeEntero(entero),
            CampoTexto texto => ClasesDeTexto(texto),
            CampoLista lista => ClasesDeLista(lista),
            _ => throw new ArgumentException($"Tipo de campo no soportado: {campo.GetType().Name}.", nameof(campo)),
        };

        return [.. clases, new ClaseEquivalencia(TipoClase.Vacio, !campo.Obligatorio, "")];
    }

    private static IEnumerable<ClaseEquivalencia> ClasesDeEntero(CampoEntero campo)
    {
        // long para que los vecinos de int.MinValue e int.MaxValue no se desborden.
        long minimo = campo.Minimo;
        long maximo = campo.Maximo;

        yield return Valida(TipoClase.DentroDelRango, Numero(minimo + (maximo - minimo) / 2));
        yield return Invalida(TipoClase.MenorQueMinimo, Numero(minimo - 1));
        yield return Invalida(TipoClase.MayorQueMaximo, Numero(maximo + 1));
        // Dentro del rango, para que lo único inválido sea el decimal.
        yield return Invalida(TipoClase.ConDecimales, (minimo + ParteDecimal).ToString(CultureInfo.InvariantCulture));
        yield return Invalida(TipoClase.NoEsNumero, ValorNoNumerico);
    }

    private static IEnumerable<ClaseEquivalencia> ClasesDeTexto(CampoTexto campo)
    {
        int minimo = campo.LongitudMinima;
        int maximo = campo.LongitudMaxima;

        yield return Valida(TipoClase.DentroDelRango, TextoDe(minimo + (maximo - minimo) / 2));

        // Con mínimo 0 o 1 no hay un texto más corto que no sea vacío.
        if (minimo > 1)
        {
            yield return Invalida(TipoClase.MenorQueMinimo, TextoDe(minimo - 1));
        }

        yield return Invalida(TipoClase.MayorQueMaximo, TextoDe(maximo + 1));
    }

    private static IEnumerable<ClaseEquivalencia> ClasesDeLista(CampoLista campo) =>
        campo.Opciones
            .Select(opcion => Valida(TipoClase.Opcion, opcion))
            .Append(Invalida(TipoClase.FueraDeLista, ValorFueraDeLista));

    private static ClaseEquivalencia Valida(TipoClase tipo, string valor) => new(tipo, true, valor);

    private static ClaseEquivalencia Invalida(TipoClase tipo, string valor) => new(tipo, false, valor);

    private static string Numero(long valor) => valor.ToString(CultureInfo.InvariantCulture);

    private static string TextoDe(int longitud) => new(CaracterDeRelleno, longitud);
}
