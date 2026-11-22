namespace Casos.Core.Modelo;

/// <summary>
/// Campo de texto con una longitud permitida, medida en caracteres.
/// </summary>
public sealed record CampoTexto(string Nombre, bool Obligatorio, int LongitudMinima, int LongitudMaxima)
    : Campo(Nombre, Obligatorio)
{
    // Propiedades sin init para que una expresión with no pueda saltarse la validación.
    public int LongitudMinima { get; } = LongitudMinima <= LongitudMaxima
        ? LongitudMinima
        : throw new CampoInvalidoException(ErrorCampo.LongitudInvalida, nameof(LongitudMinima));

    public int LongitudMaxima { get; } = LongitudMaxima;
}
