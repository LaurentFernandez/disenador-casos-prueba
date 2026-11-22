using Casos.Web.Recursos;

namespace Casos.Web.Componentes;

/// <summary>
/// Historias ya preparadas, para ver qué hace la herramienta sin definir campos desde cero.
/// Sus textos salen del idioma de la aplicación porque, a diferencia de lo que escribe el usuario, son nuestros.
/// </summary>
public static class EjemplosHistoria
{
    /// <summary>
    /// Un ejemplo: el nombre de la historia y las filas de la hoja con sus reglas.
    /// </summary>
    public sealed record Ejemplo(string Historia, IReadOnlyList<FilaCampo> Filas);

    /// <summary>
    /// Los ejemplos, con filas nuevas en cada llamada porque la hoja modifica las filas que recibe.
    /// </summary>
    public static IReadOnlyList<Ejemplo> Todos() =>
    [
        new(Textos.De("Ejemplo.Registro"),
        [
            Entero("Ejemplo.Registro.Edad", 18, 65),
            Texto("Ejemplo.Registro.Nombre", 3, 50),
            Lista("Ejemplo.Registro.TipoCliente", "Ejemplo.Registro.Opciones"),
        ]),
        new(Textos.De("Ejemplo.Contrasena"),
        [
            Texto("Ejemplo.Contrasena.Correo", 6, 100),
            Entero("Ejemplo.Contrasena.Codigo", 100000, 999999),
            Texto("Ejemplo.Contrasena.Nueva", 8, 64),
        ]),
        new(Textos.De("Ejemplo.Carrito"),
        [
            Entero("Ejemplo.Carrito.Cantidad", 1, 99),
            Texto("Ejemplo.Carrito.Cupon", 5, 10, obligatorio: false),
            Lista("Ejemplo.Carrito.MedioPago", "Ejemplo.Carrito.Opciones"),
        ]),
    ];

    private static FilaCampo Entero(string clave, int minimo, int maximo) =>
        new() { Nombre = Textos.De(clave), Tipo = TipoCampo.Entero, Minimo = minimo, Maximo = maximo };

    private static FilaCampo Texto(string clave, int minimo, int maximo, bool obligatorio = true) =>
        new() { Nombre = Textos.De(clave), Tipo = TipoCampo.Texto, Obligatorio = obligatorio, Minimo = minimo, Maximo = maximo };

    private static FilaCampo Lista(string clave, string claveOpciones) =>
        new() { Nombre = Textos.De(clave), Tipo = TipoCampo.Lista, Opciones = Textos.De(claveOpciones) };
}
