using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RFID.Settings;

namespace RFID
{
    internal class MifareHelper
    {
        private static ReaderFunctions readerFunctions_;
        private static MifareClassic mifareClassic_;
        private static CardSelector cardSelector_ = new CardSelector();
        private static int _maximumBlock;
        private static bool readerConnect_ = false;

        private int Auth(byte keyNumber, byte blockNumber, bool isKeyB)
        {
            byte[] key = new byte[6];
            KEYTYPES keyType = KEYTYPES.ACR122_KEYTYPE_A;

            try
            {
                if (isKeyB)
                    keyType = KEYTYPES.ACR122_KEYTYPE_B;

                readerFunctions_.authenticate(blockNumber, keyType, keyNumber);
                //logger_.Info("Authenticate success");

                return 1;

            }
            catch (CardException cardException)
            {
                //logger_.Error("[" + Helper.byteAsString(cardException.statusWord, true) + "] " + cardException.Message);

                return -1;
            }
            catch (PcscException pcscException)
            {
                //logger_.Error("[" + pcscException.errorCode.ToString() + "] " + pcscException.Message);

                return -1;
            }
            catch (Exception generalException)
            {
                //logger_.Error(generalException.Message);

                return -1;
            }
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

        private int LoadKey(byte keyNumber, string keyString)
        {
            byte[] key = new byte[6];
            KEY_STRUCTURE keyStructure = KEY_STRUCTURE.VOLATILE;
            try
            {
                //Key should be 6 bytes long
                key = getBytes(keyString.Trim(), ' ');
                if (key == null || key.Length == 0)
                {
                    //logger_.Error("Please key-in hex value for Key Value.");
                    return -1;
                }

                if (key == null || key.Length < 6)
                {
                    //logger_.Error("Invalid input length. Length must be 6 bytes.");
                    return -1;
                }

                readerFunctions_.loadAuthKey(keyStructure, keyNumber, key);
                //logger_.Info("Load Key success");

                return 1;
            }
            catch (CardException cardException)
            {
                //logger_.Error("[" + Helper.byteAsString(cardException.statusWord, true) + "] " + cardException.Message);

                //logger_.Error(cardException.Message);

                return -1;
            }
            catch (PcscException pcscException)
            {
                //logger_.Error("[" + pcscException.errorCode.ToString() + "] " + pcscException.Message);

                return -1;
            }
            catch (Exception generalException)
            {
                //logger_.Error(generalException.Message);

                return -1;
            }
        }

        public static String byteArrayToString(byte[] data)
        {
            String str = "";

            for (int i = 0; i < data.Length; i++)
            {
                str += (char)data[i];
            }

            return str;
        }

        private int BinRead(byte startBlock, byte lengthBlock, out string ReadValue)
        {
            byte[] tempStr;

            try
            {
                //Validate Inputs
                if (lengthBlock > _maximumBlock)
                {
                    //logger_.Error("Please key-in valid Start Block. Valid value: 0 - " + _maximumBlock.ToString() + ".");
                    ReadValue = "";
                    return -1;
                }

                //logger_.Info("Read Binary");

                tempStr = mifareClassic_.readBinary(startBlock, lengthBlock);
                ReadValue = byteArrayToString(tempStr);
                //logger_.Info("Read success");

                return 1;
            }
            catch (CardException cardException)
            {
                //logger_.Error("[" + Helper.byteAsString(cardException.statusWord, true) + "] " + cardException.Message);
                ReadValue = "";
                return -1;
            }
            catch (PcscException pcscException)
            {
                //logger_.Error("[" + pcscException.errorCode.ToString() + "] " + pcscException.Message);
                ReadValue = "";
                return -1;
            }
            catch (Exception generalException)
            {
                //logger_.Error(generalException.Message);
                ReadValue = "";
                return -1;
            }
        }

        void readerFucntions_OnSendCommand(object sender, TransmitApduEventArg e)
        {
            //Utilities.WriteDebugLog("OnSendCommand", $"Data: {e.data} | Length: {e.data.Length}");
            Console.WriteLine(string.Join(" ", e.data));
        }

        void readerFucntions_OnReceivedCommand(object sender, TransmitApduEventArg e)
        {
            //Utilities.WriteDebugLog("OnReceivedCommand", $"Data: {string.Join(" ", e.data)} | Length: {e.data.Length}");
            Console.WriteLine(string.Join(" ", e.data));
        }

        private int Connect(string readerName)
        {
            string sCardName = "";
            byte[] atr = null;

            try
            {
                readerFunctions_ = new ReaderFunctions();
                //Register to event OnReceivedCommand
                readerFunctions_.OnReceivedCommand += new TransmitApduDelegate(readerFucntions_OnReceivedCommand);

                //Register to event OnSendCommand
                readerFunctions_.OnSendCommand += new TransmitApduDelegate(readerFucntions_OnSendCommand);

                readerFunctions_.connect(readerName);
                //logger_.Info("\nSuccessfully connected to " + readerName);

                //Initialize Mifare classic class
                mifareClassic_ = new MifareClassic(readerFunctions_.pcscConnection);

                readerFunctions_.getStatusChange(ref atr);
                cardSelector_.pcscReader = readerFunctions_;

                sCardName = cardSelector_.readCardType(atr, (byte)atr.Length);

                if (sCardName == "Mifare Standard 1K")
                    _maximumBlock = 63;
                else if (sCardName == "Mifare Standard 4K")
                    _maximumBlock = 255;
                else
                {
                    //logger_.Error("Card not supported.\r\nPlease present Mifare Classic card.");
                    return -1;
                }

                //logger_.Info("Chip Type: " + sCardName);

                return 1;
            }
            catch (PcscException pcscException)
            {
                //logger_.Error("[" + pcscException.errorCode.ToString() + "] " + pcscException.Message);
                return -1;
            }
            catch (Exception generalException)
            {
                //logger_.Error(generalException.Message);
                return -1;
            }
        }

        public int ReadDataSimple(string ReaderName, byte keyNum, string keyString, byte blockNumber, bool isKeyB, byte startBlock, byte readLength, out byte[] ReadData)
        {
            try
            {
                // Connect reader
                if (!readerConnect_)
                {
                    var retConnect = Connect(ReaderName);
                    if (retConnect < 0)
                    {
                        //logger_.Error("Error to connect reader = " + ReaderName);
                        ReadData = null;
                        return -1;
                    }
                }
                // load key
                var retLoadKey = LoadKey(keyNum, keyString);
                if (retLoadKey < 0)
                {
                    //logger_.Error("Error to load key !");
                    ReadData = null;
                    return -1;
                }
                // authenticate
                var retAuth = Auth(keyNum, blockNumber, isKeyB);
                if (retAuth < 0)
                {
                    //logger_.Error("Error to authenticate !");
                    ReadData = null;
                    return -1;
                }
                // read data
                //string ReadOut = "";
                //var retRead = BinRead(startBlock, readLength, out ReadOut);
                var retRead = BinRead(startBlock, readLength, out byte[] ReadOut);
                if (retRead < 0)
                {
                    //logger_.Error("Error to read data !");
                    ReadData = null;
                    return -1;
                }
                else
                {
                    ReadData = ReadOut;
                }
            }
            catch (Exception ex)
            {
                //logger_.Error(ex.Message);
                //logger_.Error(ex.StackTrace);
                ReadData = null;
            }

            return 1;
        }

        private int BinRead(byte startBlock, byte lengthBlock, out byte[] ReadValue)
        {
            byte[] tempStr;

            try
            {
                //Validate Inputs
                if (lengthBlock > _maximumBlock)
                {
                    //logger_.Error("Please key-in valid Start Block. Valid value: 0 - " + _maximumBlock.ToString() + ".");
                    ReadValue = null;
                    return -1;
                }

                //logger_.Info("Read Binary");

                tempStr = mifareClassic_.readBinary(startBlock, lengthBlock);
                //ReadValue = byteArrayToString(tempStr);
                ReadValue = tempStr;
                //logger_.Info("Read success");

                return 1;
            }
            catch (CardException cardException)
            {
                //logger_.Error("[" + Helper.byteAsString(cardException.statusWord, true) + "] " + cardException.Message);
                ReadValue = null;
                return -1;
            }
            catch (PcscException pcscException)
            {
                //logger_.Error("[" + pcscException.errorCode.ToString() + "] " + pcscException.Message);
                ReadValue = null;
                return -1;
            }
            catch (Exception generalException)
            {
                //logger_.Error(generalException.Message);
                ReadValue = null;
                return -1;
            }
        }

        public int ReadDataSimple(string ReaderName, byte keyNum, string keyString, byte blockNumber, bool isKeyB, byte startBlock, byte readLength, out string ReadData)
        {
            try
            {
                // Connect reader
                if (!readerConnect_)
                {
                    var retConnect = Connect(ReaderName);
                    if (retConnect < 0)
                    {
                        //logger_.Error("Error to connect reader = " + ReaderName);
                        ReadData = "";
                        return -1;
                    }
                }
                // load key
                var retLoadKey = LoadKey(keyNum, keyString);
                if (retLoadKey < 0)
                {
                    //logger_.Error("Error to load key !");
                    ReadData = "";
                    return -1;
                }
                // authenticate
                var retAuth = Auth(keyNum, blockNumber, isKeyB);
                if (retAuth < 0)
                {
                    //logger_.Error("Error to authenticate !");
                    ReadData = "";
                    return -1;
                }
                // read data
                string ReadOut = "";
                var retRead = BinRead(startBlock, readLength, out ReadOut);
                if (retRead < 0)
                {
                    //logger_.Error("Error to read data !");
                    ReadData = "";
                    return -1;
                }
                else
                {
                    ReadData = ReadOut;
                }
            }
            catch (Exception ex)
            {
                //logger_.Error(ex.Message);
                //logger_.Error(ex.StackTrace);
                ReadData = "";
            }

            return 1;
        }

        public int WriteDataSimple(string ReaderName, byte keyNum, string keyString, byte blockNumber, bool isKeyB, byte startBlock, string WriteData)
        {
            try
            {
                // Connect reader
                if (!readerConnect_)
                {
                    var retConnect = Connect(ReaderName);
                    if (retConnect < 0)
                    {
                        //logger_.Error("Error to connect reader = " + ReaderName);
                        return -1;
                    }
                }
                // load key
                var retLoadKey = LoadKey(keyNum, keyString);
                if (retLoadKey < 0)
                {
                    //logger_.Error("Error to load key !");
                    return -1;
                }
                // authenticate
                var retAuth = Auth(keyNum, blockNumber, isKeyB);
                if (retAuth < 0)
                {
                    //logger_.Error("Error to authenticate !");
                    return -1;
                }
                // write data
                int length = WriteData.Length;
                var retWrite = BinUpd(startBlock, length, WriteData);
                if (retWrite < 0)
                {
                    //logger_.Error("Error to write data !");
                    return -1;
                }
                else
                {
                    //logger_.Error("Success to write data !");
                    return 1;
                }
            }
            catch (Exception ex)
            {
                //logger_.Error(ex.Message);
                //logger_.Error(ex.StackTrace);

                return -1;
            }
        }

        private int BinUpd(int startBlock, int length, string WriteData)
        {
            try
            {
                if (startBlock > _maximumBlock)
                {
                    //logger_.Error("Please key-in valid Start Block. Valid value: 0 - " + _maximumBlock.ToString() + ".");
                    return -1;
                }

                if (startBlock <= 127)
                {
                    if ((startBlock + 1) % 4 == 0)
                    {
                        //logger_.Error("The block to be updated is a sector trailer. Please change the Start Block Number.");
                        return -1;
                    }
                }

                if (startBlock > 127)
                {
                    if ((startBlock + 1) % 16 == 0)
                    {
                        //logger_.Error("The block to be updated is a sector trailer. Please change the Start Block Number.");
                        return -1;
                    }
                }

                if (length != 16 || length == 0)
                {
                    //logger_.Error("Invalid Length. Length must be 16.");
                    return -1;
                }

                if (WriteData == "")
                {
                    //logger_.Error("Please key-in the data to write.");
                    return -1;
                }

                String tmpStr = "";

                byte[] buff = new byte[16];
                tmpStr = WriteData;
                buff = Encoding.ASCII.GetBytes(tmpStr);

                if (buff.Length < 16)
                    Array.Resize(ref buff, 16);

                //logger_.Info("Update Binary");

                mifareClassic_.updateBinary((byte)startBlock, buff, (byte)length);
                //logger_.Info("Update success");

                return 1;

            }
            catch (CardException cardException)
            {
                //logger_.Error("[" + Helper.byteAsString(cardException.statusWord, true) + "] " + cardException.Message);

                return -1;
            }
            catch (PcscException pcscException)
            {
                //logger_.Error("[" + pcscException.errorCode.ToString() + "] " + pcscException.Message);

                return -1;
            }
            catch (Exception generalException)
            {
                //logger_.Error(generalException.Message);

                return -1;
            }
        }
    }
}
