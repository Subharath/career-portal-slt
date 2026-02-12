using JobApp.Models;
using JobApp.Repository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Localization;
using Microsoft.DotNet.Scaffolding.Shared.ProjectModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = int.MaxValue;
});

builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = int.MaxValue; // if don't set default value is: 128 MB
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddScoped<IDBOperations, DBOperations>();
builder.Services.AddScoped<IUtilityFn, UtilityFn>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

StaticData.DefaultConnection = app.Configuration["ConnectionStrings:DefaultConnection"];
StaticData.UploadPath = app.Configuration["Application:UploadPath"];
StaticData.RedirectPath = app.Configuration["Application:RedirectPath"];

var loggerFactory = app.Services.GetService<ILoggerFactory>();
//loggerFactory.AddFile(builder.Configuration["Logging:LogFilePath"].ToString());
loggerFactory.AddFile(builder.Configuration["Logging:LogFilePath"].ToString(), LogLevel.Information);

app.UseHttpsRedirection();

var cultureInfo = new CultureInfo("en-GB");
var supportedCultures = new[]
{
    new CultureInfo("en-GB")
};

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(cultureInfo),
    // Formatting numbers, dates, etc.
    SupportedCultures = supportedCultures,
    // UI strings that we have localized.
    SupportedUICultures = supportedCultures
});

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSession(); //The order of middleware is important. Call UseSession after UseRouting and before MapRazorPages and MapDefaultControllerRoute

//app.UseCookiePolicy(
//new CookiePolicyOptions
//{
//    Secure = app.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always
//});

app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    //context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; style-src 'self'; script-src 'self'; connect-src 'self'; object-src 'self'; frame-ancestors 'none'; img-src 'self'; form-action 'self'");
    await next();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
