using Casos.Core.Modelo;
using Casos.Web.Recursos;

namespace Casos.Web.Componentes;

/// <summary>
/// La hoja de campos y las reglas leídas: la historia con los campos y las reglas válidos, el aviso de cada fila
/// con problemas y el de la combinación por pares si se pidió y no se puede.
/// </summary>
public sealed record LecturaHoja(
    Historia Historia,
    IReadOnlyDictionary<FilaCampo, string> Avisos,
    IReadOnlyDictionary<FilaRegla, string> AvisosReglas,
    string? AvisoPares = null)
{
    // Sin un nombre escrito, la historia igual necesita uno para el Gherkin y la exportación.
    public static string NombrePorDefecto => Textos.De("Historia.SinNombre");

    /// <summary>
    /// Con algún aviso no se generan casos, para que ningún campo ni regla quede fuera sin que se note.
    /// </summary>
    public bool TieneAvisos => Avisos.Count > 0 || AvisosReglas.Count > 0 || AvisoPares is not null;

    public static LecturaHoja Leer(
        IEnumerable<FilaCampo> filas, string nombreHistoria, IEnumerable<FilaRegla>? reglas = null, bool porPares = false)
    {
        var historia = new Historia(string.IsNullOrWhiteSpace(nombreHistoria) ? NombrePorDefecto : nombreHistoria.Trim());
        var avisos = new Dictionary<FilaCampo, string>();
        foreach (var fila in filas)
        {
            if (!fila.EstaVacia && AvisoDe(fila, historia) is { } aviso)
            {
                avisos[fila] = aviso;
            }
        }

        // Las reglas van después de los campos porque se comprueban contra ellos.
        var avisosReglas = new Dictionary<FilaRegla, string>();
        foreach (var regla in reglas ?? [])
        {
            if (!regla.EstaVacia && AvisoDe(regla, historia) is { } aviso)
            {
                avisosReglas[regla] = aviso;
            }
        }

        // También va después de los campos, porque necesita al menos dos de lista.
        return new LecturaHoja(historia, avisos, avisosReglas, porPares ? AvisoDePares(historia) : null);
    }

    // Si se puede, activa la combinación por pares; si no, devuelve el aviso.
    private static string? AvisoDePares(Historia historia)
    {
        if (!historia.PuedeCombinarPorPares)
        {
            return Textos.De("Aviso.ParesSinListas");
        }

        historia.CombinarPorPares();
        return null;
    }

    // Si la fila es válida agrega su campo a la historia; si no, devuelve el aviso.
    private static string? AvisoDe(FilaCampo fila, Historia historia)
    {
        if (string.IsNullOrWhiteSpace(fila.Nombre))
        {
            return MensajesCampo.FaltaNombre;
        }

        try
        {
            var campo = fila.ACampo();
            if (campo is null)
            {
                return fila.Tipo == TipoCampo.Texto ? MensajesCampo.FaltaLongitud : MensajesCampo.FaltaRango;
            }

            historia.AgregarCampo(campo);
            return null;
        }
        catch (CampoInvalidoException excepcion)
        {
            return MensajesCampo.De(excepcion.Error);
        }
    }

    // Si la regla es válida la agrega a la historia; si no, devuelve el aviso.
    private static string? AvisoDe(FilaRegla regla, Historia historia)
    {
        if (!regla.EstaCompleta)
        {
            return MensajesRegla.Incompleta;
        }

        try
        {
            historia.AgregarRegla(regla.ARegla());
            return null;
        }
        catch (ReglaInvalidaException excepcion)
        {
            return MensajesRegla.De(excepcion);
        }
    }
}
