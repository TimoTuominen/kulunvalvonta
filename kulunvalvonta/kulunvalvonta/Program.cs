using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using kulunvalvonta.Client.Pages;
using kulunvalvonta.Components;
using kulunvalvonta.Components.Account;
using kulunvalvonta.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//  Load connection string for SQL Server Express
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Configure Entity Framework to use SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//  Configure Identity services with Entity Framework stores and add roles
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    //  Configure password requirements (modify as needed)
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddRoleManager<RoleManager<IdentityRole>>()  // Explicitly add RoleManager
    .AddRoles<IdentityRole>()  // Explicitly add roles
    .AddRoleStore<RoleStore<IdentityRole, ApplicationDbContext>>()  // Explicitly add role store
    .AddDefaultTokenProviders();

//  Register additional services
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

//  Add Blazor services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

//  Add authentication middleware (Identity cookies handled by AddIdentity())
builder.Services.AddAuthentication(); // No need to manually configure cookies

var app = builder.Build();

// Ensure database is migrated & seed admin user on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    // dbContext.Database.Migrate(); // Apply any pending migrations

    // 🔹 Seed roles and admin user
    await SeedRolesAndAdminUser(roleManager, userManager);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.UseAuthentication(); // 🔹 Ensure authentication middleware is included
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(kulunvalvonta.Client._Imports).Assembly);

// 🔹 Ensure Identity endpoints are properly mapped
app.MapAdditionalIdentityEndpoints();

app.Run();

// 🔹 Extracted Role & Admin Seeding into an Async Method
async Task SeedRolesAndAdminUser(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
{
    string adminRole = "Admin";
    string adminEmail = "admin@example.com";
    string adminPassword = "Admin@123";

    if (!await roleManager.RoleExistsAsync(adminRole))
    {
        await roleManager.CreateAsync(new IdentityRole(adminRole));
    }

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, adminRole);
        }
    }
}