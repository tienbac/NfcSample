using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace TM.NFC.Core.Entity
{
    internal enum StatusResponse
    {
        OK = 1,
        NotOk = 0
    }

    internal class Response
    {
        public StatusResponse Status { get; set; }
        public string ErrorMessage { get; set; }
    }
}
