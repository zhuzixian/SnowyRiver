using System.Security.Cryptography;
using System.Text;

namespace SnowyRiver.Security.Cryptography;

public class HashHelper
{
    private static string ToHashString(byte[] bytes)
    {
        var builder = new StringBuilder();
        foreach (var t in bytes)
        {
            builder.Append(t.ToString("x2"));
        }
        return builder.ToString();
    }

    public static string ComputeSha256Hash(Stream stream)
    {
        using var sha256Hash = SHA256.Create();
        var bytes = sha256Hash.ComputeHash(stream);
        return ToHashString(bytes);
    }

    public static string ComputeSha256Hash(string input)
    {
        using var sha256Hash = SHA256.Create();
        var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
        return ToHashString(bytes);
    }

    public static string ComputeSha256Hash(byte[] input)
    {
        using var sha256Hash = SHA256.Create();
        var bytes = sha256Hash.ComputeHash(input);
        return ToHashString(bytes);
    }

    public static string ComputeMd5Hash(string input)
    {
        using var md5Hash = MD5.Create();
        var bytes = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
        return ToHashString(bytes);
    }

    public static string ComputeMd5Hash(Stream input)
    {
        using var md5Hash = MD5.Create();
        var bytes = md5Hash.ComputeHash(input);
        return ToHashString(bytes);
    }

    public static string ComputeMd5Hash(byte[] input)
    {
        using var md5Hash = MD5.Create();
        var bytes = md5Hash.ComputeHash(input);
        return ToHashString(bytes);
    }
}
