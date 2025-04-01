using FlashCard.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Database Connection
builder.Services.AddDbContext<FlashCardDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("FlashCardConnectionString")));

// Authentication (ใช้ Cookie)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // เส้นทางไปหน้า Login
        options.LogoutPath = "/Account/Logout"; // เส้นทางไปหน้า Logout
        options.Cookie.HttpOnly = true; // ป้องกันการถูกเข้าถึงจาก JS
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // ใช้เฉพาะ HTTPS
        options.Cookie.SameSite = SameSiteMode.Lax; // ป้องกันปัญหาการเข้าถึงข้ามโดเมน
    });

// Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // อายุของ Session
    options.Cookie.HttpOnly = true; // ป้องกันการเข้าถึงจาก JavaScript
    options.Cookie.IsEssential = true; // ให้ Session ใช้งานได้เสมอ
    options.Cookie.Name = "FlashCardSession"; // ตั้งชื่อ Session Cookie (ช่วยลดปัญหาการแชร์ Session ระหว่างแท็บ)
    options.Cookie.SameSite = SameSiteMode.Lax; // ป้องกันปัญหาการใช้งานข้ามโดเมน
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.MapStaticAssets();

// ใช้งาน Session ก่อน Authentication
app.UseSession();

// ใช้งาน Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// กำหนดเส้นทางเริ่มต้น
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=FlashCard}/{action=M1_FlashCard2}/{id?}")
    .WithStaticAssets();

app.Run();
