using Casos.Core.Modelo;

namespace Casos.Web.Componentes;

/// <summary>
/// Una fila de las reglas de negocio, tal como la elige el usuario: "si [campo] es [opción], [campo] es obligatorio".
/// </summary>
public sealed class FilaRegla
{
    public string CampoCondicion { get; set; } = "";

    public string Opcion { get; set; } = "";

    public string CampoObligatorio { get; set; } = "";

    /// <summary>
    /// Una regla sin nada elegido no se valida ni se usa.
    /// </summary>
    public bool EstaVacia => CampoCondicion == "" && Opcion == "" && CampoObligatorio == "";

    public bool EstaCompleta => CampoCondicion != "" && Opcion != "" && CampoObligatorio != "";

    public ReglaObligatorio ARegla() => new(CampoCondicion, Opcion, CampoObligatorio);
}
