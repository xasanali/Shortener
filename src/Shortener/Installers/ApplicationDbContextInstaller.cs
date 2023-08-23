namespace Devblogs.Services.Shortener.Installers;

public class ApplicationDbContextInstaller : IServiceCollectionInstaller
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var conStr = configuration.GetConnectionString("SvcDbContext");

        services.AddDbContext<ShortenerDbContext>(options =>
        {
            options.UseSqlServer(conStr);
        });

        services.AddScoped<ILinkRepository, LinkRepository>();  
    }
}
