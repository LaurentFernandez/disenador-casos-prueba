namespace Casos.Core.Modelo;

/// <summary>
/// Campo que solo admite una de las opciones de su lista.
/// </summary>
/// <remarks>
/// La igualdad del record compara <see cref="Opciones"/> por referencia, no por contenido.
/// </remarks>
public sealed record CampoLista(string Nombre, bool Obligatorio, IReadOnlyList<string> Opciones)
    : Campo(Nombre, Obligatorio);
