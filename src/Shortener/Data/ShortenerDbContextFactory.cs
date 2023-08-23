namespace Devblogs.Services.Shortener.Data;

public class ShortenerDbContextFactory : IDesignTimeDbContextFactory<ShortenerDbContext>
{
    public ShortenerDbContext CreateDbContext(string[] args)
    {
        var rootPath = Path.Combine("..", Directory.GetCurrentDirectory());
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Development.json")
            .SetBasePath(rootPath)
            .Build();

        var serviceConnectionString = configuration.GetConnectionString("SvcDbContext");

        var option = new DbContextOptionsBuilder<ShortenerDbContext>()
                            .UseSqlServer(serviceConnectionString)
                            .Options;

        return new ShortenerDbContext(option);
    }
}
