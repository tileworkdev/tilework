using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

using Tilework.Core.Persistence;
using Tilework.Persistence.TokenVault.Models;

namespace Tilework.TokenVault.Services;

public class TokenService
{
    private readonly TileworkContext _dbContext;
    private readonly ILogger<TokenService> _logger;

    public TokenService(TileworkContext dbContext,
                        ILogger<TokenService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<string?> GetToken(string key)
    {
        var token = await _dbContext.Tokens.FirstOrDefaultAsync(t => t.Key == key);
        return token?.Value;
    }

    public async Task SetToken(string key, string value)
    {
        var token = await _dbContext.Tokens.FirstOrDefaultAsync(t => t.Key == key);
        if(token == null)
        {
            token = new Token() { Key = key, Value = value };
            await _dbContext.Tokens.AddAsync(token);
        }
        else
            token.Value = value;

        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteToken(string key, string value)
    {
        var token = await _dbContext.Tokens.FirstOrDefaultAsync(t => t.Key == key);
        if(token != null)
        {
            _dbContext.Tokens.Remove(token);
            await _dbContext.SaveChangesAsync();
        }
    }


    public static string GenerateToken(int length)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()-_=+[]{}<>?";
        var bytes = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);

        var result = new char[length];
        for (int i = 0; i < length; i++)
            result[i] = chars[bytes[i] % chars.Length];

        return new string(result);
    }

}