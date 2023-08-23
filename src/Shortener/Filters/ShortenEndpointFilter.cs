using Devblogs.Services.Shortener.Models.Dtos;
using System.Text.RegularExpressions;

namespace Devblogs.Services.Shortener.Filters;

public class ShortenEndpointFilter : IEndpointFilter
{
    private const int UrlArgumentIndex = 0;
    private const string pattern = @"^(?:(?:https?|ftp)://)?[^\s/$.?#].[^\s]*$";

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.GetArgument<ShortenerRequest>(UrlArgumentIndex);

        if (IsValidUrl(request.Url))
        {
            return await next(context);
        }

        return Results.BadRequest(Constants.Data.EndPointFilterMessages.InvalidUrl);
    }

    private static bool IsValidUrl(string url)
        => Regex.IsMatch(url, pattern, RegexOptions.IgnoreCase);
 
}