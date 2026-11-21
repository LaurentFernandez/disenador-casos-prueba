namespace Casos.Core.Modelo;

/// <summary>
/// Campo de texto con una longitud permitida, medida en caracteres.
/// </summary>
public sealed record CampoTexto(string Nombre, bool Obligatorio, int LongitudMinima, int LongitudMaxima)
    : Campo(Nombre, Obligatorio);
