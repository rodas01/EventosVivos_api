using EventosVivos_Api.Data;
using EventosVivos_Api.Models.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventosVivos_Api.Tests.Data;

[TestClass]
public class DatabaseSeederTests
{
    private ServiceProvider _serviceProvider = null!;
    private ApplicationDbContext _context = null!;

    [TestInitialize]
    public void Setup()
    {
        var services = new ServiceCollection();

        var dbName = Guid.NewGuid().ToString();
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase(dbName), ServiceLifetime.Singleton);

        // Add logging to satisfy UserManager's dependency
        services.AddLogging();

        // Setup Identity
        services.AddIdentity<User, Role>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
        
        _serviceProvider = services.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<ApplicationDbContext>();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _serviceProvider.Dispose();
    }

    [TestMethod]
    public async Task SeedAsync_WhenDatabaseIsEmpty_CreatesAdminUserAndRole()
    {
        // Act
        await DatabaseSeeder.SeedAsync(_serviceProvider);

        // Assert: Use a new scope to ensure we get a fresh context and services
        // reflecting the state after the seeder has run and disposed its own scope.
        using var assertScope = _serviceProvider.CreateScope();
        var assertContext = assertScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = assertScope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
        var userManager = assertScope.ServiceProvider.GetRequiredService<UserManager<User>>();
        
        var adminRole = await roleManager.FindByNameAsync("Admin");
        Assert.IsNotNull(adminRole, "Admin Role should be created.");
        Assert.AreEqual("System administrator", adminRole.Description);

        var adminUser = await userManager.FindByNameAsync("admin");
        Assert.IsNotNull(adminUser, "Admin User should be created.");
        Assert.AreEqual("admin@eventosvivos.local", adminUser.Email);
        Assert.AreEqual("System Administrator", adminUser.FullName);
        Assert.IsTrue(adminUser.IsActive);

        var isInRole = await userManager.IsInRoleAsync(adminUser, "Admin");
        Assert.IsTrue(isInRole, "Admin user should be in the Admin role.");
    }

    [TestMethod]
    public async Task SeedAsync_WhenDataAlreadyExists_DoesNotCreateDuplicates()
    {
        // Arrange
        // Pre-seed the database with the admin user and role
        await DatabaseSeeder.SeedAsync(_serviceProvider);
        
        var initialUserCount = await _context.Users.CountAsync();
        var initialRoleCount = await _context.Roles.CountAsync();

        // Act
        // Run the seeder again
        await DatabaseSeeder.SeedAsync(_serviceProvider);

        // Assert
        var finalUserCount = await _context.Users.CountAsync();
        var finalRoleCount = await _context.Roles.CountAsync();

        Assert.AreEqual(initialUserCount, finalUserCount, "User count should not change.");
        Assert.AreEqual(initialRoleCount, finalRoleCount, "Role count should not change.");
    }
}
