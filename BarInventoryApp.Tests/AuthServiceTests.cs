using BarInventoryApp.DataContexts;
using BarInventoryApp.Models;
using BarInventoryApp.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BarInventoryApp.Tests;

public class AuthServiceTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);

        context.Roles.Add(new Role { Id = 1, Name = "Barmen" });
        context.Users.Add(new User
        {
            Id = 1,
            Login = "ivanov",
            PasswordHash = "hello",
            RoleId = 1
        });
        context.SaveChanges();

        return context;
    }

    [Fact]
    public async Task AuthenticateAsync_ReturnsUser_WhenCredentialsAreValid()
    {
        await using var context = CreateContext();
        var service = new AuthService(context);

        var result = await service.AuthenticateAsync("ivanov", "hello");

        Assert.NotNull(result);
        Assert.Equal("ivanov", result.Login);
        Assert.Equal("Barmen", result.Role.Name);
    }

    [Fact]
    public async Task AuthenticateAsync_ReturnsNull_WhenPasswordIsInvalid()
    {
        await using var context = CreateContext();
        var service = new AuthService(context);

        var result = await service.AuthenticateAsync("ivanov", "wrong");

        Assert.Null(result);
    }
}
