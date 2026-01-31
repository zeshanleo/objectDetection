using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using VideoDetectionPOC.DataAccess;
using VideoDetectionPOC.Hub;
using VideoDetectionPOC.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 1073741824; // 1 GB
});

builder.Services.AddDbContext<VideoDetectionPOC.DataAccess.ApplicationDBContext>(options => {
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Add SignalR services
builder.Services.AddSignalR();
builder.Services.AddSingleton<IDetectionQueue, DetectionQueue>();
builder.Services.AddSingleton<OnnxYoloDetector>(provider =>
{
    var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
    return new OnnxYoloDetector(useGPU: true, useHalfModel: false, scopeFactory);
});
builder.Services.AddHostedService<VideoProcessor>();
builder.Services.AddScoped<DetectionRepository>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapHub<DetectionHub>("/detectionHub");

app.Run();
