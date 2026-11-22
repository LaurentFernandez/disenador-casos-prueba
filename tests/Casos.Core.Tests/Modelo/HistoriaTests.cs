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
}
