using FlashCard.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
// SQL
builder.Services.AddDbContext<FlashCardDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("FlashCardConnectionString")));

// Log-In
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // เส้นทางสำหรับหน้า Login
        options.LogoutPath = "/Account/Logout"; // เส้นทางสำหรับหน้า Logout
    });

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


app.MapStaticAssets();

// For Authentication
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=FlashCard}/{action=Upload}/{id?}")
    .WithStaticAssets();


app.Run();
