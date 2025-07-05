using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PredatorsGym.Datos;
using PredatorsGym.Servicios;
using PredatorsGym.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Servicios
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

//  SignalR configuración mejorada
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.MaximumReceiveMessageSize = 1024 * 1024; // 1MB para audio
    options.StreamBufferCapacity = 10;
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
});

// Servicios personalizados
builder.Services.AddHttpClient<IAzureOpenAIService, AzureOpenAIService>();
builder.Services.AddScoped<IAzureSpeechService, AzureSpeechService>();
builder.Services.AddScoped<IWorkoutService, WorkoutService>();

var app = builder.Build();

// Inicializar roles
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await InicializadorRoles.CrearRolesIniciales(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Un error ocurrió al inicializar los roles.");
    }
}

//  MIDDLEWARE EN ORDEN CORRECTO (.NET 8 syntax)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

//  Static Files PRIMERO
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

//  MAPEO MODERNO (.NET 8) - NO usar UseEndpoints
// SignalR Hub
app.MapHub<WorkoutHub>("/workoutHub", options =>
{
    options.Transports =
        Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets |
        Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;
});

// API Controllers
app.MapControllers();

// MVC Routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Razor Pages
app.MapRazorPages();

app.Run();