namespace Casos.Core.Modelo;

/// <summary>
/// Por qué una regla no se puede aplicar a la historia. La interfaz arma el mensaje en cada idioma.
/// </summary>
public enum ErrorRegla
{
    /// <summary>La historia no tiene un campo con ese nombre.</summary>
    CampoInexistente,

    /// <summary>El campo de la condición no es de lista.</summary>
    CondicionNoEsLista,

    /// <summary>La opción de la condición no está en la lista.</summary>
    OpcionInexistente,

    /// <summary>La condición y el campo que la regla hace obligatorio son el mismo.</summary>
    MismoCampo,

    /// <summary>El campo que la regla hace obligatorio ya es obligatorio siempre.</summary>
    CampoYaObligatorio,
}
