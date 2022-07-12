using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NfcSample.Core
{
    internal class CardException : Exception
    {
        protected byte[] _statusWord;
        public byte[] statusWord
        {
            get { return _statusWord; }
        }

        protected string _message;
        public override string Message
        {
            get { return _message; }
        }

        public CardException(string message, byte[] sw)
        {
            _message = message;
            _statusWord = sw;
        }
    }
}
