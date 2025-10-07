global using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AiRag.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddAiRagPhase0();

var app = builder.Build();

// Add request logging middleware
app.UseMiddleware<AiRag.Api.Services.RequestLoggingMiddleware>();

app.MapControllers();

app.MapGet("/", () => Results.Ok(new { status = "ok" }));

app.Run();

