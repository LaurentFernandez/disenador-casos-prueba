using Casos.Core.Modelo;

namespace Casos.Core.Tecnicas;

/// <summary>
/// Tabla de decisión (ISTQB) de una regla "si el campo de lista tiene una opción, otro campo es obligatorio".
/// Tiene dos condiciones de sí o no: ¿el campo de lista tiene esa opción? y ¿el otro campo tiene valor?
/// Cada combinación es una columna de la tabla y un caso. Para el "no" de la primera condición basta una
/// opción representativa, porque todas las demás opciones se comportan igual ante la regla.
/// </summary>
public static class TablaDecision
{
    /// <summary>
    /// Una columna de la tabla: el valor del campo de la condición, la clase del valor del campo obligatorio
    /// y si el sistema debería aceptar los datos.
    /// </summary>
    public sealed record Combinacion(string ValorCondicion, ClaseEquivalencia ClaseObligatorio, bool EsValida);

    /// <summary>
    /// Devuelve las combinaciones en el orden de las opciones de la lista y, para cada opción, primero con el campo
    /// obligatorio vacío. Si la lista no tiene otra opción, solo quedan las dos en que se cumple la condición.
    /// </summary>
    public static IReadOnlyList<Combinacion> Generar(CampoLista condicion, string opcion, Campo obligatorio)
    {
        var otraOpcion = condicion.Opciones.FirstOrDefault(otra => otra != opcion);
        var clases = ParticionEquivalencia.Generar(obligatorio);
        ClaseEquivalencia[] valores =
        [
            clases.Single(clase => clase.Tipo == TipoClase.Vacio),
            clases.First(clase => clase.EsValida && clase.Tipo != TipoClase.Vacio),
        ];

        // Lo único que la regla rechaza es que se cumpla la condición y el campo obligatorio quede vacío.
        return condicion.Opciones
            .Distinct()
            .Where(valor => valor == opcion || valor == otraOpcion)
            .SelectMany(valor => valores.Select(clase =>
                new Combinacion(valor, clase, valor != opcion || clase.Tipo != TipoClase.Vacio)))
            .ToArray();
    }
}
