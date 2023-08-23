using Devblogs.Services.Shortener.Models.Dtos;

namespace Devblogs.Services.Shortener.Endpoints;

internal static class ShortenerEndpoints
{
    internal static async Task<IResult> ShortenEndpoint(
        [FromBody] ShortenerRequest request,
        IUrlShortenerService urlShortenerService, 
        CancellationToken cancellationToken)
    {
        var shortUrl = await urlShortenerService.ShortenUrlAsync(request.Url, cancellationToken);
        return Results.Ok(new { ShortUrl = shortUrl });
    }

}
