using Casos.Core.Modelo;

namespace Casos.Core.Tecnicas;

/// <summary>
/// Partición de equivalencia (ISTQB): divide los valores posibles de un campo en clases
/// que el sistema debería tratar igual y toma un valor de ejemplo de cada una.
/// </summary>
public static class ParticionEquivalencia
{
    private const string ValorNoNumerico = "abc";
    private const string ValorQueNoEsFecha = "abc";
    private const decimal ParteDecimal = 0.5m;

    // La interfaz muestra este valor en su idioma; el motor solo garantiza que no sea una de las opciones.
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
            CampoFecha fecha => ClasesDeFecha(fecha),
            _ => throw new ArgumentException($"Tipo de campo no soportado: {campo.GetType().Name}.", nameof(campo)),
        };

        return [.. clases, new ClaseEquivalencia(TipoClase.Vacio, !campo.Obligatorio, "")];
    }

    private static IEnumerable<ClaseEquivalencia> ClasesDeEntero(CampoEntero campo)
    {
        // long para que los vecinos de int.MinValue e int.MaxValue no se desborden.
        long minimo = campo.Minimo;
        long maximo = campo.Maximo;

        yield return Valida(TipoClase.DentroDelRango, Ejemplos.Numero(minimo + (maximo - minimo) / 2));
        yield return Invalida(TipoClase.MenorQueMinimo, Ejemplos.Numero(minimo - 1));
        yield return Invalida(TipoClase.MayorQueMaximo, Ejemplos.Numero(maximo + 1));
        // Dentro del rango, para que lo único inválido sea el decimal.
        yield return Invalida(TipoClase.ConDecimales, Ejemplos.Numero(minimo + ParteDecimal));
        yield return Invalida(TipoClase.NoEsNumero, ValorNoNumerico);
    }

    private static IEnumerable<ClaseEquivalencia> ClasesDeTexto(CampoTexto campo)
    {
        int minimo = campo.LongitudMinima;
        int maximo = campo.LongitudMaxima;

        yield return Valida(TipoClase.DentroDelRango, Ejemplos.TextoDe(minimo + (maximo - minimo) / 2));

        // Con mínimo 0 o 1 no hay un texto más corto que no sea vacío.
        if (minimo > 1)
        {
            yield return Invalida(TipoClase.MenorQueMinimo, Ejemplos.TextoDe(minimo - 1));
        }

        yield return Invalida(TipoClase.MayorQueMaximo, Ejemplos.TextoDe(maximo + 1));
    }

    private static IEnumerable<ClaseEquivalencia> ClasesDeFecha(CampoFecha campo)
    {
        int minimo = campo.Minimo.DayNumber;
        int maximo = campo.Maximo.DayNumber;

        yield return Valida(TipoClase.DentroDelRango, Ejemplos.Fecha(DateOnly.FromDayNumber(minimo + (maximo - minimo) / 2)));

        // Con el rango en un extremo del calendario no hay un día anterior o siguiente que se pueda escribir.
        if (campo.Minimo > DateOnly.MinValue)
        {
            yield return Invalida(TipoClase.MenorQueMinimo, Ejemplos.Fecha(campo.Minimo.AddDays(-1)));
        }

        if (campo.Maximo < DateOnly.MaxValue)
        {
            yield return Invalida(TipoClase.MayorQueMaximo, Ejemplos.Fecha(campo.Maximo.AddDays(1)));
        }

        yield return Invalida(TipoClase.FechaInexistente, FechaInexistente(campo.Minimo.Year));
        yield return Invalida(TipoClase.NoEsFecha, ValorQueNoEsFecha);
    }

    // El 29 de febrero solo existe en los años bisiestos; el 30, nunca.
    private static string FechaInexistente(int año) => $"{año:D4}-02-{(DateTime.IsLeapYear(año) ? 30 : 29)}";

    private static IEnumerable<ClaseEquivalencia> ClasesDeLista(CampoLista campo) =>
        campo.Opciones
            .Select(opcion => Valida(TipoClase.Opcion, opcion))
            .Append(Invalida(TipoClase.FueraDeLista, campo.ValorQueNoEsOpcion(ValorFueraDeLista)));

    private static ClaseEquivalencia Valida(TipoClase tipo, string valor) => new(tipo, true, valor);

    private static ClaseEquivalencia Invalida(TipoClase tipo, string valor) => new(tipo, false, valor);
}
