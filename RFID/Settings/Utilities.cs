using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RFID.Settings
{
    class Utilities
    {
    }

    public class HexData
    {
        private string DecimalToHexadecimal(int dec)
        {
            if (dec < 1) return "0";

            int hex = dec;
            string hexStr = string.Empty;

            while (dec > 0)
            {
                hex = dec % 16;

                if (hex < 10)
                    hexStr = hexStr.Insert(0, Convert.ToChar(hex + 48).ToString());
                else
                    hexStr = hexStr.Insert(0, Convert.ToChar(hex + 55).ToString());

                dec /= 16;
            }

            return hexStr;
        }

        private int HexadecimalToDecimal(string hex)
        {
            hex = hex.ToUpper();

            int hexLength = hex.Length;
            double dec = 0;

            for (int i = 0; i < hexLength; ++i)
            {
                byte b = (byte)hex[i];

                if (b >= 48 && b <= 57)
                    b -= 48;
                else if (b >= 65 && b <= 70)
                    b -= 55;

                dec += b * Math.Pow(16, ((hexLength - i) - 1));
            }

            return (int)dec;
        }

        public string ASCIIToHexadecimal(string str)
        {
            string hex = string.Empty;

            for (int i = 0; i < str.Length; ++i)
            {
                string chex = DecimalToHexadecimal(str[i]);

                if (chex.Length == 1)
                    chex = chex.Insert(0, "0");

                hex += chex;
            }

            return hex;
        }

        public string HexadecimalToASCII(string hex)
        {
            string ascii = string.Empty;

            for (int i = 0; i < hex.Length; i += 2)
            {
                ascii += (char)HexadecimalToDecimal(hex.Substring(i, 2));
            }

            return ascii;
        }
    }

    public class UtilitiesAscEncrypt
    {
        private HexData hexData = new HexData();
        public byte[] EncryptStringToBytes_Aes(string plainText, string key)
        {
            var Key = Encoding.ASCII.GetBytes(key);
            //var hex = hexData.ASCIIToHexadecimal(plainText);

            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");

            byte[] encrypted;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = Key;
                aesAlg.Padding = PaddingMode.None;
                aesAlg.Mode = CipherMode.CBC;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            return encrypted;
        }

        public string DecryptStringFromBytes_Aes(byte[] cipherText, string key)
        {
            var Key = Encoding.ASCII.GetBytes(key);
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");

            string plaintext = null;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = Key;
                aesAlg.Padding = PaddingMode.None;
                aesAlg.Mode = CipherMode.CBC;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            //var result = hexData.HexadecimalToASCII(plaintext);

            return plaintext;
        }
    }
}
