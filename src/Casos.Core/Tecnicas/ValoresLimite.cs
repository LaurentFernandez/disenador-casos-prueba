using Casos.Core.Modelo;

namespace Casos.Core.Tecnicas;

/// <summary>
/// Análisis de valores límite de tres valores (ISTQB): en cada borde de un rango
/// se prueban el borde y sus dos vecinos inmediatos.
/// </summary>
public static class ValoresLimite
{
    /// <summary>
    /// Devuelve los valores a probar para un rango entero cerrado, ordenados y sin repetir.
    /// </summary>
    /// <remarks>
    /// Recibe <see cref="int"/> y devuelve <see cref="long"/> para que los vecinos de
    /// <see cref="int.MinValue"/> e <see cref="int.MaxValue"/> no se desborden.
    /// </remarks>
    public static IReadOnlyList<long> Generar(int minimo, int maximo)
    {
        if (minimo > maximo)
        {
            throw new ArgumentException("El mínimo no puede ser mayor que el máximo.", nameof(minimo));
        }

        long min = minimo;
        long max = maximo;

        return new[] { min - 1, min, min + 1, max - 1, max, max + 1 }
            .Distinct()
            .Order()
            .ToArray();
    }

    /// <summary>
    /// Devuelve las longitudes a probar para un campo de texto, sin longitudes negativas.
    /// </summary>
    public static IReadOnlyList<long> GenerarLongitudes(CampoTexto campo) =>
        Generar(campo.LongitudMinima, campo.LongitudMaxima)
            .Where(longitud => longitud >= 0)
            .ToArray();

    /// <summary>
    /// Devuelve las fechas a probar para un campo de fecha, día por día, sin salir del calendario.
    /// </summary>
    public static IReadOnlyList<DateOnly> GenerarFechas(CampoFecha campo) =>
        Generar(campo.Minimo.DayNumber, campo.Maximo.DayNumber)
            .Where(dia => dia >= DateOnly.MinValue.DayNumber && dia <= DateOnly.MaxValue.DayNumber)
            .Select(dia => DateOnly.FromDayNumber((int)dia))
            .ToArray();
}
