using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography;
using System.Text;

namespace Arif.Platform.Shared.Common.Security;

public class DataEncryption : IDataEncryption, IFieldEncryption
{
    private readonly IDataProtector _dataProtector;
    private readonly IDataProtector _sensitiveDataProtector;
    private const string EncryptionPrefix = "ENC:";

    public DataEncryption(IDataProtectionProvider dataProtectionProvider)
    {
        _dataProtector = dataProtectionProvider.CreateProtector("Arif.Platform.GeneralData");
        _sensitiveDataProtector = dataProtectionProvider.CreateProtector("Arif.Platform.SensitiveData");
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        return _dataProtector.Protect(plainText);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        try
        {
            return _dataProtector.Unprotect(cipherText);
        }
        catch (CryptographicException)
        {
            throw new InvalidOperationException("Failed to decrypt data. The data may be corrupted or the key may have changed.");
        }
    }

    public byte[] Encrypt(byte[] plainBytes)
    {
        if (plainBytes == null || plainBytes.Length == 0)
            return plainBytes;

        var plainText = Convert.ToBase64String(plainBytes);
        var encrypted = _dataProtector.Protect(plainText);
        return Encoding.UTF8.GetBytes(encrypted);
    }

    public byte[] Decrypt(byte[] cipherBytes)
    {
        if (cipherBytes == null || cipherBytes.Length == 0)
            return cipherBytes;

        try
        {
            var cipherText = Encoding.UTF8.GetString(cipherBytes);
            var decrypted = _dataProtector.Unprotect(cipherText);
            return Convert.FromBase64String(decrypted);
        }
        catch (CryptographicException)
        {
            throw new InvalidOperationException("Failed to decrypt data. The data may be corrupted or the key may have changed.");
        }
    }

    public string EncryptSensitiveData(string data, string purpose)
    {
        if (string.IsNullOrEmpty(data))
            return data;

        var purposeProtector = _sensitiveDataProtector.CreateProtector(purpose);
        return purposeProtector.Protect(data);
    }

    public string DecryptSensitiveData(string encryptedData, string purpose)
    {
        if (string.IsNullOrEmpty(encryptedData))
            return encryptedData;

        try
        {
            var purposeProtector = _sensitiveDataProtector.CreateProtector(purpose);
            return purposeProtector.Unprotect(encryptedData);
        }
        catch (CryptographicException)
        {
            throw new InvalidOperationException($"Failed to decrypt sensitive data for purpose: {purpose}");
        }
    }

    public string HashSensitiveData(string data)
    {
        if (string.IsNullOrEmpty(data))
            return data;

        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hashedBytes);
    }

    public bool VerifyHashedData(string data, string hash)
    {
        if (string.IsNullOrEmpty(data) || string.IsNullOrEmpty(hash))
            return false;

        var dataHash = HashSensitiveData(data);
        return string.Equals(dataHash, hash, StringComparison.Ordinal);
    }

    public string EncryptField(string value, string fieldName)
    {
        if (string.IsNullOrEmpty(value) || IsEncrypted(value))
            return value;

        var encrypted = EncryptSensitiveData(value, $"Field.{fieldName}");
        return $"{EncryptionPrefix}{encrypted}";
    }

    public string DecryptField(string encryptedValue, string fieldName)
    {
        if (string.IsNullOrEmpty(encryptedValue) || !IsEncrypted(encryptedValue))
            return encryptedValue;

        var cipherText = encryptedValue.Substring(EncryptionPrefix.Length);
        return DecryptSensitiveData(cipherText, $"Field.{fieldName}");
    }

    public bool IsEncrypted(string value)
    {
        return !string.IsNullOrEmpty(value) && value.StartsWith(EncryptionPrefix);
    }
}
