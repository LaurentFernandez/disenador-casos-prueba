using Casos.Core.Modelo;
using Casos.Web.Recursos;

namespace Casos.Web.Componentes;

/// <summary>
/// Avisos que la hoja de campos muestra en una fila con problemas.
/// </summary>
public static class MensajesCampo
{
    public static string FaltaNombre => Textos.De("Aviso.FaltaNombre");

    public static string FaltaRango => Textos.De("Aviso.FaltaRango");

    public static string FaltaLongitud => Textos.De("Aviso.FaltaLongitud");

    public static string FaltaFechas => Textos.De("Aviso.FaltaFechas");

    public static string De(ErrorCampo error) => error switch
    {
        ErrorCampo.NombreRepetido => Textos.De("Aviso.NombreRepetido"),
        ErrorCampo.ListaSinOpciones => Textos.De("Aviso.ListaSinOpciones"),
        ErrorCampo.LongitudInvalida => Textos.De("Aviso.LongitudInvalida"),
        ErrorCampo.LongitudNegativa => Textos.De("Aviso.LongitudNegativa"),
        ErrorCampo.LongitudExcesiva => Textos.De("Aviso.LongitudExcesiva", CampoTexto.LongitudMaximaPermitida),
        ErrorCampo.RangoInvalido => Textos.De("Aviso.RangoInvalido"),
        ErrorCampo.RangoDeFechasInvalido => Textos.De("Aviso.RangoDeFechasInvalido"),
        _ => throw new ArgumentOutOfRangeException(nameof(error), error, null),
    };
}
