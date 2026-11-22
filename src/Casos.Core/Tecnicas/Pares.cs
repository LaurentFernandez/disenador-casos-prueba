using Casos.Core.Modelo;

namespace Casos.Core.Tecnicas;

/// <summary>
/// Combinación por pares (pairwise, ISTQB): pocas combinaciones de opciones que, juntas, contienen cada pareja de
/// opciones de dos campos al menos una vez. Usa el algoritmo IPO (In-Parameter-Order): empieza con todas las parejas
/// de los dos campos con más opciones y agrega los demás campos de a uno, primero eligiendo para cada combinación la
/// opción que cubre más parejas pendientes y después agregando combinaciones para las parejas que falten.
/// Es determinista: los mismos campos dan siempre las mismas combinaciones.
/// </summary>
public static class Pares
{
    private const int CamposMinimos = 2;

    /// <summary>
    /// Devuelve las combinaciones; cada una trae una opción por campo, en el orden de los campos recibidos.
    /// </summary>
    public static IReadOnlyList<IReadOnlyList<string>> Generar(IReadOnlyList<CampoLista> campos)
    {
        if (campos.Count < CamposMinimos)
        {
            throw new ArgumentException("Se necesitan al menos dos campos de lista.", nameof(campos));
        }

        var opciones = campos.Select(campo => campo.Opciones.Distinct().ToArray()).ToArray();
        // Empezar por los campos con más opciones da menos combinaciones; OrderByDescending respeta el orden en los empates.
        var orden = Enumerable.Range(0, campos.Count).OrderByDescending(campo => opciones[campo].Length).ToArray();

        // Cada combinación guarda el índice de la opción de cada campo; null es un hueco que ninguna pareja necesitó todavía.
        var combinaciones = new List<int?[]>();
        foreach (var primera in Indices(opciones[orden[0]]))
        {
            foreach (var segunda in Indices(opciones[orden[1]]))
            {
                combinaciones.Add(Combinacion(campos.Count, (orden[0], primera), (orden[1], segunda)));
            }
        }

        for (var paso = CamposMinimos; paso < orden.Length; paso++)
        {
            Agregar(orden[paso], orden[..paso], opciones, combinaciones);
        }

        // Los huecos que quedan pueden llevar cualquier opción; se usa la primera.
        return combinaciones
            .Select(combinacion => (IReadOnlyList<string>)combinacion.Select((opcion, campo) => opciones[campo][opcion ?? 0]).ToArray())
            .ToArray();
    }

    // Agrega un campo a las combinaciones, cubriendo todas sus parejas con los campos que ya están.
    private static void Agregar(int campo, int[] anteriores, string[][] opciones, List<int?[]> combinaciones)
    {
        var pendientes = (
            from anterior in anteriores
            from opcionAnterior in Indices(opciones[anterior])
            from opcion in Indices(opciones[campo])
            select (Anterior: anterior, OpcionAnterior: opcionAnterior, Opcion: opcion)).ToList();

        // A lo ancho: cada combinación toma la opción que cubre más parejas pendientes; en un empate, la primera.
        foreach (var combinacion in combinaciones)
        {
            var elegida = Indices(opciones[campo]).MaxBy(opcion => anteriores.Count(anterior =>
                combinacion[anterior] is { } opcionAnterior && pendientes.Contains((anterior, opcionAnterior, opcion))));
            combinacion[campo] = elegida;
            pendientes.RemoveAll(pareja => pareja.Opcion == elegida && combinacion[pareja.Anterior] == pareja.OpcionAnterior);
        }

        // A lo largo: cada pareja que falta llena un hueco de una combinación que ya tiene esa opción, o abre una nueva.
        foreach (var (anterior, opcionAnterior, opcion) in pendientes)
        {
            if (combinaciones.Any(combinacion => combinacion[campo] == opcion && combinacion[anterior] == opcionAnterior))
            {
                continue;
            }

            var conHueco = combinaciones.FirstOrDefault(combinacion => combinacion[campo] == opcion && combinacion[anterior] is null);
            if (conHueco is not null)
            {
                conHueco[anterior] = opcionAnterior;
            }
            else
            {
                combinaciones.Add(Combinacion(opciones.Length, (anterior, opcionAnterior), (campo, opcion)));
            }
        }
    }

    private static int?[] Combinacion(int campos, params (int Campo, int Opcion)[] elegidas)
    {
        var combinacion = new int?[campos];
        foreach (var (campo, opcion) in elegidas)
        {
            combinacion[campo] = opcion;
        }

        return combinacion;
    }

    private static IEnumerable<int> Indices(string[] opciones) => Enumerable.Range(0, opciones.Length);
}
