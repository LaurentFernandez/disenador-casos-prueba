namespace Casos.Core.Modelo;

/// <summary>
/// Campo numérico entero con un rango cerrado de valores permitidos.
/// </summary>
public sealed record CampoEntero(string Nombre, bool Obligatorio, int Minimo, int Maximo)
    : Campo(Nombre, Obligatorio);
