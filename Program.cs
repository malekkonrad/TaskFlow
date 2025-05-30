using Microsoft.EntityFrameworkCore;
using TaskFlow.Models;
using TaskFlow.Services;



var builder = WebApplication.CreateBuilder(args);
// builder.Services.AddDbContext<CommentContext>(options =>
//     options.UseSqlite(builder.Configuration.GetConnectionString("CommentContext") ?? throw new InvalidOperationException("Connection string 'CommentContext' not found.")));

// Add services to the container.
builder.Services.AddScoped<IPasswordService, PasswordService>();

// Add services to the container.
builder.Services.AddControllersWithViews();
// Add services to the container.
builder.Services.AddControllers();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
        { 
            Title = "TaskFlow API", 
            Version = "v1" 
        });
        
        // Tylko API controllers w folderze Api
        c.DocInclusionPredicate((name, api) => 
        {
            var actionDescriptor = api.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
            return actionDescriptor?.ControllerTypeInfo.Namespace?.Contains("Controllers.Api") == true;
        });
    });
}

// Dodaj obsługę Web API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



builder.Services.AddDistributedMemoryCache();

// SESSION
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;//plik cookie jest niedostępny przez skrypt po stronie klienta
    options.Cookie.IsEssential = true;//pliki cookie sesji będą zapisywane dzięki czemu sesje będzie mogła być śledzona podczas nawigacji lub przeładowania strony
});


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("AppDbContext")));

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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // app.UseSwagger();
    // app.UseSwaggerUI();
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
