namespace Casos.Core.Modelo;

/// <summary>
/// Campo que solo admite una de las opciones de su lista.
/// </summary>
/// <remarks>
/// La igualdad del record compara <see cref="Opciones"/> por referencia, no por contenido.
/// </remarks>
public sealed record CampoLista(string Nombre, bool Obligatorio, IReadOnlyList<string> Opciones)
    : Campo(Nombre, Obligatorio)
{
    // Propiedad sin init para que una expresión with no pueda saltarse la validación.
    public IReadOnlyList<string> Opciones { get; } = Opciones.Count > 0
        ? Opciones
        : throw new CampoInvalidoException(ErrorCampo.ListaSinOpciones, nameof(Opciones));

    /// <summary>
    /// El valor preferido si no es una de las opciones; si lo es, el mismo con un número («Opción no válida 2») hasta
    /// que deje de serlo. No importan las mayúsculas, porque el sistema podría aceptar la opción escrita de otra forma.
    /// </summary>
    public string ValorQueNoEsOpcion(string preferido)
    {
        var valor = preferido;
        for (var numero = 2; Opciones.Contains(valor, StringComparer.OrdinalIgnoreCase); numero++)
        {
            valor = $"{preferido} {numero}";
        }

        return valor;
    }
}
