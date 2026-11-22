namespace Casos.Core.Modelo;

/// <summary>
/// Una regla no se puede aplicar a la historia. <see cref="Error"/> dice por qué; el mensaje de la excepción es solo para desarrolladores.
/// </summary>
public sealed class ReglaInvalidaException(ErrorRegla error, string nombreParametro, string? campoInexistente = null)
    : ArgumentException($"Regla inválida: {error}.", nombreParametro)
{
    public ErrorRegla Error { get; } = error;

    /// <summary>
    /// El nombre del campo que no existe, para que el aviso lo muestre; solo con <see cref="ErrorRegla.CampoInexistente"/>.
    /// </summary>
    public string? CampoInexistente { get; } = campoInexistente;
}
