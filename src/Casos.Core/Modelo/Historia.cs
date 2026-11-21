namespace Casos.Core.Modelo;

/// <summary>
/// Historia de usuario con los campos que se van a probar.
/// </summary>
public class Historia(string nombre)
{
    private readonly List<Campo> _campos = [];

    public string Nombre { get; } = nombre;

    /// <summary>
    /// Campos en el orden en que se agregaron. Solo cambian a través de <see cref="AgregarCampo"/>.
    /// </summary>
    public IReadOnlyList<Campo> Campos => _campos.AsReadOnly();

    /// <summary>
    /// Agrega el campo al final. El nombre no puede repetirse, sin importar mayúsculas.
    /// </summary>
    public void AgregarCampo(Campo campo)
    {
        if (_campos.Any(existente => string.Equals(existente.Nombre, campo.Nombre, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException($"Ya existe un campo con el nombre \"{campo.Nombre}\".", nameof(campo));
        }

        _campos.Add(campo);
    }
}
