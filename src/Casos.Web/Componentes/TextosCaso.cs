using Casos.Core.Modelo;

namespace Casos.Web.Componentes;

// shortcut: los textos solo están en español; pasarlos a archivos .resx con el cambio de idioma (HU-08).
/// <summary>
/// Textos de la tabla de casos, armados a partir de lo que devuelve el motor.
/// Evitan el género gramatical porque los nombres de los campos los escribe el usuario.
/// </summary>
public static class TextosCaso
{
    private const int LargoVisible = 6;
    private const int PrefijoVisible = 4;

    public static string Descripcion(CasoPrueba caso)
    {
        if (caso.CampoProbado is not { } campo)
        {
            return "Todos los valores válidos";
        }

        return caso.Posicion is { } posicion
            ? DescripcionLimite(campo, posicion)
            : DescripcionClase(campo, caso.ClaseProbada, caso.Datos[campo.Nombre]);
    }

    public static string Resultado(CasoPrueba caso) =>
        caso is { EsValido: false, CampoProbado: { } campo } ? $"Rechaza el valor de {campo.Nombre}" : "Acepta los datos";

    public static string NombreTecnica(Tecnica tecnica) =>
        tecnica == Tecnica.ParticionEquivalencia ? "Partición" : "Límite";

    // Un texto de relleno largo se resume con su longitud para que la tabla se pueda leer.
    public static string Valor(string valor) =>
        valor.Length == 0 ? "vacío"
        : valor.Length > LargoVisible && valor.All(caracter => caracter == valor[0]) ? $"{valor[..PrefijoVisible]}… ({valor.Length})"
        : valor;

    private static string DescripcionClase(Campo campo, TipoClase? clase, string valor)
    {
        var nombre = ConMayuscula(campo.Nombre);
        return (clase, campo) switch
        {
            (TipoClase.MenorQueMinimo, CampoEntero entero) => $"{nombre} menor que {entero.Minimo}",
            (TipoClase.MayorQueMaximo, CampoEntero entero) => $"{nombre} mayor que {entero.Maximo}",
            (TipoClase.MenorQueMinimo, CampoTexto texto) => $"{nombre} de {RangoDeCaracteres(1, texto.LongitudMinima - 1)}",
            (TipoClase.MayorQueMaximo, CampoTexto texto) => $"{nombre} de más de {Caracteres(texto.LongitudMaxima)}",
            (TipoClase.ConDecimales, _) => $"{nombre} con decimales",
            (TipoClase.NoEsNumero, _) => $"{nombre} que no es un número",
            (TipoClase.Opcion, _) => $"{nombre}: {valor}",
            (TipoClase.FueraDeLista, _) => $"{nombre} fuera de la lista",
            (TipoClase.Vacio, _) => $"{nombre} sin valor",
            _ => nombre,
        };
    }

    private static string DescripcionLimite(Campo campo, PosicionLimite posicion)
    {
        var nombre = ConMayuscula(campo.Nombre);
        // En un texto el rango es de longitud.
        var (delMinimo, enElMinimo, delMaximo, enElMaximo) = campo is CampoTexto
            ? ("de la longitud mínima", "en la longitud mínima", "de la longitud máxima", "en la longitud máxima")
            : ("del mínimo", "en el mínimo", "del máximo", "en el máximo");
        return posicion switch
        {
            PosicionLimite.DebajoDelMinimo => $"{nombre} justo debajo {delMinimo}",
            PosicionLimite.EnElMinimo => $"{nombre} {enElMinimo}",
            PosicionLimite.EncimaDelMinimo => $"{nombre} justo encima {delMinimo}",
            PosicionLimite.DebajoDelMaximo => $"{nombre} justo debajo {delMaximo}",
            PosicionLimite.EnElMaximo => $"{nombre} {enElMaximo}",
            _ => $"{nombre} justo encima {delMaximo}",
        };
    }

    private static string RangoDeCaracteres(int desde, int hasta) =>
        desde == hasta ? Caracteres(desde) : $"{desde} a {Caracteres(hasta)}";

    private static string Caracteres(int cantidad) => cantidad == 1 ? "1 carácter" : $"{cantidad} caracteres";

    private static string ConMayuscula(string texto) => string.Concat(texto[..1].ToUpperInvariant(), texto[1..]);
}
