namespace Casos.Core.Modelo;

/// <summary>
/// Caso de prueba generado. La interfaz arma la descripción y el resultado esperado
/// a partir del campo probado, su clase y si el caso es válido.
/// </summary>
/// <param name="Codigo">Código consecutivo: CP-001, CP-002…</param>
/// <param name="Tecnica">Técnica que originó el caso.</param>
/// <param name="CampoProbado">Campo cuyo valor cambia en este caso; null en el caso con todos los valores por defecto.</param>
/// <param name="ClaseProbada">Clase a la que pertenece el valor probado; null en el caso con todos los valores por defecto.</param>
/// <param name="EsValido">Si el sistema debería aceptar los datos.</param>
/// <param name="Datos">El valor de cada campo por su nombre, en el orden de la historia.</param>
public sealed record CasoPrueba(
    string Codigo,
    Tecnica Tecnica,
    Campo? CampoProbado,
    TipoClase? ClaseProbada,
    bool EsValido,
    IReadOnlyDictionary<string, string> Datos);
