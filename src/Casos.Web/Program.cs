using System.Globalization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using Casos.Web;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var host = builder.Build();

// El idioma se fija al arrancar: el elegido antes o, la primera vez, el del navegador.
var js = host.Services.GetRequiredService<IJSRuntime>();
await using (var idioma = await js.InvokeAsync<IJSObjectReference>("import", "./js/idioma.js"))
{
    var cultura = new CultureInfo(await idioma.InvokeAsync<string>("idiomaInicial"));
    CultureInfo.DefaultThreadCurrentCulture = cultura;
    CultureInfo.DefaultThreadCurrentUICulture = cultura;
}

await host.RunAsync();
