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

    [Fact]
    public void Leer_ReglaValida_LaAgregaALaHistoriaSinAvisos()
    {
        var lectura = LecturaHoja.Leer(FilasDeCliente(), Historia, [Regla("tipo de cliente", "Empresa", "NIT")]);

        Assert.False(lectura.TieneAvisos);
        Assert.Equal(new ReglaObligatorio("tipo de cliente", "Empresa", "NIT"), Assert.Single(lectura.Historia.Reglas));
    }

    [Fact]
    public void Leer_ReglaSinNadaElegido_NoAvisaNiLaAgrega()
    {
        var lectura = LecturaHoja.Leer(FilasDeCliente(), Historia, [new FilaRegla()]);

        Assert.False(lectura.TieneAvisos);
        Assert.Empty(lectura.Historia.Reglas);
    }

    [Theory]
    [InlineData("tipo de cliente", "Empresa", "", "Completa la regla: elige los dos campos y la opción.")]
    [InlineData("tipo de cliente", "Empresa", "RUT", "El campo «RUT» no existe.")]
    [InlineData("tipo de cliente", "Empresa", "edad", "Ese campo ya es obligatorio; márcalo como opcional para usarlo en la regla.")]
    [InlineData("edad", "Empresa", "NIT", "El campo de la condición tiene que ser de lista.")]
    public void Leer_ReglaConProblemas_AvisaYOcultaLosCasos(string condicion, string opcion, string obligatorio, string aviso)
    {
        var regla = Regla(condicion, opcion, obligatorio);

        var lectura = LecturaHoja.Leer(FilasDeCliente(), Historia, [regla]);

        Assert.True(lectura.TieneAvisos);
        Assert.Equal(aviso, lectura.AvisosReglas[regla]);
        Assert.Empty(lectura.Historia.Reglas);
    }

    [Fact]
    public void Leer_PorParesConDosListas_LoActivaSinAvisos()
    {
        List<FilaCampo> filas = [.. FilasDeCliente(), new() { Nombre = "canal", Tipo = TipoCampo.Lista, Opciones = "Web, Tienda" }];

        var lectura = LecturaHoja.Leer(filas, Historia, porPares: true);

        Assert.False(lectura.TieneAvisos);
        Assert.True(lectura.Historia.PorPares);
    }

    [Fact]
    public void Leer_PorParesConUnaSolaLista_AvisaYOcultaLosCasosComoEnHU12()
    {
        var lectura = LecturaHoja.Leer(FilasDeCliente(), Historia, porPares: true);

        Assert.True(lectura.TieneAvisos);
        Assert.Equal("Se necesitan al menos dos campos de lista para combinar por pares.", lectura.AvisoPares);
        Assert.False(lectura.Historia.PorPares);
    }

    private static List<FilaCampo> FilasDeCliente() =>
    [
        Entero("edad", 18, 65),
        new() { Nombre = "tipo de cliente", Tipo = TipoCampo.Lista, Opciones = "Persona, Empresa" },
        new() { Nombre = "NIT", Tipo = TipoCampo.Texto, Obligatorio = false, Minimo = 9, Maximo = 10 },
    ];

    private static FilaRegla Regla(string condicion, string opcion, string obligatorio) =>
        new() { CampoCondicion = condicion, Opcion = opcion, CampoObligatorio = obligatorio };

    private static FilaCampo Entero(string nombre, int minimo, int maximo) =>
        new() { Nombre = nombre, Tipo = TipoCampo.Entero, Minimo = minimo, Maximo = maximo };

    private static FilaCampo Texto(string nombre, int minimo, int maximo) =>
        new() { Nombre = nombre, Tipo = TipoCampo.Texto, Minimo = minimo, Maximo = maximo };
}
