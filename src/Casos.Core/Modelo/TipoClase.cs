namespace Casos.Core.Modelo;

/// <summary>
/// Qué representa una clase de equivalencia. La interfaz arma la descripción en cada idioma.
/// </summary>
public enum TipoClase
{
    /// <summary>Valor, fecha o longitud de texto dentro del rango permitido.</summary>
    DentroDelRango,

    /// <summary>Número menor que el mínimo, fecha anterior a la mínima, o texto más corto que la longitud mínima sin llegar a vacío.</summary>
    MenorQueMinimo,

    /// <summary>Número mayor que el máximo, fecha posterior a la máxima, o texto más largo que la longitud máxima.</summary>
    MayorQueMaximo,

    /// <summary>Número con decimales en un campo entero.</summary>
    ConDecimales,

    /// <summary>Texto que no es un número en un campo entero.</summary>
    NoEsNumero,

    /// <summary>Fecha con el formato correcto pero que no existe en el calendario, como el 29 de febrero de un año no bisiesto.</summary>
    FechaInexistente,

    /// <summary>Texto que no es una fecha en un campo de fecha.</summary>
    NoEsFecha,

    /// <summary>Una de las opciones de un campo de lista.</summary>
    Opcion,

    /// <summary>Valor que no está entre las opciones de un campo de lista.</summary>
    FueraDeLista,

    /// <summary>Campo sin valor. Es válida solo si el campo es opcional.</summary>
    Vacio,
}
