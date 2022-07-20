using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TN.NFC.Core.MifareCore
{
    internal class Apdu
    {
        private byte _instructionClass;
        private byte _instructionCode;
        private byte _parameter1;
        private byte _parameter2;
        private byte _parameter3;
        private int _lengthExpected;
        private int _actualLengthReceived;

        private byte[] _data;
        private byte[] _response;
        private byte[] _statusWord;

        internal Apdu()
        {

        }

        internal Apdu(byte instructionClass, byte instructionCode, byte param1, byte param2, byte param3)
        {
            this.setCommand(new byte[] { instructionClass, instructionCode, param1, param2, param3 });
        }

        internal Apdu(byte[] cmd)
        {
            if (cmd.Length != 5)
                throw new Exception("Invalid command");

            this.setCommand(new byte[] { cmd[0], cmd[1], cmd[2], cmd[3], cmd[4] });
        }

        /// <summary>
        /// The T=0 instruction class.
        /// </summary>
        internal byte instructionClass
        {
            get { return _instructionClass; }
            set { _instructionClass = value; }
        }

        /// <summary>
        /// An instruction code in the T=0 instruction class.
        /// </summary>        
        internal byte instructionCode
        {
            get { return _instructionCode; }
            set { _instructionCode = value; }
        }

        /// <summary>
        /// Reference codes that complete the instruction code.
        /// </summary>
        internal byte parameter1
        {
            get { return _parameter1; }
            set { _parameter1 = value; }
        }

        /// <summary>
        /// Reference codes that complete the instruction code.
        /// </summary>
        internal byte parameter2
        {
            get { return _parameter2; }
            set { _parameter2 = value; }
        }

        /// <summary>
        /// The number of data bytes to be transmitted during the command, per ISO 7816-4, Section 8.2.1.
        /// </summary>
        internal byte parameter3
        {
            get { return _parameter3; }
            set { _parameter3 = value; }
        }

        /// <summary>
        /// Length of data expected from the card
        /// </summary>
        internal int lengthExpected
        {
            get { return _lengthExpected; }
            set { _lengthExpected = value; }
        }

        internal int actualLengthReceived
        {
            get { return _actualLengthReceived; }
            set { _actualLengthReceived = value; }
        }

        internal byte[] data
        {
            get { return _data; }
            set { _data = value; }
        }

        internal byte[] response
        {
            get { return _response; }
            set { _response = value; }
        }

        internal byte[] statusWord
        {
            get { return _statusWord; }
            set { _statusWord = value; }
        }

        internal void setCommand(byte[] cmd)
        {
            if (cmd.Length != 5)
                throw new Exception("Invalid command");

            instructionClass = cmd[0];
            instructionCode = cmd[1];
            parameter1 = cmd[2];
            parameter2 = cmd[3];
            parameter3 = cmd[4];

            data = new byte[parameter3];
        }

        internal bool statusWordEqualTo(byte[] data)
        {
            if (statusWord == null)
                return false;

            for (int i = 0; i < statusWord.Length; i++)
                if (statusWord[i] != data[i])
                    return false;

            return true;
        }
    }
}
