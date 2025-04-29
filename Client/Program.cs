using Client;
using Client.Models;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorPages();

// Register SitterApiClient for calling sitters search API
builder.Services.AddHttpClient<SitterApiClient>(client =>
{
    client.BaseAddress = new("https+http://server");
});

var app = builder.Build();

app.MapRazorPages();
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorPages();

app.Run();
