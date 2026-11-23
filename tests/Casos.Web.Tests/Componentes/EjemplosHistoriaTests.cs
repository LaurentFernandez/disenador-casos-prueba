using Casos.Core;
using Casos.Web.Componentes;

namespace Casos.Web.Tests.Componentes;

public class EjemplosHistoriaTests() : PruebaEnIdioma("es")
{
    [Fact]
    public void Todos_SonLosCuatroEjemplosDeHU09()
    {
        Assert.Equal(
            new[] { "Registro de usuario", "Recuperar contraseña", "Carrito de compras", "Compatibilidad" },
            EjemplosHistoria.Todos().Select(ejemplo => ejemplo.Historia));
    }

    [Fact]
    public void Todos_CadaUnoSeLeeSinAvisosYGeneraCasos()
    {
        Assert.All(EjemplosHistoria.Todos(), ejemplo =>
        {
            var lectura = LecturaHoja.Leer(ejemplo.Filas, ejemplo.Historia, porPares: ejemplo.PorPares);

            Assert.False(lectura.TieneAvisos);
            Assert.Equal(ejemplo.Filas.Count, lectura.Historia.Campos.Count);
            Assert.NotEmpty(GeneradorCasos.Generar(lectura.Historia));
        });
    }

    [Fact]
    public void RegistroDeUsuario_DaLosCamposDeLasPruebasYSus24Casos()
    {
        var registro = EjemplosHistoria.Todos()[0];

        var historia = LecturaHoja.Leer(registro.Filas, registro.Historia).Historia;

        Assert.Equal(new[] { "edad", "nombre", "tipo de cliente" }, historia.Campos.Select(campo => campo.Nombre));
        Assert.Equal(24, GeneradorCasos.Generar(historia).Count);
    }

    [Fact]
    public void CarritoDeCompras_ElCuponEsOpcional()
    {
        var cupon = EjemplosHistoria.Todos()[2].Filas.Single(fila => fila.Nombre == "cupón");

        Assert.False(cupon.Obligatorio);
    }

    [Fact]
    public void Compatibilidad_EsElUnicoQueCombinaPorParesYDaSeisDeDoceComoEnHU09()
    {
        var compatibilidad = EjemplosHistoria.Todos()[3];

        var historia = LecturaHoja.Leer(compatibilidad.Filas, compatibilidad.Historia, porPares: compatibilidad.PorPares).Historia;

        Assert.Equal(new[] { false, false, false, true }, EjemplosHistoria.Todos().Select(ejemplo => ejemplo.PorPares));
        Assert.Equal(new[] { "navegador", "sistema", "idioma" }, historia.Campos.Select(campo => campo.Nombre));
        Assert.Equal("Combinaciones por pares: 6 de 12 posibles.", TextosCaso.TotalPares(historia, GeneradorCasos.Generar(historia)));
    }

    [Fact]
    public void Todos_CadaLlamadaDaFilasNuevas()
    {
        Assert.NotSame(EjemplosHistoria.Todos()[0].Filas[0], EjemplosHistoria.Todos()[0].Filas[0]);
    }
}
