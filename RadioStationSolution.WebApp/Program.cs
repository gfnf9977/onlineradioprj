using Microsoft.EntityFrameworkCore;
using OnlineRadioStation.Data;
using OnlineRadioStation.Services;
using OnlineRadioStation.Domain;
using Microsoft.AspNetCore.StaticFiles; // <-- 1. ДОДАЙТЕ ЦЕЙ USING

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<StreamingService>();

builder.Services.AddScoped<IStationRepository, StationRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<IStationService, StationService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IAudioConverter, FFmpegAdapter>();
// builder.Services.AddScoped<StreamingService>(); // <-- Ви вже додали це вище, дублікат не потрібен

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// app.UseHttpsRedirection(); // <-- 3. ВИМКНЕНО. Це прибирає попередження з консолі.

// --- 2. ВИПРАВЛЕНО (MIME-типи) ---
// Вчимо сервер розпізнавати .m3u8 та .ts файли
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".m3u8"] = "application/vnd.apple.mpegurl"; // Тип для HLS плейлиста
provider.Mappings[".ts"] = "video/mp2t"; // Тип для HLS сегмента

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider // Передаємо наші налаштування
});
// --------------------------------

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "test",
    pattern: "test/{action=IteratorTest}",
    defaults: new { controller = "Test" });

app.Run();