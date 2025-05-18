using Microsoft.EntityFrameworkCore;
using TaskFlow.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();

// SESSION
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;//plik cookie jest niedostępny przez skrypt po stronie klienta
    options.Cookie.IsEssential = true;//pliki cookie sesji będą zapisywane dzięki czemu sesje będzie mogła być śledzona podczas nawigacji lub przeładowania strony
});


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=database.db"));

// COOKIES
// builder.Services.AddAuthentication("MyCookieAuth")
//     .AddCookie("MyCookieAuth", options =>
//     {
//         options.Cookie.Name = "TaskFlow.AuthCookie";
//         options.LoginPath = "/Auth/Login";
//         options.AccessDeniedPath = "/Auth/AccessDenied";
//         options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
//     });

// builder.Services.AddAuthorization();



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

// app.UseAuthentication();
// app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


// ADDED SESSION 
app.UseSession();

app.Use(async (ctx, next) =>
{
    await next();
    if (ctx.Response.StatusCode == 404 && !ctx.Response.HasStarted)
    {
        ctx.Items["originalPath"] = ctx.Request.Path.Value;
        ctx.Request.Path = "/Auth/Login";
        await next();
    }
});

app.Run();
