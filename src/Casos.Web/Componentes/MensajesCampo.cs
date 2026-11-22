using Casos.Core.Modelo;

namespace Casos.Web.Componentes;

// shortcut: los avisos solo están en español; pasarlos a archivos .resx con el cambio de idioma (HU-08).
/// <summary>
/// Avisos que la hoja de campos muestra en una fila con problemas.
/// </summary>
public static class MensajesCampo
{
    public const string FaltaNombre = "Escribe el nombre del campo.";

    public const string FaltaRango = "Escribe el mínimo y el máximo.";

    public const string FaltaLongitud = "Escribe la longitud mínima y la máxima.";

    public static string De(ErrorCampo error) => error switch
    {
        ErrorCampo.NombreRepetido => "Ya hay otro campo con este nombre.",
        ErrorCampo.ListaSinOpciones => "Escribe al menos una opción; sepáralas con comas.",
        ErrorCampo.LongitudInvalida => "La longitud mínima no puede ser mayor que la máxima.",
        ErrorCampo.LongitudNegativa => "La longitud no puede ser negativa.",
        ErrorCampo.LongitudExcesiva => $"La longitud máxima no puede pasar de {CampoTexto.LongitudMaximaPermitida} caracteres.",
        ErrorCampo.RangoInvalido => "El mínimo no puede ser mayor que el máximo.",
        _ => throw new ArgumentOutOfRangeException(nameof(error), error, null),
    };
}
