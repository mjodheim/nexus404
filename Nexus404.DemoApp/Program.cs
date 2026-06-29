using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Nexus404.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddNexus404();

var app = builder.Build();

app.UseNexus404();

app.MapGet("/", () => "Nexus404 Demo App. Try to navigate to an unknown route to test the AI middleware.");

app.Run();
