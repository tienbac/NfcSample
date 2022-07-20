using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TN.NFC.Core.PcscCore
{
    public class PcscException : Exception
    {
        private int _errorCode;
        public int errorCode
        {
            get { return _errorCode; }
        }

        private string _message;
        public override string Message => _message;

        public PcscException(int errCode)
        {
            _errorCode = errCode;
            _message = PcscProvider.GetScardErrMsg(errCode);
        }

        public PcscException(string message)
        {
            _message = message;
        }
    }
}
