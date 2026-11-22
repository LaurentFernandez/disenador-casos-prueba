using Casos.Core.Modelo;
using Casos.Web.Recursos;

namespace Casos.Web.Componentes;

/// <summary>
/// Avisos que la sección de reglas muestra en una regla con problemas.
/// </summary>
public static class MensajesRegla
{
    public static string Incompleta => Textos.De("Aviso.ReglaIncompleta");

    public static string De(ReglaInvalidaException excepcion) => excepcion.Error switch
    {
        ErrorRegla.CampoInexistente => Textos.De("Aviso.CampoInexistente", excepcion.CampoInexistente ?? ""),
        ErrorRegla.CondicionNoEsLista => Textos.De("Aviso.CondicionNoEsLista"),
        ErrorRegla.OpcionInexistente => Textos.De("Aviso.OpcionInexistente"),
        ErrorRegla.MismoCampo => Textos.De("Aviso.MismoCampo"),
        ErrorRegla.CampoYaObligatorio => Textos.De("Aviso.CampoYaObligatorio"),
        _ => throw new ArgumentOutOfRangeException(nameof(excepcion), excepcion.Error, null),
    };
}
