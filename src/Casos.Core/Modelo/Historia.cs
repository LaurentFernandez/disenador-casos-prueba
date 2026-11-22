namespace Casos.Core.Modelo;

/// <summary>
/// Historia de usuario con los campos que se van a probar y sus reglas de negocio.
/// </summary>
public class Historia(string nombre)
{
    private readonly List<Campo> _campos = [];
    private readonly List<ReglaObligatorio> _reglas = [];

    public string Nombre { get; } = nombre;

    /// <summary>
    /// Campos en el orden en que se agregaron. Solo cambian a través de <see cref="AgregarCampo"/>.
    /// </summary>
    public IReadOnlyList<Campo> Campos => _campos.AsReadOnly();

    /// <summary>
    /// Reglas en el orden en que se agregaron. Solo cambian a través de <see cref="AgregarRegla"/>.
    /// </summary>
    public IReadOnlyList<ReglaObligatorio> Reglas => _reglas.AsReadOnly();

    /// <summary>
    /// Agrega el campo al final. El nombre no puede repetirse, sin importar mayúsculas.
    /// </summary>
    public void AgregarCampo(Campo campo)
    {
        if (CampoLlamado(campo.Nombre) is not null)
        {
            throw new CampoInvalidoException(ErrorCampo.NombreRepetido, nameof(campo));
        }

        _campos.Add(campo);
    }

    /// <summary>
    /// Agrega la regla si tiene sentido para los campos que ya están: la condición es una opción de un campo de lista
    /// y el campo que hace obligatorio es otro, opcional. Si no, lanza <see cref="ReglaInvalidaException"/>.
    /// La regla se guarda con los nombres de los campos tal como están en la historia, para buscarlos en los datos de un caso.
    /// </summary>
    public void AgregarRegla(ReglaObligatorio regla)
    {
        var condicion = CampoLlamado(regla.CampoCondicion)
            ?? throw new ReglaInvalidaException(ErrorRegla.CampoInexistente, nameof(regla), regla.CampoCondicion);
        var obligatorio = CampoLlamado(regla.CampoObligatorio)
            ?? throw new ReglaInvalidaException(ErrorRegla.CampoInexistente, nameof(regla), regla.CampoObligatorio);

        if (condicion is not CampoLista lista)
        {
            throw new ReglaInvalidaException(ErrorRegla.CondicionNoEsLista, nameof(regla));
        }

        if (!lista.Opciones.Contains(regla.Opcion))
        {
            throw new ReglaInvalidaException(ErrorRegla.OpcionInexistente, nameof(regla));
        }

        if (ReferenceEquals(condicion, obligatorio))
        {
            throw new ReglaInvalidaException(ErrorRegla.MismoCampo, nameof(regla));
        }

        if (obligatorio.Obligatorio)
        {
            throw new ReglaInvalidaException(ErrorRegla.CampoYaObligatorio, nameof(regla));
        }

        _reglas.Add(regla with { CampoCondicion = condicion.Nombre, CampoObligatorio = obligatorio.Nombre });
    }

    /// <summary>
    /// El campo con ese nombre, sin importar mayúsculas, o null si la historia no lo tiene.
    /// </summary>
    public Campo? CampoLlamado(string nombre) =>
        _campos.Find(campo => string.Equals(campo.Nombre, nombre, StringComparison.OrdinalIgnoreCase));
}
