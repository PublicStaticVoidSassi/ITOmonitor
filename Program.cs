using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using sutnance.Areas.Identity.Data;
using sutnance.Data;
using sutnance.hacking;
using sutnance.Models;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("sutnanceContextConnection") ?? throw new InvalidOperationException("Connection string 'sutnanceContextConnection' not found.");

builder.Services.AddDbContext<sutnanceContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<sutnanceUser>(options => options.SignIn.RequireConfirmedAccount = false).AddEntityFrameworkStores<sutnanceContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services.AddTransient<MachineManager, MachineManager>();
builder.Services.AddTransient<ReportManager, ReportManager>();
builder.Services.AddTransient<ICodeExecutor, CodeExecutor>();
builder.Services.AddTransient<IPingScanner, PingScanner>();
builder.Services.AddTransient<INetCat, NetCat>();
builder.Services.AddTransient<ISNMPScanner, SNMPScanner>();
builder.Services.AddTransient<IGenericHttpScanner, GenericHttpScanner>();
builder.Services.AddTransient<ISqlServerScanner, SqlServerScanner>();

// builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddTransient<IEmailSender, FakeEmailSender>();
builder.Services.AddTransient<IAlertsManager, AlertsManager>();
builder.Services.AddTransient<IHistoriqueManager, HistoriqueManager>();


// this is our scanner hacker.
builder.Services.AddHostedService<sutnance.hacking.Monitor>();


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
// seeding data

using (var scope = app.Services.CreateScope())
{
    var machineManager = scope.ServiceProvider
        .GetRequiredService<MachineManager>();
    var reportManager = scope.ServiceProvider.GetRequiredService<ReportManager>();
    await machineManager.SeedAsync();
    await reportManager.SeedAsync();
}
app.UseAuthorization();
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
