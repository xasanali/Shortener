using Devblogs.Services.Shortener.Filters;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services.InstallFromAssembly<IShortenerAssemblyMaker>(builder.Configuration);
    builder.Services.AddMemoryCache();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

}

var app = builder.Build();
{
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseDeveloperExceptionPage();
    
    app.MapPost("/shorten", ShortenerEndpoints.ShortenEndpoint)
        .WithOpenApi()
        .WithName("Shortener")
        .AddEndpointFilter<ShortenEndpointFilter>();

    app.MapGet("{shortCode}", RedirectEndpoints.RedirectEndpoint)
        .WithOpenApi()
        .WithName("Redirect")
        .AddEndpointFilter<RedirectEndpointFilter>()
        .AllowAnonymous();


}
app.Run();
