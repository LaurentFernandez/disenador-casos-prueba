namespace Casos.Core.Modelo;

/// <summary>
/// Qué representa una clase de equivalencia. La interfaz arma la descripción en cada idioma.
/// </summary>
public enum TipoClase
{
    /// <summary>Valor, o longitud de texto, dentro del rango permitido.</summary>
    DentroDelRango,

    /// <summary>Número menor que el mínimo, o texto más corto que la longitud mínima sin llegar a vacío.</summary>
    MenorQueMinimo,

    /// <summary>Número mayor que el máximo, o texto más largo que la longitud máxima.</summary>
    MayorQueMaximo,

    /// <summary>Número con decimales en un campo entero.</summary>
    ConDecimales,

    /// <summary>Texto que no es un número en un campo entero.</summary>
    NoEsNumero,

    /// <summary>Una de las opciones de un campo de lista.</summary>
    Opcion,

    /// <summary>Valor que no está entre las opciones de un campo de lista.</summary>
    FueraDeLista,

    /// <summary>Campo sin valor. Es válida solo si el campo es opcional.</summary>
    Vacio,
}
