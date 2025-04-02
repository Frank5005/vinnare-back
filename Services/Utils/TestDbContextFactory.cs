using Data;
using Microsoft.EntityFrameworkCore;

namespace Services.Utils
{
    public static class TestDbContextFactory
    {
        public static VinnareDbContext Create()
        {
            var options = new DbContextOptionsBuilder<VinnareDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            var context = new VinnareDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }
    }
}
