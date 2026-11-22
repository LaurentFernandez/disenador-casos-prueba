using Casos.Core;
using Casos.Web.Componentes;

namespace Casos.Web.Tests.Componentes;

public class EjemplosHistoriaTests() : PruebaEnIdioma("es")
{
    [Fact]
    public void Todos_SonLosTresEjemplosDeHU09()
    {
        Assert.Equal(
            new[] { "Registro de usuario", "Recuperar contraseña", "Carrito de compras" },
            EjemplosHistoria.Todos().Select(ejemplo => ejemplo.Historia));
    }

    [Fact]
    public void Todos_CadaUnoSeLeeSinAvisosYGeneraCasos()
    {
        Assert.All(EjemplosHistoria.Todos(), ejemplo =>
        {
            var lectura = LecturaHoja.Leer(ejemplo.Filas, ejemplo.Historia);

            Assert.Empty(lectura.Avisos);
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
    public void Todos_CadaLlamadaDaFilasNuevas()
    {
        Assert.NotSame(EjemplosHistoria.Todos()[0].Filas[0], EjemplosHistoria.Todos()[0].Filas[0]);
    }
}
