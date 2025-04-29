using Data;
using Data.Entities;
using DbMigrations;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<PetBnBDbContext>("petbnbdb");
builder.Services.AddHostedService<Worker>();
builder.Services.AddScoped<DatabaseSeeder>();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGet("/", () => "Hello World!");

app.Run();
