using System.Security.Cryptography;

namespace HeadBet.Core.Infrastructure;

/// <summary>
/// Gera códigos opacos para links de convite de bolão. Base62 sem caracteres ambíguos
/// (0/O, 1/I/l), aleatoriedade criptográfica. ~10 chars ≈ 57 bits — não-sequencial e
/// inviável de adivinhar a partir do Id do bolão.
/// </summary>
public static class InviteCodeGenerator
{
    private const string ALPHABET = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";
    private const int DEFAULT_LENGTH = 10;

    public static string Generate(int length = DEFAULT_LENGTH)
    {
        var bytes = RandomNumberGenerator.GetBytes(length);
        var chars = new char[length];
        for (var i = 0; i < length; i++)
            chars[i] = ALPHABET[bytes[i] % ALPHABET.Length];
        return new string(chars);
    }
}
