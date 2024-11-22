using BlazorApp.Components;
using BlazorApp.Data;
using BlazorApp.Repositories.ProductRepository;
using BlazorApp.StoredProcedureHepler;
using Grpc.Net.Client;
using GrpcServer;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

using MudBlazor.Services;
var builder = WebApplication.CreateBuilder(args);
//builder.Services.AddRazorPages();
//builder.Services.AddServerSideBlazor();
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMudServices();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IStoredProcedureHelper, StoredProcedureHelper>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
//builder.Services.AddScoped(sp =>
//{
//    var grpcChannel = GrpcChannel.ForAddress("https://localhost:5001");
//    return new Greeter.GreeterClient(grpcChannel);
//});
//commit program.cs
builder.Services.AddRazorPages();
builder.Services.AddGrpcClient<Greeter.GreeterClient>(options =>
{
    options.Address = new Uri("https://localhost:5001");
});
var app = builder.Build();
app.UseGrpcWeb();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();



app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
