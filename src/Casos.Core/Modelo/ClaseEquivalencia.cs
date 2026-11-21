namespace Casos.Core.Modelo;

/// <summary>
/// Grupo de valores que el sistema debería tratar igual, con un valor de ejemplo para probarlo.
/// </summary>
/// <param name="ValorEjemplo">El valor tal como se escribiría en el campo; cadena vacía para <see cref="TipoClase.Vacio"/>.</param>
public sealed record ClaseEquivalencia(TipoClase Tipo, bool EsValida, string ValorEjemplo);
