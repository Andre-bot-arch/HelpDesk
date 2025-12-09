using AppSysoHelp.Models;
using AppSysoHelp.Service;
using AppSysoHelp.Service.SignalRService;
using AppSysoHelp.Service.WhatsService;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

//builder.Host.UseSerilog((ctx, cfg) =>
//    cfg.ReadFrom.Configuration(ctx.Configuration));


builder.Services.AddHttpClient();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

//builder.Logging.ClearProviders();
//builder.Logging.AddSerilog();


builder.Services.AddScoped<HelpDeskIntegrationService>();
builder.Services.AddScoped<MessageProcessorService>();
builder.Services.AddScoped<SessionManager>();
builder.Services.AddScoped<WhatsAppService>(); ;

builder.Services.AddTransient<ServiceContato>();
builder.Services.AddScoped<ServiceGenerico>();
// Configurar os serviços
builder.Services.AddDbContext<HelpdesksysoContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar serviços
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index";
    });

builder.Services.AddAuthorization(options =>
{
    // Adicionar política de autorização baseada em claims
    options.AddPolicy("AdminOrManager", policy =>
        policy.RequireClaim("Role", "Admin", "Suporte"));
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });



// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSignalR();


var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseStatusCodePagesWithReExecute("/Erro/404");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Inicio}/{action=Index}/{id?}");

app.MapHub<ChatHub>("/chatHub");

app.Run();
