namespace Casos.Core.Modelo;

/// <summary>
/// Campo de fecha con un rango cerrado de fechas permitidas, sin hora.
/// </summary>
public sealed record CampoFecha(string Nombre, bool Obligatorio, DateOnly Minimo, DateOnly Maximo)
    : Campo(Nombre, Obligatorio)
{
    /// <summary>
    /// ISO 8601, en cultura invariante: el mismo formato en los dos idiomas y el que usa el selector de fecha del navegador.
    /// </summary>
    public const string Formato = "yyyy-MM-dd";

    // Propiedades sin init para que una expresión with no pueda saltarse la validación.
    public DateOnly Minimo { get; } = Minimo <= Maximo
        ? Minimo
        : throw new CampoInvalidoException(ErrorCampo.RangoDeFechasInvalido, nameof(Minimo));

    public DateOnly Maximo { get; } = Maximo;
}
