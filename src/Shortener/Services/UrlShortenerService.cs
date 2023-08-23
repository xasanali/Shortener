using Microsoft.Extensions.Caching.Memory;

namespace Devblogs.Services.Shortener.Services;

public sealed class UrlShortenerService : IUrlShortenerService
{
    private readonly UrlShortenerSetting _shortenerSetting;
    private readonly ILinkRepository _linkRepository;
    private readonly IMemoryCache _cache;

    private Dictionary<string, string> shortToLongUrlMap;

    public UrlShortenerService(
        IOptions<UrlShortenerSetting> shortenerSettingOptions,
        ILinkRepository linkRepository,
        IMemoryCache cache)
    {
        _shortenerSetting = shortenerSettingOptions.Value;
        shortToLongUrlMap = new Dictionary<string, string>();
        _linkRepository = linkRepository;
        _cache = cache;
    }

    public async Task<(bool found, string? value)> TryGetLongUrlAsync(string shortCode,
        CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(shortCode, out string? longUrl))
        {
            return (true, longUrl);
        }

        var getLinkResult = await _linkRepository.TryGetLongUrlAsync(shortCode, cancellationToken);
        if (getLinkResult.found)
        {
            SetCacheEntry(shortCode, getLinkResult.value!);
            return (true, getLinkResult.value);
        }

        return (false, null);
    }

    public async Task<(bool found, string? value)> TryGetShortUrlAsync(string longUrl,
        CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(longUrl, out string? shortCode))
        {
            return (true, shortCode);
        }

        var getShortLinkResult = await _linkRepository.TryGetShortUrlAsync(longUrl, cancellationToken);
        if (getShortLinkResult.found)
        {
            SetCacheEntry(shortCode: getShortLinkResult.value!, longUrl: longUrl);
            return (true, getShortLinkResult.value);
        }

        return (false, null);
    }

    public async Task<string> ShortenUrlAsync(string longUrl, CancellationToken cancellationToken)
    {
        var getUrlResult = await TryGetShortUrlAsync(longUrl, cancellationToken);
        if (getUrlResult.found)
        {
            return UrlResponseCombination(getUrlResult.value!);
        }

        var shortCode = GenerateShortCode(longUrl);

        var link = Link.Create(shortCode, longUrl);
        await _linkRepository.AddAsync(link, cancellationToken);
        await _linkRepository.SaveChangesAsync(cancellationToken);

        SetCacheEntry(shortCode, longUrl);
        return UrlResponseCombination(shortCode);
    }

    private string GenerateShortCode(string longUrl)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(longUrl));
            string hashCode = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            for (int i = 0; i <= hashCode.Length - _shortenerSetting.ShortCodeLength; i++)
            {
                string candidateCode = hashCode.Substring(i, _shortenerSetting.ShortCodeLength);

                if (!shortToLongUrlMap.ContainsKey(candidateCode))
                {
                    return candidateCode;
                }
            }

            throw new Exception(Constants.Data.ExceptionMessage.FailedGenerateUniqCode);
        }
    }

    private void SetCacheEntry(string shortCode, string longUrl)
    {
        _cache.Set(shortCode, longUrl,
            TimeSpan.FromDays(_shortenerSetting.DefaultAbsoluteExpirationRelativeToNowOnDays));
        _cache.Set(longUrl, shortCode,
            TimeSpan.FromDays(_shortenerSetting.DefaultAbsoluteExpirationRelativeToNowOnDays));
    }

    private string UrlResponseCombination(string shortCode)
        => $"{_shortenerSetting.BaseServiceUrl}/{shortCode}";
}