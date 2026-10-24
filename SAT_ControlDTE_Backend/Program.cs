using System;
using SAT_ControlDTE.DataBase;
using SAT_ControlDTE.Services;







var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(opciones => opciones.JsonSerializerOptions.PropertyNamingPolicy = null);

builder.Services.AddSingleton<ConexionBD>();
builder.Services.AddSingleton<NitService>();
builder.Services.AddSingleton<LectorMensajes>();
builder.Services.AddSingleton<RtuService>();
builder.Services.AddSingleton<AutorizacionService>();
builder.Services.AddSingleton<ConsultaService>();

var app = builder.Build();

app.Services.GetRequiredService<ConexionBD>().CrearTablas();

app.MapControllers();

app.Run();
