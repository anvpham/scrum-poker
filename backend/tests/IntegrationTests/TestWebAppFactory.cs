using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using scrum_poker_server;
using scrum_poker_server.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IntegrationTests
{
    [CollectionDefinition("TestWebAppFactoryCollection")]
    public class TestWebAppFactoryCollection : ICollectionFixture<TestWebAppFactory>
    {

    }

    // Custom settings for integration testing
    public class TestWebAppFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Setup in-memory db for EF Core
                var dbContextDescriptor = services.SingleOrDefault(d =>
                {
                    return d.ServiceType == typeof(DbContextOptions<AppDbContext>);
                });

                services.Remove(dbContextDescriptor);

                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDatabase");
                }, ServiceLifetime.Scoped);
            });
        }
    }
}
