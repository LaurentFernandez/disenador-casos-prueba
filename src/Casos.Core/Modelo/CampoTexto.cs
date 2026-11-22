namespace Casos.Core.Modelo;

/// <summary>
/// Campo de texto con una longitud permitida, medida en caracteres.
/// </summary>
public sealed record CampoTexto(string Nombre, bool Obligatorio, int LongitudMinima, int LongitudMaxima)
    : Campo(Nombre, Obligatorio)
{
    /// <summary>
    /// Tope de la longitud máxima: los textos de ejemplo se arman con esa longitud y uno enorme bloquearía el navegador.
    /// </summary>
    public const int LongitudMaximaPermitida = 10_000;

    // Propiedades sin init para que una expresión with no pueda saltarse la validación.
    public int LongitudMinima { get; } =
        LongitudMinima < 0 ? throw new CampoInvalidoException(ErrorCampo.LongitudNegativa, nameof(LongitudMinima))
        : LongitudMinima > LongitudMaxima ? throw new CampoInvalidoException(ErrorCampo.LongitudInvalida, nameof(LongitudMinima))
        : LongitudMinima;

    public int LongitudMaxima { get; } = LongitudMaxima <= LongitudMaximaPermitida
        ? LongitudMaxima
        : throw new CampoInvalidoException(ErrorCampo.LongitudExcesiva, nameof(LongitudMaxima));
}
