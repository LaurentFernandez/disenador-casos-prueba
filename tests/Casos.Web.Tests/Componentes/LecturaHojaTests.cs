using Casos.Core.Modelo;
using Casos.Web.Componentes;

namespace Casos.Web.Tests.Componentes;

public class LecturaHojaTests() : PruebaEnIdioma("es")
{
    private const string Historia = "Registro de usuario";

    [Fact]
    public void Leer_FilasValidas_ArmaLaHistoriaSinAvisos()
    {
        var lectura = LecturaHoja.Leer([Entero("edad", 18, 65), Texto("nombre", 3, 50)], Historia);

        Assert.Empty(lectura.Avisos);
        Assert.Equal(new[] { "edad", "nombre" }, lectura.Historia.Campos.Select(campo => campo.Nombre));
    }

    [Fact]
    public void Leer_NombreDeLaHistoria_SeGuardaSinEspaciosSobrantes()
    {
        var lectura = LecturaHoja.Leer([], "  Registro de usuario  ");

        Assert.Equal("Registro de usuario", lectura.Historia.Nombre);
    }

    [Fact]
    public void Leer_HistoriaSinNombre_UsaElNombrePorDefecto()
    {
        var lectura = LecturaHoja.Leer([], " ");

        Assert.Equal("Historia sin nombre", lectura.Historia.Nombre);
    }

    [Fact]
    public void Leer_FilaVacia_NoAvisaNiAgregaCampos()
    {
        var lectura = LecturaHoja.Leer([new FilaCampo()], Historia);

        Assert.Empty(lectura.Avisos);
        Assert.Empty(lectura.Historia.Campos);
    }

    [Fact]
    public void Leer_NombreRepetido_AvisaEnLaSegundaFilaYNoLaAgrega()
    {
        var repetida = Entero("Edad", 1, 2);

        var lectura = LecturaHoja.Leer([Entero("edad", 18, 65), repetida], Historia);

        Assert.Single(lectura.Avisos);
        Assert.Equal(MensajesCampo.De(ErrorCampo.NombreRepetido), lectura.Avisos[repetida]);
        Assert.Single(lectura.Historia.Campos);
    }

    [Theory]
    [InlineData(TipoCampo.Entero, "Escribe el mínimo y el máximo.")]
    [InlineData(TipoCampo.Texto, "Escribe la longitud mínima y la máxima.")]
    public void Leer_SinMinimoNiMaximo_AvisaQueFaltan(TipoCampo tipo, string aviso)
    {
        var fila = new FilaCampo { Nombre = "precio", Tipo = tipo };

        var lectura = LecturaHoja.Leer([fila], Historia);

        Assert.Equal(aviso, lectura.Avisos[fila]);
    }

    [Fact]
    public void Leer_SinNombre_AvisaQueFaltaElNombre()
    {
        var fila = new FilaCampo { Minimo = 1, Maximo = 2 };

        var lectura = LecturaHoja.Leer([fila], Historia);

        Assert.Equal(MensajesCampo.FaltaNombre, lectura.Avisos[fila]);
    }

    private static FilaCampo Entero(string nombre, int minimo, int maximo) =>
        new() { Nombre = nombre, Tipo = TipoCampo.Entero, Minimo = minimo, Maximo = maximo };

    private static FilaCampo Texto(string nombre, int minimo, int maximo) =>
        new() { Nombre = nombre, Tipo = TipoCampo.Texto, Minimo = minimo, Maximo = maximo };
}
