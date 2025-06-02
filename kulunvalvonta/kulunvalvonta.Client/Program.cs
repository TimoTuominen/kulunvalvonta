using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Globalization;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Kulttuuri ja kieli asetukset
var culture = new CultureInfo("fi-FI");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;


builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});



builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddBlazorBootstrap();

await builder.Build().RunAsync();
