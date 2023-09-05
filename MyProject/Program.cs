﻿using Hangfire;
using HangfireBasicAuthenticationFilter;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MyProject.Data;
using MyProject.Service;
using MyProject.Utilities.Email;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection")
));

builder.Services.AddHangfire(x => x.UseSqlServerStorage("Data Source=.;initial catalog=AuthenticationMVCHangfire;Trusted_Connection=True;"));
builder.Services.AddHangfireServer();

builder.Services.AddStackExchangeRedisCache(options => options.Configuration = "localhost:1453");
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Authentication/Login"; 
        options.LogoutPath = "/Authentication/Logout"; 
        options.AccessDeniedPath = "/Home/AccessDenied"; 
    });
builder.Services.AddSingleton<EmailService>();
var serviceProvider = builder.Services.BuildServiceProvider();
RecurringJobs.ConfigureRecurringJobs(serviceProvider);

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

IConfiguration _configuration = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build();

app.UseHangfireDashboard("/job", new DashboardOptions
{
    Authorization = new[]
{
    new HangfireCustomBasicAuthenticationFilter
    {
         User = _configuration.GetSection("HangfireSettings:UserName").Value,
         Pass = _configuration.GetSection("HangfireSettings:Password").Value
    }
    }
});

app.UseHangfireServer(new BackgroundJobServerOptions());

GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 3 });



app.Run();
