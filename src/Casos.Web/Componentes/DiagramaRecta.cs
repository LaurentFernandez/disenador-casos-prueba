using System.Globalization;
using Casos.Core.Modelo;

namespace Casos.Web.Componentes;

/// <summary>
/// Lo que la vista Recta dibuja de un campo, armado con sus casos generados: las bandas de sus clases,
/// una marca por cada valor probado sobre la recta y, en fichas, los valores que no caben en ella.
/// Las posiciones van de 0 a 1 a lo largo de la recta, que no está a escala.
/// </summary>
public sealed record DiagramaRecta(
    Campo Campo,
    IReadOnlyList<DiagramaRecta.Banda> Bandas,
    IReadOnlyList<DiagramaRecta.Marca> Marcas,
    IReadOnlyList<DiagramaRecta.Ficha> Fichas)
{
    // Distancia en unidades entre dos valores seguidos de la recta: su diferencia real, con un tope para que los
    // saltos grandes no aplasten a los bordes. Cada extremo lleva un margen para que se vean las bandas exteriores.
    private const long SaltoMaximo = 6;
    private const long Margen = 3;

    /// <summary>
    /// Tramo de la recta que ocupa una clase.
    /// </summary>
    public sealed record Banda(double Desde, double Hasta, bool Valida, string Nombre);

    /// <summary>
    /// Un valor probado sobre la recta, con los códigos de los casos que lo usan.
    /// </summary>
    public sealed record Marca(double Posicion, long Valor, Tecnica Tecnica, IReadOnlyList<string> Codigos);

    /// <summary>
    /// Un valor que no es un punto de la recta: una opción de una lista, un decimal, un texto o el vacío.
    /// </summary>
    public sealed record Ficha(string Texto, bool Valida, IReadOnlyList<string> Codigos);

    public static DiagramaRecta Crear(Campo campo, IReadOnlyList<CasoPrueba> casos)
    {
        var puntos = new List<(long Valor, Tecnica Tecnica, string Codigo)>();
        var fichas = new List<(string Texto, bool Valida, string Codigo)>();
        // El caso con todos los valores por defecto también prueba este campo. Los de una tabla de decisión y los de
        // pares cambian varios campos a la vez y no son un punto de la recta de ninguno.
        foreach (var caso in casos.Where(caso =>
            caso.Tecnica is not (Tecnica.TablaDecision or Tecnica.Pares) && (caso.CampoProbado is null || caso.CampoProbado == campo)))
        {
            var valor = TextosCaso.Dato(caso, campo);
            if (ValorEnRecta(campo, valor) is { } numero)
            {
                puntos.Add((numero, caso.Tecnica, caso.Codigo));
            }
            else
            {
                fichas.Add((TextosCaso.Valor(valor), caso.EsValido, caso.Codigo));
            }
        }

        var posiciones = Posiciones(puntos.Select(punto => punto.Valor));
        var marcas = puntos
            .GroupBy(punto => (punto.Valor, punto.Tecnica))
            .OrderBy(grupo => grupo.Key.Valor)
            .ThenBy(grupo => grupo.Key.Tecnica)
            .Select(grupo => new Marca(posiciones[grupo.Key.Valor], grupo.Key.Valor, grupo.Key.Tecnica, grupo.Select(punto => punto.Codigo).ToArray()))
            .ToArray();
        var fichasAgrupadas = fichas
            .GroupBy(ficha => (ficha.Texto, ficha.Valida))
            .Select(grupo => new Ficha(grupo.Key.Texto, grupo.Key.Valida, grupo.Select(ficha => ficha.Codigo).ToArray()))
            .ToArray();

        return new DiagramaRecta(campo, BandasDe(campo, posiciones), marcas, fichasAgrupadas);
    }

    private static long? ValorEnRecta(Campo campo, string valor) => campo switch
    {
        // En un texto la recta es la longitud, y el vacío es la longitud 0.
        CampoTexto => valor.Length,
        CampoEntero when long.TryParse(valor, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var numero) => numero,
        _ => null,
    };

    private static Dictionary<long, double> Posiciones(IEnumerable<long> valores)
    {
        var ordenados = valores.Distinct().Order().ToList();
        var unidades = new Dictionary<long, long>();
        var recorrido = Margen;
        for (var indice = 0; indice < ordenados.Count; indice++)
        {
            if (indice > 0)
            {
                recorrido += Math.Min(ordenados[indice] - ordenados[indice - 1], SaltoMaximo);
            }

            unidades[ordenados[indice]] = recorrido;
        }

        var largo = (double)(recorrido + Margen);
        return unidades.ToDictionary(par => par.Key, par => par.Value / largo);
    }

    // Los bordes caen entre dos valores seguidos; los casos generados siempre traen los vecinos de cada borde.
    private static IReadOnlyList<Banda> BandasDe(Campo campo, Dictionary<long, double> posiciones)
    {
        double Borde(long izquierda, long derecha) => (posiciones[izquierda] + posiciones[derecha]) / 2;

        switch (campo)
        {
            case CampoEntero entero:
            {
                var bajo = Borde(entero.Minimo - 1L, entero.Minimo);
                var alto = Borde(entero.Maximo, entero.Maximo + 1L);
                return
                [
                    new Banda(0, bajo, false, TextosCaso.MenorQue(entero.Minimo)),
                    new Banda(bajo, alto, true, TextosCaso.Rango(entero.Minimo, entero.Maximo)),
                    new Banda(alto, 1, false, TextosCaso.MayorQue(entero.Maximo)),
                ];
            }
            case CampoTexto texto:
            {
                // La longitud 0 es el campo vacío: válido solo si el campo es opcional.
                var finVacio = Borde(0, posiciones.Keys.Where(valor => valor > 0).Min());
                var bandas = new List<Banda> { new(0, finVacio, !texto.Obligatorio, TextosCaso.Vacio) };
                var desde = finVacio;
                if (texto.LongitudMinima > 1)
                {
                    var finCorto = Borde(texto.LongitudMinima - 1L, texto.LongitudMinima);
                    bandas.Add(new Banda(desde, finCorto, false, TextosCaso.Rango(1, texto.LongitudMinima - 1L)));
                    desde = finCorto;
                }

                var finValido = Borde(texto.LongitudMaxima, texto.LongitudMaxima + 1L);
                bandas.Add(new Banda(desde, finValido, true, TextosCaso.Rango(texto.LongitudMinima, texto.LongitudMaxima)));
                bandas.Add(new Banda(finValido, 1, false, TextosCaso.MasDe(texto.LongitudMaxima)));
                return bandas;
            }
            default:
                return [];
        }
    }
}
