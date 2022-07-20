using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TN.NFC.Core.CardCore
{
    internal class CardException : Exception
    {
        protected byte[] _statusWord;
        internal byte[] statusWord => _statusWord;

        protected string _message;
        public override string Message => _message;

        internal CardException(string message, byte[] sw)
        {
            _message = message;
            _statusWord = sw;
        }
    }
}
