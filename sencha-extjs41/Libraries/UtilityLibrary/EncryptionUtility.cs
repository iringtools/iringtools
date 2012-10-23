using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using System.Security.Cryptography;
using System.IO;

namespace org.iringtools.utility
{
  public static class EncryptionUtility
  {
    private static readonly ILog logger = LogManager.GetLogger(typeof(EncryptionUtility));
    private static String SECRET_KEY = "secret";  
    private static RijndaelManaged cipher;

    #region Using internal secret key
    static EncryptionUtility()
    {
      try
      {
        byte[] keyBytes = new byte[0x10];
        byte[] buf = Encoding.UTF8.GetBytes(SECRET_KEY);
        int len = (buf.Length > keyBytes.Length) ? keyBytes.Length : buf.Length;
        Array.Copy(buf, keyBytes, len);

        cipher = new RijndaelManaged()
        {
          Mode = CipherMode.CBC,
          Padding = PaddingMode.PKCS7,
          KeySize = 0x80,
          BlockSize = 0x80,
          Key = keyBytes,
          IV = keyBytes
        };
      }
      catch (Exception e)
      {
        logger.Error("Error initializing cryptor: " + e);
      }
    }

    public static string Encrypt(string plainText)
    {
      try
      {
        ICryptoTransform encryptor = cipher.CreateEncryptor();
        byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(encryptor.TransformFinalBlock(plainTextBytes, 0, plainTextBytes.Length));
      }
      catch (Exception e)
      {
        String message = "Error encrypting [" + plainText + "]" + e;
        logger.Error(message);
        throw new Exception(message);
      }
    }

    public static string Decrypt(string cipherText)
    {
      try
      {
        ICryptoTransform decryptor = cipher.CreateDecryptor();
        byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
        byte[] plainText = decryptor.TransformFinalBlock(cipherTextBytes, 0, cipherTextBytes.Length);
        return Encoding.UTF8.GetString(plainText);
      }
      catch (Exception e)
      {
        String message = "Error decrypting [" + cipherText + "]" + e;
        logger.Error(message);
        throw new Exception(message);
      }
    }
    #endregion

    #region Using external secret key
    private static void InitKey(string keyFile)
    {
      try
      {
        FileStream fs = File.OpenRead(keyFile);
        byte[] keyBytes = new byte[fs.Length];
        fs.Read(keyBytes, 0, keyBytes.Length);
        fs.Close();

        MD5 md5 = MD5.Create();
        keyBytes = md5.ComputeHash(keyBytes);

        cipher = new RijndaelManaged()
        {
          Mode = CipherMode.CBC,
          Padding = PaddingMode.PKCS7,
          Key = keyBytes,
          IV = keyBytes
        };
      }
      catch (Exception e)
      {
        String message = "Error initializing key from key file [" + keyFile + "]. " + e;
        throw new Exception(message);
      }
    }

    public static string Encrypt(string plainText, string keyFile)
    {
      try
      {
        InitKey(keyFile);
        ICryptoTransform encryptor = cipher.CreateEncryptor();
        byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(encryptor.TransformFinalBlock(plainTextBytes, 0, plainTextBytes.Length));
      }
      catch (Exception e)
      {
        String message = "Error encrypting [" + plainText + "]. " + e;
        throw new Exception(message);
      }
    }

    public static string Decrypt(string cipherText, string keyFile)
    {
      try
      {
        InitKey(keyFile);
        ICryptoTransform decryptor = cipher.CreateDecryptor();
        byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
        byte[] plainText = decryptor.TransformFinalBlock(cipherTextBytes, 0, cipherTextBytes.Length);
        return Encoding.UTF8.GetString(plainText);
      }
      catch (Exception e)
      {
        String message = "Error decrypting [" + cipherText + "]. " + e;
        throw new Exception(message);
      }
    }
    #endregion
  }
}
