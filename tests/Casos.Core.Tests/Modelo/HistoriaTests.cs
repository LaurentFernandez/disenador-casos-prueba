using Casos.Core.Modelo;

namespace Casos.Core.Tests.Modelo;

public class HistoriaTests
{
    [Fact]
    public void AgregarCampo_CampoEntero_QuedaEnLaListaConSusReglas()
    {
        var historia = new Historia("Registro de usuario");

        historia.AgregarCampo(new CampoEntero("edad", Obligatorio: true, Minimo: 18, Maximo: 65));

        Assert.Equal(new CampoEntero("edad", true, 18, 65), Assert.Single(historia.Campos));
    }

    [Fact]
    public void AgregarCampo_CampoTexto_QuedaEnLaListaConSusReglas()
    {
        var historia = new Historia("Registro de usuario");

        historia.AgregarCampo(new CampoTexto("nombre", Obligatorio: true, LongitudMinima: 3, LongitudMaxima: 50));

        Assert.Equal(new CampoTexto("nombre", true, 3, 50), Assert.Single(historia.Campos));
    }

    [Fact]
    public void AgregarCampo_CampoLista_QuedaEnLaListaConSusOpciones()
    {
        var historia = new Historia("Registro de usuario");

        historia.AgregarCampo(new CampoLista("tipo de cliente", Obligatorio: true, ["Persona", "Empresa"]));

        var campo = Assert.IsType<CampoLista>(Assert.Single(historia.Campos));
        Assert.Equal("tipo de cliente", campo.Nombre);
        Assert.Equal(new[] { "Persona", "Empresa" }, campo.Opciones);
    }

    [Fact]
    public void AgregarCampo_VariosCampos_ConservaElOrden()
    {
        var historia = new Historia("Registro de usuario");

        historia.AgregarCampo(new CampoTexto("nombre", true, 3, 50));
        historia.AgregarCampo(new CampoEntero("edad", true, 18, 65));

        Assert.Equal(new[] { "nombre", "edad" }, historia.Campos.Select(campo => campo.Nombre));
    }

    [Theory]
    [InlineData("edad")]
    [InlineData("Edad")]
    public void AgregarCampo_NombreRepetido_IndicaNombreRepetidoYNoLoAgrega(string nombreRepetido)
    {
        var historia = new Historia("Registro de usuario");
        historia.AgregarCampo(new CampoEntero("edad", true, 18, 65));

        var excepcion = Assert.Throws<CampoInvalidoException>(() => historia.AgregarCampo(new CampoTexto(nombreRepetido, true, 1, 3)));

        Assert.Equal(ErrorCampo.NombreRepetido, excepcion.Error);
        Assert.Single(historia.Campos);
    }

    [Theory]
    [InlineData("tipo de cliente", "NIT")]
    [InlineData("Tipo de Cliente", "nit")]
    public void AgregarRegla_ReglaQueTieneSentido_QuedaConLosNombresDeLaHistoria(string campoCondicion, string campoObligatorio)
    {
        var historia = HistoriaConClienteYNit();

        historia.AgregarRegla(new ReglaObligatorio(campoCondicion, "Empresa", campoObligatorio));

        Assert.Equal(new ReglaObligatorio("tipo de cliente", "Empresa", "NIT"), Assert.Single(historia.Reglas));
    }

    [Theory]
    [InlineData("tipo", "NIT", "tipo")]
    [InlineData("tipo de cliente", "RUT", "RUT")]
    public void AgregarRegla_CampoQueNoExiste_IndicaCualFaltaYNoLaAgrega(string campoCondicion, string campoObligatorio, string faltante)
    {
        var historia = HistoriaConClienteYNit();

        var excepcion = Assert.Throws<ReglaInvalidaException>(() =>
            historia.AgregarRegla(new ReglaObligatorio(campoCondicion, "Empresa", campoObligatorio)));

        Assert.Equal(ErrorRegla.CampoInexistente, excepcion.Error);
        Assert.Equal(faltante, excepcion.CampoInexistente);
        Assert.Empty(historia.Reglas);
    }

    [Fact]
    public void AgregarRegla_HistoriaSinElCampo_AvisaQueNoExisteComoEnHU11()
    {
        var historia = new Historia("Registro de cliente");
        historia.AgregarCampo(new CampoLista("tipo de cliente", true, ["Persona", "Empresa"]));

        var excepcion = Assert.Throws<ReglaInvalidaException>(() =>
            historia.AgregarRegla(new ReglaObligatorio("tipo de cliente", "Empresa", "NIT")));

        Assert.Equal(ErrorRegla.CampoInexistente, excepcion.Error);
        Assert.Equal("NIT", excepcion.CampoInexistente);
    }

    [Theory]
    [InlineData("NIT", "Empresa", "tipo de cliente", ErrorRegla.CondicionNoEsLista)]
    [InlineData("tipo de cliente", "Gobierno", "NIT", ErrorRegla.OpcionInexistente)]
    [InlineData("tipo de cliente", "Empresa", "Tipo de cliente", ErrorRegla.MismoCampo)]
    [InlineData("tipo de cliente", "Empresa", "edad", ErrorRegla.CampoYaObligatorio)]
    public void AgregarRegla_ReglaSinSentido_IndicaElErrorYNoLaAgrega(
        string campoCondicion, string opcion, string campoObligatorio, ErrorRegla error)
    {
        var historia = HistoriaConClienteYNit();
        historia.AgregarCampo(new CampoEntero("edad", true, 18, 65));

        var excepcion = Assert.Throws<ReglaInvalidaException>(() =>
            historia.AgregarRegla(new ReglaObligatorio(campoCondicion, opcion, campoObligatorio)));

        Assert.Equal(error, excepcion.Error);
        Assert.Empty(historia.Reglas);
    }

    private static Historia HistoriaConClienteYNit()
    {
        var historia = new Historia("Registro de cliente");
        historia.AgregarCampo(new CampoLista("tipo de cliente", true, ["Persona", "Empresa"]));
        historia.AgregarCampo(new CampoTexto("NIT", false, 9, 10));
        return historia;
    }
}
