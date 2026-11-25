namespace Casos.Core.Modelo;

/// <summary>
/// Campo de fecha con un rango cerrado de fechas permitidas, sin hora.
/// </summary>
public sealed record CampoFecha(string Nombre, bool Obligatorio, DateOnly Minimo, DateOnly Maximo)
    : Campo(Nombre, Obligatorio)
{
    // Propiedades sin init para que una expresión with no pueda saltarse la validación.
    public DateOnly Minimo { get; } = Minimo <= Maximo
        ? Minimo
        : throw new CampoInvalidoException(ErrorCampo.RangoDeFechasInvalido, nameof(Minimo));

    public DateOnly Maximo { get; } = Maximo;
}
