using Casos.Core.Modelo;

namespace Casos.Web.Componentes;

// shortcut: los textos solo están en español; pasarlos a archivos .resx con el cambio de idioma (HU-08).
/// <summary>
/// Textos de las vistas de los casos, armados a partir de lo que devuelve el motor.
/// Evitan el género gramatical porque los nombres de los campos los escribe el usuario.
/// </summary>
public static class TextosCaso
{
    public const string Vacio = "vacío";

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

    public static string ValorProbado(CasoPrueba caso) =>
        caso.CampoProbado is { } campo ? $"{campo.Nombre}: {Valor(caso.Datos[campo.Nombre])}" : "Valores por defecto";

    public static string NombreTecnica(Tecnica tecnica) =>
        tecnica == Tecnica.ParticionEquivalencia ? "Partición" : "Límite";

    // Un texto de relleno largo se resume con su longitud para que se pueda leer.
    public static string Valor(string valor) =>
        valor.Length == 0 ? Vacio
        : valor.Length > LargoVisible && valor.All(caracter => caracter == valor[0]) ? $"{valor[..PrefijoVisible]}… ({valor.Length})"
        : valor;

    /// <summary>
    /// Tipo y reglas de un campo, por ejemplo «Entero, obligatorio, de 18 a 65».
    /// </summary>
    public static string Reglas(Campo campo)
    {
        var obligatorio = campo.Obligatorio ? "obligatorio" : "opcional";
        return campo switch
        {
            CampoEntero entero => $"Entero, {obligatorio}, {Rango(entero.Minimo, entero.Maximo)}",
            CampoTexto texto => $"Texto, {obligatorio}, {Rango(texto.LongitudMinima, texto.LongitudMaxima)} caracteres",
            CampoLista lista => $"Lista, {obligatorio}, con {Cantidad(lista.Opciones.Count, "opción", "opciones")}",
            _ => obligatorio,
        };
    }

    public static string MenorQue(long minimo) => $"menor que {minimo}";

    public static string MayorQue(long maximo) => $"mayor que {maximo}";

    public static string MasDe(long maximo) => $"más de {maximo}";

    public static string Rango(long desde, long hasta) => desde == hasta ? $"{desde}" : $"de {desde} a {hasta}";

    /// <summary>
    /// La recta de un campo contada en palabras, para quien usa un lector de pantalla.
    /// </summary>
    public static string DescripcionRecta(DiagramaRecta diagrama)
    {
        var clases = string.Join("; ", diagrama.Bandas.Select(banda => $"{banda.Nombre}, {(banda.Valida ? "válido" : "inválido")}"));
        return $"Recta de {diagrama.Campo.Nombre}: {clases}. "
            + $"Valores de partición: {ValoresDe(diagrama, Tecnica.ParticionEquivalencia)}. "
            + $"Valores límite: {ValoresDe(diagrama, Tecnica.ValoresLimite)}.";
    }

    private static string DescripcionClase(Campo campo, TipoClase? clase, string valor)
    {
        var nombre = ConMayuscula(campo.Nombre);
        return (clase, campo) switch
        {
            (TipoClase.MenorQueMinimo, CampoEntero entero) => $"{nombre} {MenorQue(entero.Minimo)}",
            (TipoClase.MayorQueMaximo, CampoEntero entero) => $"{nombre} {MayorQue(entero.Maximo)}",
            (TipoClase.MenorQueMinimo, CampoTexto texto) => $"{nombre} de {RangoDeCaracteres(1, texto.LongitudMinima - 1)}",
            (TipoClase.MayorQueMaximo, CampoTexto texto) => $"{nombre} de {MasDe(texto.LongitudMaxima)} caracteres",
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

    private static string ValoresDe(DiagramaRecta diagrama, Tecnica tecnica) =>
        string.Join(", ", diagrama.Marcas.Where(marca => marca.Tecnica == tecnica).Select(marca => marca.Valor));

    private static string RangoDeCaracteres(int desde, int hasta) =>
        desde == hasta ? Caracteres(desde) : $"{desde} a {Caracteres(hasta)}";

    private static string Caracteres(int cantidad) => Cantidad(cantidad, "carácter", "caracteres");

    private static string Cantidad(int cantidad, string singular, string plural) =>
        cantidad == 1 ? $"1 {singular}" : $"{cantidad} {plural}";

    private static string ConMayuscula(string texto) => string.Concat(texto[..1].ToUpperInvariant(), texto[1..]);
}
