using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NETCORE.Services;
using NETCORE.Utils.ConfigOptions;
using Microsoft.AspNetCore.Identity;
using NETCORE.Data;
using NETCORE.Data.Entities;
using Microsoft.AspNetCore.Identity.UI.Services;
using NETCORE.Utils.ConfigOptions.Email;
using NETCORE.Repositories;
using static Google.Apis.Auth.OAuth2.ComputeCredential;




var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

// Đăng ký DbContext với Entity Framework Core

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MvcMovieContext")));
// Đăng ký Identity
builder.Services.AddScoped<DbInitializer>();
builder.Services.AddSingleton<IEmailSender, EmailSenderService>();

builder.Services.AddIdentity<User, Role>(options =>
    options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
// Đăng ký các dịch vụ tùy chỉnh
builder.Services.AddSingleton<IVnPayService, VnPayService>();
builder.Services.AddSingleton<ICloudStorageService, CloudStorageService>();

// Cấu hình các tùy chọn từ appsettings.json
builder.Services.Configure<GoogleCloudStorageConfigOptions>(builder.Configuration.GetSection("GoogleCloudStorage"));
builder.Services.Configure<VnPayConfigOptions>(builder.Configuration.GetSection("VnPay"));

// Cấu hình Session
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".NETCORE.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.IsEssential = true;
});

// Cấu hình các tùy chọn cho Identity
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = false;
});

// Cấu hình cookie cho Identity
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;
});

// Thêm các dịch vụ Razor Pages và MVC
RouteRazerPage();
AddScoped();
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

// Tạo ứng dụng
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    await dbInitializer.Seed();
}

// Sử dụng các phương thức cấu hình cho middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    if (app.Environment.IsProduction())
    {
        app.UseHsts();
    }
}
app.UseAuthentication();

app.UseHttpsRedirection();
app.UseRouting();
app.UseStaticFiles();
app.UseAuthorization();

app.UseSession();

// Định tuyến các controller và page
app.MapControllerRoute(
    name: "Admin",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.MapRazorPages();

app.Run();
void AddScoped()
{
    // services.AddScoped<IUserClaimsPrincipalFactory<User>, CustomUserClaimsPrincipalFactory>();

    services.AddTransient<Initializer>();
    services.AddTransient<UnitOfWork>();
    services.AddTransient<IUserRepository, UserRepository>();
    services.AddScoped<IRoleRepository, RoleRepository>();
    services.AddScoped<IPermissionRepository, PermissionRepository>();
    services.AddScoped<IFunctionRepository, FunctionRepository>();
    services.AddScoped<IUserSettingRepository, UserSettingRepository>();

    services.AddTransient<IEmailSender, EmailSenderService>();
    // services.AddTransient<IVNPayService, VNPayService>();
    // services.AddTransient<IStorageService, FileStorageService>();
    // services.Configure<GoogleCloudStorageSettings>(configuration.GetSection("GoogleCloudStorageSettings"));
    // services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
    // services.Configure<VNPayConfigOptions>(builder.Configuration.GetSection("VnPay"));
    // services.AddTransient<IViewRenderService, ViewRenderService>();
    // services.AddTransient<ICacheService, DistributedCacheService>();
    // services.AddTransient<ICloudStorageService, CloudStorageService>();
    // services.AddTransient<IFileValidator, FileValidator>();
    // services.AddHttpClient<FacebookService>();
    services.AddControllersWithViews();
}
void RouteRazerPage()
{
    services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/access-denied";
    });
    services.AddRazorPages(options =>
    {
        options.Conventions.AddAreaPageRoute("Identity", "/Account/Login", "login");
        options.Conventions.AddAreaPageRoute("Identity", "/Account/Logout", "logout");
        options.Conventions.AddAreaPageRoute("Identity", "/Account/AccessDenied", "access-denied");
        options.Conventions.AddAreaPageRoute("Identity", "/Account/Register", "register");
        options.Conventions.AddAreaPageRoute("Identity", "/Account/ForgotPassword", "forgot-password");
        options.Conventions.AddAreaPageRoute("Identity", "/Account/ResetPassword", "reset-password");
        options.Conventions.AddAreaPageRoute("Identity", "/Account/ConfirmEmail", "confirm-email");
        options.Conventions.AddAreaPageRoute("Identity", "/Account/ResetPasswordConfirmation", "reset-password-confirmation");
        options.Conventions.AddAreaPageRoute("Identity", "/Account/LogoutConfirmation", "logout-confirmation");

        options.Conventions.AddAreaPageRoute("Identity", "/Account/Manage/ChangePassword", "manager/change-password");
        options.Conventions.AddAreaPageRoute("Identity", "/Account/Manage/DeletePersonalData", "manager/delete-personal-data");
        options.Conventions.AddAreaPageRoute("Identity", "/Account/Manage/Disable2fa", "manager/disable2fa");
        options.Conventions.AddAreaPageRoute("Identity", "/Account/Manage/DownloadPersonalData", "manager/download-personal-data");
        options.Conventions.AddAreaPageRoute("Identity", "/Account/Manage/Email", "manager/email");
        options.Conventions.AddAreaPageRoute("Identity", "/Account/Manage/EnableAuthenticator", "manager/enable-authenticator");
        options.Conventions.AddAreaPageRoute("Identity", "/Account/Manage/ExternalLogins", "manager/external-logins");
        options.Conventions.AddAreaPageRoute("Identity", "/Account/Manage/GenerateRecoveryCodes", "manager/generate-recovery-codes");
        options.Conventions.AddAreaPageRoute("Identity", "/Account/Manage/Index", "manager");
        options.Conventions.AddAreaPageRoute("Identity", "/Account/Manage/PersonalData", "manager/personal-data");
        options.Conventions.AddAreaPageRoute("Identity", "/Account/Manage/ResetAuthenticator", "manager/reset-authenticator");
        options.Conventions.AddAreaPageRoute("Identity", "/Account/Manage/SetPassword", "manager/set-password");
        options.Conventions.AddAreaPageRoute("Identity", "/Account/Manage/ShowRecoveryCodes", "manager/show-recovery-codes");
        options.Conventions.AddAreaPageRoute("Identity", "/Account/Manage/TwoFactorAuthentication", "manager/two-factor-authentication");
    });


}