var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();
builder.Services.AddScoped<LLiveArenaWeb.Services.ISportsService, LLiveArenaWeb.Services.SportsService>();
builder.Services.AddScoped<LLiveArenaWeb.Services.IScheduleService, LLiveArenaWeb.Services.ScheduleService>();
// MatchListService as singleton to share cache across requests
builder.Services.AddSingleton<LLiveArenaWeb.Services.IMatchListService, LLiveArenaWeb.Services.MatchListService>();
builder.Services.AddScoped<LLiveArenaWeb.Services.IStreamService, LLiveArenaWeb.Services.StreamService>();

// Register background service to periodically refresh match list
builder.Services.AddHostedService<LLiveArenaWeb.Services.MatchListBackgroundService>();

// Configure HTTPS redirection - set default port to avoid warnings
// This prevents the warning when running HTTP-only (the redirection just won't work)
builder.Services.Configure<Microsoft.AspNetCore.HttpsPolicy.HttpsRedirectionOptions>(options =>
{
    var httpsPort = builder.Configuration["HTTPS_PORT"] 
        ?? builder.Configuration["ASPNETCORE_HTTPS_PORT"]
        ?? "7183"; // Default from launchSettings.json
    
    if (int.TryParse(httpsPort, out var port))
    {
        options.HttpsPort = port;
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
