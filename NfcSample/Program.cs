using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using NfcSample.Core;
using NfcSample.Settings;

namespace NfcSample
{
    internal class Block1
    {
        public string StationInId { get; set; }
        public string LaneInId { get; set; }
        public byte Status { get; set; }
        public byte CardType { get; set; }
        public DateTime ArriveTime { get; set; } = DateTime.Now;
        public string Plate { get; set; } = "30H11122";

        public Block1()
        {

        }

        public Block1(string data)
        {
            StationInId = data.Substring(0, 4);
            LaneInId = data.Substring(4, 3);
            Status = Convert.ToByte(data.Substring(7, 3));
            CardType = Convert.ToByte(data.Substring(10, 3));
        }

        public override string ToString()
        {
            return $"{{\"StationInId\":\"{StationInId}\",\"LaneInId\":\"{LaneInId}\",\"Status\":{Status},\"CardType\":{CardType},\"ArriveTime\":\"{ArriveTime}\",\"Plate\":\"{Plate}\"}}";
        }
    }

    public class Program
    {


        static void Main(string[] args)
        {
            var data = "1182001000000$$$20220711150003$$30H105033$$$$$$$";

            char[] separators = new char[] { ' ', ';', ',', '\r', '\t', '\n', '$' };

            string[] temp = data.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            //data = String.Join("\n", temp);
            //var key = "1234567890123456";

            //var k = Encoding.ASCII.GetBytes(key);

            //string original = "Here is some data to encrypt!";

            //byte[] encrypted = new byte[48];

            //// Encrypt the string to an array of bytes.
            //encrypted = EncryptStringToBytes_Aes(data, k);

            //// Decrypt the bytes to a string.
            //string roundtrip = DecryptStringFromBytes_Aes(encrypted, k);

            ////Display the original data and the decrypted data.
            //Console.WriteLine("Original:   {0}", data);
            //Console.WriteLine("Round Trip: {0}", roundtrip);

            //var aes = new TN.NFC.Core.Encrypt.AscEncrypt();
            //var dataHex = aes.EncryptStringToBytes_Aes(data, key);

            //var deHex = aes.DecryptStringFromBytes_Aes(dataHex, key);

            //Console.WriteLine(deHex);

            //new TN.NFC.Core.NFC().Initial();

            //new Process2().Initial();
            Console.Read();

        }

        static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            
            byte[] encrypted;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = Key;
                aesAlg.Padding = PaddingMode.None;
                aesAlg.Mode = CipherMode.CBC;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            //if (IV == null || IV.Length <= 0)
            //    throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = Key;
                aesAlg.Padding = PaddingMode.None;
                aesAlg.Mode = CipherMode.CBC;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }
    }

    public class Process2
    {
        private PcscReader _pcscReader = new PcscReader();
        private CardPolling _cardPolling;
        private static string PICC = "";
        public void Initial()
        {
            try
            {
                _cardPolling = new CardPolling();

                //Register to event on card found
                _cardPolling.OnCardFound += new CardStatusChangeDelegate(cardPolling_OnCardFound);

                //Register to event on card remove
                _cardPolling.OnCardRemoved += new CardStatusChangeDelegate(cardPolling_OnCardRemoved);

                //Register to event on error
                _cardPolling.OnError += new CardPollingErrorDelegate(cardPolling_OnError);

                //Get all smart card reader connected to computer
                string[] readerList = _pcscReader.getReaderList();

                PICC = readerList.First(x => x.Contains("PICC")).ToString();

                _cardPolling.add(PICC);

                if (_cardPolling == null)
                {
                    return;
                }

                if (_cardPolling.isBusy())
                {
                    _cardPolling.stop();
                }
                else
                {
                    _pcscReader.readerName = PICC;
                    _cardPolling.start(PICC);
                }

            }
            catch (PcscException pcscException)
            {
                Utilities.WriteErrorLog("Initial", $"Error: {pcscException.Message}");
            }
            catch (Exception generalException)
            {
                Utilities.WriteErrorLog("Initial", $"Error: {generalException.Message}");
            }
        }

        void cardPolling_OnCardRemoved(object sender, CardPollingEventArg e)
        {
            Console.WriteLine("Card is removed");
        }

        void cardPolling_OnError(object sender, CardPollingErrorEventArg e)
        {
            _cardPolling.stop();

            Console.WriteLine(e.errorMessage);
        }

        void cardPolling_OnCardFound(object sender, CardPollingEventArg e)
        {
            CardSelector cardSelector = new CardSelector();
            string cardName = "";
            string activeProtocol = "";

            if (e.status == CARD_STATUS.CARD_FOUND)
            {
                try
                {
                    _pcscReader.connect();
                }
                catch (PcscException pcscException)
                {
                    if (pcscException.Message.Contains("The Smart Card Resource Manager has shut down"))
                    {
                        _pcscReader.establishContext();
                        _pcscReader.connect();
                    }
                }
            }
            else
            {
                return;
            }

            try
            {
                if (_pcscReader.activeProtocol == PcscProvider.SCARD_PROTOCOL_T0)
                    activeProtocol = "T=0";
                else if (_pcscReader.activeProtocol == PcscProvider.SCARD_PROTOCOL_T1)
                    activeProtocol = "T=1";

                cardSelector.pcscReader = _pcscReader;
                cardName = cardSelector.readCardType(e.atr, (byte)_pcscReader.activeProtocol);
            }
            catch (PcscException pcscException)
            {
                Console.WriteLine($"Error: {pcscException.Message}");
            }
            catch (Exception generalException)
            {
                Console.WriteLine($"Error: {generalException.Message}");
            }

            if (cardName != "")
            {
                Console.WriteLine("+=============================================================+");
                Console.WriteLine("Card Found !");
                Console.WriteLine($"ATR             : {Helper.byteAsString(e.atr, true)}");
                Console.WriteLine($"Card Type       : {cardName}");
                Console.WriteLine($"Active Protocol : {activeProtocol}");
                Console.WriteLine("+=============================================================+");

                // READ:

                var reader = new MifareHelper();

                var keyNum = Convert.ToByte(1);
                var keyString = "FF FF FF FF FF FF";
                var isKeyB = false;
                byte blockLength = 16;
                var ReadData = "";

                var blockNum = Convert.ToByte(4);
                var startBlock = Convert.ToByte(4);
                var ret = reader.ReadDataSimple(PICC, keyNum, keyString, blockNum, isKeyB, startBlock, blockLength, out ReadData);
                Console.WriteLine(ret < 0 ? "Read data fail !" : $"4 | {blockNum} | Read data : {ReadData}");

                blockNum = Convert.ToByte(5);
                startBlock = Convert.ToByte(5);
                ret = reader.ReadDataSimple(PICC, keyNum, keyString, blockNum, isKeyB, startBlock, blockLength, out ReadData);
                Console.WriteLine(ret < 0 ? "Read data fail !" : $"5 | {blockNum} | Read data : {ReadData}");

                blockNum = Convert.ToByte(6);
                startBlock = Convert.ToByte(6);
                ret = reader.ReadDataSimple(PICC, keyNum, keyString, blockNum, isKeyB, startBlock, blockLength, out ReadData);
                Console.WriteLine(ret < 0 ? "Read data fail !" : $"6 | {blockNum} | Read data : {ReadData}");
            }
            else
            {
                Console.WriteLine("+=============================================================+");
                Console.WriteLine("Not Found");
                Console.WriteLine("+=============================================================+");
            }


        }
    }

    public class Process
    {
        private CardSelector cardSelector_ = new CardSelector();
        private static ReaderFunctions readerFunctions_;
        private MifareClassic mifareClassic_;
        private int _maximumBlock = 63;
        public void Initial()
        {
            string[] readerList;
            byte[] atr = null;
            string sCardName = "";

            readerFunctions_ = new ReaderFunctions();

            //Register to event OnReceivedCommand
            readerFunctions_.OnReceivedCommand += new TransmitApduDelegate(readerFucntions_OnReceivedCommand);

            //Register to event OnSendCommand
            readerFunctions_.OnSendCommand += new TransmitApduDelegate(readerFucntions_OnSendCommand);

            //Get all smart card reader connected to computer
            readerList = readerFunctions_.getReaderList();

            readerFunctions_.connect(readerList.First().ToString());

            mifareClassic_ = new MifareClassic(readerFunctions_.pcscConnection);

            readerFunctions_.getStatusChange(ref atr);
            cardSelector_.pcscReader = readerFunctions_;

            sCardName = cardSelector_.readCardType(atr, (byte)atr.Length);

            if (sCardName == "Mifare Standard 1K")
                _maximumBlock = 63;
            else if (sCardName == "Mifare Standard 4K")
                _maximumBlock = 255;

            byte[] key = new byte[6];
            byte keyNumber = 0x20;
            KEY_STRUCTURE keyStructure = KEY_STRUCTURE.VOLATILE;

            try
            {

                if (!byte.TryParse("1", out keyNumber) || keyNumber > 01)
                {
                    Console.WriteLine(keyNumber);
                    return;
                }


                //Key should be 6 bytes long
                key = getBytes("FF FF FF FF FF FF", ' ');
                if (key == null || key.Length == 0)
                {
                    Console.WriteLine("Please key-in hex value for Key Value.");
                    return;
                }

                if (key == null || key.Length < 6)
                {
                    Console.WriteLine("Invalid input length. Length must be 6 bytes.");
                    return;
                }
                readerFunctions_.loadAuthKey(keyStructure, keyNumber, key);
                Console.WriteLine("Load key successful !");
            }
            catch (CardException cardException)
            {
                Utilities.WriteErrorLog("cardException", $"{Helper.byteAsString(cardException.statusWord, true)} | {cardException.Message}");
            }
            catch (PcscException pcscException)
            {
                Utilities.WriteErrorLog("cardException", $"{pcscException.errorCode.ToString()} | {pcscException.Message}");
            }
            catch (Exception generalException)
            {
                Utilities.WriteErrorLog("cardException", $"{generalException.Message}");
            }

            byte blockNumber = 0x00;
            KEYTYPES keyType = KEYTYPES.ACR122_KEYTYPE_A;

            keyNumber = (byte)int.Parse("1");
            blockNumber = (byte)int.Parse("1");

            readerFunctions_.authenticate(blockNumber, keyType, keyNumber);

            var txt = "";

            String tmpStr = "";

            byte[] buff = new byte[16];
            tmpStr = "HELLOWORD!";
            buff = Encoding.ASCII.GetBytes(tmpStr);

            if (buff.Length < 16)
                Array.Resize(ref buff, 16);


            string Key = "8UHjPgXZzXCGkhxV2QCnooyJexUzvJrO";
            var a = new Process().ASCIIToHexadecimal("1182001000000");
            var aesstr = AesEncrypter.Encrypt(a, Key);

            //mifareClassic_.updateBinary((byte)int.Parse("1"), Encoding.UTF8.GetBytes(aesstr), (byte)int.Parse("48"));

            byte[] tempStr;
            int length;

            tempStr = mifareClassic_.readBinary((byte)int.Parse("1"), (byte)int.Parse("48"));
            var text = Helper.byteArrayToString(tempStr);

            var b = "";
        }

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

        void readerFucntions_OnSendCommand(object sender, TransmitApduEventArg e)
        {
            Utilities.WriteDebugLog("OnSendCommand", $"Data: {e.data} | Length: {e.data.Length}");
        }

        void readerFucntions_OnReceivedCommand(object sender, TransmitApduEventArg e)
        {
            Utilities.WriteDebugLog("OnReceivedCommand", $"Data: {string.Join(" ", e.data)} | Length: {e.data.Length}");
            Console.WriteLine(string.Join(" ", e.data));
        }

        byte[] getBytes(string stringBytes, char delimeter)
        {
            int counter = 0;
            byte tmpByte;
            string[] arrayString = stringBytes.Split(delimeter);
            byte[] bytesResult = new byte[arrayString.Length];

            foreach (string str in arrayString)
            {
                if (byte.TryParse(str, System.Globalization.NumberStyles.HexNumber, null, out tmpByte))
                {
                    bytesResult[counter] = tmpByte;
                    counter++;
                }
                else
                {
                    return null;
                }
            }

            return bytesResult;
        }



    }

    public class AesEncrypter
    {
        private static readonly Encoding encoding = Encoding.UTF8;

        public static byte[] Encrypt(string plainText, string key)
        {
            try
            {
                RijndaelManaged aes = new RijndaelManaged();
                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.Padding = PaddingMode.PKCS7;
                aes.Mode = CipherMode.CBC;

                aes.Key = encoding.GetBytes(key);
                aes.GenerateIV();

                ICryptoTransform AESEncrypt = aes.CreateEncryptor(aes.Key, aes.IV);
                byte[] buffer = encoding.GetBytes(plainText);

                string encryptedText = Convert.ToBase64String(AESEncrypt.TransformFinalBlock(buffer, 0, buffer.Length));

                //String mac = "";

                //mac = BitConverter.ToString(HmacSHA256(Convert.ToBase64String(aes.IV) + encryptedText, key)).Replace("-", "").ToLower();

                //var keyValues = new Dictionary<string, object>
                //{
                //    { "iv", Convert.ToBase64String(aes.IV) },
                //    { "value", encryptedText },
                //    { "mac", mac },
                //};

                //JavaScriptSerializer serializer = new JavaScriptSerializer();

                return encoding.GetBytes(encryptedText);
            }
            catch (Exception e)
            {
                throw new Exception("Error encrypting: " + e.Message);
            }
        }

        public static string Decrypt(byte[] plainText, string key)
        {
            try
            {
                RijndaelManaged aes = new RijndaelManaged();
                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.Padding = PaddingMode.PKCS7;
                aes.Mode = CipherMode.CBC;
                aes.Key = encoding.GetBytes(key);

                // Base 64 decode
                byte[] base64Decoded = plainText;
                string base64DecodedStr = encoding.GetString(base64Decoded);

                // JSON Decode base64Str
                JavaScriptSerializer ser = new JavaScriptSerializer();
                //var payload = ser.Deserialize<Dictionary<string, string>>(base64DecodedStr);

                //aes.IV = Convert.FromBase64String(payload["iv"]);

                ICryptoTransform AESDecrypt = aes.CreateDecryptor(aes.Key, aes.IV);
                //byte[] buffer = Convert.FromBase64String(payload["value"]);

                return encoding.GetString(AESDecrypt.TransformFinalBlock(plainText, 0, plainText.Length));
            }
            catch (Exception e)
            {
                throw new Exception("Error decrypting: " + e.Message);
            }
        }

        static byte[] HmacSHA256(String data, String key)
        {
            using (HMACSHA256 hmac = new HMACSHA256(encoding.GetBytes(key)))
            {
                return hmac.ComputeHash(encoding.GetBytes(data));
            }
        }
    }

    public class AesEncrypter2
    {
        readonly byte[] _iv;
        readonly byte[] _password;
        public AesEncrypter2(string password, byte[] iv)
        {
            _password = Encoding.ASCII.GetBytes(password);
            _iv = iv;
        }
        public AesEncrypter2(string password)
        {
            _password = Convert.FromBase64String(password);
            _iv = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        }
        public AesEncrypter2(byte[] password)
        {
            _password = password;
        }
        public byte[] Encrypt(byte[] data)
        {
            using (var ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.Padding = PaddingMode.PKCS7;
                    AES.Mode = CipherMode.ECB;
                    AES.BlockSize = 64;
                    AES.KeySize = 256; // 256
                    AES.Key = _password;
                    AES.GenerateIV();

                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        cs.Close();
                    }
                    return ms.ToArray();
                }
            }
        }

        public byte[] Decrypt(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.Padding = PaddingMode.PKCS7;
                    AES.Mode = CipherMode.ECB;
                    AES.BlockSize = 128;
                    AES.KeySize = 256; // 256
                    AES.Key = _password;
                    //AES.IV = _iv;

                    using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        cs.Close();
                    }
                    return ms.ToArray();
                }
            }
        }
    }
}
