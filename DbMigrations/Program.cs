using Data;
using DbMigrations;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<FortuneDbContext>("fortunesdb");
builder.Services.AddHostedService<Worker>();
builder.Services.AddScoped<DatabaseSeeder>();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGet("/", () => "Hello World!");

app.Run();
