using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace TutoTOONS.Cryptography
{
    /// <summary>
    /// AES is a symmetric 256-bit encryption algorthm.
    /// Read more: http://en.wikipedia.org/wiki/Advanced_Encryption_Standard
    /// </summary>
    public static class AES
    {
        private const string SALT = "7-x09X7sO4lT7rul%s_ll^VAQRNlKzHH(Cbe^Wyhp_6c*XKwdY";
        private const string INITVECTOR = "TF7VUPM0bcmR4j4W";
        private const string PASSWORD = "Inz*cMI1EMagMiXWZfjFl&Rk3IF00J5JWu&Vo$Xo*v1IvAMO#0";

        private static byte[] saltBytes;
        private static byte[] initVectorBytes;

        static AES()
        {
            saltBytes = Encoding.UTF8.GetBytes(SALT);
            initVectorBytes = Encoding.UTF8.GetBytes(INITVECTOR);
        }

        /// <summary>
        /// Encrypts a string with AES
        /// </summary>
        /// <param name="_plainText">Text to be encrypted</param>
        /// <param name="_password">Password to encrypt with</param>   
        /// <param name="_salt">Salt to encrypt with</param>    
        /// <param name="_initialVector">Needs to be 16 ASCII characters long</param>    
        /// <returns>An encrypted string</returns>        
        public static string Encrypt(string _plainText, string _password = null, string _salt = null, string _initialVector = null)
        {
            return Convert.ToBase64String(EncryptToBytes(_plainText, _password, _salt, _initialVector));
        }

        /// <summary>
        /// Encrypts a string with AES
        /// </summary>
        /// <param name="_plainText">Text to be encrypted</param>
        /// <param name="_password">Password to encrypt with</param>   
        /// <param name="_salt">Salt to encrypt with</param>    
        /// <param name="_initialVector">Needs to be 16 ASCII characters long</param>    
        /// <returns>An encrypted string</returns>        
        public static byte[] EncryptToBytes(string _plainText, string _password = null, string _salt = null, string _initialVector = null)
        {
            byte[] _plainTextBytes = Encoding.UTF8.GetBytes(_plainText);
            return EncryptToBytes(_plainTextBytes, _password, _salt, _initialVector);
        }

        /// <summary>
        /// Encrypts a string with AES
        /// </summary>
        /// <param name="_plainTextBytes">Bytes to be encrypted</param>
        /// <param name="_password">Password to encrypt with</param>   
        /// <param name="_salt">Salt to encrypt with</param>    
        /// <param name="_initialVector">Needs to be 16 ASCII characters long</param>    
        /// <returns>An encrypted string</returns>        
        public static byte[] EncryptToBytes(byte[] _plainTextBytes, string _password = null, string _salt = null, string _initialVector = null)
        {
            int _keySize = 256;

            byte[] _initialVectorBytes = string.IsNullOrEmpty(_initialVector) ? initVectorBytes : Encoding.UTF8.GetBytes(_initialVector);
            byte[] _saltValueBytes = string.IsNullOrEmpty(_salt) ? saltBytes : Encoding.UTF8.GetBytes(_salt);
            byte[] _keyBytes = new Rfc2898DeriveBytes(string.IsNullOrEmpty(_password) ? PASSWORD : _password, _saltValueBytes).GetBytes(_keySize / 8);

            using (RijndaelManaged _symmetricKey = new RijndaelManaged())
            {
                _symmetricKey.Mode = CipherMode.CBC;

                using (ICryptoTransform _encryptor = _symmetricKey.CreateEncryptor(_keyBytes, _initialVectorBytes))
                {
                    using (MemoryStream _memStream = new MemoryStream())
                    {
                        using (CryptoStream _cryptoStream = new CryptoStream(_memStream, _encryptor, CryptoStreamMode.Write))
                        {
                            _cryptoStream.Write(_plainTextBytes, 0, _plainTextBytes.Length);
                            _cryptoStream.FlushFinalBlock();

                            return _memStream.ToArray();
                        }
                    }
                }
            }
        }

        /// <summary>  
        /// Decrypts an AES-encrypted string. 
        /// </summary>  
        /// <param name="_cipherText">Text to be decrypted</param> 
        /// <param name="_password">Password to decrypt with</param> 
        /// <param name="_salt">Salt to decrypt with</param> 
        /// <param name="_initialVector">Needs to be 16 ASCII characters long</param> 
        /// <returns>A decrypted string</returns>
        public static string Decrypt(string _cipherText, string _password = null, string _salt = null, string _initialVector = null)
        {
            byte[] _cipherTextBytes = Convert.FromBase64String(_cipherText.Replace(' ', '+'));
            return Decrypt(_cipherTextBytes, _password, _salt, _initialVector).TrimEnd('\0');
        }

        /// <summary>  
        /// Decrypts an AES-encrypted string. 
        /// </summary>  
        /// <param name="cipherText">Text to be decrypted</param> 
        /// <param name="_password">Password to decrypt with</param> 
        /// <param name="_salt">Salt to decrypt with</param> 
        /// <param name="_initialVector">Needs to be 16 ASCII characters long</param> 
        /// <returns>A decrypted string</returns>
        public static string Decrypt(byte[] _cipherTextBytes, string _password = null, string _salt = null, string _initialVector = null)
        {
            int _keySize = 256;

            byte[] _initialVectorBytes = string.IsNullOrEmpty(_initialVector) ? initVectorBytes : Encoding.UTF8.GetBytes(_initialVector);
            byte[] _saltValueBytes = string.IsNullOrEmpty(_salt) ? saltBytes : Encoding.UTF8.GetBytes(_salt);
            byte[] _keyBytes = new Rfc2898DeriveBytes(string.IsNullOrEmpty(_password) ? PASSWORD : _password, _saltValueBytes).GetBytes(_keySize / 8);
            byte[] _plainTextBytes = new byte[_cipherTextBytes.Length];

            using (RijndaelManaged _symmetricKey = new RijndaelManaged())
            {
                _symmetricKey.Mode = CipherMode.CBC;

                using (ICryptoTransform _decryptor = _symmetricKey.CreateDecryptor(_keyBytes, _initialVectorBytes))
                {
                    using (MemoryStream _memStream = new MemoryStream(_cipherTextBytes))
                    {
                        using (CryptoStream _cryptoStream = new CryptoStream(_memStream, _decryptor, CryptoStreamMode.Read))
                        {
                            int _byteCount = _cryptoStream.Read(_plainTextBytes, 0, _plainTextBytes.Length);

                            return Encoding.UTF8.GetString(_plainTextBytes, 0, _byteCount);
                        }
                    }
                }
            }
        }

    }
}
