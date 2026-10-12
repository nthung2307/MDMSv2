using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using Application.Services;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class PasswordGeneratorService : IPasswordGeneratorService
{
    private readonly PasswordOptions _passwordOption;

    public PasswordGeneratorService(IOptions<PasswordOptions>? passwordOption)
    {
        _passwordOption = passwordOption?.Value ?? new PasswordOptions();
    }

    public string GenerateRandomPassword(PasswordOptions opts = null!)
    {
        // Set default PasswordOptions if not provided
        if (opts == null)
        {
            opts = new PasswordOptions
            {
                RequiredLength = 8, // Set the required password length to 8 characters
                RequireUppercase = true,
                RequireLowercase = true,
                RequireDigit = true,
                RequireNonAlphanumeric = true,
                RequiredUniqueChars = 4 // Require at least 4 unique characters
            };
        }

        string[] randomChars = new[]
        {
            "ABCDEFGHJKLMNOPQRSTUVWXYZ", // uppercase
            "abcdefghijkmnopqrstuvwxyz", // lowercase
            "0123456789", // digits
            "!@$?_-" // non-alphanumeric
        };

        CryptoRandom rand = new CryptoRandom();
        List<char> chars = new List<char>();

        // Add required characters from each category if the corresponding option is enabled
        if (opts.RequireUppercase)
            chars.Add(randomChars[0][rand.Next(0, randomChars[0].Length)]);

        if (opts.RequireLowercase)
            chars.Add(randomChars[1][rand.Next(0, randomChars[1].Length)]);

        if (opts.RequireDigit)
            chars.Add(randomChars[2][rand.Next(0, randomChars[2].Length)]);

        if (opts.RequireNonAlphanumeric)
            chars.Add(randomChars[3][rand.Next(0, randomChars[3].Length)]);

        // Fill the rest of the characters up to the required length
        while (chars.Count < opts.RequiredLength)
        {
            string rcs = randomChars[rand.Next(0, randomChars.Length)];
            chars.Add(rcs[rand.Next(0, rcs.Length)]);
        }

        // Shuffle the characters to avoid predictable patterns
        chars = chars.OrderBy(x => rand.Next()).ToList();

        // Force the password to be exactly the required length
        return new string(chars.Take(opts.RequiredLength).ToArray());
    }

    public string HashPassword(string password)
    {
        byte[] salt;
        byte[] buffer2;
        if (password == null)
        {
            // ReSharper disable once NotResolvedInText
            throw new ArgumentNullException("Password is null");
        }

        using (var bytes = new Rfc2898DeriveBytes(password, 0x10, 0x3e8))
        {
            salt = bytes.Salt;
            buffer2 = bytes.GetBytes(0x20);
        }

        byte[] dst = new byte[0x31];
        Buffer.BlockCopy(salt, 0, dst, 1, 0x10);
        Buffer.BlockCopy(buffer2, 0, dst, 0x11, 0x20);
        return Convert.ToBase64String(dst);
    }

    public bool VerifyHashPassword(string? hashedPassword, string password)
    {
        byte[] buffer4;
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (hashedPassword == null)
        {
            return false;
        }

        if (password == null)
        {
            throw new ArgumentNullException("password");
        }

        byte[] src = Convert.FromBase64String(hashedPassword);
        if ((src.Length != 0x31) || (src[0] != 0))
        {
            return false;
        }

        byte[] dst = new byte[0x10];
        Buffer.BlockCopy(src, 1, dst, 0, 0x10);
        byte[] buffer3 = new byte[0x20];
        Buffer.BlockCopy(src, 0x11, buffer3, 0, 0x20);
        using (var bytes = new Rfc2898DeriveBytes(password, dst, 0x3e8))
        {
            buffer4 = bytes.GetBytes(0x20);
        }

        return ByteArraysEqual(buffer3, buffer4);
    }

    [ExcludeFromCodeCoverage]
    private bool ByteArraysEqual(byte[] b1, byte[] b2)
    {
        if (b1 == b2) return true;
        // ReSharper disable ConditionIsAlwaysTrueOrFalse
        if (b1 == null || b2 == null) return false;
        if (b1.Length != b2.Length) return false;
        for (int i = 0; i < b1.Length; i++)
        {
            if (b1[i] != b2[i]) return false;
        }

        return true;
    }
}