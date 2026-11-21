namespace Casos.Core.Modelo;

/// <summary>
/// Dato de una historia de usuario que la herramienta debe probar.
/// Cada tipo de campo es un record derivado con sus propias reglas.
/// </summary>
public abstract record Campo(string Nombre, bool Obligatorio);
