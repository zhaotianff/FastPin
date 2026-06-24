using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace FastPin.Api.Auth;

public class TokenStore
{
    private readonly ConcurrentDictionary<string, DateTime> _tokens = new();
    private readonly ApiAuthOptions _options;

    public TokenStore(ApiAuthOptions options)
    {
        _options = options;
    }

    public string CreateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var token = Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        _tokens[token] = DateTime.UtcNow.AddMinutes(_options.TokenLifetimeMinutes);
        return token;
    }

    public bool IsValid(string token)
    {
        if (!_tokens.TryGetValue(token, out var expiresAt))
        {
            return false;
        }

        if (DateTime.UtcNow <= expiresAt)
        {
            return true;
        }

        _tokens.TryRemove(token, out _);
        return false;
    }
}
