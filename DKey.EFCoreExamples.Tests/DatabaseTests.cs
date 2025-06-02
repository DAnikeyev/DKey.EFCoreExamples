using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using DKey.EFCoreExamples.Model;

namespace DKey.EFCoreExamples.Tests
{
    [TestFixture]
    public class DatabaseTests
    {
        [Test]
        public void CanCreateAndDeleteDatabase()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql("Host=localhost;Port=5432;Database=efcore_test_db;Username=postgres;Password=password") // Use PostgreSQL
                .Options;

            using (var context = new AppDbContext(options))
            {
                context.Database.EnsureDeleted();  // Delete if exists
                context.Database.EnsureCreated();  // Create new

                Assert.IsTrue(context.Database.CanConnect());
            }
        }
    }
}
