namespace Casos.Core.Modelo;

/// <summary>
/// Regla de negocio: si el campo de lista <paramref name="CampoCondicion"/> tiene la opción <paramref name="Opcion"/>,
/// el campo <paramref name="CampoObligatorio"/>, que por sí solo es opcional, pasa a ser obligatorio.
/// Los campos se nombran como en la historia; <see cref="Historia.AgregarRegla"/> comprueba que la regla tenga sentido.
/// </summary>
public sealed record ReglaObligatorio(string CampoCondicion, string Opcion, string CampoObligatorio);
