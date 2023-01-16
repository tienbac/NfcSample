using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TN.NFC.Core.CardCore;
using TN.NFC.Core.Entity;
using TN.NFC.Core.MifareCore;
using TN.NFC.Core.PcscCore;

namespace Test
{
    class ProcessNfc
    {
        private PcscReader _pcscReader = new PcscReader();
        private CardPolling _cardPolling;
        public static string PICC = "";
        public static Response RESPONSE = null;
        private static bool ISREAD = true;

        // ĐIỀN TRUE = ĐỌC THẺ => TRẠM RA
        // ĐIỀN FALSE = GHI THẺ => TRẠM VÀO
        public void Initial(bool isRead = true)
        {
            try
            {
                ISREAD = isRead;

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
                RESPONSE = new Response("0000000000000000", StatusResponse.NotOk, pcscException.Message, null);
            }
            catch (Exception generalException)
            {
                RESPONSE = new Response("0000000000000000", StatusResponse.NotOk, generalException.Message, null);
            }
        }

        internal void cardPolling_OnCardRemoved(object sender, CardPollingEventArg e)
        {
            // THAY CONSOLE.WRITELINE = LOG HOẶC BỎ ĐI
            Console.WriteLine("Card is removed");
            RESPONSE = new Response("0000000000000000", StatusResponse.NotOk, "Card is removed !", null);
        }

        internal void cardPolling_OnError(object sender, CardPollingErrorEventArg e)
        {
            _cardPolling.stop();

            // THAY CONSOLE.WRITELINE = LOG HOẶC BỎ ĐI
            Console.WriteLine(e.errorMessage);

            RESPONSE = new Response("0000000000000000", StatusResponse.NotOk, "Card is removed !", null);
        }

        internal void cardPolling_OnCardFound(object sender, CardPollingEventArg e)
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
                // THAY CONSOLE.WRITELINE = LOG HOẶC BỎ ĐI
                Console.WriteLine($"Error: {pcscException.Message}");
            }
            catch (Exception generalException)
            {
                // THAY CONSOLE.WRITELINE = LOG HOẶC BỎ ĐI
                Console.WriteLine($"Error: {generalException.Message}");
            }

            if (cardName != "")
            {
                // THAY CONSOLE.WRITELINE = LOG HOẶC BỎ ĐI
                Console.WriteLine("+=============================================================+");
                Console.WriteLine($"Card Found !");
                Console.WriteLine($"ATR             : {Helper.byteAsString(e.atr, true)}");
                Console.WriteLine($"Card Type       : {cardName}");
                Console.WriteLine($"Active Protocol : {activeProtocol}");
                Console.WriteLine("+=============================================================+");

                var reader = new MifareHelper();
                var keyNum = Convert.ToByte(1);
                var keyString = "FF FF FF FF FF FF";
                var isKeyB = false;
                byte blockLength = 16;
                string ReadData1 = "";
                byte[] ReadData4 = new byte[16];
                byte[] ReadData5 = new byte[16];
                byte[] ReadData6 = new byte[16];
                var aes = new TN.NFC.Core.Encrypt.AscEncrypt();

                // KEY BÊN VE CẤP
                var key = "92B0D1CCBB3877B6";

                if (ISREAD)
                {
                    // READ:

                    // Đọc EMEI
                    var blockNum = Convert.ToByte(1);
                    var startBlock = Convert.ToByte(1);
                    var ret = reader.ReadDataSimple(PICC, keyNum, keyString, blockNum, isKeyB, startBlock, blockLength, out ReadData1);

                    // THAY CONSOLE.WRITELINE = LOG HOẶC BỎ ĐI
                    Console.WriteLine(ret < 0 ? "Read data fail !" : $"Block : {blockNum} | Read data successful | {ReadData1.Trim()}");

                    // ĐỌC BLOCK4
                    blockNum = Convert.ToByte(4);
                    startBlock = Convert.ToByte(4);
                    ret = reader.ReadDataSimple(PICC, keyNum, keyString, blockNum, isKeyB, startBlock, blockLength, out ReadData4);
                    Console.WriteLine(ret < 0 ? "Read data fail !" : $"Block : {blockNum} | Read data successful");

                    // ĐỌC BLOCK5
                    blockNum = Convert.ToByte(5);
                    startBlock = Convert.ToByte(5);
                    ret = reader.ReadDataSimple(PICC, keyNum, keyString, blockNum, isKeyB, startBlock, blockLength, out ReadData5);
                    Console.WriteLine(ret < 0 ? "Read data fail !" : $"Block : {blockNum} | Read data successful");

                    // ĐỌC BLOCK6
                    blockNum = Convert.ToByte(6);
                    startBlock = Convert.ToByte(6);
                    ret = reader.ReadDataSimple(PICC, keyNum, keyString, blockNum, isKeyB, startBlock, blockLength, out ReadData6);
                    Console.WriteLine(ret < 0 ? "Read data fail !" : $"Block : {blockNum} | Read data successful");

                    if (ReadData4 != null && ReadData5 != null && ReadData6 != null)
                    {
                        // GHÉP DATA 3 BLOCK 4 5 6
                        var data = ReadData4.Concat(ReadData5).Concat(ReadData6).ToArray();

                        // GIẢI MÃ DATA GHÉP TỪ BLOCK 4 5 6
                        var vehicle = aes.DecryptStringFromBytes_Aes(data, key);

                        //Console.WriteLine(JsonConvert.SerializeObject(vehicle));

                        // DATA RETURN
                        RESPONSE = new Response(ReadData1.Replace("\u0000", ""), StatusResponse.OK, "Read successful !", vehicle);
                        Console.WriteLine(JsonConvert.SerializeObject(RESPONSE));
                    }
                }
                else
                {
                    // GHI DATA
                    // Tạo string bằng cách truyền vào StationID | LaneInId | Status | Type | InTime | Plate
                    var data = new VehicleData("1182", "001", StatusBlock1.Unpaid, TypeBlock1.BinhThuong, DateTime.Now,
                        "30H25422", 1).ToString();

                    // Mã hóa
                    var dataHex = aes.EncryptStringToBytes_Aes(data, key);

                    // Chia data đã mã hóa thành 3 block
                    var b4 = dataHex.Skip(0).Take(16).ToArray();
                    var b5 = dataHex.Skip(16).Take(16).ToArray();
                    var b6 = dataHex.Skip(32).Take(16).ToArray();

                    // Ghi gói data b4 vào block số 4
                    var blockNum = Convert.ToByte(4);
                    var startBlock = Convert.ToByte(4);
                    var ret = reader.WriteDataSimple(ProcessNFC.PICC, keyNum, keyString, blockNum, isKeyB, startBlock, b4);

                    // THAY CONSOLE.WRITELINE = LOG HOẶC BỎ ĐI
                    Console.WriteLine(ret < 0 ? "Write data fail !" : $"Block : {blockNum} | Write successful !");

                    // Ghi gói data b5 vào block số 5
                    blockNum = Convert.ToByte(5);
                    startBlock = Convert.ToByte(5);
                    ret = reader.WriteDataSimple(ProcessNFC.PICC, keyNum, keyString, blockNum, isKeyB, startBlock, b5);
                    Console.WriteLine(ret < 0 ? "Write data fail !" : $"Block : {blockNum} | Write successful !");

                    // Ghi gói data b6 vào block số 6
                    blockNum = Convert.ToByte(6);
                    startBlock = Convert.ToByte(6);
                    ret = reader.WriteDataSimple(ProcessNFC.PICC, keyNum, keyString, blockNum, isKeyB, startBlock, b6);
                    Console.WriteLine(ret < 0 ? "Write data fail !" : $"Block : {blockNum} | Write successful !");
                }
            }
            else
            {
                // THAY CONSOLE.WRITELINE = LOG HOẶC BỎ ĐI
                Console.WriteLine("+=============================================================+");
                Console.WriteLine("Not Found");
                Console.WriteLine("+=============================================================+");
            }
        }
    }
}
