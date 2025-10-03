using CISS411_GroupProject.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System;

var builder = WebApplication.CreateBuilder(args);

// EF Core DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
  .AddIdentity<IdentityUser, IdentityRole>(opts =>
  {
      opts.Password.RequiredLength = 6;
      opts.Password.RequireNonAlphanumeric = false;
      opts.Password.RequireUppercase = false;
      opts.Password.RequireLowercase = true;
      opts.Password.RequireDigit = false;
      opts.User.RequireUniqueEmail = true;
      opts.SignIn.RequireConfirmedAccount = false;
  })
  .AddEntityFrameworkStores<AppDbContext>()
  .AddDefaultTokenProviders();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Run DbInitializer, seeds test data
using (var scope = app.Services.CreateScope())
{
    await DbInitializer.InitializeAsync(scope.ServiceProvider);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

// Default route pointing to registration page for testing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Users}/{action=Register}/{id?}");

app.Run();
