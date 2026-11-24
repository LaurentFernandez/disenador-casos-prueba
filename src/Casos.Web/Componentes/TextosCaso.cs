using System.Numerics;
using Casos.Core.Modelo;
using Casos.Web.Recursos;

namespace Casos.Web.Componentes;

/// <summary>
/// Textos de las vistas de los casos, armados a partir de lo que devuelve el motor.
/// Evitan el género gramatical porque los nombres de los campos los escribe el usuario.
/// </summary>
public static class TextosCaso
{
    private const int LargoVisible = 6;
    private const int PrefijoVisible = 4;
    private const string Separador = " · ";

    public static string Vacio => Textos.De("Valor.Vacio");

    /// <summary>
    /// El valor de un campo en un caso, como se muestra y se exporta. El de una opción fuera de la lista no es un dato
    /// que se escriba sino una descripción, así que sale en el idioma de la interfaz y nunca como una de las opciones.
    /// </summary>
    public static string Dato(CasoPrueba caso, Campo campo) =>
        caso.ClaseProbada == TipoClase.FueraDeLista && caso.CampoProbado == campo && campo is CampoLista lista
            ? lista.ValorQueNoEsOpcion(Textos.De("Valor.FueraDeLista"))
            : caso.Datos[campo.Nombre];

    public static string Descripcion(CasoPrueba caso)
    {
        // Una combinación por pares se describe con las opciones que junta, por ejemplo «Chrome · Windows · español».
        if (caso.CamposCombinados is { } combinados)
        {
            return string.Join(Separador, combinados.Select(campo => Valor(caso.Datos[campo.Nombre])));
        }

        if (caso.CampoProbado is not { } campo)
        {
            return Textos.De("Caso.TodosValidos");
        }

        return (caso.Regla, caso.Posicion) switch
        {
            ({ } regla, _) => DescripcionRegla(caso, regla, campo),
            (_, { } posicion) => DescripcionLimite(campo, posicion),
            _ => DescripcionClase(campo, caso.ClaseProbada, caso.Datos[campo.Nombre]),
        };
    }

    public static string Resultado(CasoPrueba caso) =>
        caso is { EsValido: false, CampoProbado: { } campo } ? Textos.De("Caso.Rechaza", campo.Nombre) : Textos.De("Caso.Acepta");

    public static string ValorProbado(CasoPrueba caso) => caso switch
    {
        { CamposCombinados: { } combinados } => string.Join(Separador, combinados.Select(campo => DatoDe(caso, campo))),
        // El campo de la condición siempre lleva una de sus opciones, así que se lee tal cual.
        { Regla: { } regla, CampoProbado: { } campo } =>
            $"{regla.CampoCondicion}: {Valor(caso.Datos[regla.CampoCondicion])}{Separador}{DatoDe(caso, campo)}",
        { CampoProbado: { } campo } => DatoDe(caso, campo),
        _ => Textos.De("Caso.ValoresPorDefecto"),
    };

    public static string NombreTecnica(Tecnica tecnica) => Textos.De(tecnica switch
    {
        Tecnica.ParticionEquivalencia => "Tecnica.Particion",
        Tecnica.ValoresLimite => "Tecnica.Limite",
        Tecnica.TablaDecision => "Tecnica.Decision",
        _ => "Tecnica.Pares",
    });

    public static string NombreLargoTecnica(Tecnica tecnica) => Textos.De(tecnica switch
    {
        Tecnica.ParticionEquivalencia => "Tecnica.ParticionLarga",
        Tecnica.ValoresLimite => "Tecnica.LimiteLarga",
        Tecnica.TablaDecision => "Tecnica.DecisionLarga",
        _ => "Tecnica.ParesLarga",
    });

    /// <summary>
    /// Cuántas combinaciones por pares salieron contra todas las posibles de los campos de lista, por ejemplo
    /// «Combinaciones por pares: 6 de 12 posibles».
    /// </summary>
    public static string TotalPares(Historia historia, IReadOnlyCollection<CasoPrueba> casos)
    {
        // BigInteger porque con muchas listas el producto no cabe en un long.
        var posibles = historia.Campos.OfType<CampoLista>()
            .Aggregate(BigInteger.One, (total, lista) => total * lista.Opciones.Distinct().Count());
        return Textos.De("Pares.Total", casos.Count(caso => caso.Tecnica == Tecnica.Pares), posibles);
    }

    public static string Resumen(IReadOnlyCollection<CasoPrueba> casos)
    {
        var aceptados = casos.Count(caso => caso.EsValido);
        var rechazados = casos.Count - aceptados;
        return Textos.De(
            "Resumen",
            casos.Count,
            Textos.De(aceptados == 1 ? "Resumen.AceptadoUno" : "Resumen.AceptadoVarios", aceptados),
            Textos.De(rechazados == 1 ? "Resumen.RechazadoUno" : "Resumen.RechazadoVarios", rechazados));
    }

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
        var obligatorio = Textos.De(campo.Obligatorio ? "Reglas.Obligatorio" : "Reglas.Opcional");
        return campo switch
        {
            CampoEntero entero => Textos.De("Reglas.Entero", obligatorio, Rango(entero.Minimo, entero.Maximo)),
            CampoTexto texto => Textos.De("Reglas.Texto", obligatorio, Rango(texto.LongitudMinima, texto.LongitudMaxima)),
            CampoLista lista => Textos.De("Reglas.Lista", obligatorio, Cantidad(lista.Opciones.Count, "Opciones.Una", "Opciones.Varias")),
            _ => obligatorio,
        };
    }

    public static string MenorQue(long minimo) => Textos.De("Rango.MenorQue", minimo);

    public static string MayorQue(long maximo) => Textos.De("Rango.MayorQue", maximo);

    public static string MasDe(long maximo) => Textos.De("Rango.MasDe", maximo);

    public static string Rango(long desde, long hasta) => desde == hasta ? $"{desde}" : Textos.De("Rango.DeA", desde, hasta);

    /// <summary>
    /// La recta de un campo contada en palabras, para quien usa un lector de pantalla.
    /// </summary>
    public static string DescripcionRecta(DiagramaRecta diagrama)
    {
        var clases = string.Join("; ", diagrama.Bandas.Select(banda => $"{banda.Nombre}, {Textos.De(banda.Valida ? "Clase.Valida" : "Clase.Invalida")}"));
        return Textos.De(
            "Recta.Descripcion",
            diagrama.Campo.Nombre,
            clases,
            ValoresDe(diagrama, Tecnica.ParticionEquivalencia),
            ValoresDe(diagrama, Tecnica.ValoresLimite));
    }

    private static string DescripcionClase(Campo campo, TipoClase? clase, string valor)
    {
        var nombre = ConMayuscula(campo.Nombre);
        return (clase, campo) switch
        {
            (TipoClase.MenorQueMinimo, CampoEntero entero) => $"{nombre} {MenorQue(entero.Minimo)}",
            (TipoClase.MayorQueMaximo, CampoEntero entero) => $"{nombre} {MayorQue(entero.Maximo)}",
            (TipoClase.MenorQueMinimo, CampoTexto texto) => Textos.De("Clase.TextoCorto", nombre, RangoDeCaracteres(1, texto.LongitudMinima - 1)),
            (TipoClase.MayorQueMaximo, CampoTexto texto) => Textos.De("Clase.TextoLargo", nombre, MasDe(texto.LongitudMaxima)),
            (TipoClase.ConDecimales, _) => Textos.De("Clase.ConDecimales", nombre),
            (TipoClase.NoEsNumero, _) => Textos.De("Clase.NoEsNumero", nombre),
            (TipoClase.Opcion, _) => $"{nombre}: {valor}",
            (TipoClase.FueraDeLista, _) => Textos.De("Clase.FueraDeLista", nombre),
            (TipoClase.Vacio, _) => Textos.De("Clase.Vacio", nombre),
            _ => nombre,
        };
    }

    // Por ejemplo «Tipo de cliente: Empresa y NIT sin valor»: el valor de la condición y si el campo obligatorio quedó vacío.
    private static string DescripcionRegla(CasoPrueba caso, ReglaObligatorio regla, Campo campo) =>
        Textos.De(
            caso.Datos[campo.Nombre].Length == 0 ? "Decision.SinValor" : "Decision.ConValor",
            ConMayuscula(regla.CampoCondicion),
            caso.Datos[regla.CampoCondicion],
            campo.Nombre);

    private static string DatoDe(CasoPrueba caso, Campo campo) => $"{campo.Nombre}: {Valor(Dato(caso, campo))}";

    // En un texto el rango es de longitud; la clave sale del nombre de la posición.
    private static string DescripcionLimite(Campo campo, PosicionLimite posicion) =>
        Textos.De($"Limite.{(campo is CampoTexto ? "Texto" : "Entero")}.{posicion}", ConMayuscula(campo.Nombre));

    private static string ValoresDe(DiagramaRecta diagrama, Tecnica tecnica) =>
        string.Join(", ", diagrama.Marcas.Where(marca => marca.Tecnica == tecnica).Select(marca => marca.Valor));

    private static string RangoDeCaracteres(int desde, int hasta) =>
        desde == hasta ? Caracteres(desde) : Textos.De("Rango.Caracteres", desde, Caracteres(hasta));

    public static string Caracteres(int cantidad) => Cantidad(cantidad, "Caracteres.Uno", "Caracteres.Varios");

    private static string Cantidad(int cantidad, string claveUno, string claveVarios) =>
        cantidad == 1 ? Textos.De(claveUno) : Textos.De(claveVarios, cantidad);

    private static string ConMayuscula(string texto) => string.Concat(texto[..1].ToUpperInvariant(), texto[1..]);
}
