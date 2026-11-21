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

    public void AgregarCampo(Campo campo) => _campos.Add(campo);
}
