namespace Casos.Core.Modelo;

/// <summary>
/// Campo numérico entero con un rango cerrado de valores permitidos.
/// </summary>
public sealed record CampoEntero(string Nombre, bool Obligatorio, int Minimo, int Maximo)
    : Campo(Nombre, Obligatorio)
{
    // Propiedades sin init para que una expresión with no pueda saltarse la validación.
    public int Minimo { get; } = Minimo <= Maximo
        ? Minimo
        : throw new CampoInvalidoException(ErrorCampo.RangoInvalido, nameof(Minimo));

    public int Maximo { get; } = Maximo;
}
