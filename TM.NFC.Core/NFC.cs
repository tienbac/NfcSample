using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TN.NFC.Core.CardCore;
using TN.NFC.Core.Entity;
using TN.NFC.Core.MifareCore;
using TN.NFC.Core.PcscCore;

namespace TN.NFC.Core
{
    internal class NFC
    {
        private PcscReader _pcscReader = new PcscReader();
        private CardPolling _cardPolling;
        private static string PICC = "";
        public static Response RESPONSE = null;
        
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
                RESPONSE = new Response("0000000000000000", StatusResponse.NotOk, pcscException.Message, null);
            }
            catch (Exception generalException)
            {
                RESPONSE = new Response("0000000000000000", StatusResponse.NotOk, generalException.Message, null);
            }
        }

        public void cardPolling_OnCardRemoved(object sender, CardPollingEventArg e)
        {
            Console.WriteLine("Card is removed");
            Console.WriteLine($"{e.status} | {e.reader} | {e.atr} | {e.currentStatus} | {e._currentStatus}");
            RESPONSE = new Response("0000000000000000", StatusResponse.NotOk, "Card is removed !", null);
        }

        public void cardPolling_OnError(object sender, CardPollingErrorEventArg e)
        {
            _cardPolling.stop();

            Console.WriteLine(e.errorMessage);

            RESPONSE = new Response("0000000000000000", StatusResponse.NotOk, "Card is removed !", null);
        }

        public void cardPolling_OnCardFound(object sender, CardPollingEventArg e)
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
                byte[] ReadData = new byte[16];

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
}
