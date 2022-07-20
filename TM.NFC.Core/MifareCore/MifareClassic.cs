using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TN.NFC.Core.CardCore;
using TN.NFC.Core.PcscCore;

namespace TN.NFC.Core.MifareCore
{
    internal class MifareClassic
    {
        internal enum VALUEBLOCKOPERATION
        {
            STORE = 0,
            INCREMENT = 1,
            DECREMENT = 2,
        }

        internal MifareClassic(string readerName)
        {
            _pcscConnection = new PcscReader(readerName);
        }

        internal MifareClassic(PcscReader pcsc)
        {
            _pcscConnection = pcsc;
        }

        private PcscReader _pcscConnection;
        internal PcscReader pcscConnection
        {
            get { return _pcscConnection; }
            set { _pcscConnection = value; }
        }

        internal void valueBlock(byte blockNumber, VALUEBLOCKOPERATION transType, int amount)
        {
            Apdu apdu;
            apdu = new Apdu();
            apdu.setCommand(new byte[] { 0xFF, 0xD7, 0x00, blockNumber, 0x05 });

            apdu.data = new byte[5];
            apdu.data[0] = (byte)transType;
            Array.Copy(Helper.intToByte(amount), 0, apdu.data, 1, 4);

            pcscConnection.sendCommand(ref apdu);

            if (!apdu.statusWordEqualTo(new byte[] { 0x90, 0x00 }))
                throw new CardException("Value block operation failed", apdu.statusWord);
        }

        internal byte[] readBinary(byte blockNumber, byte length)
        {
            Apdu apdu;

            apdu = new Apdu();
            apdu.setCommand(new byte[] { 0xFF, 0xB0, 0x00, blockNumber, length });
            apdu.data = new byte[0];
            apdu.lengthExpected = length;

            pcscConnection.sendCommand(ref apdu);
            if (apdu.statusWord[0] != 0x90)
                throw new CardException("Read failed", apdu.statusWord);

            return apdu.response.Take(length).ToArray();
        }

        internal void updateBinary(byte blockNumber, byte[] data, byte length)
        {
            Apdu apdu;
            int retCode;

            if (data.Length > 48)
                throw new Exception("Data has invalid length");

            if (length % 16 != 0)
            {
                throw new Exception("Data length must be multiple of 16");
            }

            //if (data.Length != 16)
            //    Array.Resize(ref data, 16);

            apdu = new Apdu();
            apdu.setCommand(new byte[] { 0xFF, 0xD6, 0x00, blockNumber, length });

            apdu.data = new byte[data.Length];
            Array.Copy(data, apdu.data, length);

            pcscConnection.sendCommand(ref apdu);

            if (apdu.statusWord[0] != 0x90)
                throw new CardException("Update failed", apdu.statusWord);
        }
    }
}
