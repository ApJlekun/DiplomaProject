using BarInventoryApp.DataContexts;
using BarInventoryApp.Models;
using BarInventoryApp.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BarInventoryApp.Tests;

public class IngredientServiceTests
{
    [Fact]
    public async Task SearchAsync_ReturnsMatchingIngredients_WhenSearchTermIsProvided()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);

        context.Categories.Add(new Category { Id = 1, Name = "Алкогольный напиток" });
        context.Ingredients.AddRange(
            new Ingredient { Id = 1, Name = "Водка", Quantity = 10, Unit = "л", CategoryId = 1 },
            new Ingredient { Id = 2, Name = "Джин", Quantity = 5, Unit = "л", CategoryId = 1 }
        );
        await context.SaveChangesAsync();

        var service = new IngredientService(context);

        var result = await service.SearchAsync("Вод");

        Assert.Single(result);
        Assert.Equal("Водка", result[0].Name);
    }
}
