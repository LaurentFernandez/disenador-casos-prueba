namespace Casos.Core.Modelo;

/// <summary>
/// Por qué un campo no es válido. La interfaz arma el mensaje en cada idioma.
/// </summary>
public enum ErrorCampo
{
    /// <summary>La historia ya tiene un campo con ese nombre, sin importar mayúsculas.</summary>
    NombreRepetido,

    /// <summary>Un campo de lista no tiene opciones.</summary>
    ListaSinOpciones,

    /// <summary>La longitud mínima de un texto es mayor que la máxima.</summary>
    LongitudInvalida,

    /// <summary>La longitud mínima de un texto es negativa.</summary>
    LongitudNegativa,

    /// <summary>La longitud máxima de un texto supera <see cref="CampoTexto.LongitudMaximaPermitida"/>.</summary>
    LongitudExcesiva,

    /// <summary>El mínimo de un entero es mayor que el máximo.</summary>
    RangoInvalido,
}
