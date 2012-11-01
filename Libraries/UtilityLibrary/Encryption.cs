using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace org.iringtools.utility
{
  public class Encryption
  {
    private static string _cypherWord = "nx6qknaNiHLO9JxNN2RHUbeRy+fhrO8LVy72ZV33DVclmuOpdaU5AgOMCCvO+SNo2GM3csiRB6kNfKlRkECE2Ah+6O2W5WRiCuiXWxFSmQE=";

    public static string DecryptString(string encryptedValue)
    {
      RijndaelManaged rijndaelCipher;
      PasswordDeriveBytes secretKey;
      ICryptoTransform decryptor;
      MemoryStream memoryStream;
      CryptoStream cryptoStream;
      byte[] encryptedBytes;
      byte[] key;
      byte[] valueBytes;
      string value = String.Empty;
      try
      {
        encryptedBytes = Convert.FromBase64String(encryptedValue);
        valueBytes = new byte[encryptedBytes.Length];
        key = Encoding.ASCII.GetBytes(_cypherWord.Length.ToString());
        secretKey = new PasswordDeriveBytes(_cypherWord, key);

        rijndaelCipher = new RijndaelManaged();
        rijndaelCipher.Padding = PaddingMode.PKCS7;

        decryptor = rijndaelCipher.CreateDecryptor(secretKey.GetBytes(32), secretKey.GetBytes(16));

        memoryStream = new MemoryStream(encryptedBytes);
        cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);

        int decryptedCount = cryptoStream.Read(valueBytes, 0, valueBytes.Length);

        memoryStream.Close();
        cryptoStream.Close();

        value = Encoding.Unicode.GetString(valueBytes, 0, decryptedCount);

        return value;
      }
      catch (Exception exception)
      {
        throw new Exception(exception.Message, exception);
      }
    }

    public static string EncryptString(string value)
    {
      RijndaelManaged rijndaelCipher;
      PasswordDeriveBytes secretKey;
      ICryptoTransform encryptor;
      MemoryStream memoryStream;
      CryptoStream cryptoStream;
      byte[] valueBytes;
      byte[] key;
      byte[] encryptedBytes;
      string encryptedValue = String.Empty;
      try
      {
        valueBytes = System.Text.Encoding.Unicode.GetBytes(value);

        key = Encoding.ASCII.GetBytes(_cypherWord.Length.ToString());

        secretKey = new PasswordDeriveBytes(_cypherWord, key);

        rijndaelCipher = new RijndaelManaged();
        rijndaelCipher.Padding = PaddingMode.PKCS7;
        
        encryptor = rijndaelCipher.CreateEncryptor(secretKey.GetBytes(32), secretKey.GetBytes(16));
        
        memoryStream = new MemoryStream();
        cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
        cryptoStream.Write(valueBytes, 0, valueBytes.Length);
        cryptoStream.FlushFinalBlock();

        encryptedBytes = memoryStream.ToArray();

        memoryStream.Close();
        cryptoStream.Close();

        encryptedValue = Convert.ToBase64String(encryptedBytes);

        return encryptedValue;
      }
      catch (Exception exception)
      {
        throw new Exception(exception.Message, exception);
      }
    }
  }
}
