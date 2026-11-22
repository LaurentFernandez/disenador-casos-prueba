using Casos.Core.Modelo;

namespace Casos.Web.Componentes;

/// <summary>
/// Una fila de la hoja de campos, tal como la escribe el usuario.
/// </summary>
public sealed class FilaCampo
{
    private const char SeparadorOpciones = ',';

    public string Nombre { get; set; } = "";

    public TipoCampo Tipo { get; set; }

    public bool Obligatorio { get; set; } = true;

    public int? Minimo { get; set; }

    public int? Maximo { get; set; }

    public string Opciones { get; set; } = "";

    /// <summary>
    /// Una fila sin nada escrito no se valida ni se usa.
    /// </summary>
    public bool EstaVacia =>
        string.IsNullOrWhiteSpace(Nombre) && Minimo is null && Maximo is null && string.IsNullOrWhiteSpace(Opciones);

    /// <summary>
    /// Convierte la fila en un campo del motor, que valida sus reglas y puede lanzar <see cref="CampoInvalidoException"/>.
    /// Devuelve null si a un entero o a un texto le falta el mínimo o el máximo.
    /// </summary>
    public Campo? ACampo() => (Tipo, Minimo, Maximo) switch
    {
        (TipoCampo.Lista, _, _) => new CampoLista(Nombre.Trim(), Obligatorio, OpcionesSeparadas()),
        (TipoCampo.Entero, int minimo, int maximo) => new CampoEntero(Nombre.Trim(), Obligatorio, minimo, maximo),
        (TipoCampo.Texto, int minimo, int maximo) => new CampoTexto(Nombre.Trim(), Obligatorio, minimo, maximo),
        _ => null,
    };

    private string[] OpcionesSeparadas() =>
        Opciones.Split(SeparadorOpciones, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
}
