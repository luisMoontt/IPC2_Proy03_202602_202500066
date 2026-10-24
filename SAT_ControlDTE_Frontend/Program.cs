using System.Globalization;
using Sat.Web;

// Montos y fechas con formato de Guatemala (punto decimal, coma de miles)
CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("es-GT");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("es-GT");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// HttpClient que usa ApiCliente para hablar con Sat.Api. La direccion esta en appsettings.json ("ApiUrl")
builder.Services.AddHttpClient<ApiCliente>(cliente =>
    cliente.BaseAddress = new Uri(builder.Configuration["ApiUrl"] ?? "http://localhost:5234/"));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error");

app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
