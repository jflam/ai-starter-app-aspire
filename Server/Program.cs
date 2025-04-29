using Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<FortuneDbContext>("fortunesdb");
var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGet("/", (FortuneDbContext dbContext) => dbContext.Fortunes.OrderBy(_ => Guid.NewGuid()).FirstOrDefault()!.Text);

app.Run();