namespace Arif.Platform.Shared.Common.Security;

public interface IDataEncryption
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
    byte[] Encrypt(byte[] plainBytes);
    byte[] Decrypt(byte[] cipherBytes);
    string EncryptSensitiveData(string data, string purpose);
    string DecryptSensitiveData(string encryptedData, string purpose);
    string HashSensitiveData(string data);
    bool VerifyHashedData(string data, string hash);
}

public interface IFieldEncryption
{
    string EncryptField(string value, string fieldName);
    string DecryptField(string encryptedValue, string fieldName);
    bool IsEncrypted(string value);
}
